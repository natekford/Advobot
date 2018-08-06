using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Advobot.Interfaces;
using Advobot.Windows.Classes.Controls;
using Advobot.Windows.Utilities;
using AdvorangesUtils;

namespace Advobot.Windows.Windows
{
	/// <summary>
	/// Interaction logic for OutputSearchWindow.xaml
	/// </summary>
	internal partial class OutputSearchWindow : ModalWindow
	{
		public OutputSearchWindow() : this(null, null) { }
		public OutputSearchWindow(Window mainWindow, ILowLevelConfig config) : base(mainWindow, config)
		{
			InitializeComponent();
			OutputNamesComboBox.ItemsSource = AdvobotComboBox.CreateComboBoxSourceOutOfStrings(ConsoleUtils.WrittenLines.Keys.ToArray());
		}

		private void Search(object sender, RoutedEventArgs e)
		{
			if (OutputNamesComboBox.SelectedItem is TextBox tb)
			{
				ConsoleSearchOutput.Clear();
				foreach (var line in ConsoleUtils.WrittenLines[tb.Text])
				{
					ConsoleSearchOutput.AppendText($"{line}{Environment.NewLine}");
				}
			}
		}
		private void SaveWithCtrlS(object sender, KeyEventArgs e)
		{
			if (SavingUtils.IsCtrlS(e))
			{
				Save(sender, e);
			}
		}
		private void Save(object sender, RoutedEventArgs e)
		{
			if (ConsoleSearchOutput.Text.Length > 0)
			{
				var response = SavingUtils.SaveFile(Config, ConsoleSearchOutput);
				ToolTipUtils.EnableTimedToolTip(Layout, response.GetReason());
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
