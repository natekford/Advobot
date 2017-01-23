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
				LogLevel = LogSeverity.Warning,
				AudioMode = Discord.Audio.AudioMode.Disabled,
			});

			//Logging
			client.Log += BotLogs.Log;
			client.GuildAvailable += BotLogs.OnGuildAvailable;
			client.GuildUnavailable += BotLogs.OnGuildUnavailable;
			client.JoinedGuild += BotLogs.OnJoinedGuild;
			client.LeftGuild += BotLogs.OnLeftGuild;
			client.Disconnected += BotLogs.OnDisconnected;
			client.Connected += BotLogs.OnConnected;
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
			//TODO: Hope for the option to get updates when an invite gets created

			//Say what the current bot prefix is in the console
			Console.WriteLine("The current bot prefix is: " + Properties.Settings.Default.Prefix);

			//Check if the bot doesn't already have a key
			if (String.IsNullOrWhiteSpace(Properties.Settings.Default.BotKey))
			{
				Console.WriteLine("Hello. I'd like to thank you for using my bot; I hope it works well enough for you.\nPlease enter the bot's key:");
				Properties.Settings.Default.BotKey = Console.ReadLine().Trim();
			}

			//Login and connect to Discord.
			bool success = false;
			while (!success)
			{
				if (Properties.Settings.Default.BotKey.Length != 59)
				{
					//If the length isn't the normal length of a key make it retry
					Actions.writeLine("The given key has an unusual length. Please enter a regular length key:");
					Properties.Settings.Default.BotKey = Console.ReadLine().Trim();
				}
				else
				{
					try
					{
						//Try to login with the given key
						await client.LoginAsync(TokenType.Bot, Properties.Settings.Default.BotKey);
						//If the key works then save it within the settings
						Properties.Settings.Default.Save();
						success = true;
					}
					catch (Exception)
					{
						//If the key doesn't work then retry
						Actions.writeLine("The given key is invalid. Please enter a valid key:");
						Properties.Settings.Default.BotKey = Console.ReadLine().Trim();
					}
				}
			}
			try
			{
				await client.ConnectAsync();
			}
			catch (Exception)
			{
				Actions.writeLine("Client is unable to connect.");
				Thread.Sleep(15000);
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