using Advobot.Core.Actions;
using System;
using System.Windows;

namespace Advobot.UILauncher
{
	public class UILauncher
	{
		[STAThread]
		private static void Main()
		{
			AppDomain.CurrentDomain.UnhandledException += SavingAndLoadingActions.LogUncaughtException;
			var application = new Application
			{
				MainWindow = new AdvobotWindow()
			};
			application.Run(application.MainWindow);
		}
	}
}
