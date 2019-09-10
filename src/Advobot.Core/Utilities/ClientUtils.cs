using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;

using Advobot.Services.BotSettings;
using Advobot.Settings;

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
		/// <summary>
		/// Exits the current application.
		/// </summary>
		public static async Task DisconnectBotAsync(this IDiscordClient client)
		{
			await client.StopAsync().CAF();
			Environment.Exit(0);
		}

		/// <summary>
		/// Restarts the application correctly if it's a .Net Core application.
		/// </summary>
		/// <param name="client"></param>
		/// <param name="restartArgs"></param>
		public static async Task RestartBotAsync(this IDiscordClient client, IRestartArgumentProvider restartArgs)
		{
			await client.StopAsync().CAF();
			//For some reason Process.Start("dotnet", loc); doesn't work the same as what's currently used.
			Process.Start(new ProcessStartInfo
			{
				FileName = "dotnet",
				Arguments = $@"""{Assembly.GetEntryAssembly().Location}"" {restartArgs.RestartArguments}"
			});
			Process.GetCurrentProcess().Kill();
		}

		/// <summary>
		/// Updates a given client's stream and game using settings from the <paramref name="settings"/> parameter.
		/// </summary>
		/// <param name="client">The client to update.</param>
		/// <param name="settings">The information to update with.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentException"></exception>
		public static Task UpdateGameAsync(this BaseSocketClient client, IBotSettings settings)
		{
			var game = settings.Game;
			var stream = settings.Stream;
			var activityType = ActivityType.Playing;
			if (!string.IsNullOrWhiteSpace(stream))
			{
				stream = $"https://www.twitch.tv/{stream.Substring(stream.LastIndexOf('/') + 1)}";
				activityType = ActivityType.Streaming;
			}
			return client.SetGameAsync(game, stream, activityType);
		}
	}
}