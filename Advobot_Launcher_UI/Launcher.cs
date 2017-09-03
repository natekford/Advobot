using Advobot.Actions;
using Advobot.Graphics.UserInterface;
using System;
using System.Threading.Tasks;

namespace Advobot.Launcher
{
	public class UILauncher
	{
		[STAThread]
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

			//Only has a reason to call this method in the UI launcher because the console version has no way of displaying them.
			ConsoleActions.CreateWrittenLines();

			MainAsync(ClientActions.CreateServicesAndServiceProvider()).GetAwaiter().GetResult();
		}

		private static async Task MainAsync(IServiceProvider provider)
		{
			await CommandHandler.Install(provider);
			new System.Windows.Application().Run(new MyWindow(provider));
		}
	}
}
