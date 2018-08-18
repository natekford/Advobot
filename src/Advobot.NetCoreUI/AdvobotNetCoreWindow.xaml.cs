using Advobot.NetCoreUI.Classes.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Advobot.NetCoreUI.Utils;

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

		public void EnterKeyPressed(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter || e.Key == Key.Return)
			{
				((AdvobotNetCoreWindowViewModel)DataContext).InputCommand.Execute(null);
			}
		}
	}
}