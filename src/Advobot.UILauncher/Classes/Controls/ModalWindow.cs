using System;
using System.Windows;

namespace Advobot.UILauncher.Classes.Controls
{
	public class ModalWindow : Window
	{
		private Window _MainWindow;

		public ModalWindow(Window mainWindow)
		{
			if (mainWindow != null)
			{
				_MainWindow = mainWindow;
			}
		}
		public ModalWindow() { }

		//Set the owner so this acts as a modal
		public override void EndInit()
		{
			base.EndInit();
			this.Owner = _MainWindow;
			this.Height = _MainWindow.Height / 2;
			this.Width = _MainWindow.Width / 2;
		}

		protected void WindowClosed(object sender, EventArgs e)
		{
			//Return false if the user has not done something to set this to true (search, etc)
			if (this.DialogResult == null)
			{
				this.DialogResult = false;
			}
		}
		protected void Close(object sender, RoutedEventArgs e)
		{
			this.Close();
		}
	}
}
