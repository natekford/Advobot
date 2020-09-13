using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Advobot.Attributes;
using Advobot.CommandAssemblies;
using Advobot.Localization;
using Advobot.Modules;
using Advobot.Services.BotSettings;
using Advobot.Services.GuildSettings;
using Advobot.Services.HelpEntries;
using Advobot.Utilities;

using AdvorangesUtils;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Advobot.Services.Commands
{
	/// <summary>
	/// Handles user input commands.
	/// </summary>
	internal sealed class CommandHandlerService : ICommandHandlerService
	{
		private readonly IBotSettings _BotSettings;
		private readonly DiscordShardedClient _Client;
		private readonly CommandServiceConfig _CommandConfig;
		private readonly AsyncEvent<Func<CommandInfo, ICommandContext, IResult, Task>> _CommandInvoked
			= new AsyncEvent<Func<CommandInfo, ICommandContext, IResult, Task>>();
		private readonly Localized<CommandService> _CommandService;
		private readonly ConcurrentDictionary<ulong, byte> _GatheringUsers
			= new ConcurrentDictionary<ulong, byte>();
		private readonly IGuildSettingsFactory _GuildSettings;
		private readonly IHelpEntryService _HelpEntries;
		private readonly AsyncEvent<Func<LogMessage, Task>> _Log
			= new AsyncEvent<Func<LogMessage, Task>>();
		private readonly IServiceProvider _Provider;
		private readonly AsyncEvent<Func<Task>> _Ready
			= new AsyncEvent<Func<Task>>();
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
			IGuildSettingsFactory guildSettings,
			IHelpEntryService help)
		{
			_Provider = provider;
			_CommandConfig = config;
			_Client = client;
			_BotSettings = botSettings;
			_GuildSettings = guildSettings;
			_HelpEntries = help;
			_CommandService = new Localized<CommandService>(_ =>
			{
				var commands = new CommandService(_CommandConfig);
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
			var defaultTypeReaders = Assembly.GetExecutingAssembly().CreateTypeReaders();
			foreach (var assembly in assemblies)
			{
				var typeReaders = assembly.Assembly.CreateTypeReaders().Concat(defaultTypeReaders);
				foreach (var culture in assembly.SupportedCultures)
				{
					await AddCommandsAsync(culture, assembly.Assembly, typeReaders).CAF();
				}
			}
			CultureInfo.CurrentUICulture = currentCulture;
		}

		private async Task AddCommandsAsync(
			CultureInfo culture,
			Assembly assembly,
			IEnumerable<TypeReaderInfo> typeReaders)
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

			var modules = await commandService.AddModulesAsync(assembly, _Provider).CAF();
			int moduleCount = 0, commandCount = 0, helpEntryCount = 0;
			var ids = new Dictionary<Guid, ModuleInfo>();
			foreach (var module in modules)
			{
				++moduleCount;
				foreach (var command in module.Submodules)
				{
					++commandCount;
					var attributes = command.Attributes;

					var meta = attributes.GetAttribute<MetaAttribute>();
					if (ids.TryGetValue(meta.Guid, out var original))
					{
						throw new InvalidOperationException($"Duplicate id between {original.Name} and {command.Name}.");
					}
					ids.Add(meta.Guid, command);

					if (!attributes.Any(a => a is HiddenAttribute))
					{
						++helpEntryCount;

						var category = attributes.GetAttribute<CategoryAttribute>();
						_HelpEntries.Add(new ModuleHelpEntry(command, meta, category));
					}
				}
			}

			ConsoleUtils.WriteLine($"Successfully loaded {moduleCount} modules " +
				$"containing {commandCount} commands " +
				$"({helpEntryCount} were given help entries) " +
				$"from {assembly.GetName().Name} in the {culture} culture.");
		}

		private async Task OnCommandExecuted(
			Optional<CommandInfo> command,
			ICommandContext context,
			IResult result)
		{
			static bool CanBeIgnored(IAdvobotCommandContext c, IResult r)
			{
				return r == null
					|| r.Error == CommandError.UnknownCommand
					|| (!r.IsSuccess && (r.ErrorReason == null || c.Settings.NonVerboseErrors))
					|| (r is PreconditionGroupResult g && g.PreconditionResults.All(x => CanBeIgnored(c, x)));
			}

			if (!(context is IAdvobotCommandContext ctx))
			{
				throw new ArgumentException(nameof(context));
			}
			if (!command.IsSpecified || CanBeIgnored(ctx, result))
			{
				return;
			}

			await _CommandInvoked.InvokeAsync(command.Value, ctx, result).CAF();
		}

		private Task OnLog(LogMessage arg)
			=> _Log.InvokeAsync(arg);

		private async Task OnMessageReceived(IMessage message)
		{
			var argPos = -1;
			if (_ShardsReady != _Client.Shards.Count
				|| _BotSettings.Pause
				|| message.Author.IsBot
				|| string.IsNullOrWhiteSpace(message.Content)
				|| _BotSettings.UsersIgnoredFromCommands.Contains(message.Author.Id)
				|| !(message is SocketUserMessage msg)
				|| !(msg.Author is SocketGuildUser user)
				|| !(await _GuildSettings.GetOrCreateAsync(user.Guild).CAF() is IGuildSettings settings)
				|| settings.IgnoredCommandChannels.Contains(msg.Channel.Id)
				|| !(msg.HasStringPrefix(settings.GetPrefix(_BotSettings), ref argPos)
					|| msg.HasMentionPrefix(_Client.CurrentUser, ref argPos)))
			{
				return;
			}

			if (!user.Guild.HasAllMembers && _GatheringUsers.TryAdd(user.Guild.Id, 0))
			{
				_ = user.Guild.DownloadUsersAsync();
			}

			CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(settings.Culture);
			var context = new AdvobotCommandContext(settings, _Client, msg);
			var commands = _CommandService.Get();
			await commands.ExecuteAsync(context, argPos, _Provider).CAF();
		}

		private async Task OnShardReady(DiscordSocketClient _)
		{
			if (++_ShardsReady < _Client.Shards.Count)
			{
				return;
			}
			_Client.ShardReady -= OnShardReady;

			await _Client.UpdateGameAsync(_BotSettings).CAF();
			await _Ready.InvokeAsync().CAF();
		}
	}
}