using Advobot.Core.Actions;
using Advobot.UILauncher.Classes.Controls;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Advobot.UILauncher.Classes
{
	public partial class Test : Window
	{
		private void OpenOutputSearch(object sender, RoutedEventArgs e)
		{
			_OutputSearchComboBox.ItemsSource = 
				AdvobotComboBox.CreateComboBoxSourceOutOfStrings(ConsoleActions.GetWrittenLines().Keys.ToArray());
		}
		private void CloseOutputSearch(object sender, RoutedEventArgs e)
		{
			_OutputSearchComboBox.SelectedItem = null;
			_OutputSearchResults.Text = null;
		}
		private void SearchOutput(object sender, RoutedEventArgs e)
		{
			var selectedItem = (TextBox)_OutputSearchComboBox.SelectedItem;
			if (selectedItem != null)
			{
				_OutputSearchResults.Text = null;
				ConsoleActions.GetWrittenLines()[selectedItem.Text].ForEach(x => 
				_OutputSearchResults.AppendText(x + Environment.NewLine));
			}
		}

		private readonly TextBox _OutputSearchResults = new AdvobotTextBox
		{
			VerticalScrollBarVisibility = ScrollBarVisibility.Visible,
			IsReadOnly = true,
		};
		private readonly ComboBox _OutputSearchComboBox = new AdvobotComboBox
		{
			IsEditable = true,
			FontResizeValue = .022,
		};
		private readonly Button _OutputSearchButton = new AdvobotButton
		{
			Content = "Search",
		};
		private readonly Button _OutputSearchCloseButton = new AdvobotButton
		{
			Content = "Close",
		};
	}
}
