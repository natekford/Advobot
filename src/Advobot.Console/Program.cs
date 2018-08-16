using System;
using System.Linq;
using System.Threading.Tasks;
using Advobot.Classes;
using Advobot.Interfaces;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Console
{
	using Console = System.Console;

	/// <summary>
	/// Starting point for Advobot.
	/// </summary>
	public sealed class ConsoleLauncher
	{
		private static async Task Main(string[] args)
		{
			AppDomain.CurrentDomain.UnhandledException += (sender, e) => IOUtils.LogUncaughtException(e.ExceptionObject);
			ConsoleUtils.PrintingFlags = 0
				| ConsolePrintingFlags.Print
				| ConsolePrintingFlags.LogTime
				| ConsolePrintingFlags.LogCaller
				| ConsolePrintingFlags.RemoveDuplicateNewLines;

			var config = LowLevelConfig.Load(args);
			ConsoleUtils.DebugWrite($"Args: {config.CurrentInstance}|{config.PreviousProcessId}");

			//Get the save path
			var startup = true;
			while (!config.ValidatedPath)
			{
				startup = config.ValidatePath((startup ? null : Console.ReadLine()), startup);
			}
			//Get the bot key
			startup = true;
			while (!config.ValidatedKey)
			{
				startup = await config.ValidateBotKey((startup ? null : Console.ReadLine()), startup, ClientUtils.RestartBotAsync).CAF();
			}

			var provider = new IterableServiceProvider(config.CreateDefaultServices(), true);
			foreach (var db in provider.OfType<IUsesDatabase>())
			{
				db.Start();
			}
			await config.StartAsync(provider.GetRequiredService<DiscordShardedClient>());
		}
	}
}