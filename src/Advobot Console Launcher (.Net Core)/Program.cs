using Advobot.Actions;
using Advobot.Commands;
using System;
using System.Threading.Tasks;

namespace Advobot.Launcher
{
	/// <summary>
	/// Starting point for Advobot.
	/// </summary>
	public class ConsoleLauncher
	{
		private static async Task Main()
		{
			AppDomain.CurrentDomain.UnhandledException += SavingAndLoadingActions.LogUncaughtException;

			//Get the save path
			var savePath = true;
			while (!Config.ValidatePath((savePath ? null : Console.ReadLine()), savePath))
			{
				savePath = false;
			}

			var provider = await CreationActions.CreateServiceProvider();
			var client = CommandHandler.Install(provider);

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