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
		/// Tries to start the bot by making sure a save path and bot key are provided and the bot is not already running.
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
		/// <exception cref="InvalidCastException"></exception>
		public static async Task LoginAsync(IDiscordClient client, string key)
		{
			if (client is DiscordSocketClient)
			{
				await ((DiscordSocketClient)client).LoginAsync(TokenType.Bot, key);
			}
			else
			{
				await ((DiscordShardedClient)client).LoginAsync(TokenType.Bot, key);
			}
		}
		/// <summary>
		/// Returns the shard id for a <see cref="DiscordSocketClient"/> else returns -1.
		/// </summary>
		/// <param name="client"></param>
		/// <returns></returns>
		public static int GetShardId(IDiscordClient client)
		{
			if (client is DiscordSocketClient)
			{
				return ((DiscordSocketClient)client).ShardId;
			}
			else
			{
				return -1;
			}
		}
		/// <summary>
		/// Returns the latency for a client.
		/// </summary>
		/// <param name="client"></param>
		/// <returns></returns>
		/// <exception cref="InvalidCastException">/></exception>
		public static int GetLatency(IDiscordClient client)
		{
			if (client is DiscordSocketClient)
			{
				return ((DiscordSocketClient)client).Latency;
			}
			else
			{
				return ((DiscordShardedClient)client).Latency;
			}
		}
		/// <summary>
		/// Returns the shard count of a client.
		/// </summary>
		/// <param name="client"></param>
		/// <returns></returns>
		/// <exception cref="InvalidCastException"></exception>
		public static int GetShardCount(IDiscordClient client)
		{
			if (client is DiscordSocketClient)
			{
				return 1;
			}
			else
			{
				return ((DiscordShardedClient)client).Shards.Count;
			}
		}
		/// <summary>
		/// Returns the shard id for a guild is on.
		/// </summary>
		/// <param name="client"></param>
		/// <param name="guild"></param>
		/// <returns></returns>
		/// <exception cref="InvalidCastException"></exception>
		public static int GetShardIdFor(IDiscordClient client, IGuild guild)
		{
			if (client is DiscordSocketClient)
			{
				return ((DiscordSocketClient)client).ShardId;
			}
			else
			{
				return ((DiscordShardedClient)client).GetShardIdFor(guild);
			}
		}

		/// <summary>
		/// Updates a given client's stream and game using settings from the <paramref name="botSettings"/> parameter.
		/// </summary>
		/// <param name="client">The client to update.</param>
		/// <param name="botSettings">The information to update with.</param>
		/// <returns></returns>
		/// <exception cref="InvalidCastException"></exception>
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

			if (client is DiscordSocketClient)
			{
				await ((DiscordSocketClient)client).SetGameAsync(game, stream, streamType);
			}
			else
			{
				await ((DiscordShardedClient)client).SetGameAsync(game, stream, streamType);
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
		public static void DisconnectBot()
		{
			Environment.Exit(0);
		}
	}
}