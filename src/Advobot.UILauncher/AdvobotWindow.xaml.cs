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
using Advobot.Core;
using Advobot.Core.Actions;
using Microsoft.Extensions.DependencyInjection;

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
		private DispatcherTimer _Timer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 0, 0, 500) };

		private MenuType _LastButtonClicked;

		public AdvobotWindow()
		{
			InitializeComponent();

			Console.SetOut(new TextBoxStreamWriter(this.Output));
			new ColorSettings().ActivateTheme();
			ColorSettings.SwitchElementColorOfChildren(this.Content as DependencyObject);
		}

		private async void AttemptToLogIn(object sender, RoutedEventArgs e)
		{
			await HandleInput(null);
			await HandleInput(null);
		}
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
		private void Pause(object sender, RoutedEventArgs e)
		{
			if (_BotSettings.Pause)
			{
				ConsoleActions.WriteLine("The bot is now unpaused.");
				_BotSettings.TogglePause();
			}
			else
			{
				ConsoleActions.WriteLine("The bot is now paused.");
				_BotSettings.TogglePause();
			}
		}
		private async void AcceptInput(object sender, KeyEventArgs e)
		{
			var text = this.InputBox.Text;
			if (String.IsNullOrWhiteSpace(text))
			{
				this.InputButton.IsEnabled = false;
				return;
			}

			if (e.Key.Equals(Key.Enter) || e.Key.Equals(Key.Return))
			{
				await HandleInput(UICommandHandler.GatherInput(this.InputBox, this.InputButton));
			}
			else
			{
				this.InputButton.IsEnabled = true;
			}
		}
		private async void AcceptInput(object sender, RoutedEventArgs e)
		{
			await HandleInput(UICommandHandler.GatherInput(this.InputBox, this.InputButton));
		}
		private async void UpdateMenus(object sender, EventArgs e)
		{
			var guilds = await _Client.GetGuildsAsync();
			var users = await Task.WhenAll(guilds.Select(async g => await g.GetUsersAsync()));

			this.Latency.Text = $"Latency: {ClientActions.GetLatency(_Client)}ms";
			this.Memory.Text = $"Memory: {GetActions.GetMemory().ToString("0.00")}MB";
			this.ThreadCount.Text = $"Threads: {Process.GetCurrentProcess().Threads.Count}";
			this.GuildCount.Text = $"Guilds: {guilds.Count}";
			this.UserCount.Text = $"Members: {users.SelectMany(x => x).Select(x => x.Id).Distinct().Count()}";
			this.InfoOutput.Document = UIModification.MakeInfoMenu(_Logging);
		}

		//TODO: Fix this entire terrible method
		private static bool _StartUp = true;
		private static bool _GotPath;
		private static bool _GotKey;
		private async Task HandleInput(string input)
		{
			if (!_GotPath)
			{
				var provider = await UIBotWindowLogic.GetPath(input, _StartUp);
				if (provider != null)
				{
					_StartUp = true;
					_GotPath = true;
					_Client = provider.GetService<IDiscordClient>();
					_BotSettings = provider.GetService<IBotSettings>();
					_Logging = provider.GetService<ILogService>();
					_UISettings = ColorSettings.LoadUISettings(_StartUp);

					_UISettings.ActivateTheme();
					ColorSettings.SwitchElementColorOfChildren(this.Content as DependencyObject);
				}
				else
				{
					_StartUp = false;
				}
			}
			else if (!_GotKey)
			{
				if (await Config.ValidateBotKey(_Client, input, _StartUp))
				{
					_StartUp = true;
					_GotKey = true;
				}
				else
				{
					_StartUp = false;
				}
			}

			if (!_GotKey && _StartUp)
			{
				if (await Config.ValidateBotKey(_Client, null, _StartUp))
				{
					_StartUp = true;
					_GotKey = true;
				}
				else
				{
					_StartUp = false;
				}
			}

			if (_GotPath && _GotKey && _StartUp)
			{
				_Timer.Tick += UpdateMenus;
				_Timer.Start();
				await ClientActions.StartAsync(_Client);
				_StartUp = false;
			}
		}
	}
}
