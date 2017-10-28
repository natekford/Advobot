using System;
using System.Windows;
using System.Windows.Threading;

namespace Advobot.UILauncher
{
	public sealed class AdvobotApplication : Application
	{
		public AdvobotApplication()
		{
			this.DispatcherUnhandledException += new DispatcherUnhandledExceptionEventHandler(this.OnAppDispatcherUnhandledException);
			this.MainWindow = new AdvobotWindow();
		}

		private void OnAppDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
		{
			MessageBox.Show($"UNHANDLED EXCEPTION:\n\n{e.Exception.GetType().Name}\n\n{e.Exception.Message}");
			e.Handled = true;
			this.Shutdown();
		}
	}
}
