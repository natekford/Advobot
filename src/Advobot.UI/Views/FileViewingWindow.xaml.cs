using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Advobot.UI.Views;

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
	}

	private void InitializeComponent()
		=> AvaloniaXamlLoader.Load(this);
}