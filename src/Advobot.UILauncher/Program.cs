using Advobot.Core.Actions;
using Advobot.UILauncher.Classes;
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

			ConsoleActions.CreateWrittenLines();
			new Application().Run(new AdvobotWindow());
		}
	}
}
