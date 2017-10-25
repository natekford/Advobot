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
			new Application().Run(new AdvobotWindow());
		}
	}
}
