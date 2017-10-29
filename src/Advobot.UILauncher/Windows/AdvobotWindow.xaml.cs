using Advobot.Core;
using Advobot.Core.Actions;
using Advobot.Core.Actions.Formatting;
using Advobot.Core.Interfaces;
using Advobot.UILauncher.Actions;
using Advobot.UILauncher.Classes;
using Advobot.UILauncher.Classes.Controls;
using Advobot.UILauncher.Enums;
using Discord;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using System.Windows.Threading;

namespace Advobot.UILauncher.Windows
{
	/// <summary>
	/// Interaction logic for AdvobotApplication.xaml
	/// </summary>
	public partial class AdvobotWindow : Window
	{
		public Holder<IDiscordClient> Client { get; private set; } = new Holder<IDiscordClient>();
		public Holder<IBotSettings> BotSettings { get; private set; } = new Holder<IBotSettings>();
		public Holder<ILogService> LogHolder { get; private set; } = new Holder<ILogService>();
		public ILogService Logging { get; private set; } = null;

		private ColorSettings _Colors = new ColorSettings();
		private LoginHandler _LoginHandler = new LoginHandler();
		private MenuType _LastButtonClicked;
		private BindingListener _Listener;

		public AdvobotWindow()
		{
			_Listener = new BindingListener();
			InitializeComponent();

			_LoginHandler.AbleToStart += Start;
			Console.SetOut(new TextBoxStreamWriter(this.Output));
			ColorSettings.SwitchElementColorOfChildren(this.Layout);
		}

