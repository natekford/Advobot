using Advobot.Actions;
using Advobot.Interfaces;
using Advobot.NonSavedClasses;
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

			SetUpCrucialEvents(_Client);

			_Commands.AddTypeReader(typeof(IInvite), new IInviteTypeReader());
			_Commands.AddTypeReader(typeof(IBan), new IBanTypeReader());
			_Commands.AddTypeReader(typeof(Emote), new IEmoteTypeReader());
			_Commands.AddTypeReader(typeof(Color), new ColorTypeReader());
			await _Commands.AddModulesAsync(System.Reflection.Assembly.GetExecutingAssembly()); //Use executing assembly to get all of the commands from Advobot_Core. Entry and Calling assembly give Advobot_Launcher
		}

		private static void SetUpCrucialEvents(IDiscordClient client)
		{
			if (client is DiscordSocketClient)
			{
				var socketClient = client as DiscordSocketClient;
				socketClient.MessageReceived += (message) => HandleCommand(message as SocketUserMessage);
				socketClient.Connected += async () =>
				{
					await SavingAndLoadingActions.LoadInformation(_Client, _BotSettings, _GuildSettings);
				};
			}
			else if (client is DiscordShardedClient)
			{
				var shardedClient = client as DiscordShardedClient;
				shardedClient.MessageReceived += (SocketMessage message) => HandleCommand(message as SocketUserMessage);
				shardedClient.Shards.FirstOrDefault().Connected += async () =>
				{
					await SavingAndLoadingActions.LoadInformation(_Client, _BotSettings, _GuildSettings);
				};
			}
			else
			{
				throw new ArgumentException("Invalid client supplied. Must be DiscordSocketClient or DiscordShardedClient.");
			}
		}

		public static async Task HandleCommand(SocketUserMessage message)
		{
			if (_BotSettings.Pause)
				return;

			var guild = (message?.Channel as SocketTextChannel)?.Guild;
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
			var result = await _Commands.ExecuteAsync(context, argPos, _Provider);

			if (result.IsSuccess)
			{
				await _Logging.Log.LogCommand(context);
				_Logging.IncrementSuccessfulCommands();
			}
			else if (!Constants.IGNORE_ERROR.CaseInsEquals(result.ErrorReason))
			{
				//Ignore commands with the unknown command error because it's annoying
				switch (result.Error)
				{
					case CommandError.UnknownCommand:
					{
						return;
					}
					case CommandError.Exception:
					{
						ConsoleActions.WriteLine(result.ErrorReason);
						break;
					}
					default:
					{
						await MessageActions.MakeAndDeleteSecondaryMessage(context, FormattingActions.ERROR(result.ErrorReason));
						break;
					}
				}

				_Logging.IncrementFailedCommands();
			}
		}

		private static bool TryGetArgPos(IUserMessage message, string guildPrefix, string globalPrefix, out int argPos)
		{
			argPos = -1;
			//Always allow mentioning as a prefix,
			//only use the global prefix if the guild doesn't have a prefix set,
			//don't use the global prefix if the guild has a prefix set.
			return false
				|| message.HasMentionPrefix(_Client.CurrentUser, ref argPos)
				|| String.IsNullOrWhiteSpace(guildPrefix)
				? message.HasStringPrefix(globalPrefix, ref argPos) 
				: message.HasStringPrefix(guildPrefix, ref argPos);
		}
	}
}