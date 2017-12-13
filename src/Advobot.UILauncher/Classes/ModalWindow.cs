using System;
using System.Windows;

namespace Advobot.UILauncher.Classes.Controls
{
	public class ModalWindow : Window
	{
		protected Window _MainWindow;

		public ModalWindow(Window mainWindow)
		{
			_MainWindow = mainWindow ?? throw new ArgumentException($"{nameof(mainWindow)} cannot be null.");
			_MainWindow.Opacity = .25;
		}
		public ModalWindow() : this(null) { }

		//Set the owner so this acts as a modal
		public override void EndInit()
		{
			base.EndInit();
			Owner = _MainWindow;
			Height = _MainWindow.Height / 2;
			Width = _MainWindow.Width / 2;
		}

		protected void WindowClosed(object sender, EventArgs e)
		{
			//Return false if the user has not done something to set this to true (search, etc)
			if (DialogResult == null)
			{
				DialogResult = false;
			}
			_MainWindow.Opacity = 100;
		}
		protected void Close(object sender, RoutedEventArgs e) => Close();
	}
}
