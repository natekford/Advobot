using Advobot.Interfaces;
using Discord;
using Discord.WebSocket;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using System.IO;

namespace Advobot.Actions
{
	public static class ClientActions
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
				await socketClient.LoginAsync(TokenType.Bot, key);
			}
			else if (client is DiscordShardedClient shardedClient)
			{
				await shardedClient.LoginAsync(TokenType.Bot, key);
			}
			else
			{
				throw new ArgumentException("Invalid client provided.");
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
			if (client is DiscordSocketClient socketClient)
			{
				return socketClient.ShardId;
			}
			else if (client is DiscordShardedClient shardedClient)
			{
				return -1;
			}
			else
			{
				throw new ArgumentException("Invalid client provided.");
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
			if (client is DiscordSocketClient socketClient)
			{
				return socketClient.Latency;
			}
			else if (client is DiscordShardedClient shardedClient)
			{
				return shardedClient.Latency;
			}
			else
			{
				throw new ArgumentException("Invalid client provided.");
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
			if (client is DiscordSocketClient socketClient)
			{
				return 1;
			}
			else if (client is DiscordShardedClient shardedClient)
			{
				return shardedClient.Shards.Count;
			}
			else
			{
				throw new ArgumentException("Invalid client provided.");
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
			if (client is DiscordSocketClient socketClient)
			{
				return socketClient.ShardId;
			}
			else if (client is DiscordShardedClient shardedClient)
			{
				return shardedClient.GetShardIdFor(guild);
			}
			else
			{
				throw new ArgumentException("Invalid client provided.");
			}
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
			var prefix = botSettings.Prefix;
			var game = botSettings.Game;
			var stream = botSettings.Stream;

			var streamType = StreamType.NotStreaming;
			if (!String.IsNullOrWhiteSpace(stream))
			{
				stream = Constants.TWITCH_URL + stream.Substring(stream.LastIndexOf('/') + 1);
				streamType = StreamType.Twitch;
			}

			if (client is DiscordSocketClient socketClient)
			{
				await socketClient.SetGameAsync(game, stream, streamType);
			}
			else if (client is DiscordShardedClient shardedClient)
			{
				await shardedClient.SetGameAsync(game, stream, streamType);
			}
			else
			{
				throw new ArgumentException("Invalid client provided.");
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
				await client.CurrentUser.ModifyAsync(x => x.Avatar = new Image(stream.BaseStream));
			}
		}

		/// <summary>
		/// Creates a new bot that uses the same console. The bot that starts is created using <see cref="Process.Start"/> and specifying the filename as dotnet and the arguments as the location of the .dll.
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
				Arguments = $@"""{Assembly.GetEntryAssembly().Location}""",
			});
			ConsoleActions.WriteLine("Restarted the bot." + Environment.NewLine);
			Process.GetCurrentProcess().Kill();
		}
		/// <summary>
		/// Exits the current application.
		/// </summary>
		public static async Task DisconnectBot(IDiscordClient client)
		{
			if (client is DiscordSocketClient socketClient)
			{
				await socketClient.SetStatusAsync(UserStatus.Invisible);
			}
			else if (client is DiscordShardedClient shardedClient)
			{
				await shardedClient.SetStatusAsync(UserStatus.Invisible);
			}

			//I think it doesn't really matter if this isn't awaited.
			//What is the worst that could happen when the client is being killed anyways.
			#pragma warning disable
			client.StopAsync();
			#pragma warning restore
			Environment.Exit(0);
		}
	}
}