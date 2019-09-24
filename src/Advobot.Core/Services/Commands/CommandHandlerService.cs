using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Advobot.Attributes;
using Advobot.Classes;
using Advobot.CommandAssemblies;
using Advobot.Localization;
using Advobot.Modules;
using Advobot.Services.BotSettings;
using Advobot.Services.GuildSettings;
using Advobot.Services.HelpEntries;
using Advobot.Services.Timers;
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
		private static readonly RequestOptions _Options = DiscordUtils.GenerateRequestOptions("Command successfully executed.");
		private static readonly TimeSpan _RemovalTime = TimeSpan.FromSeconds(10);

		private readonly IBotSettings _BotSettings;
		private readonly DiscordShardedClient _Client;
		private readonly CommandServiceConfig _CommandConfig;
		private readonly Localized<CommandService> _CommandService;
		private readonly IGuildSettingsFactory _GuildSettings;
		private readonly IHelpEntryService _HelpEntries;
		private readonly IServiceProvider _Provider;
		private readonly ITimerService _Timers;
		private int _LoadedState;
		private ulong _OwnerId;
		private bool IsLoaded => _LoadedState != default;

		/// <inheritdoc />
		public event Action<IResult>? CommandInvoked;

		/// <summary>
		/// Creates an instance of <see cref="CommandHandlerService"/>.
		/// </summary>
		/// <param name="provider"></param>
		/// <param name="config"></param>
		/// <param name="client"></param>
		/// <param name="botSettings"></param>
		/// <param name="guildSettings"></param>
		/// <param name="help"></param>
		/// <param name="timers"></param>
		public CommandHandlerService(
			IServiceProvider provider,
			CommandServiceConfig config,
			DiscordShardedClient client,
			IBotSettings botSettings,
			IGuildSettingsFactory guildSettings,
			IHelpEntryService help,
			ITimerService timers)
		{
			_Provider = provider;
			_CommandConfig = config;
			_Client = client;
			_BotSettings = botSettings;
			_GuildSettings = guildSettings;
			_HelpEntries = help;
			_Timers = timers;
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
				if (assembly.Attribute.Instantiator != null)
				{
					await assembly.Attribute.Instantiator.ConfigureServicesAsync(_Provider).CAF();
				}

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

		private static string FormatResult(IAdvobotCommandContext context, IResult result)
		{
			var resp = $"\n\tGuild: {context.Guild.Format()}" +
				$"\n\tChannel: {context.Channel.Format()}" +
				$"\n\tUser: {context.User.Format()}" +
				$"\n\tTime: {context.Message.CreatedAt.UtcDateTime.ToReadable()} ({context.ElapsedMilliseconds}ms)" +
				$"\n\tText: {context.Message.Content}";
			if (!result.IsSuccess && result.ErrorReason != null)
			{
				resp += $"\n\tError: {result.ErrorReason}";
			}
			return resp;
		}

		private static async Task LogAsync(IAdvobotCommandContext context)
		{
			if (context.Settings.IgnoredLogChannels.Contains(context.Channel.Id))
			{
				return;
			}

			var modLog = await context.Guild.GetTextChannelAsync(context.Settings.ModLogId).CAF();
			if (modLog == null)
			{
				return;
			}

			await MessageUtils.SendMessageAsync(modLog, embedWrapper: new EmbedWrapper
			{
				Description = context.Message.Content,
				Author = context.User.CreateAuthor(),
				Footer = new EmbedFooterBuilder { Text = "Mod Log", },
			}).CAF();
		}

		private async Task HandleCommand(IMessage message)
		{
			var argPos = -1;
			if (!IsLoaded || _BotSettings.Pause || message.Author.IsBot || string.IsNullOrWhiteSpace(message.Content)
				//Disallow running commands if the user is blocked, unless the owner of the bot blocks themselves either accidentally or idiotically
				|| (_BotSettings.UsersIgnoredFromCommands.Contains(message.Author.Id) && message.Author.Id != _OwnerId)
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
			ICommandContext originalContext,
			IResult result)
		{
			if (!(originalContext is IAdvobotCommandContext context))
			{
				throw new ArgumentException(nameof(originalContext));
			}
			else if (CanBeIgnored(context, result))
			{
				return;
			}

			if (result.IsSuccess)
			{
				await context.Message.DeleteAsync(_Options).CAF();
				await LogAsync(context).CAF();
			}

			CommandInvoked?.Invoke(result);
			var color = result.IsSuccess ? ConsoleColor.Green : ConsoleColor.Red;
			ConsoleUtils.WriteLine(FormatResult(context, result), color);

			if (result is AdvobotResult a)
			{
				await a.SendAsync(context).CAF();
			}
			else if (!result.IsSuccess)
			{
				var message = await MessageUtils.SendMessageAsync(context.Channel, result.ErrorReason).CAF();
				var removable = new RemovableMessage(context, new[] { message }, _RemovalTime);
				_Timers.Add(removable);
			}
		}

		private Task OnLog(LogMessage message)
		{
			message.Write();
			return Task.CompletedTask;
		}

		private async Task OnReady(DiscordSocketClient _)
		{
			if (Interlocked.Exchange(ref _LoadedState, 1) != default)
			{
				return;
			}

			var application = await _Client.GetApplicationInfoAsync().CAF();
			_OwnerId = application.Owner.Id;
			await _Client.UpdateGameAsync(_BotSettings).CAF();
			ConsoleUtils.WriteLine($"Version: {Constants.BOT_VERSION}; " +
				$"Prefix: {_BotSettings.Prefix}; " +
				$"Launch Time: {ProcessInfoUtils.GetUptime().TotalMilliseconds:n}ms");
		}
	}
}