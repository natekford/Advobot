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
			Activated += (sender, e) =>
			{
				//Resize so dynamic font works
				Width = 800;
				Height = 600;
			};

			InitializeComponent();
#if DEBUG
			this.AttachDevTools();
#endif
		}

		private void InitializeComponent()
			=> AvaloniaXamlLoader.Load(this);
	}
}
