using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Advobot.NetCoreUI
{
	public class FileSearchWindow : Window
	{
		public FileSearchWindow()
		{
			this.InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}
	}
}
