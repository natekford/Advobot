using Discord;
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
			//Create the client
			Variables.Client = createClient();

			if (!Variables.Console)
			{
				//Start the UI
				new System.Windows.Application().Run(new BotWindow());
			}
			else
			{
				//Set the path to save stuff to
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

		//Create the client
		private static DiscordShardedClient createClient()
		{
			//Define the DiscordSocketClient
			DiscordShardedClient client = new DiscordShardedClient(new DiscordSocketConfig
			{
				AlwaysDownloadUsers = true,
				MessageCacheSize = 10000,
				LogLevel = LogSeverity.Warning,
				TotalShards = Properties.Settings.Default.ShardCount,
			});

			//Botlogs
			client.Log += BotLogs.Log;
			client.GuildAvailable += BotLogs.OnGuildAvailable;
			client.GuildUnavailable += BotLogs.OnGuildUnavailable;
			client.JoinedGuild += BotLogs.OnJoinedGuild;
			client.LeftGuild += BotLogs.OnLeftGuild;
			//Serverlogs
			client.UserJoined += ServerLogs.OnUserJoined;
			client.UserLeft += ServerLogs.OnUserLeft;
			client.UserUnbanned += ServerLogs.OnUserUnbanned;
			client.UserBanned += ServerLogs.OnUserBanned;
			client.GuildMemberUpdated += ServerLogs.OnGuildMemberUpdated;
			client.UserUpdated += ServerLogs.OnUserUpdated;
			client.MessageReceived += ServerLogs.OnMessageReceived;
			client.MessageUpdated += ServerLogs.OnMessageUpdated;
			client.MessageDeleted += ServerLogs.OnMessageDeleted;
			client.RoleCreated += ServerLogs.OnRoleCreated;
			client.RoleUpdated += ServerLogs.OnRoleUpdated;
			client.RoleDeleted += ServerLogs.OnRoleDeleted;
			client.ChannelCreated += ServerLogs.OnChannelCreated;
			client.ChannelUpdated += ServerLogs.OnChannelUpdated;
			client.ChannelDestroyed += ServerLogs.OnChannelDeleted;

			return client;
		}

		//Try to have the bot connect and then add the dependency map
		public async Task Start(DiscordShardedClient client)
		{
			Actions.WriteLine("Connecting the client...");
			//Connect the bot
			try
			{
				await client.ConnectAsync();
			}
			catch (System.Exception)
			{
				Actions.WriteLine("Client is unable to connect.");
				return;
			}

			var map = new DependencyMap();
			map.Add(client);

			await new CommandHandler().Install(map);

			//Block this program until it is closed.
			await Task.Delay(-1);
		}
	}
}