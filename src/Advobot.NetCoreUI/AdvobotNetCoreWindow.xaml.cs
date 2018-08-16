using Advobot.NetCoreUI.Classes.ViewModels;
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
			DataContext = new AdvobotNetCoreWindowViewModel();
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
