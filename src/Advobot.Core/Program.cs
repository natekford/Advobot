using System;
using System.Threading.Tasks;
using Advobot.Classes;
using Advobot.Interfaces;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Core
{
	/// <summary>
	/// Starting point for Advobot.
	/// </summary>
	public class ConsoleLauncher
	{
		private static async Task Main(string[] args)
		{
			AppDomain.CurrentDomain.UnhandledException += (sender, e) => IOUtils.LogUncaughtException(e.ExceptionObject);
			ConsoleUtils.PrintingFlags = 0
				| ConsolePrintingFlags.Print
				| ConsolePrintingFlags.LogTime
				| ConsolePrintingFlags.LogCaller
				| ConsolePrintingFlags.RemoveDuplicateNewLines;

			var parsed = new AdvobotStartupArgs(args);
			ConsoleUtils.DebugWrite($"Args: {parsed.CurrentInstance}|{parsed.PreviousProcessId}");
			var config = parsed.CreateConfig();

			//Get the save path
			var savePath = true;
			while (!config.ValidatePath((savePath ? null : Console.ReadLine()), savePath))
			{
				savePath = false;
			}

			var provider = CreationUtils.CreateDefaultServices<BotSettings, GuildSettings>(config).BuildServiceProvider();
			provider.GetRequiredService<ICommandHandlerService>().RestartRequired += ClientUtils.RestartBotAsync;
			var client = provider.GetService<DiscordShardedClient>();

			//Get the bot key
			var botKey = true;
			while (!await config.ValidateBotKey(client, (botKey ? null : Console.ReadLine()), botKey).CAF())
			{
				botKey = false;
			}

			await ClientUtils.StartAsync(client).CAF();
		}
	}
}