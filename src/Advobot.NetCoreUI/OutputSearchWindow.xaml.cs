using System;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Advobot.NetCoreUI
{
	public class OutputSearchWindow : Window
	{
		public OutputSearchWindow()
		{
			InitializeComponent();
		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}
		private void OnActivated(object sender, EventArgs e)
		{
			Width = 800;
			Height = 600;
		}
	}
}
