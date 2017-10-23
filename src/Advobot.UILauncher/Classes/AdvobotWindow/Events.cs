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
		private async void AttemptToLogin(object sender, RoutedEventArgs e)
		{
			await HandleInput(null);
			await HandleInput(null);
		}
		private async void UpdateMenus(object sender, EventArgs e)
		{
			var guilds = await _Client.GetGuildsAsync();
			var users = await Task.WhenAll(guilds.Select(async g => await g.GetUsersAsync()));

			((TextBox)_Latency.Child).Text = $"Latency: {ClientActions.GetLatency(_Client)}ms";
			((TextBox)_Memory.Child).Text = $"Memory: {GetActions.GetMemory().ToString("0.00")}MB";
			((TextBox)_Threads.Child).Text = $"Threads: {Process.GetCurrentProcess().Threads.Count}";
			((TextBox)_Guilds.Child).Text = $"Guilds: {guilds.Count}";
			((TextBox)_Users.Child).Text = $"Members: {users.SelectMany(x => x).Select(x => x.Id).Distinct().Count()}";
			_InfoOutput.Document = UIModification.MakeInfoMenu(_Logging);
		}
		private void Pause(object sender, RoutedEventArgs e)
		{
			UIBotWindowLogic.PauseBot(_BotSettings);
		}
		private void Restart(object sender, RoutedEventArgs e)
		{
			switch (MessageBox.Show("Are you sure you want to restart the bot?", Constants.PROGRAM_NAME, MessageBoxButton.OKCancel))
			{
				case MessageBoxResult.OK:
				{
					ClientActions.RestartBot();
					return;
				}
			}
		}
		private void Disconnect(object sender, RoutedEventArgs e)
		{
			switch (MessageBox.Show("Are you sure you want to disconnect the bot?", Constants.PROGRAM_NAME, MessageBoxButton.OKCancel))
			{
				case MessageBoxResult.OK:
				{
					ClientActions.DisconnectBot(_Client);
					return;
				}
			}
		}

		private async void SaveSettings(object sender, RoutedEventArgs e)
		{
			await SettingModification.SaveSettings(_SettingsLayout, _Client, _BotSettings);
		}
		private void SaveColors(object sender, RoutedEventArgs e)
		{
			foreach (var child in _ColorsLayout.GetChildren())
			{
				if (child is AdvobotTextBox tb && tb.Tag is ColorTarget target)
				{
					var childText = tb.Text;
					if (String.IsNullOrWhiteSpace(childText))
					{
						continue;
					}
					else if (!childText.StartsWith("#"))
					{
						childText = "#" + childText;
					}

					Brush brush = null;
					try
					{
						brush = UIModification.MakeBrush(childText);
					}
					catch
					{
						ConsoleActions.WriteLine($"Invalid color supplied for {target.EnumName()}.");
						continue;
					}

					_UISettings.ColorTargets[target] = brush;
					tb.Text = UIModification.FormatBrush(brush);
					ConsoleActions.WriteLine($"Successfully updated the color for {target.EnumName()}.");
				}
				else if (child is ComboBox cb && cb.SelectedItem is AdvobotTextBox tb2 && tb2.Tag is ColorTheme theme)
				{
					_UISettings.SetTheme(theme);
					ConsoleActions.WriteLine("Successfully updated the theme type.");
				}
			}

			_UISettings.SaveSettings();
			_UISettings.ActivateTheme();
			ColorSettings.SwitchElementColorOfChildren(_Layout);
		}
		private async void AddTrustedUser(object sender, RoutedEventArgs e)
		{
			await SettingModification.AddTrustedUserToComboBox(_TrustedUsersComboBox, _Client, _TrustedUsersAddBox.Text);
			_TrustedUsersAddBox.Text = null;
		}
		private void RemoveTrustedUser(object sender, RoutedEventArgs e)
		{
			SettingModification.RemoveTrustedUserFromComboBox(_TrustedUsersComboBox);
		}

		private async void AcceptInput(object sender, KeyEventArgs e)
		{
			var text = _Input.Text;
			if (String.IsNullOrWhiteSpace(text))
			{
				_InputButton.IsEnabled = false;
				return;
			}

			if (e.Key.Equals(Key.Enter) || e.Key.Equals(Key.Return))
			{
				await HandleInput(UICommandHandler.GatherInput(_Input, _InputButton));
			}
			else
			{
				_InputButton.IsEnabled = true;
			}
		}
		private async void AcceptInput(object sender, RoutedEventArgs e)
		{
			await HandleInput(UICommandHandler.GatherInput(_Input, _InputButton));
		}

		private async void SaveOutput(object sender, RoutedEventArgs e)
		{
			await UIModification.MakeFollowingToolTip(_Layout, _ToolTip, UIBotWindowLogic.SaveOutput(_Output).GetReason());
		}
		private void ClearOutput(object sender, RoutedEventArgs e)
		{
			switch (MessageBox.Show("Are you sure you want to clear the output window?", Constants.PROGRAM_NAME, MessageBoxButton.OKCancel))
			{
				case MessageBoxResult.OK:
				{
					_Output.Text = null;
					return;
				}
			}
		}

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

				guild = _FileTreeView.Items.Cast<TreeViewItem>().FirstOrDefault(x => ((GuildFileInformation)x.Tag).Id == guildID);
				if (guild == null)
				{
					ConsoleActions.WriteLine($"No guild could be found with the ID '{guildID}'.");
					return;
				}
			}
			else if (!String.IsNullOrWhiteSpace(nameStr))
			{
				var guilds = _FileTreeView.Items.Cast<TreeViewItem>().Where(x => ((GuildFileInformation)x.Tag).Name.CaseInsEquals(nameStr));
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
			}
		}

		private void OpenSpecificFileLayout(object sender, RoutedEventArgs e)
		{
			if (UIModification.AppendTextToTextEditorIfPathExists(_SpecificFileDisplay, (TreeViewItem)sender))
			{
				UIModification.SetRowAndSpan(_FileLayout, 0, 100);
				_SpecificFileLayout.Visibility = Visibility.Visible;
				_FileSearchButton.Visibility = Visibility.Collapsed;
			}
		}
		private void CloseSpecificFileLayout(object sender, RoutedEventArgs e)
		{
			switch (MessageBox.Show("Are you sure you want to close the edit window?", Constants.PROGRAM_NAME, MessageBoxButton.OKCancel))
			{
				case MessageBoxResult.OK:
				{
					UIModification.SetRowAndSpan(_FileLayout, 0, 87);
					_SpecificFileDisplay.Tag = null;
					_SpecificFileLayout.Visibility = Visibility.Collapsed;
					_FileSearchButton.Visibility = Visibility.Visible;
					return;
				}
			}
		}
		private async void SaveSpecificFile(object sender, RoutedEventArgs e)
		{
			await UIModification.MakeFollowingToolTip(_Layout, _ToolTip, UIBotWindowLogic.SaveFile(_SpecificFileDisplay).GetReason());
		}

		private async void OpenMenu(object sender, RoutedEventArgs e)
		{
			if (!_StartUp || !(sender is Button button))
			{
				return;
			}

			//Hide everything so stuff doesn't overlap
			_MainMenuLayout.Visibility = Visibility.Collapsed;
			_SettingsLayout.Visibility = Visibility.Collapsed;
			_ColorsLayout.Visibility = Visibility.Collapsed;
			_InfoLayout.Visibility = Visibility.Collapsed;
			_FileLayout.Visibility = Visibility.Collapsed;

			//If clicking the same button then resize the output window to the regular size
			var type = button.Tag as MenuType? ?? default;
			if (type == _LastButtonClicked)
			{
				UIModification.SetColAndSpan(_Output, 0, 4);
				_LastButtonClicked = default;
			}
			else
			{
				//Resize the regular output window and have the menubox appear
				UIModification.SetColAndSpan(_Output, 0, 3);
				_LastButtonClicked = type;

				switch (type)
				{
					case MenuType.Main:
					{
						_MainMenuLayout.Visibility = Visibility.Visible;
						return;
					}
					case MenuType.Info:
					{
						_InfoLayout.Visibility = Visibility.Visible;
						return;
					}
					case MenuType.Settings:
					{
						await UpdateSettingsWhenOpened();
						_SettingsLayout.Visibility = Visibility.Visible;
						return;
					}
					case MenuType.Colors:
					{
						UIModification.MakeColorDisplayer(_UISettings, _ColorsLayout, _ColorsSaveButton, .018);
						_ColorsLayout.Visibility = Visibility.Visible;
						return;
					}
					case MenuType.Files:
					{
						var treeView = UIModification.MakeGuildTreeView(_FileTreeView, await _Client.GetGuildsAsync());
						foreach (var item in treeView.Items.Cast<TreeViewItem>().SelectMany(x => x.Items.Cast<TreeViewItem>()))
						{
							item.MouseDoubleClick += OpenSpecificFileLayout;
						}
						_FileOutput.Document = new FlowDocument(new Paragraph(new InlineUIContainer(treeView)));
						_FileLayout.Visibility = Visibility.Visible;
						return;
					}
				}
			}
		}
	}
}
