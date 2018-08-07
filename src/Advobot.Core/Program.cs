using System;
using System.Threading.Tasks;
using Advobot.Classes;
using Advobot.Interfaces;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord.Rest;
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

			var config = LowLevelConfig.Load(args);
			ConsoleUtils.DebugWrite($"Args: {config.CurrentInstance}|{config.PreviousProcessId}");

			//Get the save path
			var savePath = true;
			while (!config.ValidatePath((savePath ? null : Console.ReadLine()), savePath))
			{
				savePath = false;
			}

			var provider = CreationUtils.CreateDefaultServices(config).BuildServiceProvider();
			//Retrieve the command handler to initialize it.
			var cmd = provider.GetRequiredService<ICommandHandlerService>();
			var client = provider.GetRequiredService<DiscordShardedClient>();

			//Get the bot key
			var botKey = true;
			while (!await config.ValidateBotKey(client, (botKey ? null : Console.ReadLine()), botKey).CAF())
			{
				botKey = false;
			}

			await config.VerifyBotDirectory(ClientUtils.RestartBotAsync).CAF();
			await ClientUtils.StartAsync(client).CAF();
		}
	}
}