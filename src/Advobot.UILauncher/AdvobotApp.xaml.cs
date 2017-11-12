using Advobot.Core.Actions;
using Advobot.UILauncher.Classes;
using Advobot.UILauncher.Windows;
using System;
using System.Windows;
using System.Windows.Threading;

namespace Advobot.UILauncher
{
	/// <summary>
	/// Interaction logic for AdvobotApp.xaml
	/// </summary>
	public partial class AdvobotApp : Application
	{
		public AdvobotApp()
		{
			InitializeComponent();
		}

		public void OnStartup(object sender, StartupEventArgs e)
		{
			this.DispatcherUnhandledException += this.OnDispatcherUnHandledException;
			AppDomain.CurrentDomain.UnhandledException += this.OnUnhandledException;

			SyntaxHighlighting.LoadJSONHighlighting();
			this.MainWindow = new AdvobotWindow();
			this.MainWindow.Show();
		}
		private void OnDispatcherUnHandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
		{
			//Display to the user what happened and also log it
			MessageBox.Show($"UNHANDLED EXCEPTION:\n\n{e.Exception.ToString()}", "UNHANDLED EXCEPTION", MessageBoxButton.OK, MessageBoxImage.Error);
			SavingAndLoadingActions.LogUncaughtException(e.Exception);
			e.Handled = true;
			this.Shutdown();
		}
		private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
			=> SavingAndLoadingActions.LogUncaughtException(e.ExceptionObject);
	}
}
