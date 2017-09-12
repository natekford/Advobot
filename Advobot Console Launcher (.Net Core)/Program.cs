using Advobot.Actions;
using Advobot.Interfaces;
using Discord;
using System;
using System.Threading.Tasks;

namespace Advobot.Launcher
{
	/// <summary>
	/// Starting point for Advobot.
	/// </summary>
	class ConsoleLauncher
	{
		private static void Main()
		{
			AppDomain.CurrentDomain.UnhandledException += SavingAndLoadingActions.LogUncaughtException;
			MainAsync().GetAwaiter().GetResult();
		}

		private static async Task MainAsync()
		{
			var test = Console.ForegroundColor;
			//Get the save path
			var savePath = true;
			while (!Config.ValidatePath((savePath ? null : Console.ReadLine()), savePath))
			{
				savePath = false;
			}

			var provider = ClientActions.CreateServicesAndServiceProvider();
			await CommandHandler.Install(provider);

			var client = provider.GetService<IDiscordClient>();

			//Get the bot key
			var botKey = true;
			while (!await Config.ValidateBotKey(client, (botKey ? null : Console.ReadLine()), botKey))
			{
				botKey = false;
			}

			await ClientActions.ConnectClient(client);
		}
	}
}