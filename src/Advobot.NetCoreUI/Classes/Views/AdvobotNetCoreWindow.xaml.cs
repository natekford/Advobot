using System;
using Advobot.NetCoreUI.Classes.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;

namespace Advobot.NetCoreUI.Classes.Views
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

		private void EnterKeyPressed(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter || e.Key == Key.Return)
			{
				((AdvobotNetCoreWindowViewModel)DataContext).TakeInputCommand.Execute(null);
			}
		}
		private void OnActivated(object sender, EventArgs e)
		{
			//Because unless the state is changed directly after creation the height will be double.NaN
			WindowState = WindowState.Maximized;
		}
		private void OnClosed(object sender, EventArgs e)
		{
			Environment.Exit(0);
		}
	}
}