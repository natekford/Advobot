using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using Advobot.Classes;
using Advobot.NetFrameworkUI.Classes;
using Advobot.NetFrameworkUI.Windows;
using AdvorangesUtils;

namespace Advobot.NetFrameworkUI
{
	/// <summary>
	/// Interaction logic for AdvobotApp.xaml
	/// </summary>
	public sealed partial class AdvobotApp : Application, IDisposable
	{
		private BindingListener _Listener = new BindingListener();

		/// <summary>
		/// Creates an instance of advobotapp.
		/// </summary>
		public AdvobotApp()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Creates the main window.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public void OnStartup(object sender, StartupEventArgs e)
		{
			DispatcherUnhandledException += (dueSender, dueE) => LogException(dueE.Exception, dueE);
			AppDomain.CurrentDomain.UnhandledException += (ueSender, ueE) => LogException(ueE.ExceptionObject, ueE);

			var config = LowLevelConfig.Load(e.Args);
			//Wait until the old process is killed
			if (config.PreviousProcessId != -1)
			{
				try
				{
					while (Process.GetProcessById(config.PreviousProcessId) != null)
					{
						Thread.Sleep(25);
					}
				}
				catch (ArgumentException) { }
			}

			SyntaxHighlightingUtils.LoadJsonHighlighting();
			//Make sure it's restarted with the correct instance number for config reasons
			MainWindow = new AdvobotWindow(config);
			MainWindow.Show();
			ConsoleUtils.DebugWrite($"Args: {config.CurrentInstance}|{config.PreviousProcessId}");
		}
		private void LogException(object exception, EventArgs e)
		{
			//Display to the user what happened and also log it
			IOUtils.LogUncaughtException(exception);
			MessageBox.Show($"UNHANDLED EXCEPTION:\n\n{exception}", "UNHANDLED EXCEPTION", MessageBoxButton.OK, MessageBoxImage.Error);
			Shutdown();
		}
		/// <inheritdoc />
		public void Dispose()
		{
			_Listener.Dispose();
		}
	}
}
