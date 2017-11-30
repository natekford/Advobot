using Advobot.Core.Actions;
using Advobot.UILauncher.Classes;
using Advobot.UILauncher.Windows;
using System;
using System.Windows;
using System.Windows.Threading;

//Note about System.Net.Http: upgrading the discord library requires this assembly to be added to references.
//Otherwise the wrapper fails to install and uninstalls itself because of that.
//It's removed from this project because on both computers I work on it gives a warning saying that the reference
//to System.Net.Http "could not be found." However, this project compiles completely fine with or without it.
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
