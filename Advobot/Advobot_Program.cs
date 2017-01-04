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

		public async Task Start()
		{
			//Define the DiscordSocketClient
			DiscordSocketClient client = new DiscordSocketClient(new DiscordSocketConfig
			{
				DownloadUsersOnGuildAvailable = true,
				MessageCacheSize = 10000,
				LogLevel = LogSeverity.Debug,
				AudioMode = Discord.Audio.AudioMode.Disabled,
			});

			//Logging
			client.Log += BotLogs.Log;
			client.GuildAvailable += BotLogs.OnGuildAvailable;
			client.JoinedGuild += BotLogs.OnJoinedGuild;
			client.LeftGuild += BotLogs.OnLeftGuild;
			client.Disconnected += BotLogs.OnDisconnected;
			client.UserJoined += ServerLogs.OnUserJoined;
			client.UserLeft += ServerLogs.OnUserLeft;
			client.UserBanned += ServerLogs.OnUserBanned;
			client.UserUnbanned += ServerLogs.OnUserUnbanned;
			client.GuildMemberUpdated += ServerLogs.OnGuildMemberUpdated;
			client.UserUpdated += ServerLogs.OnUserUpdated;
			client.MessageUpdated += ServerLogs.OnMessageUpdated;
			client.MessageDeleted += ServerLogs.OnMessageDeleted;
			client.MessageReceived += ServerLogs.OnMessageReceived;
			client.ChannelCreated += ServerLogs.OnChannelCreated;
			client.ChannelUpdated += ServerLogs.OnChannelUpdated;

			//Login and connect to Discord.
			await client.LoginAsync(TokenType.Bot, "Bot Key");
			try
			{
				await client.ConnectAsync();
			}
			catch (Exception)
			{
				Actions.writeLine("!!!Client unable to connect. Shutting down in five seconds!!!");
				Thread.Sleep(5000);
				Environment.Exit(126);
			}

			var map = new DependencyMap();
			map.Add(client);

			await new CommandHandler().Install(map);

			//Block this program until it is closed.
			await Task.Delay(-1);
		}
	}
}
