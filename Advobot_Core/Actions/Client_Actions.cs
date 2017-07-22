using Advobot.Interfaces;
using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace Advobot
{
	namespace Actions
	{
		public static class ClientActions
		{
			public static async Task MaybeStartBot(IDiscordClient client, IBotSettings botSettings)
			{
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

			public static IDiscordClient CreateBotClient(IBotSettings botSettings)
			{
				IDiscordClient client;
				if (botSettings.ShardCount > 1)
				{
					client = CreateShardedClient(botSettings);
				}
				else
				{
					client = CreateSocketClient(botSettings);
				}
				return client;
			}
			public static DiscordShardedClient CreateShardedClient(IBotSettings botSettings)
			{
				return new DiscordShardedClient(new DiscordSocketConfig
				{
					AlwaysDownloadUsers = botSettings.AlwaysDownloadUsers,
					MessageCacheSize = (int)botSettings.MessageCacheCount,
					LogLevel = botSettings.LogLevel,
					TotalShards = (int)botSettings.ShardCount,
				});
			}
			public static DiscordSocketClient CreateSocketClient(IBotSettings botSettings)
			{
				return new DiscordSocketClient(new DiscordSocketConfig
				{
					AlwaysDownloadUsers = botSettings.AlwaysDownloadUsers,
					MessageCacheSize = (int)botSettings.MaxUserGatherCount,
					LogLevel = botSettings.LogLevel,
				});
			}

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
			public static async Task SetGame(IDiscordClient client, IBotSettings botSettings)
			{
				var game = botSettings.Game;
				var stream = botSettings.Stream;
				var prefix = botSettings.Prefix;

				var streamType = StreamType.NotStreaming;
				if (!String.IsNullOrWhiteSpace(stream))
				{
					stream = Constants.TWITCH_URL + stream.Substring(stream.LastIndexOf('/') + 1);
					streamType = StreamType.Twitch;
				}

				if (client is DiscordSocketClient)
				{
					await (client as DiscordSocketClient).SetGameAsync(game ?? String.Format("type \"{0}help\" for help.", prefix), stream, streamType);
				}
				else if (client is DiscordShardedClient)
				{
					await (client as DiscordShardedClient).SetGameAsync(game ?? String.Format("type \"{0}help\" for help.", prefix), stream, streamType);
				}
			}
			public static int GetShardID(IDiscordClient client)
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
					return -1;
				}
			}
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
					return -1;
				}
			}
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
					return -1;
				}
			}
		}
	}
}