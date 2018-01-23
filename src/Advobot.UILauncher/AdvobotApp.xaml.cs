using System;
using System.Windows;
using System.Windows.Threading;
using Advobot.Core.Utilities;
using Advobot.UILauncher.Classes;
using Advobot.UILauncher.Windows;

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
		private BindingListener _Listener = new BindingListener();

		public AdvobotApp()
		{
			InitializeComponent();
		}

		public void OnStartup(object sender, StartupEventArgs e)
		{
			DispatcherUnhandledException += OnDispatcherUnhandledException;
			AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

			SyntaxHighlighting.LoadJsonHighlighting();
			MainWindow = new AdvobotWindow();
			MainWindow.Show();
		}
		private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
		{
			//Display to the user what happened and also log it
			MessageBox.Show($"UNHANDLED EXCEPTION:\n\n{e.Exception}", "UNHANDLED EXCEPTION", MessageBoxButton.OK, MessageBoxImage.Error);
			IOUtils.LogUncaughtException(e.Exception);
			e.Handled = true;
			Shutdown();
		}
		private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			IOUtils.LogUncaughtException(e.ExceptionObject);
		}
	}
}
