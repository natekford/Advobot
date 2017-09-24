using Advobot.Actions;
using Advobot.Actions.Formatting;
using Advobot.Classes;
using Advobot.Classes.TypeReaders;
using Advobot.Interfaces;
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
			if (_Client is DiscordSocketClient socketClient)
			{
				socketClient.MessageReceived += (message) => HandleCommand(message as SocketUserMessage);
				socketClient.Connected += async () => await SavingAndLoadingActions.DoStartupActions(_Client, _BotSettings);
			}
			else if (_Client is DiscordShardedClient shardedClient)
			{
				shardedClient.MessageReceived += (message) => HandleCommand(message as SocketUserMessage);
				shardedClient.Shards.FirstOrDefault().Connected += async () => await SavingAndLoadingActions.DoStartupActions(_Client, _BotSettings);
			}
			else
			{
				throw new ArgumentException($"Invalid client supplied. Must be {nameof(DiscordSocketClient)} or {nameof(DiscordShardedClient)}.");
			}
		}

		private static async Task HandleCommand(SocketUserMessage message)
		{
			var loggedCommand = new LoggedCommand();
			if (_BotSettings.Pause)
			{
				return;
			}

			var guildSettings = await _GuildSettings.GetOrCreateSettings(message.Channel.GetGuild());
			if (guildSettings == null || !TryGetArgPos(message, GetActions.GetPrefix(_BotSettings, guildSettings), out int argPos))
			{
				return;
			}

			var context = new MyCommandContext(_Provider, _Client, guildSettings, message);
			var result = await _Commands.ExecuteAsync(context, argPos, _Provider);
			await CommandLogger(loggedCommand, context, result);
		}
		//TODO: put this back into the CommandService.CommandExecute event once it stops firing twice.
		private static async Task CommandLogger(LoggedCommand loggedCommand, IMyCommandContext context, IResult result)
		{
			//Success
			if (result.IsSuccess)
			{
				_Logging.IncrementSuccessfulCommands();
				await MessageActions.DeleteMessage(context.Message);

				var guildSettings = context.GuildSettings;
				if (guildSettings.ModLog == null && !guildSettings.IgnoredLogChannels.Contains(context.Channel.Id))
				{
					return;
				}

				var embed = EmbedActions.MakeNewEmbed(null, context.Message.Content)
					.MyAddAuthor(context.User)
					.MyAddFooter("Mod Log");
				await MessageActions.SendEmbedMessage(guildSettings.ModLog, embed);
			}
			//Failure in a valid fail way
			else if (GetActions.TryGetErrorReason(result, out string errorReason))
			{
				_Logging.IncrementFailedCommands();
				await MessageActions.MakeAndDeleteSecondaryMessage(context, GeneralFormatting.ERROR(errorReason));
			}
			//Failure in a way that doesn't need to get logged (unknown command, etc)
			else
			{
				return;
			}

			loggedCommand.FinalizeAndWrite(context, result, _Logging);
		}
		private static bool TryGetArgPos(IUserMessage message, string prefix, out int argPos)
		{
			argPos = -1;
			return message.HasMentionPrefix(_Client.CurrentUser, ref argPos) || message.HasStringPrefix(prefix, ref argPos);
		}
	}
}