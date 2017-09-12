using Advobot.Classes;
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
		/// </summary>
		/// <param name="client">The client to start.</param>
		/// <returns></returns>
		public static async Task ConnectClient(IDiscordClient client)
		{
			switch (client.ConnectionState)
			{
				case ConnectionState.Connecting:
				case ConnectionState.Connected:
				case ConnectionState.Disconnecting:
				{
					return;
				}
				case ConnectionState.Disconnected:
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
					return;
				}
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

			var serviceCollection = new ServiceCollection()
				.AddSingleton(botSettings)
				.AddSingleton(guildSettings)
				.AddSingleton(client)
				.AddSingleton(timers)
				.AddSingleton(logging)
				.AddSingleton(new CommandService(new CommandServiceConfig { CaseSensitiveCommands = false, ThrowOnError = false, }));

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
		/// Attempts to login with the given key.
		/// </summary>
		/// <param name="client"></param>
		/// <param name="key"></param>
		/// <returns></returns>
		public static async Task Login(IDiscordClient client, string key)
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
		/// Exits the current application.
		/// </summary>
		public static void DisconnectBot()
		{
			Environment.Exit(0);
		}
	}
}