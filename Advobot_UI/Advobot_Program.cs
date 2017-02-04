﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace Advobot
{
	public class Program
	{
		[System.STAThread]
		public static void Main(string[] args)
		{
			//Get the OS and if console
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
				while (!Actions.validatePathText(startup ? Properties.Settings.Default.Path : System.Console.ReadLine(), startup))
				{
					startup = false;
				}
				//Set the bot's key
				startup = true;
				while (!(Actions.validateBotKey(Variables.Client, startup ? Properties.Settings.Default.BotKey : System.Console.ReadLine(), startup)).Result)
				{
					startup = false;
				}

				//Start the bot
				new Program().Start(Variables.Client).GetAwaiter().GetResult();
			}
		}

		private static DiscordSocketClient createClient()
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

			return client;
		}

		public async Task Start(DiscordSocketClient client)
		{
			//Connect the bot
			try
			{
				await client.ConnectAsync();
			}
			catch (System.Exception)
			{
				Actions.writeLine("Client is unable to connect.");
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