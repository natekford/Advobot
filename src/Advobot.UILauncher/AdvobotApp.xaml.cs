using Advobot.UILauncher.Classes;
using Advobot.UILauncher.Windows;
using AdvorangesUtils;
using System;
using System.Windows;

namespace Advobot.UILauncher
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

		#region IDisposable Support
		private bool _Disposed = false; // To detect redundant calls

		/// <summary>
		/// Disposes the object.
		/// </summary>
		/// <param name="disposing"></param>
		protected virtual void Dispose(bool disposing)
		{
			if (!_Disposed)
			{
				if (disposing)
				{
					_Listener?.Dispose();
				}

				_Disposed = true;
			}
		}

		/// <summary>
		/// Disposes the object through its finalizer.
		/// </summary>
		~AdvobotApp()
		{
			Dispose(false);
		}

		/// <summary>
		/// Disposes the object and suppressed finalize.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
