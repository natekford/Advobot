using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using Advobot.Interfaces;
using AdvorangesUtils;
using Discord;
using Discord.WebSocket;

namespace Advobot.Utilities
{
	/// <summary>
	/// Actions done on discord clients.
	/// </summary>
	public static class ClientUtils
	{
		private static ulong _BotOwnerId;

		/// <summary>
		/// Tries to start the bot and start command handling.
		/// </summary>
		/// <param name="client"></param>
		/// <returns></returns>
		public static async Task StartAsync(BaseSocketClient client)
		{
			ConsoleUtils.WriteLine("Connecting the client...");
			await client.StartAsync().CAF();
			ConsoleUtils.WriteLine("Successfully connected the client.");
			await Task.Delay(-1).CAF();
		}
		/// <summary>
		/// Gets the id of the bot owner.
		/// </summary>
		/// <param name="client"></param>
		/// <returns></returns>
		public static async Task<ulong> GetOwnerIdAsync(BaseSocketClient client)
		{
			return _BotOwnerId != 0 ? _BotOwnerId : (_BotOwnerId = (await client.GetApplicationInfoAsync().CAF()).Owner.Id);
		}
		/// <summary>
		/// Updates a given client's stream and game using settings from the <paramref name="botSettings"/> parameter.
		/// </summary>
		/// <param name="client">The client to update.</param>
		/// <param name="botSettings">The information to update with.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentException"></exception>
		public static async Task UpdateGameAsync(BaseSocketClient client, IBotSettings botSettings)
		{
			var game = botSettings.Game;
			var stream = botSettings.Stream;

			var activityType = ActivityType.Playing;
			if (!String.IsNullOrWhiteSpace(stream))
			{
				stream = "https://www.twitch.tv/" + stream.Substring(stream.LastIndexOf('/') + 1);
				activityType = ActivityType.Streaming;
			}

			await client.SetGameAsync(game, stream, activityType).CAF();
		}
		/// <summary>
		/// Restarts the application correctly if it's a .Net Core application.
		/// </summary>
		public static async Task RestartBotAsync(BaseSocketClient client)
		{
			await client.StopAsync().CAF();
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
		public static async Task DisconnectBotAsync(BaseSocketClient client)
		{
			await client.StopAsync().CAF();
			Environment.Exit(0);
		}
		/// <summary>
		/// Returns request options, with <paramref name="reason"/> as the audit log reason.
		/// </summary>
		/// <param name="reason"></param>
		/// <returns></returns>
		public static RequestOptions CreateRequestOptions(string reason)
		{
			return new RequestOptions
			{
				AuditLogReason = reason,
				RetryMode = RetryMode.RetryRatelimit,
			};
		}
	}
}