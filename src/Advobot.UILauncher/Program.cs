using Advobot.Core.Actions;
using System;
using System.Windows;
using System.Windows.Threading;

namespace Advobot.UILauncher
{
	public class UILauncher
	{
		[STAThread]
		private static void Main()
		{
			AppDomain.CurrentDomain.UnhandledException += SavingAndLoadingActions.LogUncaughtException;
			var application = new AdvobotApplication();
			application.Run(application.MainWindow);
		}
	}
}
