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
using System.Windows.Navigation;
using System.Diagnostics;

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

			//Hide everything so stuff doesn't overlap
			this.MainMenu.Visibility = Visibility.Collapsed;
			this.SettingsMenu.Visibility = Visibility.Collapsed;
			this.ColorsMenu.Visibility = Visibility.Collapsed;
			this.InfoMenu.Visibility = Visibility.Collapsed;
			this.FilesMenu.Visibility = Visibility.Collapsed;

			var currentColumn = Grid.GetColumn(this.Output);
			var currentColumnSpan = Grid.GetColumnSpan(this.Output);

			//If clicking the same button then resize the output window to the regular size
			var type = button.Tag as MenuType? ?? default;
			if (type == _LastButtonClicked)
			{
				UIModification.SetColAndSpan(this.Output, currentColumn, currentColumnSpan + 1);
				_LastButtonClicked = default;
			}
			else
			{
				//Resize the regular output window and have the menubox appear
				UIModification.SetColAndSpan(this.Output, currentColumn, currentColumnSpan - 1);
				_LastButtonClicked = type;

				switch (type)
				{
					case MenuType.Main:
					{
						this.MainMenu.Visibility = Visibility.Visible;
						return;
					}
					case MenuType.Info:
					{
						this.InfoMenu.Visibility = Visibility.Visible;
						return;
					}
					case MenuType.Settings:
					{
						var llSelected = this.LogLevel.Items.OfType<TextBox>()
							.SingleOrDefault(x => x?.Tag is LogSeverity ls && ls == _BotSettings.LogLevel);
						var tuSource = await Task.WhenAll(_BotSettings.TrustedUsers
							.Select(async x => AdvobotTextBox.CreateUserBox(await _Client.GetUserAsync(x))));

						this.AlwaysDownloadUsers.IsChecked = _BotSettings.AlwaysDownloadUsers;
						this.Prefix.Text = _BotSettings.Prefix;
						this.Game.Text = _BotSettings.Game;
						this.Stream.Text = _BotSettings.Stream;
						this.ShardCount.Text = _BotSettings.ShardCount.ToString();
						this.MessageCache.Text = _BotSettings.MessageCacheCount.ToString();
						this.UserCount.Text = _BotSettings.MaxUserGatherCount.ToString();
						this.MessageGather.Text = _BotSettings.MaxMessageGatherSize.ToString();
						this.LogLevel.SelectedItem = llSelected;
						this.TrustedUsers.ItemsSource = tuSource;
						
						this.SettingsMenu.Visibility = Visibility.Visible;
						return;
					}
					case MenuType.Colors:
					{
						/*
						UIModification.MakeColorDisplayer(_UISettings, _ColorsLayout, _ColorsSaveButton, .018);*/
						this.ColorsMenu.Visibility = Visibility.Visible;
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
						this.FilesMenu.Visibility = Visibility.Visible;
						return;
					}
				}
			}
		}
		private void OpenHyperLink(object sender, RequestNavigateEventArgs e)
		{
			Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
			e.Handled = true;
		}
	}
}
