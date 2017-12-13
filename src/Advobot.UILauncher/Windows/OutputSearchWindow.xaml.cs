using Advobot.Core.Actions;
using Advobot.UILauncher.Actions;
using Advobot.UILauncher.Classes.Controls;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Advobot.UILauncher.Windows
{
	/// <summary>
	/// Interaction logic for OutputSearchWindow.xaml
	/// </summary>
	internal partial class OutputSearchWindow : ModalWindow
	{
		public OutputSearchWindow() : this(null) { }
		public OutputSearchWindow(Window mainWindow) : base(mainWindow)
		{
			InitializeComponent();
			OutputNamesComboBox.ItemsSource = AdvobotComboBox.CreateComboBoxSourceOutOfStrings(ConsoleActions.GetOrCreateWrittenLines().Keys.ToArray());
		}

		private void Search(object sender, RoutedEventArgs e)
		{
			if (OutputNamesComboBox.SelectedItem is TextBox tb)
			{
				ConsoleSearchOutput.Clear();
				foreach (var line in ConsoleActions.GetOrCreateWrittenLines()[tb.Text])
				{
					ConsoleSearchOutput.AppendText($"{line}{Environment.NewLine}");
				}
			}
		}
		private void SaveWithCtrlS(object sender, KeyEventArgs e)
		{
			if (SavingActions.IsCtrlS(e))
			{
				Save(sender, e);
			}
		}
		private void Save(object sender, RoutedEventArgs e)
		{
			if (ConsoleSearchOutput.Text.Length > 0)
			{
				var response = SavingActions.SaveFile(ConsoleSearchOutput);
				ToolTipActions.EnableTimedToolTip(Layout, response.GetReason());
			}
		}
		private void MoveToolTip(object sender, MouseEventArgs e)
		{
			if (!(sender is FrameworkElement fe) || !(fe.ToolTip is ToolTip tt))
			{
				return;
			}

			var pos = e.GetPosition(fe);
			tt.HorizontalOffset = pos.X + 10;
			tt.VerticalOffset = pos.Y + 10;
		}
	}
}
