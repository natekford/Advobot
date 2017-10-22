using Advobot.Core.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Advobot.Commands;
using Advobot.UILauncher.Classes;

namespace Advobot.UILauncher
{
	public class UILauncher
	{
		[STAThread]
		private static void Main()
		{
			ConsoleActions.CreateWrittenLines();
			AppDomain.CurrentDomain.UnhandledException += SavingAndLoadingActions.LogUncaughtException;
			new System.Windows.Application().Run(new AdvobotWindow());
		}
	}
}
