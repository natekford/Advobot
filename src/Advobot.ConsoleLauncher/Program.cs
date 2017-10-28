using Advobot.Commands;
using Advobot.Core;
using Advobot.Core.Actions;
using System;
using System.Threading.Tasks;

namespace Advobot.ConsoleLauncher
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

			var provider = await CreationActions.CreateServiceProvider().CAF();
			var client = CommandHandler.Install(provider);

			//Get the bot key
			var botKey = true;
			while (!await Config.ValidateBotKey(client, (botKey ? null : Console.ReadLine()), botKey).CAF())
			{
				botKey = false;
			}

			await ClientActions.StartAsync(client).CAF();
		}
	}
}