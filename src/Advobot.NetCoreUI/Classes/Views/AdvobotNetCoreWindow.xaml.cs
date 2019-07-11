﻿using System;
using Advobot.NetCoreUI.Classes.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;

namespace Advobot.NetCoreUI.Classes.Views
{
	public sealed class AdvobotNetCoreWindow : Window
	{
		public new AdvobotNetCoreWindowViewModel DataContext
		{
			get => (AdvobotNetCoreWindowViewModel)base.DataContext;
			set
			{
				if (!(value is AdvobotNetCoreWindowViewModel))
				{
					throw new ArgumentException("Invalid data context provided.");
				}
				base.DataContext = value;
			}
		}

		public AdvobotNetCoreWindow()
		{
			//Unless the state is changed after creation the height will be double.NaN
			Activated += (sender, e)
				=> WindowState = WindowState.Maximized;
			Closed += (sender, e)
				=> Environment.Exit(0);

			InitializeComponent();
#if DEBUG
			this.AttachDevTools();
#endif
		}

		private void InitializeComponent()
			=> AvaloniaXamlLoader.Load(this);
		public void EnterKeyPressed(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter || e.Key == Key.Return)
			{
				DataContext.TakeInputCommand.Execute(null);
			}
		}
	}
}