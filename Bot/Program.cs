using System;
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
			client = new DiscordSocketClient(new DiscordSocketConfig());

			client.Log += Log;
			client.GuildAvailable += BotLogs.OnGuildAvailable;
			client.JoinedGuild += BotLogs.OnJoinedGuild;
			client.LeftGuild += BotLogs.OnLeftGuild;
			client.UserJoined += ServerLogs.OnUserJoined;
			client.UserLeft += ServerLogs.OnUserLeft;
			client.GuildMemberUpdated += ServerLogs.OnGuildMemberUpdated;
			client.UserBanned += ServerLogs.OnUserBanned;
			client.UserUnbanned += ServerLogs.OnUserUnbanned;
			client.MessageUpdated += ServerLogs.OnMessageUpdated;
			client.MessageDeleted += ServerLogs.OnMessageDeleted;

			//Login and connect to Discord.
			await client.LoginAsync(TokenType.Bot, "Key");
			await client.ConnectAsync();

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
