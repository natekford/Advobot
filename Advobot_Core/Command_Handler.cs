using Advobot.Actions;
using Advobot.Interfaces;
using Advobot.NonSavedClasses;
using Advobot.SavedClasses;
using Advobot.Structs;
using Advobot.TypeReaders;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Advobot
{
	public class CommandHandler
	{
		private static IServiceProvider _Provider;
		private static CommandService _Commands;
		private static IBotSettings _BotSettings;
		private static IGuildSettingsModule _GuildSettings;
		private static IDiscordClient _Client;
		private static ITimersModule _Timers;
		private static ILogModule _Logging;

		public static async Task Install(IServiceProvider provider)
		{
			_Provider = provider;
			_Commands = (CommandService)provider.GetService(typeof(CommandService));
			_BotSettings = (IBotSettings)provider.GetService(typeof(IBotSettings));
			_GuildSettings = (IGuildSettingsModule)provider.GetService(typeof(IGuildSettingsModule));
			_Client = (IDiscordClient)provider.GetService(typeof(IDiscordClient));
			_Timers = (ITimersModule)provider.GetService(typeof(ITimersModule));
			_Logging = (ILogModule)provider.GetService(typeof(ILogModule));

			_Commands.CommandExecuted += CommandLogger;
			if (_Client is DiscordSocketClient)
			{
				var socketClient = _Client as DiscordSocketClient;
				socketClient.MessageReceived += (message) => HandleCommand(message as SocketUserMessage);
				socketClient.Connected += async () => await SavingAndLoadingActions.LoadInformation(_Client, _BotSettings, _GuildSettings);
			}
			else if (_Client is DiscordShardedClient)
			{
				var shardedClient = _Client as DiscordShardedClient;
				shardedClient.MessageReceived += (message) => HandleCommand(message as SocketUserMessage);
				shardedClient.Shards.FirstOrDefault().Connected += async () => await SavingAndLoadingActions.LoadInformation(_Client, _BotSettings, _GuildSettings);
			}
			else
			{
				throw new ArgumentException($"Invalid client supplied. Must be {nameof(DiscordSocketClient)} or {nameof(DiscordShardedClient)}.");
			}

			_Commands.AddTypeReader(typeof(IInvite), new InviteTypeReader());
			_Commands.AddTypeReader(typeof(IBan), new BanTypeReader());
			_Commands.AddTypeReader(typeof(Emote), new EmoteTypeReader());
			_Commands.AddTypeReader(typeof(Color), new ColorTypeReader());
			_Commands.AddTypeReader(typeof(CommandSwitch), new CommandSwitchTypeReader());
			await _Commands.AddModulesAsync(System.Reflection.Assembly.GetExecutingAssembly()); //Use executing assembly to get all of the commands from Advobot_Core. Entry and Calling assembly give Advobot_Launcher
		}

		public static async Task HandleCommand(SocketUserMessage message)
		{
			if (_BotSettings.Pause)
			{
				return;
			}

			var guild = message.Channel.GetGuild();
			if (guild == null)
			{
				return;
			}
			if (!_GuildSettings.TryGetSettings(guild, out IGuildSettings guildSettings))
			{
				await _GuildSettings.AddGuild(guild);
				guildSettings = _GuildSettings.GetSettings(guild);
			}
			if (!TryGetArgPos(message, guildSettings.Prefix, _BotSettings.Prefix, out int argPos))
			{
				return;
			}

			var context = new MyCommandContext(_BotSettings, guildSettings, _Logging, _Timers, _Client, message);
			_Logging.RanCommands.Add(new LoggedCommand(context));

			var result = await _Commands.ExecuteAsync(context, argPos, _Provider);
			if (!String.IsNullOrWhiteSpace(result.ErrorReason) && !Constants.IGNORE_ERROR.CaseInsEquals(result.ErrorReason))
			{
				_Logging.IncrementFailedCommands();
				//Ignore commands with the unknown command error because it's annoying
				switch (result.Error)
				{
					case CommandError.UnknownCommand:
					{
						return;
					}
					case CommandError.Exception:
					{
						ConsoleActions.WriteLine(result.ErrorReason, color: ConsoleColor.Red);
						goto default;
					}
					default:
					{
						await MessageActions.MakeAndDeleteSecondaryMessage(context, FormattingActions.ERROR(result.ErrorReason));
						break;
					}
				}
			}
		}

		public static async Task CommandLogger(CommandInfo commandInfo, ICommandContext context, IResult result)
		{
			if (!(context is IMyCommandContext))
			{
				throw new ArgumentException($"{context.GetType().Name} is not a valid context type. Must be {nameof(IMyCommandContext)}");
			}

			_Logging.IncrementSuccessfulCommands();
			await MessageActions.DeleteMessage(context.Message);

			var modLog = (context as IMyCommandContext)?.GuildSettings?.ModLog;
			if (modLog != null)
			{
				var embed = EmbedActions.MakeNewEmbed(null, context.Message.Content);
				EmbedActions.AddFooter(embed, "Mod Log");
				EmbedActions.AddAuthor(embed, context.User);
				await MessageActions.SendEmbedMessage(modLog, embed);
			}
		}

		private static bool TryGetArgPos(IUserMessage message, string guildPrefix, string globalPrefix, out int argPos)
		{
			argPos = -1;
			//Always allow mentioning as a prefix.
			var hasMentionPrefix = message.HasMentionPrefix(_Client.CurrentUser, ref argPos);
			//Only use the global prefix if the guild doesn't have a prefix set.
			var hasGlobalPrefix = String.IsNullOrWhiteSpace(guildPrefix) && message.HasStringPrefix(globalPrefix, ref argPos);
			//Don't use the global prefix if the guild has a prefix set.
			var hasGuildPrefix = !String.IsNullOrWhiteSpace(guildPrefix) && message.HasStringPrefix(guildPrefix, ref argPos);
			return hasMentionPrefix || hasGlobalPrefix || hasGuildPrefix;
		}
	}
}