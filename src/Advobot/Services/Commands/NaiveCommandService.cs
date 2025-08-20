using Advobot.Attributes;
using Advobot.CommandAssemblies;
using Advobot.Localization;
using Advobot.Modules;
using Advobot.Services.BotConfig;
using Advobot.Services.GuildSettings;
using Advobot.Services.Help;
using Advobot.TypeReaders;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

using System.Collections.Concurrent;
using System.Globalization;
using System.Reflection;

namespace Advobot.Services.Commands;

/// <summary>
/// Handles user input commands.
/// </summary>
public sealed class NaiveCommandService
{
	private readonly IRuntimeConfig _BotConfig;
	private readonly DiscordShardedClient _Client;
	private readonly AsyncEvent<Func<CommandInfo, ICommandContext, IResult, Task>> _CommandInvoked = new();
	private readonly Localized<CommandService> _CommandService;
	private readonly CommandServiceConfig _Config;
	private readonly ConcurrentDictionary<ulong, byte> _GatheringUsers = new();
	private readonly IGuildSettingsService _GuildSettings;
	private readonly IHelpService _Help;
	private readonly AsyncEvent<Func<LogMessage, Task>> _Log = new();
	private readonly AsyncEvent<Func<Task>> _Ready = new();
	private readonly IServiceProvider _Services;
	private int _ShardsReady;

	/// <inheritdoc />
	public event Func<CommandInfo, ICommandContext, IResult, Task> CommandInvoked
	{
		add => _CommandInvoked.Add(value);
		remove => _CommandInvoked.Remove(value);
	}

	/// <inheritdoc />
	public event Func<LogMessage, Task> Log
	{
		add => _Log.Add(value);
		remove => _Log.Remove(value);
	}

	/// <inheritdoc />
	public event Func<Task> Ready
	{
		add => _Ready.Add(value);
		remove => _Ready.Remove(value);
	}

	/// <summary>
	/// Creates an instance of <see cref="NaiveCommandService"/>.
	/// </summary>
	/// <param name="services"></param>
	/// <param name="commandConfig"></param>
	/// <param name="client"></param>
	/// <param name="botConfig"></param>
	/// <param name="guildSettings"></param>
	/// <param name="help"></param>
	public NaiveCommandService(
		IServiceProvider services,
		CommandServiceConfig commandConfig,
		DiscordShardedClient client,
		IRuntimeConfig botConfig,
		IGuildSettingsService guildSettings,
		IHelpService help)
	{
		_Services = services;
		_Config = commandConfig;
		_Client = client;
		_BotConfig = botConfig;
		_GuildSettings = guildSettings;
		_Help = help;
		_CommandService = new(_ =>
		{
			var commands = new CommandService(_Config);
			commands.Log += OnLog;
			commands.CommandExecuted += OnCommandExecuted;
			return commands;
		});

		_Client.ShardReady += OnShardReady;
		_Client.MessageReceived += OnMessageReceived;
	}

	/// <summary>
	/// Adds commands to this service.
	/// </summary>
	/// <param name="assemblies"></param>
	/// <returns></returns>
	/// <exception cref="InvalidCastException"></exception>
	public async Task AddCommandsAsync(IEnumerable<CommandAssembly> assemblies)
	{
		var commandServices = assemblies
			.SelectMany(x => x.SupportedCultures)
			.ToHashSet()
			.Select(_CommandService.Get)
			.ToArray();
		var types = assemblies
			.Select(x => x.Assembly)
			.Prepend(Assembly.GetExecutingAssembly())
			.SelectMany(x => x.GetTypes());
		foreach (var type in types)
		{
			var attr = type.GetCustomAttribute<TypeReaderTargetTypeAttribute>();
			if (attr is null)
			{
				continue;
			}
			if (Activator.CreateInstance(type) is not TypeReader instance)
			{
				throw new InvalidCastException($"{type} is not a {nameof(TypeReader)}.");
			}

			foreach (var commandService in commandServices)
			{
				foreach (var targetType in attr.TargetTypes)
				{
					commandService.AddTypeReader(targetType, instance, replaceDefault: true);
				}
			}
		}

		foreach (var assembly in assemblies)
		{
			foreach (var culture in assembly.SupportedCultures)
			{
				await AddCommandsAsync(culture, assembly.Assembly).ConfigureAwait(false);
			}
		}
	}

