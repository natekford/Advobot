using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Advobot.UI.Views
{
	public sealed class OutputSearchWindow : Window
	{
		public OutputSearchWindow()
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
}