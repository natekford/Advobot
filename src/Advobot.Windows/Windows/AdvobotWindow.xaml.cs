using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using System.Windows.Threading;
using Advobot.Interfaces;
using Advobot.Utilities;
using Advobot.Windows.Classes;
using Advobot.Windows.Classes.Controls;
using Advobot.Windows.Enums;
using Advobot.Windows.Utilities;
using AdvorangesUtils;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace Advobot.Windows.Windows
{
	/// <summary>
	/// Interaction logic for AdvobotApplication.xaml
	/// </summary>
	public partial class AdvobotWindow : Window
	{
		/// <summary>
		/// Holds a reference to the client even when it doesn't exist for XAML binding.
		/// </summary>
		public Holder<DiscordShardedClient> Client { get; private set; } = new Holder<DiscordShardedClient>();
		/// <summary>
		/// Holds a reference to the bot settings even when it doesn't exist for XAML binding.
		/// </summary>
		public Holder<IBotSettings> BotSettings { get; private set; } = new Holder<IBotSettings>();
		/// <summary>
		/// Holds a reference to the guild settings service even when it doesn't exist for XAML binding.
		/// </summary>
		public Holder<IGuildSettingsService> GuildSettings { get; private set; } = new Holder<IGuildSettingsService>();
		/// <summary>
		/// Holds a reference to the log service even when it doesn't exist for XAML binding.
		/// </summary>
		public Holder<ILogService> LogHolder { get; private set; } = new Holder<ILogService>();

		private ColorSettings _Colors = new ColorSettings();
		private LoginHandler _LoginHandler = new LoginHandler();
		private MenuType _LastButtonClicked;

		/// <summary>
		/// Creates an instance of advobotwindow.
		/// </summary>
		public AdvobotWindow()
		{
			InitializeComponent();
			//Make console output show up in the window
			Console.SetOut(new TextBoxStreamWriter(Output));
			//Start the timer that shows latency, memory usage, etc.
			((DispatcherTimer)Resources["ApplicationInformationTimer"]).Start();
			_LoginHandler.AbleToStart += Start;
		}

		private async Task EnableButtons(DiscordSocketClient client)
		{
			await Dispatcher.InvokeAsync(() =>
			{
				ButtonMenu.IsEnabled = true;
				OutputContextMenu.IsEnabled = true;
			});
		}
		private async Task Start()
		{
			await Dispatcher.InvokeAsync(async () =>
			{
				Client.HeldObject = _LoginHandler.Provider.GetRequiredService<DiscordShardedClient>();
				BotSettings.HeldObject = _LoginHandler.Provider.GetRequiredService<IBotSettings>();
				GuildSettings.HeldObject = _LoginHandler.Provider.GetRequiredService<IGuildSettingsService>();
				LogHolder.HeldObject = _LoginHandler.Provider.GetRequiredService<ILogService>();
				_Colors = ColorSettings.LoadUISettings();

				Client.HeldObject.ShardConnected += EnableButtons;
				await ClientUtils.StartAsync(Client.HeldObject);
			});
		}
		private async void AttemptToLogin(object sender, RoutedEventArgs e)
		{
			//Send null once to check if a path is set in the config
			//If it is set, then send null again to check for the bot key
			if (await _LoginHandler.AttemptToStart(null))
			{
				await _LoginHandler.AttemptToStart(null);
			}
		}
		private async void AcceptInput(object sender, RoutedEventArgs e)
		{
			var input = CommandHandler.GatherInput(InputBox);
			if (String.IsNullOrWhiteSpace(input))
			{
				return;
			}
			ConsoleUtils.WriteLine(input);

			if (!_LoginHandler.CanLogin)
			{
				await _LoginHandler.AttemptToStart(input);
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
			var input = TrustedUsersBox.Text;
			if (!ulong.TryParse(input, out var userId))
			{
				ConsoleUtils.WriteLine($"The given input '{input}' is not a valid ID.");
				return;
			}
			if (TrustedUsers.Items.OfType<TextBox>().Any(x => x?.Tag is ulong id && id == userId))
			{
				ConsoleUtils.WriteLine($"The given input '{input}' is already a trusted user.");
				return;
			}
			TrustedUsersBox.Text = null;
			if (Client.HeldObject.GetUser(userId) is SocketUser user)
			{
				TrustedUsers.Items.Add(AdvobotTextBox.CreateUserBox(user));
				TrustedUsers.Items.SortDescriptions.Clear();
				TrustedUsers.Items.SortDescriptions.Add(new SortDescription("Text", ListSortDirection.Ascending));
			}
		}
		private void RemoveTrustedUser(object sender, RoutedEventArgs e)
		{
			TrustedUsers.Items.Remove(TrustedUsers.SelectedItem);
		}
		private void SaveSettings(object sender, RoutedEventArgs e)
		{
			SavingUtils.SaveSettings(SettingsMenuDisplay, BotSettings.HeldObject);
			//In a task.run since the result is unimportant and unused
			Task.Run(async () => await ClientUtils.UpdateGameAsync(Client.HeldObject, BotSettings.HeldObject));
		}
		private void SaveSettingsWithCtrlS(object sender, KeyEventArgs e)
		{
			if (SavingUtils.IsCtrlS(e))
			{
				SaveSettings(sender, e);
			}
		}
		private void SaveColors(object sender, RoutedEventArgs e)
		{
			var c = _Colors;
			var children = ColorsMenuDisplay.GetChildren();
			foreach (var tb in children.OfType<AdvobotTextBox>())
			{
				if (!(tb.Tag is ColorTarget target))
				{
					continue;
				}

				var childText = tb.Text;
				var name = target.ToString().FormatTitle().ToLower();
				//Removing a brush
				if (String.IsNullOrWhiteSpace(childText))
				{
					if (c[target] != null)
					{
						c[target] = null;
						ConsoleUtils.WriteLine($"Successfully removed the custom color for {name}.");
					}
				}
				//Failed to add a brush
				else if (!BrushUtils.TryCreateBrush(childText, out var brush))
				{
					ConsoleUtils.WriteLine($"Invalid custom color supplied for {name}: '{childText}'.");
				}
				//Succeeding in adding a brush
				else if (!BrushUtils.CheckIfSameBrush(c[target], brush))
				{
					c[target] = brush;
					ConsoleUtils.WriteLine($"Successfully updated the custom color for {name}: '{childText} ({c[target].ToString()})'.");

					//Update the text here because if someone has the hex value for yellow but they put in Yellow as a string 
					//It won't update in the above if statement since they produce the same value
					tb.Text = c[target].ToString();
				}
			}
			//Has to go after the textboxes so the theme will be applied
			foreach (var cb in children.OfType<AdvobotComboBox>())
			{
				if (!(cb.SelectedItem is AdvobotTextBox tb) || !(tb.Tag is ColorTheme theme))
				{
					continue;
				}
				if (c.Theme == theme)
				{
					continue;
				}

				c.Theme = theme;
				ConsoleUtils.WriteLine($"Successfully updated the theme to {c.Theme.ToString().FormatTitle().ToLower()}.");
			}

			c.SaveSettings();
		}
		private void SaveColorsWithCtrlS(object sender, KeyEventArgs e)
		{
			if (SavingUtils.IsCtrlS(e))
			{
				SaveColors(sender, e);
			}
		}
		private void SaveOutput(object sender, RoutedEventArgs e)
		{
			ToolTipUtils.EnableTimedToolTip(Layout, SavingUtils.SaveFile(Output).GetReason());
		}
		private void ClearOutput(object sender, RoutedEventArgs e)
		{
			var caption = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyProductAttribute>().Product;
			switch (MessageBox.Show("Are you sure you want to clear the output window?", caption, MessageBoxButton.OKCancel))
			{
				case MessageBoxResult.OK:
					Output.Clear();
					return;
			}
		}
		private void SearchForFile(object sender, RoutedEventArgs e)
		{
			using (var dialog = new CommonOpenFileDialog { DefaultDirectory = FileUtils.GetBaseBotDirectory().FullName })
			{
				switch (dialog.ShowDialog())
				{
					case CommonFileDialogResult.Ok:
						if (SavingUtils.TryGetFileText(dialog.FileName, out var text, out var fileInfo))
						{
							new FileViewingWindow(this, text, fileInfo).ShowDialog();
						}
						break;
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
				case Modal.OutputSearch:
					new OutputSearchWindow(this).ShowDialog();
					break;
				//This modal should not be opened through this method, it should be opened through SearchForFile
				case Modal.FileViewing:
					return;
				default:
					throw new ArgumentException("invalid type supplied", nameof(m));
			}
		}
		private void OpenMenu(object sender, RoutedEventArgs e)
		{
			if (!(sender is FrameworkElement ele))
			{
				return;
			}

			//Hide everything so stuff doesn't overlap
			MainMenu.Visibility = Visibility.Collapsed;
			SettingsMenu.Visibility = Visibility.Collapsed;
			ColorsMenu.Visibility = Visibility.Collapsed;
			InfoMenu.Visibility = Visibility.Collapsed;

			var type = ele.Tag as MenuType? ?? default;
			if (type == _LastButtonClicked)
			{
				//If clicking the same button then resize the output window to the regular size
				ElementUtils.SetColSpan(Output, Grid.GetColumnSpan(Output) + (Layout.ColumnDefinitions.Count - 1));
				_LastButtonClicked = default;
			}
			else
			{
				//Resize the regular output window and have the menubox appear
				ElementUtils.SetColSpan(Output, Grid.GetColumnSpan(Output) - (Layout.ColumnDefinitions.Count - 1));
				_LastButtonClicked = type;

				switch (type)
				{
					case MenuType.Main:
						MainMenu.Visibility = Visibility.Visible;
						return;
					case MenuType.Info:
						InfoMenu.Visibility = Visibility.Visible;
						return;
					case MenuType.Settings:
						var s = BotSettings.HeldObject;
						var llSelected = LogLevel.Items.OfType<TextBox>()
							.SingleOrDefault(x => x?.Tag is LogSeverity ls && ls == s.LogLevel);

						AlwaysDownloadUsers.IsChecked = s.AlwaysDownloadUsers;
						Prefix.Text = s.Prefix;
						Game.Text = s.Game;
						Stream.Text = s.Stream;
						MessageCacheCount.StoredValue = s.MessageCacheCount;
						MaxUserGatherCount.StoredValue = s.MaxUserGatherCount;
						MaxMessageGatherSize.StoredValue = s.MaxMessageGatherSize;
						LogLevel.SelectedItem = llSelected;

						SettingsMenu.Visibility = Visibility.Visible;
						return;
					case MenuType.Colors:
						var c = _Colors;
						var tcbSelected = ThemesComboBox.Items.OfType<TextBox>()
							.SingleOrDefault(x => x?.Tag is ColorTheme t && t == c.Theme);

						ThemesComboBox.SelectedItem = tcbSelected;
						BaseBackground.Text = c[ColorTarget.BaseBackground]?.ToString();
						BaseForeground.Text = c[ColorTarget.BaseForeground]?.ToString();
						BaseBorder.Text = c[ColorTarget.BaseBorder]?.ToString();
						ButtonBackground.Text = c[ColorTarget.ButtonBackground]?.ToString();
						ButtonForeground.Text = c[ColorTarget.ButtonForeground]?.ToString();
						ButtonBorder.Text = c[ColorTarget.ButtonBorder]?.ToString();
						ButtonDisabledBackground.Text = c[ColorTarget.ButtonDisabledBackground]?.ToString();
						ButtonDisabledForeground.Text = c[ColorTarget.ButtonDisabledForeground]?.ToString();
						ButtonDisabledBorder.Text = c[ColorTarget.ButtonDisabledBorder]?.ToString();
						ButtonMouseOverBackground.Text = c[ColorTarget.ButtonMouseOverBackground]?.ToString();
						JsonDigits.Text = c[ColorTarget.JsonDigits]?.ToString();
						JsonValue.Text = c[ColorTarget.JsonValue]?.ToString();
						JsonParamName.Text = c[ColorTarget.JsonParamName]?.ToString();

						ColorsMenu.Visibility = Visibility.Visible;
						return;
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
			var caption = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyProductAttribute>().Product;
			switch (MessageBox.Show("Are you sure you want to disconnect the bot?", caption, MessageBoxButton.OKCancel))
			{
				case MessageBoxResult.OK:
					ClientUtils.DisconnectBotAsync(Client.HeldObject);
					return;
			}
		}
		private void Restart(object sender, RoutedEventArgs e)
		{
			var caption = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyProductAttribute>().Product;
			switch (MessageBox.Show("Are you sure you want to restart the bot?", caption, MessageBoxButton.OKCancel))
			{
				case MessageBoxResult.OK:
					Process.Start(Application.ResourceAssembly.Location);
					Application.Current.Shutdown();
					return;
			}
		}
		private void Pause(object sender, RoutedEventArgs e)
		{
			if (BotSettings.HeldObject.Pause)
			{
				PauseButton.Content = "Pause";
				ConsoleUtils.WriteLine("The bot is now unpaused.");
			}
			else
			{
				PauseButton.Content = "Unpause";
				ConsoleUtils.WriteLine("The bot is now paused.");
			}
			BotSettings.HeldObject.Pause = !BotSettings.HeldObject.Pause;
		}
		private void UpdateApplicationInfo(object sender, EventArgs e)
		{
			Uptime.Text = $"Uptime: {Formatting.GetUptime()}";
			Latency.Text = $"Latency: {(Client.HeldObject == null ? -1 : Client.HeldObject.Latency)}ms";
			Memory.Text = $"Memory: {IOUtils.GetMemory().ToString("0.00")}MB";
			ThreadCount.Text = $"Threads: {Process.GetCurrentProcess().Threads.Count}";
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
