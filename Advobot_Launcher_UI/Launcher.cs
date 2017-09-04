using Advobot.Actions;
using Advobot.Graphics;
using System;
using System.Threading.Tasks;

namespace Advobot.Launcher
{
	class UILauncher
	{
		/// <summary>
		/// Starting point for Advobot with UI.
		/// </summary>
		/// <remarks>
		/// Requires <see cref="STAThreadAttribute"/> since all UI needs that attribute to run.
		/// </remarks>
		[STAThread]
		private static void Main()
		{
#if RELEASE
			//Make sure only one instance is running at the same time
			if (System.Diagnostics.Process.GetProcessesByName(System.IO.Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetEntryAssembly().Location)).Length > 1)
			{
				return;
			}
#endif
			//Only has a reason to call this method in the UI launcher because the console version has no way of displaying them.
			ConsoleActions.CreateWrittenLines();

			AppDomain.CurrentDomain.UnhandledException += SavingAndLoadingActions.LogUncaughtException;
			MainAsync(ClientActions.CreateServicesAndServiceProvider()).GetAwaiter().GetResult();
		}

		private static async Task MainAsync(IServiceProvider provider)
		{
			await CommandHandler.Install(provider);
			new System.Windows.Application().Run(new MyWindow(provider));
		}
	}
}
