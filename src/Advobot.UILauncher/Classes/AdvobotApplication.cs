using Advobot.Core.Actions;
using Advobot.UILauncher.Windows;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using System;
using System.Reflection;
using System.Windows;
using System.Xml;

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
			//Add in the JSON highlighter for the file output
			using (var s = Assembly.GetExecutingAssembly().GetManifestResourceStream("Advobot.UILauncher.Resources.JSONSyntaxHighlighting.xshd"))
			{
				if (s == null)
				{
					throw new InvalidOperationException("JSONSyntaxHighlighting is missing.");
				}

				using (var r = new XmlTextReader(s))
				{
					var highlighting = HighlightingLoader.Load(r, HighlightingManager.Instance);
					HighlightingManager.Inst‌​ance.RegisterHighlighting("JSON", new[] { ".json" }, highlighting);
				}
			}

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
