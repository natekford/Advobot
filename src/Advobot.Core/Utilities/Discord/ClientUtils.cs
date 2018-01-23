using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Advobot.Core.Interfaces;
using Discord;
using Discord.WebSocket;

namespace Advobot.Core.Utilities
{
	/// <summary>
	/// Actions done on an <see cref="IDiscordClient"/>.
	/// </summary>
	public static class ClientUtils
	{
		/// <summary>
		/// Tries to start the bot.
		/// </summary>
		/// <param name="client"></param>
		/// <returns></returns>
		public static async Task StartAsync(IDiscordClient client)
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
					ConsoleUtils.WriteLine("Connecting the client...");
					await client.StartAsync().CAF();
					ConsoleUtils.WriteLine("Successfully connected the client.");
					await Task.Delay(-1).CAF();
					return;
				}
			}
		}
		/// <summary>
		/// Attempts to login with the given key.
		/// </summary>
		/// <param name="client"></param>
		/// <param name="key"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentException"></exception>
		public static async Task LoginAsync(IDiscordClient client, string key)
		{
			if (client is DiscordSocketClient socketClient)
			{
				await socketClient.LoginAsync(TokenType.Bot, key).CAF();
			}
			else if (client is DiscordShardedClient shardedClient)
			{
				await shardedClient.LoginAsync(TokenType.Bot, key).CAF();
			}
			else
			{
				throw new ArgumentException("invalid type", nameof(client));
			}
		}
		/// <summary>
		/// Returns the user who owns the bot.
		/// </summary>
		/// <param name="client"></param>
		/// <returns></returns>
		public static async Task<IUser> GetBotOwnerAsync(IDiscordClient client)
		{
			return (await client.GetApplicationInfoAsync().CAF()).Owner;
		}
		/// <summary>
		/// Updates a given client's stream and game using settings from the <paramref name="botSettings"/> parameter.
		/// </summary>
		/// <param name="client">The client to update.</param>
		/// <param name="botSettings">The information to update with.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentException"></exception>
		public static async Task UpdateGameAsync(IDiscordClient client, IBotSettings botSettings)
		{
			var game = botSettings.Game;
			var stream = botSettings.Stream;

			var activityType = ActivityType.Playing;
			if (!String.IsNullOrWhiteSpace(stream))
			{
				stream = Constants.TWITCH_URL + stream.Substring(stream.LastIndexOf('/') + 1);
				activityType = ActivityType.Streaming;
			}

			if (client is DiscordSocketClient socketClient)
			{
				await socketClient.SetGameAsync(game, stream, activityType).CAF();
			}
			else if (client is DiscordShardedClient shardedClient)
			{
				await shardedClient.SetGameAsync(game, stream, activityType).CAF();
			}
			else
			{
				throw new ArgumentException("invalid type", nameof(client));
			}
		}
		/// <summary>
		/// Updates the bot's icon to the given image.
		/// </summary>
		/// <param name="client"></param>
		/// <param name="fileInfo"></param>
		/// <returns></returns>
		public static async Task ModifyBotIconAsync(IDiscordClient client, FileInfo fileInfo)
		{
			using (var stream = new StreamReader(fileInfo.FullName))
			{
				await client.CurrentUser.ModifyAsync(x => x.Avatar = new Image(stream.BaseStream)).CAF();
			}
		}
		/// <summary>
		/// Returns the shard id for a <see cref="DiscordSocketClient"/> else returns -1.
		/// </summary>
		/// <param name="client"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentException"></exception>
		public static int GetShardId(IDiscordClient client)
		{
			switch (client)
			{
				case DiscordSocketClient socketClient:
					return socketClient.ShardId;
				case DiscordShardedClient _:
					return -1;
				default:
					throw new ArgumentException("invalid type", nameof(client));
			}
		}
		/// <summary>
		/// Returns the latency for a client.
		/// </summary>
		/// <param name="client"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentException">/></exception>
		public static int GetLatency(IDiscordClient client)
		{
			switch (client)
			{
				case DiscordSocketClient socketClient:
					return socketClient.Latency;
				case DiscordShardedClient shardedClient:
					return shardedClient.Latency;
				default:
					throw new ArgumentException("invalid type", nameof(client));
			}
		}
		/// <summary>
		/// Returns the shard count of a client.
		/// </summary>
		/// <param name="client"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentException"></exception>
		public static int GetShardCount(IDiscordClient client)
		{
			switch (client)
			{
				case DiscordSocketClient socketClient:
					return 1;
				case DiscordShardedClient shardedClient:
					return shardedClient.Shards.Count;
				default:
					throw new ArgumentException("invalid type", nameof(client));
			}
		}
		/// <summary>
		/// Returns the shard id for a guild is on.
		/// </summary>
		/// <param name="client"></param>
		/// <param name="guild"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentException"></exception>
		public static int GetShardIdFor(IDiscordClient client, IGuild guild)
		{
			switch (client)
			{
				case DiscordSocketClient socketClient:
					return socketClient.ShardId;
				case DiscordShardedClient shardedClient:
					return shardedClient.GetShardIdFor(guild);
				default:
					throw new ArgumentException("invalid type", nameof(client));
			}
		}
		/// <summary>
		/// Creates a new bot that uses the same console. The bot that starts is created using <see cref="Process.Start()"/> and specifying the filename as dotnet and the arguments as the location of the .dll.
		/// <para>
		/// The old bot is then killed
		/// </para>
		/// </summary>
		public static void RestartBot()
		{
			//For some reason Process.Start("dotnet", loc); doesn't work the same as what's currently used.
			Process.Start(new ProcessStartInfo
			{
				FileName = "dotnet",
				Arguments = $@"""{Assembly.GetEntryAssembly().Location}"""
			});
			ConsoleUtils.WriteLine($"Restarted the bot.{Environment.NewLine}");
			Process.GetCurrentProcess().Kill();
		}
		/// <summary>
		/// Exits the current application.
		/// </summary>
		public static void DisconnectBot(IDiscordClient client)
		{
			//When this gets awaited the client hangs
			switch (client)
			{
#pragma warning disable 4014
				case DiscordSocketClient socketClient:
					socketClient.SetStatusAsync(UserStatus.Invisible);
					break;
				case DiscordShardedClient shardedClient:
					shardedClient.SetStatusAsync(UserStatus.Invisible);
					break;
				default:
					throw new ArgumentException("invalid type", nameof(client));
#pragma warning restore 4014
			}
			client.StopAsync();
			Environment.Exit(0);
		}
	}
}