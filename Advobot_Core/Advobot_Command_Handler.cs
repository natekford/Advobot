using Advobot.Actions;
using Advobot.Logging;
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
		private static IServiceProvider Provider;
		private static CommandService Commands;
		private static IBotSettings BotSettings;
		private static IGuildSettingsModule GuildSettings;
		private static IDiscordClient Client;
		private static ITimersModule Timers;
		private static ILogModule Logging;

		public static async Task Install(IServiceProvider provider)
		{
			Provider = provider;
			Commands = (CommandService)provider.GetService(typeof(CommandService));
			BotSettings = (IBotSettings)provider.GetService(typeof(IBotSettings));
			GuildSettings = (IGuildSettingsModule)provider.GetService(typeof(IGuildSettingsModule));
			Client = (IDiscordClient)provider.GetService(typeof(IDiscordClient));
			Timers = (ITimersModule)provider.GetService(typeof(ITimersModule));
			Logging = (ILogModule)provider.GetService(typeof(ILogModule));

			SetUpCrucialEvents(Client);

			Commands.AddTypeReader(typeof(IInvite), new IInviteTypeReader());
			Commands.AddTypeReader(typeof(IBan), new IBanTypeReader());
			Commands.AddTypeReader(typeof(Emote), new IEmoteTypeReader());
			Commands.AddTypeReader(typeof(Color), new ColorTypeReader());
			await Commands.AddModulesAsync(System.Reflection.Assembly.GetExecutingAssembly()); //Use executing assembly to get all of the commands from Advobot_Core. Entry and Calling assembly give Advobot_Launcher
		}

		private static void SetUpCrucialEvents(IDiscordClient client)
		{
			if (client is DiscordSocketClient)
			{
				var socketClient = client as DiscordSocketClient;
				socketClient.MessageReceived += (message) => HandleCommand(message as SocketUserMessage);
				socketClient.Connected += async () =>
				{
					await SavingAndLoading.LoadInformation(Client, BotSettings, GuildSettings);
				};
			}
			else if (client is DiscordShardedClient)
			{
				var shardedClient = client as DiscordShardedClient;
				shardedClient.MessageReceived += (SocketMessage message) => HandleCommand(message as SocketUserMessage);
				shardedClient.Shards.FirstOrDefault().Connected += async () =>
				{
					await SavingAndLoading.LoadInformation(Client, BotSettings, GuildSettings);
				};
			}
			else
			{
				throw new ArgumentException("Invalid client supplied. Must be DiscordSocketClient or DiscordShardedClient.");
			}
		}

		public static async Task HandleCommand(SocketUserMessage message)
		{
			if (BotSettings.Pause)
				return;

			var guild = (message?.Channel as SocketTextChannel)?.Guild;
			if (guild == null)
				return;

			if (!GuildSettings.TryGetSettings(guild, out IGuildSettings guildSettings))
			{
				await GuildSettings.AddGuild(guild);
				guildSettings = GuildSettings.GetSettings(guild);
			}
			if (!TryGetArgPos(message, guildSettings.Prefix, BotSettings.Prefix, out int argPos))
				return;

			var context = new MyCommandContext(BotSettings, guildSettings, Logging, Timers, Client, message);
			var result = await Commands.ExecuteAsync(context, argPos, Provider);

			if (result.IsSuccess)
			{
				await (Logging.Log as MyLog).LogCommand(context);

				Logging.IncrementSuccessfulCommands();
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
						await Messages.MakeAndDeleteSecondaryMessage(context, Formatting.ERROR(result.ErrorReason));
						break;
					}
				}

				Logging.IncrementFailedCommands();
			}
		}

		private static bool TryGetArgPos(IUserMessage message, string guildPrefix, string globalPrefix, out int argPos)
		{
			argPos = -1;
			if (String.IsNullOrWhiteSpace(guildPrefix))
			{
				if (message.HasStringPrefix(globalPrefix, ref argPos))
				{
					return true;
				}
			}
			else
			{
				if (message.HasStringPrefix(guildPrefix, ref argPos) || message.HasMentionPrefix(Client.CurrentUser, ref argPos))
				{
					return true;
				}
			}
			return false;
		}
	}
}