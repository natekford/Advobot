using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Advobot.UILauncher.Enums;
using Advobot.UILauncher.Classes;
using Advobot.UILauncher.Actions;
using System.Windows.Threading;
using Advobot.Core.Interfaces;
using Discord;

namespace Advobot.UILauncher
{
	/// <summary>
	/// Interaction logic for AdvobotWindow.xaml
	/// </summary>
	public partial class AdvobotWindow : Window
	{
		private IDiscordClient _Client;
		private IBotSettings _BotSettings;
		private ILogService _Logging;
		private ColorSettings _UISettings;
		private readonly DispatcherTimer _UpdateTimer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 0, 0, 500) };

		public AdvobotWindow()
		{
			FontFamily = new FontFamily("Courier New");
			InitializeComponent();

			//Console.SetOut(new TextBoxStreamWriter(_Output));

			new ColorSettings().ActivateTheme();
			ColorSettings.SwitchElementColorOfChildren(this.Content as FrameworkElement);

			//Loaded += AttemptToLogin;
			//_UpdateTimer.Tick += UpdateMenus;
		}

		private MenuType _LastButtonClicked;
		private async void OpenMenu(object sender, RoutedEventArgs e)
		{
			if (!(sender is Button button))
			{
				return;
			}

			var parent = this.Content as FrameworkElement;
			var children = parent.GetChildren();

			var output       = (AdvobotTextEditor)parent.FindName("Output");
			var mainMenu     = (Grid)children.SingleOrDefault(x => x?.Tag is MenuType m && m == MenuType.Main);
			var infoMenu     = (Grid)children.SingleOrDefault(x => x?.Tag is MenuType m && m == MenuType.Info);
			var settingsMenu = (Grid)children.SingleOrDefault(x => x?.Tag is MenuType m && m == MenuType.Settings);
			var colorsMenu   = (Grid)children.SingleOrDefault(x => x?.Tag is MenuType m && m == MenuType.Colors);
			var fileMenu     = (Grid)children.SingleOrDefault(x => x?.Tag is MenuType m && m == MenuType.Files);

			//Hide everything so stuff doesn't overlap
			mainMenu.Visibility = Visibility.Collapsed;
			settingsMenu.Visibility = Visibility.Collapsed;
			colorsMenu.Visibility = Visibility.Collapsed;
			infoMenu.Visibility = Visibility.Collapsed;
			fileMenu.Visibility = Visibility.Collapsed;

			var currentColumn = Grid.GetColumn(output);
			var currentColumnSpan = Grid.GetColumnSpan(output);

			//If clicking the same button then resize the output window to the regular size
			var type = button.Tag as MenuType? ?? default;
			if (type == _LastButtonClicked)
			{
				UIModification.SetColAndSpan(output, currentColumn, currentColumnSpan + 1);
				_LastButtonClicked = default;
			}
			else
			{
				//Resize the regular output window and have the menubox appear
				UIModification.SetColAndSpan(output, currentColumn, currentColumnSpan - 1);
				_LastButtonClicked = type;

				switch (type)
				{
					case MenuType.Main:
					{
						mainMenu.Visibility = Visibility.Visible;
						return;
					}
					case MenuType.Info:
					{
						infoMenu.Visibility = Visibility.Visible;
						return;
					}
					case MenuType.Settings:
					{
						/*
						((CheckBox)((Viewbox)_DownloadUsersSetting.Setting).Child).IsChecked = _BotSettings.AlwaysDownloadUsers;
						((TextBox)_PrefixSetting.Setting).Text = _BotSettings.Prefix;
						((TextBox)_GameSetting.Setting).Text = _BotSettings.Game;
						((TextBox)_StreamSetting.Setting).Text = _BotSettings.Stream;
						((TextBox)_ShardSetting.Setting).Text = _BotSettings.ShardCount.ToString();
						((TextBox)_MessageCacheSetting.Setting).Text = _BotSettings.MessageCacheCount.ToString();
						((TextBox)_UserGatherCountSetting.Setting).Text = _BotSettings.MaxUserGatherCount.ToString();
						((TextBox)_MessageGatherSizeSetting.Setting).Text = _BotSettings.MaxMessageGatherSize.ToString();
						((ComboBox)_LogLevelComboBox.Setting).SelectedItem = ((ComboBox)_LogLevelComboBox.Setting).Items.OfType<TextBox>().FirstOrDefault(x => (LogSeverity)x.Tag == _BotSettings.LogLevel);
						_TrustedUsersComboBox.ItemsSource = await Task.WhenAll(_BotSettings.TrustedUsers.Select(async x => AdvobotTextBox.CreateUserBox(await _Client.GetUserAsync(x))));*/
						settingsMenu.Visibility = Visibility.Visible;
						return;
					}
					case MenuType.Colors:
					{
						/*
						UIModification.MakeColorDisplayer(_UISettings, _ColorsLayout, _ColorsSaveButton, .018);*/
						colorsMenu.Visibility = Visibility.Visible;
						return;
					}
					case MenuType.Files:
					{
						/*
						var treeView = UIModification.MakeGuildTreeView(_FileTreeView, await _Client.GetGuildsAsync());
						foreach (var item in treeView.Items.Cast<TreeViewItem>().SelectMany(x => x.Items.Cast<TreeViewItem>()))
						{
							item.MouseDoubleClick += OpenSpecificFileLayout;
						}
						_FileOutput.Document = new FlowDocument(new Paragraph(new InlineUIContainer(treeView)));*/
						fileMenu.Visibility = Visibility.Visible;
						return;
					}
				}
			}
		}
	}
}
