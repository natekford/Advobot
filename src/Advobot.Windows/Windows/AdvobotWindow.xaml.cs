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
using Advobot.Classes;
using Advobot.Interfaces;
using Advobot.Utilities;
using Advobot.Windows.Classes;
using Advobot.Windows.Classes.Controls;
using Advobot.Windows.Enums;
using Advobot.Windows.Utilities;
using AdvorangesUtils;
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
		private static readonly string _Caption = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyProductAttribute>().Product;

		/// <summary>
		/// Holds a reference to the client even when it doesn't exist for XAML binding.
		/// </summary>
		public Holder<DiscordShardedClient> Client { get; private set; } = new Holder<DiscordShardedClient>();
		/// <summary>
		/// Holds a reference to the bot settings even when it doesn't exist for XAML binding.
		/// </summary>
		public Holder<IBotSettings> BotSettings { get; private set; } = new Holder<IBotSettings>();
		/// <summary>
		/// Holds a reference to the log service even when it doesn't exist for XAML binding.
		/// </summary>
		public Holder<ILogService> LogHolder { get; private set; } = new Holder<ILogService>();

		private MenuType _LastButtonClicked;
		private IterableServiceProvider _Provider;
		private ColorSettings _Colors;
		private ILowLevelConfig _Config;

		/// <summary>
		/// Creates an instance of <see cref="AdvobotWindow"/>.
		/// </summary>
		public AdvobotWindow(ILowLevelConfig config)
		{
			InitializeComponent();
			_Config = config;
			//Make console output show up in the window
			Console.SetOut(new TextBoxStreamWriter(Output));
			//Start the timer that shows latency, memory usage, etc.
			((DispatcherTimer)Resources["ApplicationInformationTimer"]).Start();
		}

		private async Task<bool> AttemptToStart(string input)
		{
			//Null means it's from the loaded event, which is start up so it's telling the bot to look up the config value
			var startup = input == null && !(_Config.ValidatedPath && _Config.ValidatedKey);
			var set = false;
			if (!_Config.ValidatedPath)
			{
				//Set startup to whatever returned value is so it can be used in GotKey, and then after GotKey in the last if statement
				set = _Config.ValidatePath(input, startup);
				if (_Config.ValidatedPath)
				{
					_Provider = new IterableServiceProvider(CreationUtils.CreateDefaultServices(_Config), true);
				}
			}
			else if (!_Config.ValidatedKey)
			{
				set = await _Config.ValidateBotKey(input, startup, Restart);
			}

			if (set && _Config.ValidatedKey && _Config.ValidatedPath)
			{
				//Was getting some access exceptions on a computer when this was not inside a dispatcher call
				await Dispatcher.InvokeAsync(async () =>
				{
					//Retrieve the command handler to initialize it.
					var cmd = _Provider.GetRequiredService<ICommandHandlerService>();
					Client.HeldObject = _Provider.GetRequiredService<DiscordShardedClient>();
					BotSettings.HeldObject = _Provider.GetRequiredService<IBotSettings>();
					LogHolder.HeldObject = _Provider.GetRequiredService<ILogService>();

					foreach (var dbUser in _Provider.OfType<IUsesDatabase>())
					{
						dbUser.Start();
					}
					await _Config.StartAsync(Client).CAF();
				});

				_Colors = ColorSettings.Load(_Config);
				ButtonMenu.IsEnabled = true;
				OutputContextMenu.IsEnabled = true;
			}
			return set;
		}
		private async void AttemptToLogin(object sender, RoutedEventArgs e)
		{
			//Send null once to check if a path is set in the config
			//If it is set, then send null again to check for the bot key
			if (await AttemptToStart(null).CAF())
			{
				await AttemptToStart(null).CAF();
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
			if (!(_Config.ValidatedPath && _Config.ValidatedKey))
			{
				await AttemptToStart(input).CAF();
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
			if (!ulong.TryParse(TrustedUsersBox.Text, out var userId))
			{
				ConsoleUtils.WriteLine($"The given input '{TrustedUsersBox.Text}' is not a valid ID.");
				return;
			}
			if (TrustedUsers.Items.OfType<TextBox>().Any(x => x?.Tag is ulong id && id == userId))
			{
				ConsoleUtils.WriteLine($"The given input '{TrustedUsersBox.Text}' is already a trusted user.");
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
		private async void SaveSettings(object sender, RoutedEventArgs e)
		{
			SavingUtils.SaveSettings(BotSettings.HeldObject, SettingsMenuDisplay, BotSettings.HeldObject);
			await ClientUtils.UpdateGameAsync(Client, BotSettings.HeldObject).CAF();
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
				if (!(cb.SelectedItem is AdvobotTextBox tb && tb.Tag is ColorTheme theme))
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

			c.SaveSettings(BotSettings.HeldObject);
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
			ToolTipUtils.EnableTimedToolTip(Layout, SavingUtils.SaveFile(BotSettings.HeldObject, Output).GetReason());
		}
		private void ClearOutput(object sender, RoutedEventArgs e)
		{
			if (MessageBox.Show("Are you sure you want to clear the output window?", _Caption, MessageBoxButton.OKCancel) == MessageBoxResult.OK)
			{
				Output.Clear();
			}
		}
		private void SearchForFile(object sender, RoutedEventArgs e)
		{
			using (var d = new CommonOpenFileDialog { DefaultDirectory = BotSettings.HeldObject.BaseBotDirectory.FullName })
			{
				if (d.ShowDialog() == CommonFileDialogResult.Ok && SavingUtils.TryGetFileText(d.FileName, out var text, out var file))
				{
					var type = _Provider.GetRequiredService<IGuildSettingsFactory>().GetSettings().First().Value.DeclaringType;
					new FileViewingWindow(this, BotSettings.HeldObject, type, file, text).ShowDialog();
				}
			}
		}
		private void OpenModal(object sender, RoutedEventArgs e)
		{
			if (!(sender is FrameworkElement ele && ele.Tag is Modal m))
			{
				return;
			}

			switch (m)
			{
				case Modal.OutputSearch:
					new OutputSearchWindow(this, BotSettings.HeldObject).ShowDialog();
					break;
				//This modal should not be opened through this method, it should be opened through SearchForFile
				case Modal.FileViewing:
					return;
				default:
					throw new ArgumentException("Invalid modal type supplied.", nameof(m));
			}
		}
		private void OpenMenu(object sender, RoutedEventArgs e)
		{
			if (!(sender is FrameworkElement ele && ele.Tag is MenuType type))
			{
				return;
			}

			//Hide everything so stuff doesn't overlap
			MainMenu.Visibility = SettingsMenu.Visibility = ColorsMenu.Visibility = InfoMenu.Visibility = Visibility.Collapsed;
			if (type == _LastButtonClicked)
			{
				//If clicking the same button then resize the output window to the regular size
				ElementUtils.SetColSpan(Output, Grid.GetColumnSpan(Output) + (Layout.ColumnDefinitions.Count - 1));
				_LastButtonClicked = default;
				return;
			}

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

					Prefix.Text = s.Prefix;
					Game.Text = s.Game;
					Stream.Text = s.Stream;
					MaxUserGatherCount.StoredValue = s.MaxUserGatherCount;
					MaxMessageGatherSize.StoredValue = s.MaxMessageGatherSize;

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
		private void OpenHyperLink(object sender, RequestNavigateEventArgs e)
		{
			Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
			e.Handled = true;
		}
		private async void Disconnect(object sender, RoutedEventArgs e)
		{
			if (MessageBox.Show("Are you sure you want to disconnect the bot?", _Caption, MessageBoxButton.OKCancel) == MessageBoxResult.OK)
			{
				await ClientUtils.DisconnectBotAsync(Client).CAF();
			}
		}
		private async void Restart(object sender, RoutedEventArgs e)
		{
			if (MessageBox.Show("Are you sure you want to restart the bot?", _Caption, MessageBoxButton.OKCancel) == MessageBoxResult.OK)
			{
				await Restart(Client, BotSettings.HeldObject).CAF();
			}
		}
		private static async Task Restart(BaseSocketClient client, IRestartArgumentProvider provider)
		{
			if (client != null)
			{
				await client.StopAsync().CAF();
			}
			Process.Start(Application.ResourceAssembly.Location, provider.RestartArguments);
			Environment.Exit(0);
		}
		private void Pause(object sender, RoutedEventArgs e)
		{
			PauseButton.Content = BotSettings.HeldObject.Pause ? "Pause" : "Unpause";
			ConsoleUtils.WriteLine($"The bot is now {(BotSettings.HeldObject.Pause ? "unpaused" : "paused")}.");
			BotSettings.HeldObject.Pause = !BotSettings.HeldObject.Pause;
		}
		private void UpdateApplicationInfo(object sender, EventArgs e)
		{
			Uptime.Text = $"Uptime: {FormattingUtils.GetUptime()}";
			Latency.Text = $"Latency: {(Client.HeldObject == null ? -1 : Client.HeldObject.Latency)}ms";
			Memory.Text = $"Memory: {IOUtils.GetMemory().ToString("0.00")}MB";
			ThreadCount.Text = $"Threads: {Process.GetCurrentProcess().Threads.Count}";
		}
		private void MoveToolTip(object sender, MouseEventArgs e)
		{
			if (!(sender is FrameworkElement fe && fe.ToolTip is ToolTip tt))
			{
				return;
			}

			var pos = e.GetPosition(fe);
			tt.HorizontalOffset = pos.X + 10;
			tt.VerticalOffset = pos.Y + 10;
		}
	}
}
