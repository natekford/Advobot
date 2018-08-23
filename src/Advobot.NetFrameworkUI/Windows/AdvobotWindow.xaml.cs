using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using System.Windows.Threading;
using Advobot.Classes;
using Advobot.Interfaces;
using Advobot.NetFrameworkUI.Classes;
using Advobot.NetFrameworkUI.Classes.Colors;
using Advobot.NetFrameworkUI.Enums;
using Advobot.NetFrameworkUI.Utilities;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace Advobot.NetFrameworkUI.Windows
{
	/// <summary>
	/// Interaction logic for AdvobotApplication.xaml
	/// </summary>
	public partial class AdvobotWindow : Window, INotifyPropertyChanged
	{
		private static readonly string _Caption = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyProductAttribute>().Product;

		/// <summary>
		/// Holds a reference to the client even when it doesn't exist for XAML binding.
		/// </summary>
		public DiscordShardedClient Client
		{
			get => _Client;
			set
			{
				_Client = value;
				NotifyPropertyChanged();
			}
		}
		private DiscordShardedClient _Client;
		/// <summary>
		/// Holds a reference to the bot settings even when it doesn't exist for XAML binding.
		/// </summary>
		public IBotSettings BotSettings
		{
			get => _BotSettings;
			set
			{
				_BotSettings = value;
				NotifyPropertyChanged();
			}
		}
		private IBotSettings _BotSettings;
		/// <summary>
		/// Holds a reference to the log service even when it doesn't exist for XAML binding.
		/// </summary>
		public ILogService LogService
		{
			get => _LogService;
			set
			{
				_LogService = value;
				NotifyPropertyChanged();
			}
		}
		private ILogService _LogService;
		/// <summary>
		/// Holds a reference to the color settings even when they don't exist for XAML binding.
		/// </summary>
		public NetFrameworkColorSettings Colors
		{
			get => _Colors;
			set
			{
				_Colors = value;
				NotifyPropertyChanged();
			}
		}
		private NetFrameworkColorSettings _Colors;
		/// <summary>
		/// The currently open menu in the UI.
		/// </summary>
		public string CurrentOpenMenu
		{
			get => _CurrentOpenMenu;
			set
			{
				_CurrentOpenMenu = value;
				NotifyPropertyChanged();
			}
		}
		private string _CurrentOpenMenu;

		private IterableServiceProvider _Provider;
		private ILowLevelConfig _Config;

		/// <summary>
		/// Command for opening menus.
		/// </summary>
		public ICommand MenuCommand => _MenuCommand ?? (_MenuCommand = new Command<string>(name =>
		{
			CurrentOpenMenu = (CurrentOpenMenu == name) ? null : name;
		}));
		private ICommand _MenuCommand;

		/// <summary>
		/// Notifies that the held object has been updated.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

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
			}
			else if (!_Config.ValidatedKey)
			{
				set = await _Config.ValidateBotKey(input, startup, UIUtils.Restart);
			}

			if (set && _Config.ValidatedKey && _Config.ValidatedPath)
			{
				_Provider = new IterableServiceProvider(_Config.CreateDefaultServices(), true);
				Client = _Provider.GetRequiredService<DiscordShardedClient>();
				BotSettings = _Provider.GetRequiredService<IBotSettings>();
				LogService = _Provider.GetRequiredService<ILogService>();
				Colors = NetFrameworkColorSettings.Load<NetFrameworkColorSettings>(BotSettings);

				foreach (var dbUser in _Provider.OfType<IUsesDatabase>())
				{
					dbUser.Start();
				}
				ButtonMenu.IsEnabled = true;
				OutputContextMenu.IsEnabled = true;
				await _Config.StartAsync(Client).CAF();
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
			var input = UIUtils.GatherInput(InputBox);
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
		private void SaveSettings(object sender, RoutedEventArgs e)
		{
			BotSettings.SaveSettings(BotSettings);
			ConsoleUtils.WriteLine("Successfully saved bot settings.");
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
			Colors.SaveSettings(BotSettings);
			ConsoleUtils.WriteLine("Successfully saved color settings.");
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
			ToolTipUtils.EnableTimedToolTip(Layout, SavingUtils.SaveFile(BotSettings, Output).GetReason());
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
			using (var d = new CommonOpenFileDialog { DefaultDirectory = BotSettings.BaseBotDirectory.FullName })
			{
				if (d.ShowDialog() == CommonFileDialogResult.Ok && SavingUtils.TryGetFileText(d.FileName, out var text, out var file))
				{
					var type = _Provider.GetRequiredService<IGuildSettingsFactory>().GetSettings().First().Value.DeclaringType;
					new FileViewingWindow(this, BotSettings, type, file, text).ShowDialog();
				}
			}
		}
		private void OpenModal(object sender, RoutedEventArgs e)
		{
			switch ((Modal)((FrameworkElement)sender).Tag)
			{
				case Modal.OutputSearch:
					new OutputSearchWindow(this, BotSettings).ShowDialog();
					break;
				//This modal should not be opened through this method, it should be opened through SearchForFile
				case Modal.FileViewing:
					return;
				default:
					throw new ArgumentException("Invalid modal type supplied.");
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
				await UIUtils.Restart(Client, BotSettings).CAF();
			}
		}
		private void Pause(object sender, RoutedEventArgs e)
		{
			PauseButton.Content = BotSettings.Pause ? "Pause" : "Unpause";
			ConsoleUtils.WriteLine($"The bot is now {(BotSettings.Pause ? "unpaused" : "paused")}.");
			BotSettings.Pause = !BotSettings.Pause;
		}
		private void UpdateApplicationInfo(object sender, EventArgs e)
		{
			Uptime.Text = $"Uptime: {FormattingUtils.GetUptime()}";
			Latency.Text = $"Latency: {(Client?.CurrentUser == null ? -1 : Client.Latency)}ms";
			Memory.Text = $"Memory: {IOUtils.GetMemory().ToString("0.00")}MB";
			ThreadCount.Text = $"Threads: {Advobot.Utilities.Utils.GetThreadCount()}";
		}
		private void MoveToolTip(object sender, MouseEventArgs e)
		{
			var fe = (FrameworkElement)sender;
			var tt = (ToolTip)fe.ToolTip;
			var pos = e.GetPosition(fe);
			tt.HorizontalOffset = pos.X + 10;
			tt.VerticalOffset = pos.Y + 10;
		}
		private void UserListModified(object sender, UserListModificationEventArgs e)
		{
			BotSettings.ModifyList((string)((FrameworkElement)sender).Tag, e.Value, e.Add);
		}
		private void NotifyPropertyChanged([CallerMemberName] string name = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}
	}
}
