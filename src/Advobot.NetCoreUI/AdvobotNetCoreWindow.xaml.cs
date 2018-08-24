using System;
using Advobot.NetCoreUI.Classes.ViewModels;
using Advobot.NetCoreUI.Utils;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;

namespace Advobot.NetCoreUI
{
	public class AdvobotNetCoreWindow : Window
	{
		public AdvobotNetCoreWindow()
		{
			InitializeComponent();
		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}

		private void EnterKeyPressed(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter || e.Key == Key.Return)
			{
				((AdvobotNetCoreWindowViewModel)DataContext).InputCommand.Execute(null);
			}
		}
		private void OnActivated(object sender, EventArgs e)
		{
			//Because unless the state is changed directly after creation the height will be double.NaN
			WindowState = WindowState.Maximized;
		}
	}
}