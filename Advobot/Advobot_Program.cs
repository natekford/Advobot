﻿using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

//I am too lazy to type out .ConfigureAwait(false) on every await I do so I don't use it.
//Also, a lot of the things that go into DontWaitForResultOfUnimportantBigFunction make the bot hang, so that's why they use async void
//If I wasn't the one writing this, I probably wouldn't be using it. Be warned. 
namespace Advobot
{
	public class Program
	{
		[STAThread]
		private static void Main(string[] args)
		{
			//Make sure only one instance is running at the same time
#if RELEASE
			if (System.Diagnostics.Process.GetProcessesByName(System.IO.Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetEntryAssembly().Location)).Length > 1)
				return;
#endif

			//Things that when not loaded fuck the bot completely
			Actions.LoadCriticalInformation();

			var botInfo = Variables.BotInfo;
			if (((int)botInfo.GetSetting(SettingOnBot.ShardCount)) > 1)
			{
				Variables.Client = new ShardedClient(CreateShardedClient(botInfo));
			}
			else
			{
				Variables.Client = new SocketClient(CreateSocketClient(botInfo));
			}

			//If not a console application then start the UI
			if (!Variables.Console)
			{
				new System.Windows.Application().Run(new BotWindow());
			}
			else
			{
				//Set the path to save files
				var startup = true;
				while (!Actions.ValidatePath(startup ? Properties.Settings.Default.Path : Console.ReadLine(), startup))
				{
					startup = false;
				}
				//Set the bot's key
				startup = true;
				while (!Actions.ValidateBotKey(Variables.Client, startup ? Properties.Settings.Default.BotKey : Console.ReadLine(), startup).Result)
				{
					startup = false;
				}

				//Start the bot. This line needs to stay the same. Do not change the method to MaybeStartBot
				new Program().Start(Variables.Client).GetAwaiter().GetResult();
			}
		}

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
				Actions.ExceptionToConsole(e);
				return;
			}

			//Add in the dependency map
			await new Command_Handler().Install(ConfigureServices(client));

			//Block this program until it is closed.
			await Task.Delay(-1);
		}

		private IServiceProvider ConfigureServices(BotClient client)
		{
			return new DefaultServiceProviderFactory().CreateServiceProvider(
				new ServiceCollection()
				.AddSingleton(client)
				.AddSingleton(new CommandService(new CommandServiceConfig
				{
					CaseSensitiveCommands = false,
					ThrowOnError = false,
				})));
		}

		private static DiscordShardedClient CreateShardedClient(BotGlobalInfo botInfo)
		{
			var ShardedClient = new DiscordShardedClient(new DiscordSocketConfig
			{
				AlwaysDownloadUsers = ((bool)botInfo.GetSetting(SettingOnBot.AlwaysDownloadUsers)),
				MessageCacheSize = ((int)botInfo.GetSetting(SettingOnBot.MessageCacheCount)),
				LogLevel = ((Discord.LogSeverity)botInfo.GetSetting(SettingOnBot.LogLevel)),
				TotalShards = ((int)botInfo.GetSetting(SettingOnBot.ShardCount)),
			});

			ShardedClient.Log += Bot_Logs.Log;
			ShardedClient.MessageReceived += Command_Handler.HandleCommand;
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
			ShardedClient.Shards.FirstOrDefault().Connected += Actions.LoadInformation;

			return ShardedClient;
		}

		private static DiscordSocketClient CreateSocketClient(BotGlobalInfo botInfo)
		{
			var SocketClient = new DiscordSocketClient(new DiscordSocketConfig
			{
				AlwaysDownloadUsers = ((bool)botInfo.GetSetting(SettingOnBot.AlwaysDownloadUsers)),
				MessageCacheSize = ((int)botInfo.GetSetting(SettingOnBot.MaxUserGatherCount)),
				LogLevel = ((Discord.LogSeverity)Variables.BotInfo.GetSetting(SettingOnBot.LogLevel)),
			});

			SocketClient.Log += Bot_Logs.Log;
			SocketClient.MessageReceived += Command_Handler.HandleCommand;
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
			SocketClient.Connected += Actions.LoadInformation;

			return SocketClient;
		}
	}
}