using Advobot.UILauncher.Windows;
using System;
using System.Windows;

namespace Advobot.UILauncher.Classes
{
	public sealed class AdvobotApplication : Application
	{
		public AdvobotApplication()
		{
			this.MainWindow = new AdvobotWindow();
			this.Resources = new ResourceDictionary
			{
				Source = new Uri("/Advobot.UILauncher;component/Resources/ApplicationResources.xaml", UriKind.RelativeOrAbsolute),
			};
			this.DispatcherUnhandledException += (sender, e) =>
			{
				MessageBox.Show($"UNHANDLED EXCEPTION:\n\n{e.Exception.ToString()}", "UNHANDLED EXCEPTION", MessageBoxButton.OK, MessageBoxImage.Error);
				e.Handled = true;
				this.Shutdown();
			};
		}

		public new void Run()
		{
			base.Run(this.MainWindow);
		}
	}
}
