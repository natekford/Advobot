using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Advobot.NetCoreUI
{
	public class AdvobotNetCoreWindow : Window
	{
		public AdvobotNetCoreWindow()
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
	}
}