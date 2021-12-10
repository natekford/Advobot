using Advobot.UI.ViewModels;

using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;

namespace Advobot.UI.Views;

public sealed class AdvobotNetCoreWindow : Window
{
	public new AdvobotNetCoreWindowViewModel DataContext
	{
		get => (AdvobotNetCoreWindowViewModel)base.DataContext!;
		set
		{
			if (value is null)
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
	}

	public void EnterKeyPressed(object sender, KeyEventArgs e)
	{
		if (e.Key is Key.Enter or Key.Return)
		{
			DataContext.TakeInputCommand.Execute(null);
		}
	}

	private void InitializeComponent()
		=> AvaloniaXamlLoader.Load(this);
}