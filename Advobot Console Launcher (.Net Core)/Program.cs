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
			//Get the save path
			var startup = true;
			while (true)
			{
				var input = startup ? null : Console.ReadLine();
				if (Config.ValidatePath(input, startup))
				{
					break;
				}
				startup = false;
			}

			var provider = ClientActions.CreateServicesAndServiceProvider();
			await CommandHandler.Install(provider);
			await ClientActions.MaybeStartConsole(provider.GetService<IDiscordClient>(), provider.GetService<IBotSettings>());
		}
	}
}