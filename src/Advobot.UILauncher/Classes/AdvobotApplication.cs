using Advobot.Core.Actions;
using Advobot.UILauncher.Windows;
using System;
using System.Windows;

namespace Advobot.UILauncher.Classes
{
	/// <summary>
	/// Creates a new application with <see cref="AdvobotWindow"/> as the main window.
	/// </summary>
	public sealed class AdvobotApplication : Application
	{
		/// <summary>
		/// Sets <see cref="Application.MainWindow"/> as <see cref="AdvobotWindow"/>,
		/// sets <see cref="Application.Resources"/> as ApplicationResources.xaml,
		/// and displays to the user and logs whenever a dispatcher unhandled exception occurs.
		/// </summary>
		public AdvobotApplication()
		{
			this.MainWindow = new AdvobotWindow();
			this.Resources = new ResourceDictionary
			{
				Source = new Uri("/Advobot.UILauncher;component/Resources/ApplicationResources.xaml", UriKind.RelativeOrAbsolute),
			};
			//Display to the user what happened and also log it
			this.DispatcherUnhandledException += (sender, e) =>
			{
				MessageBox.Show($"UNHANDLED EXCEPTION:\n\n{e.Exception.ToString()}", "UNHANDLED EXCEPTION", MessageBoxButton.OK, MessageBoxImage.Error);
				SavingAndLoadingActions.LogUncaughtException(e.Exception);
				e.Handled = true;
				this.Shutdown();
			};
		}

		/// <summary>
		/// Starts the application with the main window.
		/// </summary>
		public new void Run()
		{
			base.Run(this.MainWindow);
		}
	}
}
