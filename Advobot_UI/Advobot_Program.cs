using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Advobot
{
	public class Program
	{
		//Start the bot or start the UI then the bot
		[STAThread]
		private static void Main(string[] args)
		{
			//Make sure only one instance is running at the same time
			if (System.Diagnostics.Process.GetProcessesByName(System.IO.Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetEntryAssembly().Location)).Length > 1)
				return;

			//Check if Windows and if console
			Actions.LoadBasicInformation();

			//If the shard count is greater than one create a sharded client
			if (Properties.Settings.Default.ShardCount > 1)
			{
				Variables.Client = new ShardedClient(CreateShardedClient());
			}
			//If not create a regular socket client
			else
			{
				Variables.Client = new SocketClient(CreateSocketClient());
			}

			//If not a console application then start the UI
			if (!Variables.Console)
			{
				//Start the UI
				new System.Windows.Application().Run(new BotWindow());
			}
			else
			{
				//Set the path to save files
				var startup = true;
				while (!Actions.ValidatePath(startup ? Properties.Settings.Default.Path : System.Console.ReadLine(), startup))
				{
					startup = false;
				}
				//Set the bot's key
				startup = true;
				while (!Actions.ValidateBotKey(Variables.Client, startup ? Properties.Settings.Default.BotKey : System.Console.ReadLine(), startup).Result)
				{
					startup = false;
				}

				//Start the bot. This line needs to stay the same. Do not change the method to MaybeStartBot
				new Program().Start(Variables.Client).GetAwaiter().GetResult();
			}
		}

		//Try to have the bot connect and then add the dependency map
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
				Actions.ExceptionToConsole("Client is unable to connect.", e);
				return;
			}

			//Add in the dependency map
			var map = ConfigureServices(client);
			await new Command_Handler().Install(map);

			//Block this program until it is closed.
			await Task.Delay(-1);
		}

		private IServiceProvider ConfigureServices(BotClient client)
		{
			var services = new ServiceCollection().AddSingleton(client).AddSingleton(new CommandService(new CommandServiceConfig { CaseSensitiveCommands = false, ThrowOnError = false }));
			var provider = new DefaultServiceProviderFactory().CreateServiceProvider(services);
			return provider;
		}

		private static DiscordShardedClient CreateShardedClient()
		{
			var ShardedClient = new DiscordShardedClient(new DiscordSocketConfig
			{
				AlwaysDownloadUsers = Constants.ALWAYS_DOWNLOAD_USERS,
				MessageCacheSize = Constants.CACHED_MESSAGE_COUNT,
				LogLevel = Constants.LOG_LEVEL,
				TotalShards = Properties.Settings.Default.ShardCount,
			});

			ShardedClient.Log += Bot_Logs.Log;
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

			return ShardedClient;
		}

		private static DiscordSocketClient CreateSocketClient()
		{
			var SocketClient = new DiscordSocketClient(new DiscordSocketConfig
			{
				AlwaysDownloadUsers = Constants.ALWAYS_DOWNLOAD_USERS,
				MessageCacheSize = Constants.CACHED_MESSAGE_COUNT,
				LogLevel = Constants.LOG_LEVEL,
			});

			SocketClient.Log += Bot_Logs.Log;
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

			return SocketClient;
		}
	}
}