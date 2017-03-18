using Discord.Commands;
using Discord.WebSocket;
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

		//Create a sharded client
		private static DiscordShardedClient CreateShardedClient()
		{
			//Define the DiscordSocketClient
			var ShardedClient = new DiscordShardedClient(new DiscordSocketConfig
			{
				AlwaysDownloadUsers = Constants.ALWAYS_DOWNLOAD_USERS,
				MessageCacheSize = Constants.CACHED_MESSAGE_COUNT,
				LogLevel = Constants.LOG_LEVEL,
				TotalShards = Properties.Settings.Default.ShardCount,
			});

			//Botlogs
			ShardedClient.Log += Bot_Logs.Log;
			ShardedClient.GuildAvailable += Bot_Logs.OnGuildAvailable;
			ShardedClient.GuildUnavailable += Bot_Logs.OnGuildUnavailable;
			ShardedClient.JoinedGuild += Bot_Logs.OnJoinedGuild;
			ShardedClient.LeftGuild += Bot_Logs.OnLeftGuild;
			//Serverlogs
			ShardedClient.UserJoined += Server_Logs.OnUserJoined;
			ShardedClient.UserLeft += Server_Logs.OnUserLeft;
			ShardedClient.UserUnbanned += Server_Logs.OnUserUnbanned;
			ShardedClient.UserBanned += Server_Logs.OnUserBanned;
			ShardedClient.GuildMemberUpdated += Server_Logs.OnGuildMemberUpdated;
			ShardedClient.UserUpdated += Server_Logs.OnUserUpdated;
			ShardedClient.MessageReceived += Server_Logs.OnMessageReceived;
			ShardedClient.MessageUpdated += Server_Logs.OnMessageUpdated;
			ShardedClient.MessageDeleted += Server_Logs.OnMessageDeleted;
			ShardedClient.RoleCreated += Server_Logs.OnRoleCreated;
			ShardedClient.RoleUpdated += Server_Logs.OnRoleUpdated;
			ShardedClient.RoleDeleted += Server_Logs.OnRoleDeleted;
			ShardedClient.ChannelCreated += Server_Logs.OnChannelCreated;
			ShardedClient.ChannelUpdated += Server_Logs.OnChannelUpdated;
			ShardedClient.ChannelDestroyed += Server_Logs.OnChannelDeleted;

			return ShardedClient;
		}

		//Create a regular client
		private static DiscordSocketClient CreateSocketClient()
		{
			//Define the DiscordSocketClient
			var SocketClient = new DiscordSocketClient(new DiscordSocketConfig
			{
				AlwaysDownloadUsers = Constants.ALWAYS_DOWNLOAD_USERS,
				MessageCacheSize = Constants.CACHED_MESSAGE_COUNT,
				LogLevel = Constants.LOG_LEVEL,
			});

			//Botlogs
			SocketClient.Log += Bot_Logs.Log;
			SocketClient.GuildAvailable += Bot_Logs.OnGuildAvailable;
			SocketClient.GuildUnavailable += Bot_Logs.OnGuildUnavailable;
			SocketClient.JoinedGuild += Bot_Logs.OnJoinedGuild;
			SocketClient.LeftGuild += Bot_Logs.OnLeftGuild;
			//Serverlogs
			SocketClient.UserJoined += Server_Logs.OnUserJoined;
			SocketClient.UserLeft += Server_Logs.OnUserLeft;
			SocketClient.UserUnbanned += Server_Logs.OnUserUnbanned;
			SocketClient.UserBanned += Server_Logs.OnUserBanned;
			SocketClient.GuildMemberUpdated += Server_Logs.OnGuildMemberUpdated;
			SocketClient.UserUpdated += Server_Logs.OnUserUpdated;
			SocketClient.MessageReceived += Server_Logs.OnMessageReceived;
			SocketClient.MessageUpdated += Server_Logs.OnMessageUpdated;
			SocketClient.MessageDeleted += Server_Logs.OnMessageDeleted;
			SocketClient.RoleCreated += Server_Logs.OnRoleCreated;
			SocketClient.RoleUpdated += Server_Logs.OnRoleUpdated;
			SocketClient.RoleDeleted += Server_Logs.OnRoleDeleted;
			SocketClient.ChannelCreated += Server_Logs.OnChannelCreated;
			SocketClient.ChannelUpdated += Server_Logs.OnChannelUpdated;
			SocketClient.ChannelDestroyed += Server_Logs.OnChannelDeleted;

			return SocketClient;
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
			var map = new DependencyMap();
			map.Add(client);
			await new Command_Handler().Install(map);

			//Block this program until it is closed.
			await Task.Delay(-1);
		}
	}
}