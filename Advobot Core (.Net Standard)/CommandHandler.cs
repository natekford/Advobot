using Advobot.Actions;
using Advobot.Classes;
using Advobot.Interfaces;
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
			//Member variables
			_Provider = provider;
			_Commands = provider.GetService<CommandService>();
			_BotSettings = provider.GetService<IBotSettings>();
			_GuildSettings = provider.GetService<IGuildSettingsModule>();
			_Client = provider.GetService<IDiscordClient>();
			_Timers = provider.GetService<ITimersModule>();
			_Logging = provider.GetService<ILogModule>();

			//Type readers
			_Commands.AddTypeReader(typeof(IInvite), new InviteTypeReader());
			_Commands.AddTypeReader(typeof(IBan), new BanTypeReader());
			_Commands.AddTypeReader(typeof(Emote), new EmoteTypeReader());
			_Commands.AddTypeReader(typeof(Color), new ColorTypeReader());
			_Commands.AddTypeReader(typeof(CommandSwitch), new CommandSwitchTypeReader());

			//Commands
			await _Commands.AddModulesAsync(System.Reflection.Assembly.GetExecutingAssembly()); //Use executing assembly to get all of the commands from the core. Entry and Calling assembly give the launcher

			//Events
			_Commands.CommandExecuted += (commandInfo, context, result) => CommandLogger(commandInfo, context as IMyCommandContext, result);
			if (_Client is DiscordSocketClient)
			{
				var socketClient = _Client as DiscordSocketClient;
				socketClient.MessageReceived += (message) => HandleCommand(message as SocketUserMessage);
				socketClient.Connected += async () => await SavingAndLoadingActions.LoadInformation(_Client, _BotSettings);
			}
			else if (_Client is DiscordShardedClient)
			{
				var shardedClient = _Client as DiscordShardedClient;
				shardedClient.MessageReceived += (message) => HandleCommand(message as SocketUserMessage);
				shardedClient.Shards.FirstOrDefault().Connected += async () => await SavingAndLoadingActions.LoadInformation(_Client, _BotSettings);
			}
			else
			{
				throw new ArgumentException($"Invalid client supplied. Must be {nameof(DiscordSocketClient)} or {nameof(DiscordShardedClient)}.");
			}
		}

		private static async Task HandleCommand(SocketUserMessage message)
		{
			var startTime = DateTime.UtcNow;

			if (_BotSettings.Pause)
			{
				return;
			}

			var guild = message.Channel.GetGuild();
			if (guild == null)
			{
				return;
			}
			if (!_GuildSettings.TryGetSettings(guild.Id, out IGuildSettings guildSettings))
			{
				await _GuildSettings.AddGuild(guild);
				guildSettings = _GuildSettings.GetSettings(guild.Id);
			}
			if (!TryGetArgPos(message, guildSettings.Prefix, _BotSettings.Prefix, out int argPos))
			{
				return;
			}

			var context = new MyCommandContext(_BotSettings, guildSettings, _Logging, _Timers, _Client, message);
			var result = await _Commands.ExecuteAsync(context, argPos, _Provider);

			var loggedCommand = new LoggedCommand(context, startTime);
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
						loggedCommand.Errored(result.ErrorReason);
						goto default;
					}
					default:
					{
						await MessageActions.MakeAndDeleteSecondaryMessage(context, FormattingActions.ERROR(result.ErrorReason));
						break;
					}
				}
			}

			loggedCommand.Finished();
			_Logging.RanCommands.Add(loggedCommand);
		}
		private static async Task CommandLogger(CommandInfo commandInfo, IMyCommandContext context, IResult result)
		{
			if (context == null)
			{
				throw new ArgumentException("Invalid context provided to the command logger.");
			}

			_Logging.IncrementSuccessfulCommands();
			await MessageActions.DeleteMessage(context.Message);

			var modLog = context.GuildSettings?.ModLog;
			if (modLog != null && !context.GuildSettings.IgnoredLogChannels.Contains(context.Channel.Id))
			{
				var embed = EmbedActions.MakeNewEmbed(null, context.Message.Content)
					.MyAddAuthor(context.User)
					.MyAddFooter("Mod Log");
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