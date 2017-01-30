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
				AlwaysDownloadUsers = true,
				MessageCacheSize = 10000,
				LogLevel = LogSeverity.Warning,
				AudioMode = Discord.Audio.AudioMode.Disabled,
			});

			//Botlogs
			client.Log += BotLogs.Log;
			client.GuildAvailable += BotLogs.OnGuildAvailable;
			client.GuildUnavailable += BotLogs.OnGuildUnavailable;
			client.JoinedGuild += BotLogs.OnJoinedGuild;
			client.LeftGuild += BotLogs.OnLeftGuild;
			client.Disconnected += BotLogs.OnDisconnected;
			client.Connected += BotLogs.OnConnected;
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

			//Make sure the bot's key and save path are gotten
			await Actions.start(client);

			var map = new DependencyMap();
			map.Add(client);

			await new CommandHandler().Install(map);

			//Block this program until it is closed.
			await Task.Delay(-1);
		}
	}
}