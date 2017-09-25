using Advobot.Actions;
using Discord;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Advobot.Launcher
{
	/// <summary>
	/// Starting point for Advobot.
	/// </summary>
	public class ConsoleLauncher
	{
		private static void Main()
		{
			AppDomain.CurrentDomain.UnhandledException += SavingAndLoadingActions.LogUncaughtException;
			MainAsync().GetAwaiter().GetResult();
		}

		private static async Task MainAsync()
		{
			//Get the save path
			var savePath = true;
			while (!Config.ValidatePath((savePath ? null : Console.ReadLine()), savePath))
			{
				savePath = false;
			}

			var client = await CommandHandler.Install(CreationActions.CreateServicesAndServiceProvider());

			//Get the bot key
			var botKey = true;
			while (!await Config.ValidateBotKey(client, (botKey ? null : Console.ReadLine()), botKey))
			{
				botKey = false;
			}

			await ClientActions.StartAsync(client);
		}
	}
}