	private async Task AddCommandsAsync(CultureInfo culture, Assembly assembly)
	{
		var commandService = _CommandService.Get(culture);
		var modules = await commandService.AddModulesAsync(assembly, _Services).ConfigureAwait(false);

		static IEnumerable<ModuleInfo> GetAllModules(ModuleInfo module)
		{
			yield return module;
			foreach (var submodule in module.Submodules)
			{
				foreach (var m in GetAllModules(submodule))
				{
					yield return m;
				}
			}
		}

		static void ThrowIfDuplicateId(
			ModuleInfo module,
			Dictionary<Guid, ModuleInfo> ids,
			MetaAttribute meta)
		{
			if (!ids.TryGetValue(meta.Guid, out var original))
			{
				ids.Add(meta.Guid, module);
				return;
			}

			var shouldThrow = true;
			for (var m = module; m != null; m = m.Parent)
			{
				if (m == original)
				{
					shouldThrow = false;
					break;
				}
			}

			if (shouldThrow)
			{
				throw new InvalidOperationException($"Duplicate id between {original.Name} and {module.Name}.");
			}
		}

		int commandCount = 0, helpEntryCount = 0;
		var ids = new Dictionary<Guid, ModuleInfo>();
		var categories = new HashSet<string>();
		foreach (var module in modules.SelectMany(GetAllModules))
		{
			var meta = module.Attributes.OfType<MetaAttribute>().FirstOrDefault();
			if (meta is null)
			{
				continue;
			}
			ThrowIfDuplicateId(module, ids, meta);

			++commandCount;
			if (!module.Attributes.Any(a => a is HiddenAttribute))
			{
				++helpEntryCount;

				var category = module.Attributes.OfType<CategoryAttribute>().First();
				categories.Add(category.Category);
				_Help.Add(new HelpModule(module, meta, category));
			}
		}

		await OnLog(new(
			severity: LogSeverity.Info,
			source: nameof(AddCommandsAsync),
			message: $"Successfully loaded {categories.Count} categories " +
				$"containing {commandCount} commands " +
				$"({helpEntryCount} were given help entries) " +
				$"from {assembly.GetName().Name} in the {culture} culture."
		)).ConfigureAwait(false);
	}

	private Task OnCommandExecuted(
		Optional<CommandInfo> command,
		ICommandContext context,
		IResult result)
	{
		if (!command.IsSpecified || result.Error == CommandError.UnknownCommand)
		{
			return Task.CompletedTask;
		}
		return _CommandInvoked.InvokeAsync(command.Value, context, result);
	}

	private Task OnLog(LogMessage arg)
		=> _Log.InvokeAsync(arg);

	private async Task OnMessageReceived(IMessage message)
	{
		if (_ShardsReady != _Client.Shards.Count
			|| _BotConfig.Pause
			|| message.Author.IsBot
			|| string.IsNullOrWhiteSpace(message.Content)
			|| _BotConfig.UsersIgnoredFromCommands.Contains(message.Author.Id)
			|| message is not SocketUserMessage msg
			|| msg.Author is not SocketGuildUser user)
		{
			return;
		}

		if (!user.Guild.HasAllMembers && _GatheringUsers.TryAdd(user.Guild.Id, 0))
		{
			_ = user.Guild.DownloadUsersAsync();
		}

		var argPos = -1;
		if (!msg.HasMentionPrefix(_Client.CurrentUser, ref argPos))
		{
			var prefix = await _GuildSettings.GetPrefixAsync(user.Guild).ConfigureAwait(false);
			if (!msg.HasStringPrefix(prefix, ref argPos))
			{
				return;
			}
		}

		var culture = await _GuildSettings.GetCultureAsync(user.Guild).ConfigureAwait(false);
		CultureInfo.CurrentUICulture = culture;
		CultureInfo.CurrentCulture = culture;
		var commands = _CommandService.Get(culture);
		var context = new AdvobotCommandContext(_Client, msg);
		await commands.ExecuteAsync(context, argPos, _Services).ConfigureAwait(false);
	}

	private async Task OnShardReady(DiscordSocketClient _)
	{
		if (++_ShardsReady < _Client.Shards.Count)
		{
			return;
		}
		_Client.ShardReady -= OnShardReady;

		var game = _BotConfig.Game;
		var stream = _BotConfig.Stream;
		var activityType = ActivityType.Playing;
		if (!string.IsNullOrWhiteSpace(stream))
		{
			stream = $"https://www.twitch.tv/{stream[(stream.LastIndexOf('/') + 1)..]}";
			activityType = ActivityType.Streaming;
		}
		await _Client.SetGameAsync(game, stream, activityType).ConfigureAwait(false);

		await _Ready.InvokeAsync().ConfigureAwait(false);
	}
}