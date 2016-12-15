using System;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Advobot
{
	public class Program
	{
		//Convert sync main to an async main.
		public static void Main(string[] args) =>
			new Program().Start().GetAwaiter().GetResult();

		//public DiscordSocketClient Client { get { return client; } }

		public async Task Start()
		{
			//Define the DiscordSocketClient
			DiscordSocketClient client;
			client = new DiscordSocketClient(new DiscordSocketConfig { DownloadUsersOnGuildAvailable = true, MessageCacheSize = 100 });

			client.Log += Log;
			client.GuildAvailable += BotLogs.OnGuildAvailable;
			client.JoinedGuild += BotLogs.OnJoinedGuild;
			client.LeftGuild += BotLogs.OnLeftGuild;
			client.UserJoined += ServerLogs.OnUserJoined;
			client.UserLeft += ServerLogs.OnUserLeft;
			client.UserBanned += ServerLogs.OnUserBanned;
			client.UserUnbanned += ServerLogs.OnUserUnbanned;
			client.GuildMemberUpdated += ServerLogs.OnGuildMemberUpdated;
			client.MessageUpdated += ServerLogs.OnMessageUpdated;
			client.MessageDeleted += ServerLogs.OnMessageDeleted;

			//Login and connect to Discord.
			await client.LoginAsync(TokenType.Bot, "Key");
			try
			{
				await client.ConnectAsync();
			}
			catch (Exception)
			{
				Console.WriteLine("Client unable to connect. Shutting down in five seconds.");
				Thread.Sleep(5000);
				Environment.Exit(126);
			}

			var map = new DependencyMap();
			map.Add(client);

			CommandHandler handler;
			handler = new CommandHandler();
			await handler.Install(map);

			//Block this program until it is closed.
			await Task.Delay(-1);
		}

		private Task Log(LogMessage msg)
		{
			Console.WriteLine(msg.ToString());
			return Task.CompletedTask;
		}
	}
}
