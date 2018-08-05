using System;
using System.Windows;
using Advobot.Classes;

namespace Advobot.Windows.Classes.Controls
{
	/// <summary>
	/// A window that has an owner.
	/// </summary>
	public class ModalWindow : Window
	{
		/// <summary>
		/// The owning window.
		/// </summary>
		protected Window Window;
		/// <summary>
		/// The low level configuration of the bot.
		/// </summary>
		protected LowLevelConfig Config;

		/// <summary>
		/// Creates an instance of modalwindow.
		/// </summary>
		public ModalWindow() : this(null, null) { }
		/// <summary>
		/// Creates an instance of modalwindow.
		/// </summary>
		/// <param name="mainWindow"></param>
		/// <param name="config"></param>
		public ModalWindow(Window mainWindow, LowLevelConfig config)
		{
			Window = mainWindow ?? throw new ArgumentNullException(nameof(mainWindow));
			Config = config ?? throw new ArgumentNullException(nameof(config));
			Window.Opacity = .25;
		}

		/// <summary>
		/// Set the passed in window as the owner and make its height and width smaller than the owner.
		/// </summary>
		public override void EndInit()
		{
			base.EndInit();
			Owner = Window;
			Height = Window.Height / 2;
			Width = Window.Width / 2;
		}

		/// <summary>
		/// Set the owner window's opacity back to 100.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void WindowClosed(object sender, EventArgs e)
		{
			//Return false if the user has not done something to set this to true (search, etc)
			if (DialogResult == null)
			{
				DialogResult = false;
			}
			Window.Opacity = 100;
		}
		/// <summary>
		/// Close the window.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void Close(object sender, RoutedEventArgs e)
		{
			Close();
		}
	}
}
