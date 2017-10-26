using Advobot.Core.Actions;
using System;

namespace Advobot.UILauncher
{
	public class UILauncher
	{
		[STAThread]
		private static void Main()
		{
			AppDomain.CurrentDomain.UnhandledException += SavingAndLoadingActions.LogUncaughtException;
			new AdvobotApplication().RunWithMainWindow();
		}
	}
}
