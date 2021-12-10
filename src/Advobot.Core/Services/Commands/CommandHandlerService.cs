using Advobot.Attributes;
using Advobot.CommandAssemblies;
using Advobot.Localization;
using Advobot.Modules;
using Advobot.Services.BotSettings;
using Advobot.Services.GuildSettingsProvider;
using Advobot.Services.HelpEntries;
using Advobot.Utilities;

using AdvorangesUtils;

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
internal sealed class CommandHandlerService : ICommandHandlerService
{
	private readonly IBotSettings _BotSettings;
	private readonly DiscordShardedClient _Client;
	private readonly AsyncEvent<Func<CommandInfo, ICommandContext, IResult, Task>> _CommandInvoked = new();
	private readonly Localized<CommandService> _CommandService;
	private readonly CommandServiceConfig _Config;
	private readonly ConcurrentDictionary<ulong, byte> _GatheringUsers = new();
	private readonly IGuildSettingsProvider _GuildSettings;
	private readonly IHelpEntryService _Help;
	private readonly AsyncEvent<Func<LogMessage, Task>> _Log = new();
	private readonly IServiceProvider _Provider;
	private readonly AsyncEvent<Func<Task>> _Ready = new();
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
	/// Creates an instance of <see cref="CommandHandlerService"/>.
	/// </summary>
	/// <param name="provider"></param>
	/// <param name="config"></param>
	/// <param name="client"></param>
	/// <param name="botSettings"></param>
	/// <param name="guildSettings"></param>
	/// <param name="help"></param>
	public CommandHandlerService(
		IServiceProvider provider,
		CommandServiceConfig config,
		DiscordShardedClient client,
		IBotSettings botSettings,
		IGuildSettingsProvider guildSettings,
		IHelpEntryService help)
	{
		_Provider = provider;
		_Config = config;
		_Client = client;
		_BotSettings = botSettings;
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

	public async Task AddCommandsAsync(IEnumerable<CommandAssembly> assemblies)
	{
		var currentCulture = CultureInfo.CurrentUICulture;

		AddTypeReaders(assemblies);
		foreach (var assembly in assemblies)
		{
			foreach (var culture in assembly.SupportedCultures)
			{
				await AddCommandsAsync(culture, assembly.Assembly).CAF();
			}
		}

		CultureInfo.CurrentUICulture = currentCulture;
	}

	private static void ThrowIfDuplicateId(
		ModuleInfo module,
		IDictionary<Guid, ModuleInfo> ids,
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

	private async Task AddCommandsAsync(CultureInfo culture, Assembly assembly)
	{
		CultureInfo.CurrentUICulture = culture;
		var commandService = _CommandService.Get();

		var modules = await commandService.AddModulesAsync(assembly, _Provider).CAF();

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

		int commandCount = 0, helpEntryCount = 0;
		var ids = new Dictionary<Guid, ModuleInfo>();
		var newCategories = new HashSet<string>();
		foreach (var module in modules.SelectMany(GetAllModules))
		{
			var meta = module.Attributes.GetAttribute<MetaAttribute>();
			if (meta is null)
			{
				continue;
			}
			ThrowIfDuplicateId(module, ids, meta);

			++commandCount;
			if (!module.Attributes.Any(a => a is HiddenAttribute))
			{
				++helpEntryCount;

				var category = module.Attributes.GetAttribute<CategoryAttribute>();
				newCategories.Add(category.Category);
				_Help.Add(new ModuleHelpEntry(module, meta, category));
			}
		}

		ConsoleUtils.WriteLine($"Successfully loaded {newCategories.Count} categories " +
			$"containing {commandCount} commands " +
			$"({helpEntryCount} were given help entries) " +
			$"from {assembly.GetName().Name} in the {culture} culture.");
	}

	private void AddTypeReaders(IEnumerable<CommandAssembly> assemblies)
	{
		var cultures = new HashSet<CultureInfo>();
		var typeReaders = Assembly.GetExecutingAssembly().CreateTypeReaders();
		foreach (var assembly in assemblies)
		{
			typeReaders.AddRange(assembly.Assembly.CreateTypeReaders());

			foreach (var culture in assembly.SupportedCultures)
			{
				cultures.Add(culture);
			}
		}

		foreach (var culture in cultures)
		{
			CultureInfo.CurrentUICulture = culture;
			var commandService = _CommandService.Get();

			foreach (var typeReader in typeReaders)
			{
				foreach (var type in typeReader.TargetTypes)
				{
					commandService.AddTypeReader(type, typeReader.Instance, true);
				}
			}
		}
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
			|| _BotSettings.Pause
			|| message.Author.IsBot
			|| string.IsNullOrWhiteSpace(message.Content)
			|| _BotSettings.UsersIgnoredFromCommands.Contains(message.Author.Id)
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
			var prefix = await _GuildSettings.GetPrefixAsync(user.Guild).CAF();
			if (!msg.HasStringPrefix(prefix, ref argPos))
			{
				return;
			}
		}

		var culture = await _GuildSettings.GetCultureAsync(user.Guild).CAF();
		CultureInfo.CurrentUICulture = culture;
		CultureInfo.CurrentCulture = culture;
		var commands = _CommandService.Get();
		var context = new AdvobotCommandContext(_Client, msg);
		await commands.ExecuteAsync(context, argPos, _Provider).CAF();
	}

	private async Task OnShardReady(DiscordSocketClient _)
	{
		if (++_ShardsReady < _Client.Shards.Count)
		{
			return;
		}
		_Client.ShardReady -= OnShardReady;

		var game = _BotSettings.Game;
		var stream = _BotSettings.Stream;
		var activityType = ActivityType.Playing;
		if (!string.IsNullOrWhiteSpace(stream))
		{
			stream = $"https://www.twitch.tv/{stream[(stream.LastIndexOf('/') + 1)..]}";
			activityType = ActivityType.Streaming;
		}
		await _Client.SetGameAsync(game, stream, activityType).CAF();

		await _Ready.InvokeAsync().CAF();
	}
}