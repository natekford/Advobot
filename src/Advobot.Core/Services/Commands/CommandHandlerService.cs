using System;
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
		private readonly IGuildSettingsFactory _GuildSettings;
		private readonly IHelpEntryService _HelpEntries;

		private readonly AsyncEvent<Func<LogMessage, Task>> _Log
			= new AsyncEvent<Func<LogMessage, Task>>();

		private readonly IServiceProvider _Provider;

		private readonly AsyncEvent<Func<Task>> _Ready
			= new AsyncEvent<Func<Task>>();

		private int _ShardsReady;
		private bool IsReady => _ShardsReady == _Client.Shards.Count;

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

			_Client.ShardReady += OnReady;
			_Client.MessageReceived += HandleCommand;
		}

		public async Task AddCommandsAsync(IEnumerable<CommandAssembly> assemblies)
		{
			var currentCulture = CultureInfo.CurrentUICulture;
			var defaultTr = TypeReaderInfo.Create(Assembly.GetExecutingAssembly());
			foreach (var assembly in assemblies)
			{
				var typeReaders = TypeReaderInfo.Create(assembly.Assembly).Concat(defaultTr);
				foreach (var culture in assembly.Attribute.SupportedCultures)
				{
					CultureInfo.CurrentUICulture = culture;

					var commandService = _CommandService.Get();
					foreach (var tr in typeReaders)
					{
						foreach (var type in tr.Attribute.TargetTypes)
						{
							commandService.AddTypeReader(type, tr.Instance, true);
						}
					}

					var modules = await commandService.AddModulesAsync(assembly.Assembly, _Provider).CAF();
					int moduleCount = 0, commandCount = 0, helpEntryCount = 0;
					foreach (var module in modules)
					{
						++moduleCount;
						foreach (var command in module.Submodules)
						{
							++commandCount;
							if (!command.Attributes.Any(a => a is HiddenAttribute))
							{
								++helpEntryCount;
								_HelpEntries.Add(new ModuleHelpEntry(command));
							}
						}
					}

					ConsoleUtils.WriteLine($"Successfully loaded {moduleCount} modules " +
						$"containing {commandCount} commands " +
						$"({helpEntryCount} were given help entries) " +
						$"from {assembly.Assembly.GetName().Name} in the {culture} culture.");
				}
			}
			CultureInfo.CurrentUICulture = currentCulture;
		}

		private static bool CanBeIgnored(IAdvobotCommandContext c, IResult r)
		{
			return r == null
				|| r.Error == CommandError.UnknownCommand
				|| (!r.IsSuccess && (r.ErrorReason == null || c.Settings.NonVerboseErrors))
				|| (r is PreconditionGroupResult g && g.PreconditionResults.All(x => CanBeIgnored(c, x)));
		}

		private async Task HandleCommand(IMessage message)
		{
			var argPos = -1;
			if (!IsReady || _BotSettings.Pause || message.Author.IsBot
				|| string.IsNullOrWhiteSpace(message.Content)
				|| _BotSettings.UsersIgnoredFromCommands.Contains(message.Author.Id)
				|| !(message is IUserMessage msg)
				|| !(msg.Author is IGuildUser user)
				|| !(await _GuildSettings.GetOrCreateAsync(user.Guild).CAF() is IGuildSettings settings)
				|| settings.IgnoredCommandChannels.Contains(msg.Channel.Id)
				|| !(msg.HasStringPrefix(settings.GetPrefix(_BotSettings), ref argPos)
					|| msg.HasMentionPrefix(_Client.CurrentUser, ref argPos)))
			{
				return;
			}

			CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(settings.Culture);
			var context = new AdvobotCommandContext(settings, _Client, (SocketUserMessage)msg);
			var commands = _CommandService.Get();
			await commands.ExecuteAsync(context, argPos, _Provider).CAF();
		}

		private async Task OnCommandExecuted(
			Optional<CommandInfo> command,
			ICommandContext context,
			IResult result)
		{
			if (!(context is IAdvobotCommandContext ctx))
			{
				throw new ArgumentException(nameof(context));
			}
			if (!command.IsSpecified || CanBeIgnored(ctx, result))
			{
				return;
			}

			/* TODO: make this a toggle
			if (result.IsSuccess)
			{
				await context.Message.DeleteAsync(_Options).CAF();
			}*/

			await _CommandInvoked.InvokeAsync(command.Value, ctx, result).CAF();
		}

		private Task OnLog(LogMessage arg)
			=> _Log.InvokeAsync(arg);

		private async Task OnReady(DiscordSocketClient _)
		{
			if (++_ShardsReady < _Client.Shards.Count)
			{
				return;
			}

			_Client.ShardReady -= OnReady;

			await _Client.UpdateGameAsync(_BotSettings).CAF();
			await _Ready.InvokeAsync().CAF();
		}
	}
}