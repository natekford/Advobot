using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Advobot.NetCoreUI.Classes.Views
{
	public sealed class FileViewingWindow : Window
	{
		public FileViewingWindow()
		{
			InitializeComponent();
#if DEBUG
			this.AttachDevTools();
#endif
		}

		private void InitializeComponent()
			=> AvaloniaXamlLoader.Load(this);
		//TODO: make modal window class that implements this since it's shared with OutputSearchWindow
		private void OnActivated(object sender, EventArgs e)
		{
			//Resize so dynamic font works
			Width = 800;
			Height = 600;
		}
	}
}
