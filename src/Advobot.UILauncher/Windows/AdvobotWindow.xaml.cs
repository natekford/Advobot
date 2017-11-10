using Advobot.Core;
using Advobot.Core.Actions;
using Advobot.Core.Actions.Formatting;
using Advobot.Core.Interfaces;
using Advobot.UILauncher.Actions;
using Advobot.UILauncher.Classes;
using Advobot.UILauncher.Classes.Controls;
using Advobot.UILauncher.Enums;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

		private ColorSettings _Colors = new ColorSettings();
		private LoginHandler _LoginHandler = new LoginHandler();
		private BindingListener _Listener = new BindingListener();
		private List<FileSystemWatcher> _TreeViewUpdaters = new List<FileSystemWatcher>();
		private MenuType _LastButtonClicked;

		public AdvobotWindow()
		{
			InitializeComponent();
			Console.SetOut(new TextBoxStreamWriter(this.Output));
			_LoginHandler.AbleToStart += Start;
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
			_Colors = ColorSettings.LoadUISettings();

			if (Client.HeldObject is DiscordSocketClient socket)
			{
				socket.Connected += EnableButtons;
				socket.GuildAvailable += AddGuildToTreeView;
			}
			else if (Client.HeldObject is DiscordShardedClient sharded)
			{
				sharded.Shards.LastOrDefault().Connected += EnableButtons;
			}

			//Has to be started after the client due to the latency tab
			((DispatcherTimer)this.Resources["ApplicationInformationTimer"]).Start();
			await ClientActions.StartAsync(Client.HeldObject);
		}
		private async Task EnableButtons()
		{
			await this.Dispatcher.InvokeAsync(() =>
			{
				this.MainMenuButton.IsEnabled = true;
				this.InfoMenuButton.IsEnabled = true;
				this.FilesMenuButton.IsEnabled = true;
				this.ColorsMenuButton.IsEnabled = true;
				this.SettingsMenuButton.IsEnabled = true;
				this.OutputContextMenu.IsEnabled = true;
			});
		}
		private async Task AddGuildToTreeView(SocketGuild guild)
		{
			await this.Dispatcher.InvokeAsync(() =>
			{
				//Make sure the guild isn't already in the treeview
				var items = this.FilesTreeView.Items.OfType<AdvobotTreeViewHeader>();
				var item = items.SingleOrDefault(x => x.Guild.Id == guild.Id);
				if (item != null)
				{
					item.Visibility = Visibility.Visible;
					return;
				}

				//Add to tree view then resort based on member count
				this.FilesTreeView.Items.Add(new AdvobotTreeViewHeader(guild));
				this.FilesTreeView.Items.SortDescriptions.Clear();
				this.FilesTreeView.Items.SortDescriptions.Add(new SortDescription("Tag", ListSortDirection.Descending));
			}, DispatcherPriority.Background);
		}
		private async Task RemoveGuildFromTreeView(SocketGuild guild)
		{
			await this.Dispatcher.InvokeAsync(() =>
			{
				//Just make the item invisible so if need be it can be made visible instead of having to recreate it.
				var items = this.FilesTreeView.Items.OfType<AdvobotTreeViewHeader>();
				var item = items.SingleOrDefault(x => x.Guild.Id == guild.Id);
				if (item != null)
				{
					item.Visibility = Visibility.Collapsed;
				}
			}, DispatcherPriority.Background);
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
		private async void AcceptInput(object sender, RoutedEventArgs e)
		{
			var input = CommandHandler.GatherInput(this.InputBox);
			if (String.IsNullOrWhiteSpace(input))
			{
				return;
			}
			ConsoleActions.WriteLine(input);

			if (!_LoginHandler.CanLogin)
			{
				await _LoginHandler.AttemptToStart(input);
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

			var tb = new AdvobotUserBox(await Client.HeldObject.GetUserAsync(userId));
			if (tb != null)
			{
				this.TrustedUsers.AddItem(tb);
			}

			this.TrustedUsersBox.Text = null;
		}
		private async void SaveSettings(object sender, RoutedEventArgs e)
		{
			SavingActions.SaveSettings(this.SettingsMenuDisplay, BotSettings.HeldObject);
			await ClientActions.UpdateGameAsync(Client.HeldObject, BotSettings.HeldObject);
		}
		private void SaveOutput(object sender, RoutedEventArgs e)
		{
			var response = SavingActions.SaveFile(this.Output);
			ToolTipActions.EnableTimedToolTip(this.Layout, response.GetReason());
		}
		private void OpenMenu(object sender, RoutedEventArgs e)
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
				EntityActions.SetColSpan(this.Output, Grid.GetColumnSpan(this.Output) + (this.Layout.ColumnDefinitions.Count - 1));
				_LastButtonClicked = default;
			}
			else
			{
				//Resize the regular output window and have the menubox appear
				EntityActions.SetColSpan(this.Output, Grid.GetColumnSpan(this.Output) - (this.Layout.ColumnDefinitions.Count - 1));
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

						this.AlwaysDownloadUsers.IsChecked = s.AlwaysDownloadUsers;
						this.Prefix.Text = s.Prefix;
						this.Game.Text = s.Game;
						this.Stream.Text = s.Stream;
						this.ShardCount.Text = s.ShardCount.ToString();
						this.MessageCacheCount.Text = s.MessageCacheCount.ToString();
						this.MaxUserGatherCount.Text = s.MaxUserGatherCount.ToString();
						this.MaxMessageGatherSize.Text = s.MaxMessageGatherSize.ToString();
						this.LogLevel.SelectedItem = llSelected;

						this.SettingsMenu.Visibility = Visibility.Visible;
						return;
					}
					case MenuType.Colors:
					{
						var c = _Colors;
						var tcbSelected = this.ThemesComboBox.Items.OfType<TextBox>()
							.SingleOrDefault(x => x?.Tag is ColorTheme t && t == c.Theme);

						this.ThemesComboBox.SelectedItem = tcbSelected;
						this.BaseBackground.Text = c[ColorTarget.BaseBackground]?.ToString();
						this.BaseForeground.Text = c[ColorTarget.BaseForeground]?.ToString();
						this.BaseBorder.Text = c[ColorTarget.BaseBorder]?.ToString();
						this.ButtonBackground.Text = c[ColorTarget.ButtonBackground]?.ToString();
						this.ButtonForeground.Text = c[ColorTarget.ButtonForeground]?.ToString();
						this.ButtonBorder.Text = c[ColorTarget.ButtonBorder]?.ToString();
						this.ButtonDisabledBackground.Text = c[ColorTarget.ButtonDisabledBackground]?.ToString();
						this.ButtonDisabledForeground.Text = c[ColorTarget.ButtonDisabledForeground]?.ToString();
						this.ButtonDisabledBorder.Text = c[ColorTarget.ButtonDisabledBorder]?.ToString();
						this.ButtonMouseOverBackground.Text = c[ColorTarget.ButtonMouseOverBackground]?.ToString();
						this.JsonDigits.Text = c[ColorTarget.JsonDigits]?.ToString();
						this.JsonValue.Text = c[ColorTarget.JsonValue]?.ToString();
						this.JsonParamName.Text = c[ColorTarget.JsonParamName]?.ToString();

						this.ColorsMenu.Visibility = Visibility.Visible;
						return;
					}
					case MenuType.Files:
					{
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
		private void AcceptInputWithKey(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter || e.Key == Key.Return)
			{
				AcceptInput(sender, e);
			}
		}
		private void UpdateApplicationInfo(object sender, EventArgs e)
		{
			this.Uptime.Text = $"Uptime: {TimeFormatting.FormatUptime()}";
			this.Latency.Text = $"Latency: {ClientActions.GetLatency(Client.HeldObject)}ms";
			this.Memory.Text = $"Memory: {GetActions.GetMemory().ToString("0.00")}MB";
			this.ThreadCount.Text = $"Threads: {Process.GetCurrentProcess().Threads.Count}";
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
			this.TrustedUsers.RemoveItem(this.TrustedUsers.SelectedItem);
		}
		private void SaveColors(object sender, RoutedEventArgs e)
		{
			var c = _Colors;
			var children = this.ColorsMenuDisplay.GetChildren();
			foreach (var tb in children.OfType<AdvobotTextBox>())
			{
				if (tb.Tag is ColorTarget target)
				{
					var childText = tb.Text;
					var name = target.EnumName().FormatTitle().ToLower();
					if (String.IsNullOrWhiteSpace(childText))
					{
						if (c[target] != null)
						{
							c[target] = null;
							ConsoleActions.WriteLine($"Successfully updated the color for {name}.");
						}
						continue;
					}
					if (!AdvobotColor.TryCreateColor(childText, out var color))
					{
						ConsoleActions.WriteLine($"Invalid color supplied for {name}: '{childText}'.");
						continue;
					}

					var brush = color.CreateBrush();
					if (!AdvobotColor.CheckIfSameBrush(c[target], brush))
					{
						c[target] = brush;
						ConsoleActions.WriteLine($"Successfully updated the color for {name}: '{childText} ({brush.ToString()})'.");
					}

					//Update the text here because if someone has the hex value for yellow but they put in Yellow as a string 
					//It won't update in the above if statement since they produce the same value
					tb.Text = c[target].ToString();
				}
			}
			//Has to go after the textboxes so the theme will be applied
			foreach (var cb in children.OfType<AdvobotComboBox>())
			{
				if (cb.SelectedItem is AdvobotTextBox tb && tb.Tag is ColorTheme theme)
				{
					if (c.Theme != theme)
					{
						ConsoleActions.WriteLine($"Successfully updated the theme to {theme.EnumName().FormatTitle().ToLower()}.");
					}
					c.Theme = theme;
				}
			}

			c.SaveSettings();
		}
		private void OpenSpecificFileLayout(object sender, RoutedEventArgs e)
		{
			if (SavingActions.TryGetFileText(sender, out var text, out var fileInfo))
			{
				//OpenSpecificFileLayout(text, fileInfo);
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
		private void OpenModal(object sender, RoutedEventArgs e)
		{
			if (!(sender is FrameworkElement ele) || !(ele.Tag is Modal m))
			{
				return;
			}

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
				case Modal.FileViewing:
				{
					//This modal should not be opened through this method.
					//Opened instead on double click on a treeview file item
					//or through guild search
					return;
				}
				default:
				{
					throw new ArgumentException($"Invalid modal type supplied: {m}");
				}
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
