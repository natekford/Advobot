using Advobot.Interfaces;
using Advobot.Modules.Log;
using Advobot.Modules.Timers;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Advobot.Actions
{
	public static class ClientActions
	{
		/// <summary>
		/// Tries to start the bot by making sure a save path and bot key are provided and the bot is not already running.
		/// <para/>If the program is a console application it will not do anything until a valid save path and bot key are provided.
		/// <para/>If the program is a non console application it will only check that those are set, otherwise they will have to be set in a different method.
		/// </summary>
		/// <param name="client">The client to start.</param>
		/// <param name="botSettings">The settings to use in the client.</param>
		/// <returns></returns>
		public static async Task MaybeStartBot(IDiscordClient client, IBotSettings botSettings)
		{
			if (botSettings.Console)
			{
				var startup = true;
				while (!botSettings.GotPath)
				{
					var input = startup ? Properties.Settings.Default.Path : Console.ReadLine();
					if (SavingAndLoadingActions.ValidatePath(input, botSettings.Windows, startup))
					{
						botSettings.SetGotPath();
					}
					startup = false;
				}
				startup = true;
				while (!botSettings.GotKey)
				{
					var input = startup ? Properties.Settings.Default.BotKey : Console.ReadLine();
					if (await ValidateBotKey(client, input, startup))
					{
						botSettings.SetGotKey();
					}
					startup = false;
				}
			}
			else
			{
				if (SavingAndLoadingActions.ValidatePath(Properties.Settings.Default.Path, botSettings.Windows, true))
				{
					botSettings.SetGotPath();
				}
				if (await ValidateBotKey(client, Properties.Settings.Default.BotKey, true))
				{
					botSettings.SetGotKey();
				}
			}

			if (botSettings.GotPath && botSettings.GotKey && !botSettings.Loaded)
			{
				ConsoleActions.WriteLine("Connecting the client...");

				try
				{
					await client.StartAsync();
					ConsoleActions.WriteLine("Successfully connected the client.");
				}
				catch (Exception e)
				{
					ConsoleActions.ExceptionToConsole(e);
				}

				await Task.Delay(-1);
			}
		}

		/// <summary>
		/// Creates services the bot uses. Such as <see cref="IBotSettings"/>, <see cref="IGuildSettingsModule"/>, <see cref="IDiscordClient"/>,
		/// <see cref="ITimersModule"/>, and <see cref="ILogModule"/>.
		/// </summary>
		/// <returns>The service provider which holds all the services.</returns>
		public static IServiceProvider CreateServicesAndServiceProvider()
		{
			IBotSettings botSettings = SavingAndLoadingActions.CreateBotSettings(Constants.GLOBAL_SETTINGS_TYPE);
			IGuildSettingsModule guildSettings = SavingAndLoadingActions.CreateGuildSettingsModule(Constants.GUILDS_SETTINGS_TYPE);
			IDiscordClient client = CreateBotClient(botSettings);
			ITimersModule timers = new MyTimersModule(guildSettings);
			ILogModule logging = new MyLogModule(client, botSettings, guildSettings, timers);

			var serviceCollection = new ServiceCollection();
			serviceCollection.AddSingleton(botSettings);
			serviceCollection.AddSingleton(guildSettings);
			serviceCollection.AddSingleton(client);
			serviceCollection.AddSingleton(timers);
			serviceCollection.AddSingleton(logging);
			serviceCollection.AddSingleton(new CommandService(new CommandServiceConfig { CaseSensitiveCommands = false, ThrowOnError = false, }));

			return new DefaultServiceProviderFactory().CreateServiceProvider(serviceCollection);
		}
		/// <summary>
		/// Returns <see cref="DiscordSocketClient"/> if shard count in <paramref name="botSettings"/> is 1. Else returns <see cref="DiscordShardedClient"/>.
		/// </summary>
		/// <param name="botSettings">The settings to initialize the client with.</param>
		/// <returns>A discord client.</returns>
		public static IDiscordClient CreateBotClient(IBotSettings botSettings)
		{
			return botSettings.ShardCount > 1 ? CreateShardedClient(botSettings) : (IDiscordClient)CreateSocketClient(botSettings);
		}
		private static DiscordShardedClient CreateShardedClient(IBotSettings botSettings)
		{
			return new DiscordShardedClient(new DiscordSocketConfig
			{
				AlwaysDownloadUsers = botSettings.AlwaysDownloadUsers,
				MessageCacheSize = (int)botSettings.MessageCacheCount,
				LogLevel = botSettings.LogLevel,
				TotalShards = (int)botSettings.ShardCount,
			});
		}
		private static DiscordSocketClient CreateSocketClient(IBotSettings botSettings)
		{
			return new DiscordSocketClient(new DiscordSocketConfig
			{
				AlwaysDownloadUsers = botSettings.AlwaysDownloadUsers,
				MessageCacheSize = (int)botSettings.MaxUserGatherCount,
				LogLevel = botSettings.LogLevel,
			});
		}

		/// <summary>
		/// Attempts to login with the given input. Returns a boolean signifying whether the login was successful or not.
		/// </summary>
		/// <param name="client">The client to login.</param>
		/// <param name="input">The bot key.</param>
		/// <param name="startup">Whether or not this should be treated as the first attempt at logging in.</param>
		/// <returns>A boolean signifying whether the login was successful or not.</returns>
		public static async Task<bool> ValidateBotKey(IDiscordClient client, string input, bool startup = false)
		{
			var key = input?.Trim();

			if (startup)
			{
				if (!String.IsNullOrWhiteSpace(input))
				{
					try
					{
						await Login(client, key);
						return true;
					}
					catch (Exception)
					{
						ConsoleActions.WriteLine("The given key is no longer valid. Please enter a new valid key:");
					}
				}
				else
				{
					ConsoleActions.WriteLine("Please enter the bot's key:");
				}
				return false;
			}

			try
			{
				await Login(client, key);

				ConsoleActions.WriteLine("Succesfully logged in via the given bot key.");
				Properties.Settings.Default.BotKey = key;
				Properties.Settings.Default.Save();
				return true;
			}
			catch (Exception)
			{
				ConsoleActions.WriteLine("The given key is invalid. Please enter a valid key:");
				return false;
			}
		}
		private static async Task Login(IDiscordClient client, string key)
		{
			if (client is DiscordSocketClient)
			{
				await (client as DiscordSocketClient).LoginAsync(TokenType.Bot, key);
			}
			else if (client is DiscordShardedClient)
			{
				await (client as DiscordShardedClient).LoginAsync(TokenType.Bot, key);
			}
		}

		/// <summary>
		/// Updates a given client's stream and game using settings from the <paramref name="botSettings"/> parameter.
		/// </summary>
		/// <param name="client">The client to update.</param>
		/// <param name="botSettings">The information to update with.</param>
		/// <returns></returns>
		public static async Task UpdateGame(IDiscordClient client, IBotSettings botSettings)
		{
			var prefix = botSettings.Prefix;
			var game = botSettings.Game;
			var stream = botSettings.Stream;

			var streamType = StreamType.NotStreaming;
			if (!String.IsNullOrWhiteSpace(stream))
			{
				stream = Constants.TWITCH_URL + stream.Substring(stream.LastIndexOf('/') + 1);
				streamType = StreamType.Twitch;
			}

			if (client is DiscordSocketClient)
			{
				await (client as DiscordSocketClient).SetGameAsync(game, stream, streamType);
			}
			else if (client is DiscordShardedClient)
			{
				await (client as DiscordShardedClient).SetGameAsync(game, stream, streamType);
			}
		}
		/// <summary>
		/// Returns the shard Id for a <see cref="DiscordSocketClient"/> else returns -1.
		/// </summary>
		/// <param name="client">The client to get the shard from.</param>
		/// <returns>Int representing the shard Id.</returns>
		public static int GetShardId(IDiscordClient client)
		{
			if (client is DiscordSocketClient)
			{
				return (client as DiscordSocketClient).ShardId;
			}
			else
			{
				return -1;
			}
		}
		/// <summary>
		/// Returns the latency for a <see cref="DiscordSocketClient"/> or <see cref="DiscordShardedClient"/> else throws an exception.
		/// </summary>
		/// <param name="client">The client to get the latency for.</param>
		/// <returns>Int representing the client's latency.</returns>
		/// <exception cref="ArgumentException">/></exception>
		public static int GetLatency(IDiscordClient client)
		{
			if (client is DiscordSocketClient)
			{
				return (client as DiscordSocketClient).Latency;
			}
			else if (client is DiscordShardedClient)
			{
				return (client as DiscordShardedClient).Latency;
			}
			else
			{
				throw new ArgumentException($"{client.GetType().Name} is not a valid client type.");
			}
		}
		/// <summary>
		/// Returns the shard count <see cref="DiscordSocketClient"/> or <see cref="DiscordShardedClient"/> else throws an exception.
		/// </summary>
		/// <param name="client">The client to get the shard count for.</param>
		/// <returns>Int representing the client's shard count.</returns>
		/// <exception cref="ArgumentException">/></exception>
		public static int GetShardCount(IDiscordClient client)
		{
			if (client is DiscordSocketClient)
			{
				return 1;
			}
			else if (client is DiscordShardedClient)
			{
				return (client as DiscordShardedClient).Shards.Count;
			}
			else
			{
				throw new ArgumentException($"{client.GetType().Name} is not a valid client type.");
			}
		}
		/// <summary>
		/// Returns the shard Id for a specified guild.
		/// </summary>
		/// <param name="client">The client hosting the guild</param>
		/// <param name="guild">The guild to search for.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentException"></exception>
		public static int GetShardIdFor(IDiscordClient client, IGuild guild)
		{
			if (client is DiscordSocketClient)
			{
				return (client as DiscordSocketClient).ShardId;
			}
			else if (client is DiscordShardedClient)
			{
				return (client as DiscordShardedClient).GetShardIdFor(guild);
			}
			else
			{
				throw new ArgumentException($"{client.GetType().Name} is not a valid client type.");
			}
		}

		/// <summary>
		/// Resets all setting saved in <see cref="Properties.Settings.Default"/>.
		/// </summary>
		public static void ResetSettingsSavedInPropertiesSettings()
		{
			Properties.Settings.Default.BotKey = null;
			Properties.Settings.Default.Path = null;
			Properties.Settings.Default.BotName = null;
			Properties.Settings.Default.BotId = 0;
			Properties.Settings.Default.Save();
		}
		/// <summary>
		/// Creates a new instance of the current application then exits the current application.
		/// </summary>
		public static void RestartBot()
		{
			try
			{
				//Create a new instance of the bot and close the old one
				System.Diagnostics.Process.Start(System.Windows.Application.ResourceAssembly.Location);
				DisconnectBot();
			}
			catch (Exception e)
			{
				ConsoleActions.ExceptionToConsole(e);
			}
		}
		/// <summary>
		/// Exits the current application.
		/// </summary>
		public static void DisconnectBot()
		{
			Environment.Exit(0);
		}
	}
}