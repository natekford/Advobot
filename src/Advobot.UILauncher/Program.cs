using Advobot.Core.Actions;
using Advobot.UILauncher.Classes;
using System;

namespace Advobot.UILauncher
{
	public class UILauncher
	{
		[STAThread]
		private static void Main()
		{
			AppDomain.CurrentDomain.UnhandledException += (sender, e) => SavingAndLoadingActions.LogUncaughtException(e.ExceptionObject);
			new AdvobotApplication().Run();
		}
	}
}
