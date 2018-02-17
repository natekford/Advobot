using System;
using System.Windows;
using System.Windows.Threading;
using Advobot.Core.Utilities;
using Advobot.UILauncher.Classes;
using Advobot.UILauncher.Windows;

namespace Advobot.UILauncher
{
	/// <summary>
	/// Interaction logic for AdvobotApp.xaml
	/// </summary>
	public partial class AdvobotApp : Application, IDisposable
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

		#region IDisposable Support
		private bool disposedValue = false; // To detect redundant calls

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					_Listener.Dispose();
				}

				_Listener = null;
				disposedValue = true;
			}
		}

		~AdvobotApp()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
