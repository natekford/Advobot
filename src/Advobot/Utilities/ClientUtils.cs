using Advobot.Settings;

using AdvorangesUtils;

using Discord;

using System.Diagnostics;
using System.Reflection;

namespace Advobot.Utilities;

/// <summary>
/// Actions done on discord clients.
/// </summary>
public static class ClientUtils
{
	/// <summary>
	/// Restarts the application correctly if it's a .Net Core application.
	/// </summary>
	/// <param name="client"></param>
	/// <param name="restartArgs"></param>
	public static async Task RestartBotAsync(this IDiscordClient client, IRestartArgumentProvider restartArgs)
	{
		await client.StopAsync().CAF();
		// For some reason Process.Start("dotnet", loc); doesn't work the same as what's currently used.
		Process.Start(new ProcessStartInfo
		{
			FileName = "dotnet",
			Arguments = $@"""{Assembly.GetEntryAssembly()!.Location}"" {restartArgs.RestartArguments}"
		});
		Process.GetCurrentProcess().Kill();
	}
}