		private async void Start(object sender, RoutedEventArgs e)
		{
			if (!(sender is LoginHandler lh))
			{
				throw new ArgumentException($"This event must be triggered by a {nameof(LoginHandler)}.");
			}

			Client.HeldObject = lh.GetRequiredService<IDiscordClient>();
			BotSettings.HeldObject = lh.GetRequiredService<IBotSettings>();
			LogHolder.HeldObject = lh.GetRequiredService<ILogService>();
			Logging = lh.GetRequiredService<ILogService>();
			_Colors = ColorSettings.LoadUISettings();

			//Has to be started after the client due to the latency tab
			((DispatcherTimer)this.Resources["ApplicationInformationTimer"]).Start();
			await ClientActions.StartAsync(Client.HeldObject);
		}
		private async void AttemptToLogin(object sender, RoutedEventArgs e)
		{
			//Send null once to check if a path is set in the config
			await _LoginHandler.AttemptToStart(null);
			//If it is set, then send null again to check for the bot key
			if (_LoginHandler.GotPath)
			{
				await _LoginHandler.AttemptToStart(null);
			}
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

			var type = button.Tag as MenuType? ?? default;
			if (type == _LastButtonClicked)
			{
				//If clicking the same button then resize the output window to the regular size
				UIModification.SetColSpan(this.Output, Grid.GetColumnSpan(this.Output) + (this.Layout.ColumnDefinitions.Count - 1));
				_LastButtonClicked = default;
			}
			else
			{
				//Resize the regular output window and have the menubox appear
				UIModification.SetColSpan(this.Output, Grid.GetColumnSpan(this.Output) - (this.Layout.ColumnDefinitions.Count - 1));
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
						var s = BotSettings.HeldObject;
						var llSelected = this.LogLevel.Items.OfType<TextBox>()
							.SingleOrDefault(x => x?.Tag is LogSeverity ls && ls == s.LogLevel);
						var tuSource = await Task.WhenAll(s.TrustedUsers
							.Select(async x => AdvobotTextBox.CreateUserBox(await Client.HeldObject.GetUserAsync(x))));

						this.AlwaysDownloadUsers.IsChecked = s.AlwaysDownloadUsers;
						this.Prefix.Text = s.Prefix;
						this.Game.Text = s.Game;
						this.Stream.Text = s.Stream;
						this.ShardCount.Text = s.ShardCount.ToString();
						this.MessageCacheCount.Text = s.MessageCacheCount.ToString();
						this.MaxUserGatherCount.Text = s.MaxUserGatherCount.ToString();
						this.MaxMessageGatherSize.Text = s.MaxMessageGatherSize.ToString();
						this.LogLevel.SelectedItem = llSelected;
						this.TrustedUsers.ItemsSource = tuSource;

						this.SettingsMenu.Visibility = Visibility.Visible;
						return;
					}
					case MenuType.Colors:
					{
						var c = _Colors;
						var tcbSelected = this.ThemesComboBox.Items.OfType<TextBox>()
							.SingleOrDefault(x => x?.Tag is ColorTheme t && t == c.Theme);

						this.BaseBackground.Text = c[ColorTarget.BaseBackground]?.ToString() ?? "";
						this.BaseForeground.Text = c[ColorTarget.BaseForeground]?.ToString() ?? "";
						this.BaseBorder.Text = c[ColorTarget.BaseBorder]?.ToString() ?? "";
						this.ButtonBackground.Text = c[ColorTarget.ButtonBackground]?.ToString() ?? "";
						this.ButtonBorder.Text = c[ColorTarget.ButtonBorder]?.ToString() ?? "";
						this.ButtonDisabledBackground.Text = c[ColorTarget.ButtonDisabledBackground]?.ToString() ?? "";
						this.ButtonDisabledForeground.Text = c[ColorTarget.ButtonDisabledForeground]?.ToString() ?? "";
						this.ButtonDisabledBorder.Text = c[ColorTarget.ButtonDisabledBorder]?.ToString() ?? "";
						this.ButtonMouseOverBackground.Text = c[ColorTarget.ButtonMouseOverBackground]?.ToString() ?? "";
						this.ThemesComboBox.SelectedItem = tcbSelected;

						this.ColorsMenu.Visibility = Visibility.Visible;
						return;
					}
					case MenuType.Files:
					{
						this.FilesTreeView.ItemsSource = AdvobotTreeView.MakeGuildTreeViewItemsSource(await Client.HeldObject.GetGuildsAsync());
						foreach (var item in this.FilesTreeView.Items.Cast<TreeViewItem>().SelectMany(x => x.Items.Cast<TreeViewItem>()))
						{
							item.MouseDoubleClick += OpenSpecificFileLayout;
						}
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
					ClientActions.DisconnectBot(Client.HeldObject);
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
			if (BotSettings.HeldObject.Pause)
			{
				(e.Source as Button).Content = "Pause";
				ConsoleActions.WriteLine("The bot is now unpaused.");
			}
			else
			{
				(e.Source as Button).Content = "Unpause";
				ConsoleActions.WriteLine("The bot is now paused.");
			}
			BotSettings.HeldObject.TogglePause();
		}
		private async void AcceptInput(object sender, KeyEventArgs e)
		{
			if (String.IsNullOrWhiteSpace(this.InputBox.Text))
			{
				this.InputButton.IsEnabled = false;
				return;
			}

			if (e.Key == Key.Enter || e.Key == Key.Return)
			{
				var input = UICommandHandler.GatherInput(this.InputBox, this.InputButton);
				if (!_LoginHandler.CanLogin)
				{
					await _LoginHandler.AttemptToStart(input);
				}
			}
			else
			{
				this.InputButton.IsEnabled = true;
			}
		}
		private async void AcceptInput(object sender, RoutedEventArgs e)
		{
			var input = UICommandHandler.GatherInput(this.InputBox, this.InputButton);
			if (!_LoginHandler.CanLogin)
			{
				await _LoginHandler.AttemptToStart(input);
			}
		}
		private void UpdateApplicationInfo(object sender, EventArgs e)
		{
			this.Uptime.Text = $"Uptime: {TimeFormatting.FormatUptime()}";
			this.Latency.Text = $"Latency: {ClientActions.GetLatency(Client.HeldObject)}ms";
			this.Memory.Text = $"Memory: {GetActions.GetMemory().ToString("0.00")}MB";
			this.ThreadCount.Text = $"Threads: {Process.GetCurrentProcess().Threads.Count}";
		}
		private async void SaveOutput(object sender, RoutedEventArgs e)
		{
			var response = SavingActions.SaveFile(this.Output);
			await ToolTipActions.EnableTimedToolTip(this.Layout, response.GetReason());
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

			var tb = AdvobotTextBox.CreateUserBox(await Client.HeldObject.GetUserAsync(userId));
			if (tb != null)
			{
				this.TrustedUsers.ItemsSource = this.TrustedUsers.ItemsSource.OfType<TextBox>()
					.Concat(new[] { tb }).Where(x => x != null);
			}

			this.TrustedUsersBox.Text = null;
		}
		private void SaveColors(object sender, RoutedEventArgs e)
		{
			var c = _Colors;
			foreach (var child in this.ColorsMenu.GetChildren())
			{
				if (child is AdvobotTextBox tb && tb.Tag is ColorTarget target)
				{
					var childText = tb.Text;
					if (String.IsNullOrWhiteSpace(childText))
					{
						continue;
					}
					if (!UIModification.TryCreateBrush(childText, out var brush))
					{
						ConsoleActions.WriteLine($"Invalid color supplied for {target.EnumName()}.");
						continue;
					}

					if (!UIModification.CheckIfSameBrush(c[target], brush))
					{
						tb.Text = (c[target] = brush).ToString();
						ConsoleActions.WriteLine($"Successfully updated the color for {target.EnumName()}.");
					}
				}
				else if (child is ComboBox cb && cb.SelectedItem is AdvobotTextBox tb2 && tb2.Tag is ColorTheme theme)
				{
					if (c.Theme != theme)
					{
						c.Theme = theme;
						ConsoleActions.WriteLine($"Successfully updated the theme to {theme.EnumName().FormatTitle().ToLower()}.");
					}
				}
			}

			c.SaveSettings();
		}
		private async void SaveSettings(object sender, RoutedEventArgs e)
		{
			await SavingActions.SaveSettings(this.SettingsMenu, Client.HeldObject, BotSettings.HeldObject);
		}
		private void OpenSpecificFileLayout(object sender, RoutedEventArgs e)
		{
			if (UIModification.TryGetFileText(sender, out var text, out var fileInfo))
			{
				OpenSpecificFileLayout(text, fileInfo);
			}
			else
			{
				ConsoleActions.WriteLine($"Unable to open the file.");
			}
		}
		private void SaveSettingsWithCtrlS(object sender, KeyEventArgs e)
		{
			if (SavingActions.IsCtrlS(e))
			{
				SaveSettings(sender, e);
			}
		}
		private void SaveColorsWithCtrlS(object sender, KeyEventArgs e)
		{
			if (SavingActions.IsCtrlS(e))
			{
				SaveColors(sender, e);
			}
		}
		private void SaveFileWithCtrlS(object sender, KeyEventArgs e)
		{
			if (SavingActions.IsCtrlS(e))
			{
				SaveFile(sender, e);
			}
		}
		private void OpenModal(object sender, RoutedEventArgs e)
		{
			if (!(sender is FrameworkElement ele) || !(ele.Tag is Modal m))
			{
				return;
			}

			//Make the screen look dark and then bring up the modal
			this.Opacity = .25;
			switch (m)
			{
				case Modal.FileSearch:
				{
					new FileSearchWindow(this).ShowDialog();
					break;
				}
				case Modal.OutputSearch:
				{
					new OutputSearchWindow(this).ShowDialog();
					break;
				}
				default:
				{
					throw new ArgumentException($"Invalid modal type supplied: {m.EnumName()}");
				}
			}
			//Reset the opacity
			this.Opacity = 100;
		}
		private void CloseFile(object sender, RoutedEventArgs e)
		{
			switch (MessageBox.Show("Are you sure you want to close the file window?", Constants.PROGRAM_NAME, MessageBoxButton.OKCancel))
			{
				case MessageBoxResult.OK:
				{
					UIModification.SetRowSpan(this.FilesMenu, Grid.GetRowSpan(this.FilesMenu) - (this.Layout.RowDefinitions.Count - 1));
					this.SpecificFileOutput.Tag = null;
					this.SpecificFileOutput.Visibility = Visibility.Collapsed;
					this.SaveFileButton.Visibility = Visibility.Collapsed;
					this.CloseFileButton.Visibility = Visibility.Collapsed;
					this.FileSearchButton.Visibility = Visibility.Visible;
					return;
				}
			}
		}
		private async void SaveFile(object sender, RoutedEventArgs e)
		{
			var response = SavingActions.SaveFile(this.SpecificFileOutput);
			await ToolTipActions.EnableTimedToolTip(this.Layout, response.GetReason());
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

		private void OpenOutputSearch(object sender, RoutedEventArgs e)
		{
		}
		private void CloseOutputSearch(object sender, RoutedEventArgs e)
		{
		}
		private void SearchOutput(object sender, RoutedEventArgs e)
		{
		}

		public void OpenSpecificFileLayout(string text, FileInfo fileInfo)
		{
			this.SpecificFileOutput.Tag = fileInfo;
			this.SpecificFileOutput.Clear();
			this.SpecificFileOutput.AppendText(text);
			UIModification.SetRowSpan(this.FilesMenu, Grid.GetRowSpan(this.FilesMenu) + (this.Layout.RowDefinitions.Count - 1));

			this.SpecificFileOutput.Visibility = Visibility.Visible;
			this.SaveFileButton.Visibility = Visibility.Visible;
			this.CloseFileButton.Visibility = Visibility.Visible;
			this.FileSearchButton.Visibility = Visibility.Collapsed;
		}
	}
}
