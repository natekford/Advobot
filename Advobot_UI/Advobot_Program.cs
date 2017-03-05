using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace Advobot
{
	public class Program
	{
		//Start the bot or start the UI then the bot
		[System.STAThread]
		private static void Main(string[] args)
		{
			//Check if Windows and if console
			Actions.LoadBasicInformation();

			//If the shard count is greater than one create a sharded client
			if (Properties.Settings.Default.ShardCount > 1)
			{
				Variables.Client = new ShardedClient(createShardedClient());
			}
			//If not create a regular socket client
			else
			{
				Variables.Client = new SocketClient(createSocketClient());
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
				while (!(Actions.ValidateBotKey(Variables.Client, startup ? Properties.Settings.Default.BotKey : System.Console.ReadLine(), startup)).Result)
				{
					startup = false;
				}

				//Start the bot. This line needs to stay the same. Do not change the method to MaybeStartBot
				new Program().Start(Variables.Client).GetAwaiter().GetResult();
			}
		}

		//Create a sharded client
		private static DiscordShardedClient createShardedClient()
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
			ShardedClient.Log += BotLogs.Log;
			ShardedClient.GuildAvailable += BotLogs.OnGuildAvailable;
			ShardedClient.GuildUnavailable += BotLogs.OnGuildUnavailable;
			ShardedClient.JoinedGuild += BotLogs.OnJoinedGuild;
			ShardedClient.LeftGuild += BotLogs.OnLeftGuild;
			//Serverlogs
			ShardedClient.UserJoined += ServerLogs.OnUserJoined;
			ShardedClient.UserLeft += ServerLogs.OnUserLeft;
			ShardedClient.UserUnbanned += ServerLogs.OnUserUnbanned;
			ShardedClient.UserBanned += ServerLogs.OnUserBanned;
			ShardedClient.GuildMemberUpdated += ServerLogs.OnGuildMemberUpdated;
			ShardedClient.UserUpdated += ServerLogs.OnUserUpdated;
			ShardedClient.MessageReceived += ServerLogs.OnMessageReceived;
			ShardedClient.MessageUpdated += ServerLogs.OnMessageUpdated;
			ShardedClient.MessageDeleted += ServerLogs.OnMessageDeleted;
			ShardedClient.RoleCreated += ServerLogs.OnRoleCreated;
			ShardedClient.RoleUpdated += ServerLogs.OnRoleUpdated;
			ShardedClient.RoleDeleted += ServerLogs.OnRoleDeleted;
			ShardedClient.ChannelCreated += ServerLogs.OnChannelCreated;
			ShardedClient.ChannelUpdated += ServerLogs.OnChannelUpdated;
			ShardedClient.ChannelDestroyed += ServerLogs.OnChannelDeleted;

			return ShardedClient;
		}

		//Create a regular client
		private static DiscordSocketClient createSocketClient()
		{
			//Define the DiscordSocketClient
			var SocketClient = new DiscordSocketClient(new DiscordSocketConfig
			{
				AlwaysDownloadUsers = Constants.ALWAYS_DOWNLOAD_USERS,
				MessageCacheSize = Constants.CACHED_MESSAGE_COUNT,
				LogLevel = Constants.LOG_LEVEL,
			});

			//Botlogs
			SocketClient.Log += BotLogs.Log;
			SocketClient.GuildAvailable += BotLogs.OnGuildAvailable;
			SocketClient.GuildUnavailable += BotLogs.OnGuildUnavailable;
			SocketClient.JoinedGuild += BotLogs.OnJoinedGuild;
			SocketClient.LeftGuild += BotLogs.OnLeftGuild;
			//Serverlogs
			SocketClient.UserJoined += ServerLogs.OnUserJoined;
			SocketClient.UserLeft += ServerLogs.OnUserLeft;
			SocketClient.UserUnbanned += ServerLogs.OnUserUnbanned;
			SocketClient.UserBanned += ServerLogs.OnUserBanned;
			SocketClient.GuildMemberUpdated += ServerLogs.OnGuildMemberUpdated;
			SocketClient.UserUpdated += ServerLogs.OnUserUpdated;
			SocketClient.MessageReceived += ServerLogs.OnMessageReceived;
			SocketClient.MessageUpdated += ServerLogs.OnMessageUpdated;
			SocketClient.MessageDeleted += ServerLogs.OnMessageDeleted;
			SocketClient.RoleCreated += ServerLogs.OnRoleCreated;
			SocketClient.RoleUpdated += ServerLogs.OnRoleUpdated;
			SocketClient.RoleDeleted += ServerLogs.OnRoleDeleted;
			SocketClient.ChannelCreated += ServerLogs.OnChannelCreated;
			SocketClient.ChannelUpdated += ServerLogs.OnChannelUpdated;
			SocketClient.ChannelDestroyed += ServerLogs.OnChannelDeleted;

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
			catch (System.Exception e)
			{
				Actions.ExceptionToConsole("Client is unable to connect.", e);
				return;
			}

			//Wait for all the guilds to be added
			try
			{
				await client.WaitForGuildsAsync();
			}
			catch (System.Exception e)
			{
				Actions.ExceptionToConsole("Client is unable to wait for all the guilds.", e);
				return;
			}

			//Add in the dependency map
			var map = new DependencyMap();
			map.Add(client);
			await new CommandHandler().Install(map);

			//Block this program until it is closed.
			await Task.Delay(-1);
		}
	}
}