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
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;

namespace Advobot.UILauncher
{
	/// <summary>
	/// Interaction logic for AdvobotApplication.xaml
	/// </summary>
	public partial class AdvobotWindow : Window
	{
		public Holder<IDiscordClient> Client { get; private set; } = new Holder<IDiscordClient>();
		public Holder<IBotSettings> BotSettings { get; private set; } = new Holder<IBotSettings>();
		public Holder<ILogService> LogHolder { get; private set; } = new Holder<ILogService>();
		internal Holder<ColorSettings> Colors { get; private set; } = new Holder<ColorSettings>();

		private DispatcherTimer _Timer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 0, 0, 500) };
		private CancellationTokenSource _ToolTipCancellationTokenSource;
		private MenuType _LastButtonClicked;

		public AdvobotWindow()
		{
			InitializeComponent();
#if DEBUG
			VerifyEveryBindingPathIsValid(this.Layout);
#endif

			Console.SetOut(new TextBoxStreamWriter(this.Output));
			ColorSettings.SwitchElementColorOfChildren(this.Layout);
		}

		private static Dictionary<Type, DependencyProperty[]> _Props = new Dictionary<Type, DependencyProperty[]>();
		private void VerifyEveryBindingPathIsValid(DependencyObject obj)
		{
			foreach (var child in obj.GetChildren())
			{
				var t = child.GetType();
				if (!_Props.TryGetValue(t, out var dependencyProperties))
				{
					var p = t.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy).Where(x => x.FieldType == typeof(DependencyProperty));
					_Props.Add(t, (dependencyProperties = p.Select(x => x.GetValue(null)).Cast<DependencyProperty>().ToArray()));
				}

				if (child is FrameworkElement fe)
				{
					foreach (var dependencyProperty in dependencyProperties)
					{
						var binding = fe.GetBindingExpression(dependencyProperty);
						if (false
							|| binding == null 
							|| binding.ParentBinding.Path.Path == null
							|| binding.Status != BindingStatus.PathError)
						{
							continue;
						}

						var pathParts = binding.ParentBinding.Path.Path.Split('.');
						var potentialSources = new[] { fe.DataContext, binding.ParentBinding.Source, binding.ParentBinding.RelativeSource, binding.ResolvedSource };
						if (true
							&& !VerifyPathIsOnObject(fe.DataContext, pathParts)
							&& !VerifyPathIsOnObject(binding.ParentBinding.Source, pathParts)
							&& !VerifyPathIsOnObject(binding.ParentBinding.RelativeSource, pathParts)
							&& !VerifyPathIsOnObject(binding.ResolvedSource, pathParts))
						{
							throw new ArgumentException($"Invalid path supplied on {fe.Name ?? fe.GetType().Name}: {binding.ParentBinding.Path.Path}");
						}
					}
				}
				VerifyEveryBindingPathIsValid(child);
			}
		}
		private bool VerifyPathIsOnObject(object obj, string[] pathParts)
		{
			if (obj == null)
			{
				return false;
			}

			var currentType = obj.GetType();
			for (int i = 0; i < pathParts.Length; ++i)
			{
				var properties = currentType.GetProperties(BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
				var targettedProperty = properties.SingleOrDefault(x => x.Name == pathParts[i]);
				if (targettedProperty == null)
				{
					return false;
				}
				else if (i == pathParts.Length - 1)
				{
					return true;
				}

				currentType = targettedProperty.PropertyType;
			}
			return false;
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
						var c = Colors.HeldObject;
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
						this.FilesTreeView.ItemsSource = UIModification.MakeGuildTreeViewItemsSource(await Client.HeldObject.GetGuildsAsync());
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
			var text = this.InputBox.Text;
			if (String.IsNullOrWhiteSpace(text))
			{
				this.InputButton.IsEnabled = false;
				return;
			}

			if (e.Key == Key.Enter || e.Key == Key.Return)
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
			this.Latency.Text = $"Latency: {ClientActions.GetLatency(Client.HeldObject)}ms";
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
			var c = Colors.HeldObject;
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

					if (!UIModification.CheckIfTwoBrushesAreTheSame(c[target], brush))
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
						ConsoleActions.WriteLine("Successfully updated the theme type.");
					}
				}
			}

			c.SaveSettings();
		}
		private async void SaveSettings(object sender, RoutedEventArgs e)
		{
			await SettingModification.SaveSettings(this.SettingsMenu, Client.HeldObject, BotSettings.HeldObject);
		}

		private void OpenSpecificFileLayout(object sender, RoutedEventArgs e)
		{
			if (UIModification.TryToAppendText(this.SpecificFileOutput, sender))
			{
				UIModification.SetRowSpan(this.FilesMenu, Grid.GetRowSpan(this.FilesMenu) + (this.Layout.RowDefinitions.Count - 1));
				this.SpecificFileOutput.Visibility = Visibility.Visible;
				this.SaveFileButton.Visibility = Visibility.Visible;
				this.CloseFileButton.Visibility = Visibility.Visible;
				this.FileSearchButton.Visibility = Visibility.Collapsed;
			}
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
					Client.HeldObject = provider.GetRequiredService<IDiscordClient>();
					BotSettings.HeldObject = provider.GetRequiredService<IBotSettings>();
					LogHolder.HeldObject = provider.GetRequiredService<ILogService>();
					Colors.HeldObject = ColorSettings.LoadUISettings();
				}
				else
				{
					_StartUp = false;
				}
			}
			else if (!_GotKey)
			{
				if (await Config.ValidateBotKey(Client.HeldObject, input, _StartUp))
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
				if (await Config.ValidateBotKey(Client.HeldObject, null, _StartUp))
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
				await ClientActions.StartAsync(Client.HeldObject);
			}
		}

		public async Task MakeFollowingToolTip(string text, int timeInMS = 2500)
		{
			this.ActualToolTip.Content = text;
			this.Layout.MouseMove += (sender, e) =>
			{
				var point = System.Windows.Forms.Control.MousePosition;
				this.ActualToolTip.HorizontalOffset = point.X;
				this.ActualToolTip.VerticalOffset = point.Y;
			};
			UIModification.ToggleToolTip(this.ActualToolTip);

			_ToolTipCancellationTokenSource?.Cancel();
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

				UIModification.ToggleToolTip(this.ActualToolTip);
			});
		}

		public static bool IsCtrlS(KeyEventArgs e)
		{
			return e.Key == Key.S && e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Control);
		}
		private void SaveSettingsWithCtrlS(object sender, KeyEventArgs e)
		{
			if (IsCtrlS(e))
			{
				SaveSettings(sender, e);
			}
		}
		private void SaveColorsWithCtrlS(object sender, KeyEventArgs e)
		{
			if (IsCtrlS(e))
			{
				SaveColors(sender, e);
			}
		}
		private void SaveFileWithCtrlS(object sender, KeyEventArgs e)
		{
			if (IsCtrlS(e))
			{
				SaveFile(sender, e);
			}
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

		private void OpenFileSearch(object sender, RoutedEventArgs e)
		{
		}
		private void CloseFileSearch(object sender, RoutedEventArgs e)
		{
		}
		private void SearchForFile(object sender, RoutedEventArgs e)
		{

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
			var fileSaveStatus = UIBotWindowLogic.SaveFile(this.SpecificFileOutput).GetReason();
			await MakeFollowingToolTip(fileSaveStatus);
		}
	}
}
