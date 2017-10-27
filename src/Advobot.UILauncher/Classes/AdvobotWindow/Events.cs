using Advobot.Core;
using Advobot.Core.Actions;
using Advobot.Core.Interfaces;
using Advobot.UILauncher.Actions;
using Advobot.UILauncher.Enums;
using Discord;
using ICSharpCode.AvalonEdit;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Advobot.UILauncher.Classes.AdvobotWindow
{
	public partial class AdvobotWindow : Window
	{
		private void OpenOutputSearch(object sender, RoutedEventArgs e)
		{
			_OutputSearchComboBox.ItemsSource = AdvobotComboBox.CreateComboBoxSourceOutOfStrings(ConsoleActions.GetWrittenLines().Keys.ToArray());
			_OutputSearchLayout.Visibility = Visibility.Visible;
		}
		private void CloseOutputSearch(object sender, RoutedEventArgs e)
		{
			_OutputSearchComboBox.SelectedItem = null;
			_OutputSearchResults.Text = null;
			_OutputSearchLayout.Visibility = Visibility.Collapsed;
		}
		private void SearchOutput(object sender, RoutedEventArgs e)
		{
			var selectedItem = (TextBox)_OutputSearchComboBox.SelectedItem;
			if (selectedItem != null)
			{
				_OutputSearchResults.Text = null;
				ConsoleActions.GetWrittenLines()[selectedItem.Text].ForEach(x => _OutputSearchResults.AppendText(x + Environment.NewLine));
			}
		}

		private void OpenFileSearch(object sender, RoutedEventArgs e)
		{
			_GuildSearchLayout.Visibility = Visibility.Visible;
		}
		private void CloseFileSearch(object sender, RoutedEventArgs e)
		{
			_GuildSearchFileComboBox.SelectedItem = null;
			_GuildSearchNameInput.Text = null;
			_GuildSearchIDInput.Text = null;
			_GuildSearchLayout.Visibility = Visibility.Collapsed;
		}
		private void SearchForFile(object sender, RoutedEventArgs e)
		{
			/*
			var tb = (TextBox)_GuildSearchFileComboBox.SelectedItem;
			if (tb == null)
				return;

			var nameStr = _GuildSearchNameInput.Text;
			var idStr = _GuildSearchIDInput.Text;
			if (String.IsNullOrWhiteSpace(nameStr) && String.IsNullOrWhiteSpace(idStr))
				return;

			var fileType = (FileType)tb.Tag;
			CloseFileSearch(sender, e);

			TreeViewItem guild = null;
			if (!String.IsNullOrWhiteSpace(idStr))
			{
				if (!ulong.TryParse(idStr, out ulong guildID))
				{
					ConsoleActions.WriteLine($"The ID '{idStr}' is not a valid number.");
					return;
				}

				guild = _FileTreeView.Items.Cast<TreeViewItem>().FirstOrDefault(x => ((GuildInformation)x.Tag).Id == guildID);
				if (guild == null)
				{
					ConsoleActions.WriteLine($"No guild could be found with the ID '{guildID}'.");
					return;
				}
			}
			else if (!String.IsNullOrWhiteSpace(nameStr))
			{
				var guilds = _FileTreeView.Items.Cast<TreeViewItem>().Where(x => ((GuildInformation)x.Tag).Name.CaseInsEquals(nameStr));
				if (guilds.Count() == 0)
				{
					ConsoleActions.WriteLine($"No guild could be found with the name '{nameStr}'.");
					return;
				}
				else if (guilds.Count() == 1)
				{
					guild = guilds.FirstOrDefault();
				}
				else
				{
					ConsoleActions.WriteLine($"More than one guild has the name '{nameStr}'.");
					return;
				}
			}

			if (guild != null)
			{
				var item = guild.Items.Cast<TreeViewItem>().FirstOrDefault(x => ((FileInformation)x.Tag).FileType == fileType);
				if (item != null)
				{
					OpenSpecificFileLayout(item, e);
				}
			}*/
		}
	}
}
