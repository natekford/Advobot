using Advobot.Actions;
using System;
using System.Threading.Tasks;

namespace Advobot.Launcher
{
	public class ConsoleLauncher
	{
		private static void Main()
		{
			//Make sure only one instance is running at the same time
#if RELEASE
			if (System.Diagnostics.Process.GetProcessesByName(System.IO.Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetEntryAssembly().Location)).Length > 1)
			{
				return;
			}
#endif
			AppDomain.CurrentDomain.UnhandledException += SavingAndLoadingActions.LogUncaughtException;
			MainAsync(ClientActions.CreateServicesAndServiceProvider()).GetAwaiter().GetResult();
		}

		private static async Task MainAsync(IServiceProvider provider)
		{
			await CommandHandler.Install(provider);
			await ClientActions.MaybeStartBot(provider.GetService<Discord.IDiscordClient>(), provider.GetService<Interfaces.IBotSettings>());
		}
	}
}
