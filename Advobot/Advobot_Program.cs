using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

/* First, to get the really shitty part of the bot out of the way:
 * I am too lazy to type out .ConfigureAwait(false) on every await I do and I don't really know what it does so I don't use it.
 * A lot of the things that go into DontWaitForResultOfUnimportantBigFunction make the bot hang, so that's why they use async void. I don't know the correct way to not make them hang.
 * I wasn't aware of the arg parsing of Discord.Net when I first used it, so that's why I have my custom arg parsing.
 * My arg parsing is definitely more inefficient, but since I'm the one writing it I can provide more specific error messages.
 */
namespace Advobot
{
	public class Program
	{
		[STAThread]
		private static void Main()
		{
			//Make sure only one instance is running at the same time
#if RELEASE
			if (System.Diagnostics.Process.GetProcessesByName(System.IO.Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetEntryAssembly().Location)).Length > 1)
				return;
#endif

			//Things that when not loaded fuck the bot completely
			Actions.LoadCriticalInformation();

			var botInfo = Variables.BotInfo;
			if (((int)botInfo.GetSetting(SettingOnBot.ShardCount)) > 1)
			{
				Variables.Client = new ShardedClient(CreateShardedClient(botInfo));
			}
			else
			{
				Variables.Client = new SocketClient(CreateSocketClient(botInfo));
			}

			//If not a console application then start the UI
			if (!Variables.Console)
			{
				new System.Windows.Application().Run(new BotWindow());
			}
			else
			{
				var startup = true;
				while (!Variables.GotPath)
				{
					Actions.ValidatePath(startup ? Properties.Settings.Default.Path : Console.ReadLine(), startup);
					startup = false;
				}
				startup = true;
				while (!Variables.GotKey)
				{
					Actions.ValidateBotKey(Variables.Client, startup ? Properties.Settings.Default.BotKey : Console.ReadLine(), startup).GetAwaiter().GetResult();
					startup = false;
				}

				Actions.MaybeStartBot();
			}
		}

		public async Task Start(BotClient client)
		{
			//Notify the user the bot has started connecting
			Actions.WriteLine("Connecting the client...");

			//Connect the bot
			try
			{
				await client.StartAsync();
			}
			catch (Exception e)
			{
				Actions.ExceptionToConsole(e);
				return;
			}

			//Add in the dependency map
			await new Command_Handler().Install(ConfigureServices(client));

			//Block this program until it is closed.
			await Task.Delay(-1);
		}

		private IServiceProvider ConfigureServices(BotClient client)
		{
			return new DefaultServiceProviderFactory().CreateServiceProvider(
				new ServiceCollection()
				.AddSingleton(client)
				.AddSingleton(new CommandService(new CommandServiceConfig
				{
					CaseSensitiveCommands = false,
					ThrowOnError = false,
				})));
		}

		private static DiscordShardedClient CreateShardedClient(BotGlobalInfo botInfo)
		{
			var ShardedClient = new DiscordShardedClient(new DiscordSocketConfig
			{
				AlwaysDownloadUsers = ((bool)botInfo.GetSetting(SettingOnBot.AlwaysDownloadUsers)),
				MessageCacheSize = ((int)botInfo.GetSetting(SettingOnBot.MessageCacheCount)),
				LogLevel = ((Discord.LogSeverity)botInfo.GetSetting(SettingOnBot.LogLevel)),
				TotalShards = ((int)botInfo.GetSetting(SettingOnBot.ShardCount)),
			});

			ShardedClient.Log += Bot_Logs.Log;
			ShardedClient.MessageReceived += Command_Handler.HandleCommand;
			ShardedClient.GuildAvailable += Bot_Logs.OnGuildAvailable;
			ShardedClient.GuildUnavailable += Bot_Logs.OnGuildUnavailable;
			ShardedClient.JoinedGuild += Bot_Logs.OnJoinedGuild;
			ShardedClient.LeftGuild += Bot_Logs.OnLeftGuild;
			ShardedClient.UserJoined += Server_Logs.OnUserJoined;
			ShardedClient.UserLeft += Server_Logs.OnUserLeft;
			ShardedClient.UserUpdated += Server_Logs.OnUserUpdated;
			ShardedClient.MessageReceived += Server_Logs.OnMessageReceived;
			ShardedClient.MessageUpdated += Server_Logs.OnMessageUpdated;
			ShardedClient.MessageDeleted += Server_Logs.OnMessageDeleted;
			ShardedClient.Shards.FirstOrDefault().Connected += Actions.LoadInformation;

			return ShardedClient;
		}

		private static DiscordSocketClient CreateSocketClient(BotGlobalInfo botInfo)
		{
			var SocketClient = new DiscordSocketClient(new DiscordSocketConfig
			{
				AlwaysDownloadUsers = ((bool)botInfo.GetSetting(SettingOnBot.AlwaysDownloadUsers)),
				MessageCacheSize = ((int)botInfo.GetSetting(SettingOnBot.MaxUserGatherCount)),
				LogLevel = ((Discord.LogSeverity)Variables.BotInfo.GetSetting(SettingOnBot.LogLevel)),
			});

			SocketClient.Log += Bot_Logs.Log;
			SocketClient.MessageReceived += Command_Handler.HandleCommand;
			SocketClient.GuildAvailable += Bot_Logs.OnGuildAvailable;
			SocketClient.GuildUnavailable += Bot_Logs.OnGuildUnavailable;
			SocketClient.JoinedGuild += Bot_Logs.OnJoinedGuild;
			SocketClient.LeftGuild += Bot_Logs.OnLeftGuild;
			SocketClient.UserJoined += Server_Logs.OnUserJoined;
			SocketClient.UserLeft += Server_Logs.OnUserLeft;
			SocketClient.UserUpdated += Server_Logs.OnUserUpdated;
			SocketClient.MessageReceived += Server_Logs.OnMessageReceived;
			SocketClient.MessageUpdated += Server_Logs.OnMessageUpdated;
			SocketClient.MessageDeleted += Server_Logs.OnMessageDeleted;
			SocketClient.Connected += Actions.LoadInformation;

			return SocketClient;
		}
	}
}