using Advobot.Windows.Classes;
using Advobot.Windows.Windows;
using AdvorangesUtils;
using System;
using System.Windows;

namespace Advobot.Windows
{
	/// <summary>
	/// Interaction logic for AdvobotApp.xaml
	/// </summary>
	public partial class AdvobotApp : Application, IDisposable
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
				MessageBox.Show($"UNHANDLED EXCEPTION:\n\n{dueE.Exception}", "UNHANDLED EXCEPTION", MessageBoxButton.OK, MessageBoxImage.Error);
				IOUtils.LogUncaughtException(dueE.Exception);
				dueE.Handled = true;
				Shutdown();
			};
			AppDomain.CurrentDomain.UnhandledException += (ueSender, ueE) =>
			{
				IOUtils.LogUncaughtException(ueE.ExceptionObject);
			};

			SyntaxHighlighting.LoadJsonHighlighting();
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
