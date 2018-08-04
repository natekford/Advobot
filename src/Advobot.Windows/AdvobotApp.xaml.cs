using Advobot.Windows.Classes;
using Advobot.Windows.Windows;
using AdvorangesUtils;
using System;
using System.Threading;
using System.Windows;

namespace Advobot.Windows
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
			DispatcherUnhandledException += (dueSender, dueE) =>
			{
				//Display to the user what happened and also log it
				IOUtils.LogUncaughtException(dueE.Exception);
				MessageBox.Show($"UNHANDLED EXCEPTION:\n\n{dueE.Exception}", "UNHANDLED EXCEPTION", MessageBoxButton.OK, MessageBoxImage.Error);
				dueE.Handled = true;
				Shutdown();
			};
			AppDomain.CurrentDomain.UnhandledException += (ueSender, ueE) =>
			{
				IOUtils.LogUncaughtException(ueE.ExceptionObject);
			};

			SyntaxHighlightingUtils.LoadJsonHighlighting();
			MainWindow = new AdvobotWindow();
			MainWindow.Show();
		}
		/// <inheritdoc />
		public void Dispose()
		{
			_Listener.Dispose();
		}
	}
}
