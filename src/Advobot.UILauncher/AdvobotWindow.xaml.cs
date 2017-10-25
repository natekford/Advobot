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
using Advobot.UILauncher.Classes.Converters;
using Advobot.Core.Actions.Formatting;
using Advobot.Core.Classes;
using System.Threading;

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
		private ColorSettings _ColorSettings = new ColorSettings();

		private DispatcherTimer _Timer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 0, 0, 500) };
		private ToolTip _ToolTip = new ToolTip();
		private CancellationTokenSource _ToolTipCancellationTokenSource;
		private MenuType _LastButtonClicked;

		public AdvobotWindow()
		{
			InitializeComponent();

			Console.SetOut(new TextBoxStreamWriter(this.Output));
			ColorSettings.SwitchElementColorOfChildren(this.Layout);
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
						this.UserGather.Text = _BotSettings.MaxUserGatherCount.ToString();
						this.MessageGather.Text = _BotSettings.MaxMessageGatherSize.ToString();
						this.LogLevel.SelectedItem = llSelected;
						this.TrustedUsers.ItemsSource = tuSource;
						
						this.SettingsMenu.Visibility = Visibility.Visible;
						return;
					}
					case MenuType.Colors:
					{
						var tcbSelected = this.ThemesComboBox.Items.OfType<TextBox>()
							.SingleOrDefault(x => x?.Tag is ColorTheme t && t == _ColorSettings.Theme);

						this.BaseBackground.Text = _ColorSettings[ColorTarget.BaseBackground]?.ToString() ?? "";
						this.BaseForeground.Text = _ColorSettings[ColorTarget.BaseForeground]?.ToString() ?? "";
						this.BaseBorder.Text = _ColorSettings[ColorTarget.BaseBorder]?.ToString() ?? "";
						this.ButtonBackground.Text = _ColorSettings[ColorTarget.ButtonBackground]?.ToString() ?? "";
						this.ButtonBorder.Text = _ColorSettings[ColorTarget.ButtonBorder]?.ToString() ?? "";
						this.ButtonDisabledBackground.Text = _ColorSettings[ColorTarget.ButtonDisabledBackground]?.ToString() ?? "";
						this.ButtonDisabledForeground.Text = _ColorSettings[ColorTarget.ButtonDisabledForeground]?.ToString() ?? "";
						this.ButtonDisabledBorder.Text = _ColorSettings[ColorTarget.ButtonDisabledBorder]?.ToString() ?? "";
						this.ButtonMouseOverBackground.Text = _ColorSettings[ColorTarget.ButtonMouseOverBackground]?.ToString() ?? "";
						this.ThemesComboBox.SelectedItem = tcbSelected;

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
		private void UpdateApplicationInfo(object sender, EventArgs e)
		{
			this.Uptime.Text = $"Uptime: {TimeFormatting.FormatUptime()}";
			this.Latency.Text = $"Latency: {ClientActions.GetLatency(_Client)}ms";
			this.Memory.Text = $"Memory: {GetActions.GetMemory().ToString("0.00")}MB";
			this.ThreadCount.Text = $"Threads: {Process.GetCurrentProcess().Threads.Count}";
		}
		private async void SaveOutput(object sender, RoutedEventArgs e)
		{
			await MakeFollowingToolTip(UIBotWindowLogic.SaveOutput(this.Output).GetReason());
		}
		private void ClearOutput(object sender, RoutedEventArgs e)
		{
			switch (MessageBox.Show("Are you sure you want to clear the output window?", Constants.PROGRAM_NAME, MessageBoxButton.OKCancel))
			{
				case MessageBoxResult.OK:
				{
					this.Output.Text = null;
					return;
				}
			}
		}
		private void RemoveTrustedUser(object sender, RoutedEventArgs e)
		{
			if (this.TrustedUsers.SelectedItem != null)
			{
				this.TrustedUsers.ItemsSource = this.TrustedUsers.ItemsSource.OfType<TextBox>()
					.Except(new[] { this.TrustedUsers.SelectedItem }).Where(x => x != null);
			}
		}
		private async void AddTrustedUser(object sender, RoutedEventArgs e)
		{
			var input = this.TrustedUsersBox.Text;
			if (!ulong.TryParse(input, out ulong userId))
			{
				ConsoleActions.WriteLine($"The given input '{input}' is not a valid ID.");
			}
			else if (this.TrustedUsers.Items.OfType<TextBox>().Any(x => x?.Tag is ulong id && id == userId))
			{
				return;
			}

			var tb = AdvobotTextBox.CreateUserBox(await _Client.GetUserAsync(userId));
			if (tb != null)
			{
				this.TrustedUsers.ItemsSource = this.TrustedUsers.ItemsSource.OfType<TextBox>()
					.Concat(new[] { tb }).Where(x => x != null);
			}

			this.TrustedUsersBox.Text = null;
		}
		private void SaveColors(object sender, RoutedEventArgs e)
		{
			foreach (var child in this.ColorsMenu.GetChildren())
			{
				if (child is AdvobotTextBox tb && tb.Tag is ColorTarget target)
				{
					var childText = tb.Text;
					if (String.IsNullOrWhiteSpace(childText))
					{
						continue;
					}
					if (!UIModification.TryMakeBrush(childText, out var brush))
					{
						ConsoleActions.WriteLine($"Invalid color supplied for {target.EnumName()}.");
						continue;
					}

					tb.Text = (_ColorSettings[target] = brush).ToString();
					ConsoleActions.WriteLine($"Successfully updated the color for {target.EnumName()}.");
				}
				else if (child is ComboBox cb && cb.SelectedItem is AdvobotTextBox tb2 && tb2.Tag is ColorTheme theme)
				{
					_ColorSettings.Theme = theme;
					ConsoleActions.WriteLine("Successfully updated the theme type.");
				}
			}

			_ColorSettings.SaveSettings();
			ColorSettings.SwitchElementColorOfChildren(this.Layout);
		}
		private async void SaveSettings(object sender, RoutedEventArgs e)
		{
			await SettingModification.SaveSettings(this.SettingsMenu, _Client, _BotSettings);
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

					_ColorSettings = ColorSettings.LoadUISettings(_StartUp);
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
				_Timer.Tick += UpdateApplicationInfo;
				_Timer.Start();
				_StartUp = false;
				SetBindings();
				await ClientActions.StartAsync(_Client);
			}
		}
		public async Task MakeFollowingToolTip(string text, int timeInMS = 2500)
		{
			_ToolTip.Content = text ?? "Blank";
			_ToolTip.IsOpen = true;
			this.Layout.MouseMove += (sender, e) =>
			{
				var point = System.Windows.Forms.Control.MousePosition;
				_ToolTip.HorizontalOffset = point.X;
				_ToolTip.VerticalOffset = point.Y;
			};

			if (_ToolTipCancellationTokenSource != null)
			{
				_ToolTipCancellationTokenSource.Cancel();
			}
			_ToolTipCancellationTokenSource = new CancellationTokenSource();

			await this.Layout.Dispatcher.InvokeAsync(async () =>
			{
				try
				{
					await Task.Delay(timeInMS, _ToolTipCancellationTokenSource.Token);
				}
				catch (TaskCanceledException)
				{
					return;
				}

				_ToolTip.IsOpen = false;
			});
		}

		private void SetBindings()
		{
			SetCommandCountBinding(this.Guilds, _Logging.TotalGuilds);
			SetCommandCountBinding(this.Users, _Logging.TotalUsers);
			SetCommandCountBinding(this.AttemptedCommands, _Logging.AttemptedCommands);
			SetCommandCountBinding(this.SuccessfulCommands, _Logging.SuccessfulCommands);
			SetCommandCountBinding(this.FailedCommands, _Logging.FailedCommands);
			SetCommandCountBinding(this.UserJoins, _Logging.UserJoins);
			SetCommandCountBinding(this.UserLeaves, _Logging.UserLeaves);
			SetCommandCountBinding(this.UserChanges, _Logging.UserChanges);
			SetCommandCountBinding(this.MessageEdits, _Logging.MessageEdits);
			SetCommandCountBinding(this.MessageDeletes, _Logging.MessageDeletes);
			SetCommandCountBinding(this.Images, _Logging.Images);
			SetCommandCountBinding(this.Gifs, _Logging.Gifs);
			SetCommandCountBinding(this.Files, _Logging.Files);
		}
		private void SetCommandCountBinding(TextBox tb, LogCounter source)
		{
			tb.SetBinding(TextBox.TextProperty, new Binding
			{
				Path = new PropertyPath(nameof(LogCounter.Count)),
				Source = source,
				Mode = BindingMode.OneWay,
				UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
			});
		}
	}
}
