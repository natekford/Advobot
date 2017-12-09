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
using System.ComponentModel;
using System.Diagnostics;
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
		private MenuType _LastButtonClicked;

		public AdvobotWindow()
		{
			InitializeComponent();
			//Make console output show up in the window
			Console.SetOut(new TextBoxStreamWriter(this.Output));
			//Start the timer that shows latency, memory usage, etc.
			((DispatcherTimer)this.Resources["ApplicationInformationTimer"]).Start();
			this._LoginHandler.AbleToStart += this.Start;
		}

		private async Task EnableButtons()
			=> await this.Dispatcher.InvokeAsync(() =>
			{
				this.MainMenuButton.IsEnabled = true;
				this.InfoMenuButton.IsEnabled = true;
				this.FilesMenuButton.IsEnabled = true;
				this.ColorsMenuButton.IsEnabled = true;
				this.SettingsMenuButton.IsEnabled = true;
				this.OutputContextMenu.IsEnabled = true;
			});
		private async Task AddGuildToTreeView(SocketGuild guild)
			=> await this.Dispatcher.InvokeAsync(() =>
			{
				//Make sure the guild isn't already in the treeview
				var item = this.FilesTreeView.Items.OfType<AdvobotTreeViewHeader>().SingleOrDefault(x => x.Guild.Id == guild.Id);
				if (item != null)
				{
					item.Visibility = Visibility.Visible;
					return;
				}

				//Add to tree view then resort based on member count
				this.FilesTreeView.Items.Add(new AdvobotTreeViewHeader(guild));
				//Not sure why the two lines below have to be used instead of Items.Refresh
				this.FilesTreeView.Items.SortDescriptions.Clear();
				this.FilesTreeView.Items.SortDescriptions.Add(new SortDescription("Tag", ListSortDirection.Descending));
			}, DispatcherPriority.Background);
		private async Task RemoveGuildFromTreeView(SocketGuild guild)
			=> await this.Dispatcher.InvokeAsync(() =>
			{
				//Just make the item invisible so if need be it can be made visible instead of having to recreate it.
				var item = this.FilesTreeView.Items.OfType<AdvobotTreeViewHeader>().SingleOrDefault(x => x.Guild.Id == guild.Id);
				if (item != null)
				{
					item.Visibility = Visibility.Collapsed;
				}
			}, DispatcherPriority.Background);
		private async Task Start()
		{
			this.Client.HeldObject = this._LoginHandler.GetRequiredService<IDiscordClient>();
			this.BotSettings.HeldObject = this._LoginHandler.GetRequiredService<IBotSettings>();
			this.LogHolder.HeldObject = this._LoginHandler.GetRequiredService<ILogService>();
			this._Colors = ColorSettings.LoadUISettings();

			if (this.Client.HeldObject is DiscordSocketClient socket)
			{
				socket.Connected += this.EnableButtons;
				socket.GuildAvailable += this.AddGuildToTreeView;
			}
			else if (this.Client.HeldObject is DiscordShardedClient sharded)
			{
				sharded.Shards.LastOrDefault().Connected += this.EnableButtons;
				sharded.GuildAvailable += this.AddGuildToTreeView;
			}
			await ClientActions.StartAsync(this.Client.HeldObject);
		}
		private async void AttemptToLogin(object sender, RoutedEventArgs e)
		{
			//Send null once to check if a path is set in the config
			//If it is set, then send null again to check for the bot key
			if (await this._LoginHandler.AttemptToStart(null))
			{
				await this._LoginHandler.AttemptToStart(null);
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

			if (!this._LoginHandler.CanLogin)
			{
				await this._LoginHandler.AttemptToStart(input);
			}
		}
		private void AcceptInputWithKey(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter || e.Key == Key.Return)
			{
				AcceptInput(sender, e);
			}
		}
		private void AddTrustedUser(object sender, RoutedEventArgs e)
		{
			var input = this.TrustedUsersBox.Text;
			if (!ulong.TryParse(input, out ulong userId))
			{
				ConsoleActions.WriteLine($"The given input '{input}' is not a valid ID.");
				return;
			}
			else if (this.TrustedUsers.Items.OfType<TextBox>().Any(x => x?.Tag is ulong id && id == userId))
			{
				ConsoleActions.WriteLine($"The given input '{input}' is already a trusted user.");
				return;
			}

			IUser user;
			if (this.Client.HeldObject is DiscordSocketClient socket)
			{
				user = socket.GetUser(userId);
			}
			else if (this.Client.HeldObject is DiscordShardedClient sharded)
			{
				user = sharded.GetUser(userId);
			}
			else
			{
				throw new ArgumentException($"Invalid {nameof(IDiscordClient)} passed into {nameof(AddTrustedUser)}");
			}

			this.TrustedUsersBox.Text = null;
			if (user != null)
			{
				this.TrustedUsers.Items.Add(new AdvobotUserBox(user));
				this.TrustedUsers.Items.SortDescriptions.Clear();
				this.TrustedUsers.Items.SortDescriptions.Add(new SortDescription("Text", ListSortDirection.Ascending));
			}
		}
		private void RemoveTrustedUser(object sender, RoutedEventArgs e)
			=> this.TrustedUsers.Items.Remove(this.TrustedUsers.SelectedItem);
		private void SaveSettings(object sender, RoutedEventArgs e)
		{
			SavingActions.SaveSettings(this.SettingsMenuDisplay, this.BotSettings.HeldObject);
			//In a task.run since the result is unimportant and unused
			Task.Run(async () => await ClientActions.UpdateGameAsync(this.Client.HeldObject, this.BotSettings.HeldObject));
		}
		private void SaveSettingsWithCtrlS(object sender, KeyEventArgs e)
		{
			if (SavingActions.IsCtrlS(e))
			{
				SaveSettings(sender, e);
			}
		}
		private void SaveColors(object sender, RoutedEventArgs e)
		{
			var c = this._Colors;
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
					if (!ColorWrapper.TryCreateColor(childText, out var color))
					{
						ConsoleActions.WriteLine($"Invalid color supplied for {name}: '{childText}'.");
						continue;
					}

					var brush = color.CreateBrush();
					if (!ColorWrapper.CheckIfSameBrush(c[target], brush))
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
		private void SaveColorsWithCtrlS(object sender, KeyEventArgs e)
		{
			if (SavingActions.IsCtrlS(e))
			{
				SaveColors(sender, e);
			}
		}
		private void SaveOutput(object sender, RoutedEventArgs e)
			=> ToolTipActions.EnableTimedToolTip(this.Layout, SavingActions.SaveFile(this.Output).GetReason());
		private void ClearOutput(object sender, RoutedEventArgs e)
		{
			switch (MessageBox.Show("Are you sure you want to clear the output window?", Constants.PROGRAM_NAME, MessageBoxButton.OKCancel))
			{
				case MessageBoxResult.OK:
				{
					this.Output.Clear();
					return;
				}
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
					//Opened instead on double click on a treeview file item or through guild search
					return;
				}
				default:
				{
					throw new ArgumentException($"Invalid modal type supplied: {m}");
				}
			}
		}
		private void OpenMenu(object sender, RoutedEventArgs e)
		{
			if (!(sender is FrameworkElement ele))
			{
				return;
			}

			//Hide everything so stuff doesn't overlap
			this.MainMenu.Visibility = Visibility.Collapsed;
			this.SettingsMenu.Visibility = Visibility.Collapsed;
			this.ColorsMenu.Visibility = Visibility.Collapsed;
			this.InfoMenu.Visibility = Visibility.Collapsed;
			this.FilesMenu.Visibility = Visibility.Collapsed;

			var type = ele.Tag as MenuType? ?? default;
			if (type == this._LastButtonClicked)
			{
				//If clicking the same button then resize the output window to the regular size
				EntityActions.SetColSpan(this.Output, Grid.GetColumnSpan(this.Output) + (this.Layout.ColumnDefinitions.Count - 1));
				this._LastButtonClicked = default;
			}
			else
			{
				//Resize the regular output window and have the menubox appear
				EntityActions.SetColSpan(this.Output, Grid.GetColumnSpan(this.Output) - (this.Layout.ColumnDefinitions.Count - 1));
				this._LastButtonClicked = type;

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
						var s = this.BotSettings.HeldObject;
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
						var c = this._Colors;
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
					ClientActions.DisconnectBot(this.Client.HeldObject);
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
			if (this.BotSettings.HeldObject.Pause)
			{
				this.PauseButton.Content = "Pause";
				ConsoleActions.WriteLine("The bot is now unpaused.");
			}
			else
			{
				this.PauseButton.Content = "Unpause";
				ConsoleActions.WriteLine("The bot is now paused.");
			}
			this.BotSettings.HeldObject.TogglePause();
		}
		private void UpdateApplicationInfo(object sender, EventArgs e)
		{
			this.Uptime.Text = $"Uptime: {TimeFormatting.FormatUptime()}";
			this.Latency.Text = $"Latency: {(this.Client.HeldObject == null ? -1 : ClientActions.GetLatency(this.Client.HeldObject))}ms";
			this.Memory.Text = $"Memory: {GetActions.GetMemory().ToString("0.00")}MB";
			this.ThreadCount.Text = $"Threads: {Process.GetCurrentProcess().Threads.Count}";
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
