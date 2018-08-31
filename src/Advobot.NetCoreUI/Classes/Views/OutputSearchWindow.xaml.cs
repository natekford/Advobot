using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Advobot.NetCoreUI.Classes.Views
{
	public class OutputSearchWindow : Window
	{
		public OutputSearchWindow()
		{
			InitializeComponent();
#if DEBUG
			this.AttachDevTools();
#endif
		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}
		private void OnActivated(object sender, EventArgs e)
		{
			//Resize so dynamic font works
			Width = 800;
			Height = 600;
		}
	}
}
