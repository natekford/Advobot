using Advobot.Actions;
using Discord;
using ICSharpCode.AvalonEdit;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

//This entire file *should* be able to be removed from the solution with no adverse effects aside from the single error in Advobot_Program.cs
namespace Advobot
{
	namespace UserInterface
	{

	}
	//Probably should split this up into like 10 classes
	public class BotWindow : Window
	{
		private readonly Grid _Layout = new Grid();
		private readonly ToolTip _ToolTip = new ToolTip { Placement = PlacementMode.Relative };

		#region Input
		private readonly Grid _InputLayout = new Grid();
		//Max height has to be set here as a large number to a) not get in the way and b) not crash when resized small. I don't want to use a RTB for input.
		private readonly TextBox _Input = new MyTextBox { TextWrapping = TextWrapping.Wrap, MaxLength = 250, MaxLines = 5, MaxHeight = 1000, };
		private readonly Button _InputButton = new MyButton { Content = "Enter", IsEnabled = false, };
		#endregion

		#region Output
		private readonly MenuItem _OutputContextMenuSearch = new MenuItem { Header = "Search For...", };
		private readonly MenuItem _OutputContextMenuSave = new MenuItem { Header = "Save Output Log", };
		private readonly MenuItem _OutputContextMenuClear = new MenuItem { Header = "Clear Output Log", };
		private readonly MyTextBox _Output = new MyTextBox
		{
			VerticalScrollBarVisibility = ScrollBarVisibility.Visible,
			TextWrapping = TextWrapping.Wrap,
			IsReadOnly = true,
		};

		private readonly Grid _OutputSearchLayout = new Grid { Background = UIModification.MakeBrush("#BF000000"), Visibility = Visibility.Collapsed, };
		private readonly Grid _OutputSearchTextLayout = new Grid();
		private readonly TextBox _OutputSearchResults = new MyTextBox { VerticalScrollBarVisibility = ScrollBarVisibility.Visible, IsReadOnly = true, };
		private readonly ComboBox _OutputSearchComboBox = new MyComboBox { IsEditable = true, };
		private readonly Button _OutputSearchButton = new MyButton { Content = "Search", };
		private readonly Button _OutputSearchCloseButton = new MyButton { Content = "Close", };
		#endregion

		#region Buttons
		private readonly Grid _ButtonLayout = new Grid();
		private readonly Button _MainButton = new MyButton { Content = "Main", Tag = MenuType.Main, };
		private readonly Button _InfoButton = new MyButton { Content = "Info", Tag = MenuType.Info, };
		private readonly Button _SettingsButton = new MyButton { Content = "Settings", Tag = MenuType.Settings, };
		private readonly Button _ColorsButton = new MyButton { Content = "Colors", Tag = MenuType.Colors, };
		private readonly Button _DMButton = new MyButton { Content = "DMs", Tag = MenuType.DMs, };
		private readonly Button _FileButton = new MyButton { Content = "Files", Tag = MenuType.Files, };
		private MenuType _LastButtonClicked;
		#endregion

		#region Main Menu
		private readonly Grid _MainMenuLayout = new Grid { Visibility = Visibility.Collapsed, };
		private readonly RichTextBox _MainMenuOutput = new MyRichTextBox
		{
			Document = UIModification.MakeMainMenu(),
			IsReadOnly = true,
			IsDocumentEnabled = true,
		};
		private readonly Button _DisconnectButton = new MyButton { Content = "Disconnect", };
		private readonly Button _RestartButton = new MyButton { Content = "Restart", };
		private readonly Button _PauseButton = new MyButton { Content = "Pause",};
		#endregion

		#region Settings Menu
		private readonly Grid _SettingsLayout = new Grid { Visibility = Visibility.Collapsed, };
		private readonly Button _SettingsSaveButton = new MyButton { Content = "Save Settings" };

		private readonly SettingInMenu _DownloadUsersSetting = new SettingInMenu
		{
			Setting = new Viewbox
			{
				Child = new CheckBox
				{
					Tag = SettingOnBot.AlwaysDownloadUsers,
				},
				HorizontalAlignment = HorizontalAlignment.Left,
				VerticalAlignment = VerticalAlignment.Center,
				Tag = SettingOnBot.AlwaysDownloadUsers,
			},
			Title = UIModification.MakeTitle("Download Users:", "This automatically puts users in the bots cache. With it off, many commands will not work since I haven't added in a manual way to download users."),
		};
		private readonly SettingInMenu _PrefixSetting = new SettingInMenu
		{
			Setting = UIModification.MakeSetting(SettingOnBot.Prefix, 10),
			Title = UIModification.MakeTitle("Prefix:", "The prefix which is needed to be said before commands."),
		};
		private readonly SettingInMenu _BotOwnerSetting = new SettingInMenu
		{
			Setting = UIModification.MakeSetting(SettingOnBot.BotOwnerID, 18),
			Title = UIModification.MakeTitle("Bot Owner:", "The number here is the ID of a user. The bot owner can use some additional commands."),
		};
		private readonly SettingInMenu _GameSetting = new SettingInMenu
		{
			Setting = UIModification.MakeSetting(SettingOnBot.Game, 100),
			Title = UIModification.MakeTitle("Game:", "Changes what the bot says it's playing."),
		};
		private readonly SettingInMenu _StreamSetting = new SettingInMenu
		{
			Setting = UIModification.MakeSetting(SettingOnBot.Stream, 50),
			Title = UIModification.MakeTitle("Stream:", "Can set whatever stream you want as long as it's a valid Twitch.tv stream."),
		};
		private readonly SettingInMenu _ShardSetting = new SettingInMenu
		{
			Setting = UIModification.MakeSetting(SettingOnBot.ShardCount, 3),
			Title = UIModification.MakeTitle("Shard Count:", "Each shard can hold up to 2500 guilds."),
		};
		private readonly SettingInMenu _MessageCacheSetting = new SettingInMenu
		{
			Setting = UIModification.MakeSetting(SettingOnBot.MessageCacheCount, 6),
			Title = UIModification.MakeTitle("Message Cache:", "The amount of messages the bot will hold in its cache."),
		};
		private readonly SettingInMenu _UserGatherCountSetting = new SettingInMenu
		{
			Setting = UIModification.MakeSetting(SettingOnBot.MaxUserGatherCount, 5),
			Title = UIModification.MakeTitle("Max User Gather:", "Limits the amount of users a command can modify at once."),
		};
		private readonly SettingInMenu _MessageGatherSizeSetting = new SettingInMenu
		{
			Setting = UIModification.MakeSetting(SettingOnBot.MaxMessageGatherSize, 7),
			Title = UIModification.MakeTitle("Max Msg Gather:", "This is in bytes, which to be very basic is roughly two bytes per character."),
		};
		private readonly SettingInMenu _LogLevelComboBox = new SettingInMenu
		{
			Setting = new MyComboBox { ItemsSource = UIModification.MakeComboBoxSourceOutOfEnum(typeof(Discord.LogSeverity)), Tag = SettingOnBot.LogLevel, },
			Title = UIModification.MakeTitle("Log Level:", "Certain events in the Discord library used in this bot have a required log level to be said in the console."),
		};
		private readonly SettingInMenu _TrustedUsersAdd = new SettingInMenu
		{
			Setting = new Grid() { Tag = SettingOnBot.TrustedUsers, },
			Title = UIModification.MakeTitle("Trusted Users:", "Some commands can only be run by the bot owner or user IDs that they have designated as trust worthy."),
		};
		private readonly TextBox _TrustedUsersAddBox = UIModification.MakeSetting(SettingOnBot.TrustedUsers, 18);
		private readonly Button _TrustedUsersAddButton = new MyButton { Content = "+", };
		private readonly SettingInMenu _TrustedUsersRemove = new SettingInMenu
		{
			Setting = new Grid() { Tag = SettingOnBot.TrustedUsers, },
			Title = UIModification.MakeTitle("", ""),
		};
		private readonly ComboBox _TrustedUsersComboBox = new MyComboBox { Tag = SettingOnBot.TrustedUsers, };
		private readonly Button _TrustedUsersRemoveButton = new MyButton { Content = "-", };
		#endregion

		#region Colors Menu
		private readonly Grid _ColorsLayout = new Grid { Visibility = Visibility.Collapsed, };
		private readonly Button _ColorsSaveButton = new MyButton { Content = "Save Colors", };
		#endregion

		#region Info Menu
		private readonly Grid _InfoLayout = new Grid { Visibility = Visibility.Collapsed, };
		private readonly RichTextBox _InfoOutput = new MyRichTextBox
		{
			BorderThickness = new Thickness(0, 1, 0, 1),
			IsReadOnly = true,
			IsDocumentEnabled = true,
		};
		#endregion

		#region Guild Menu
		private readonly Grid _FileLayout = new Grid { Visibility = Visibility.Collapsed, };
		private readonly RichTextBox _FileOutput = new MyRichTextBox { IsReadOnly = true, IsDocumentEnabled = true, };
		private readonly TreeView _FileTreeView = new TreeView();
		private readonly Button _FileSearchButton = new MyButton { Content = "Search Guilds", };

		private readonly Grid _SpecificFileLayout = new Grid { Visibility = Visibility.Collapsed, };
		private readonly MenuItem _SpecificFileContextMenuSave = new MenuItem { Header = "Save File", };
		private readonly TextEditor _SpecificFileDisplay = new TextEditor
		{
			Background = null,
			Foreground = null,
			BorderBrush = null,
			VerticalScrollBarVisibility = ScrollBarVisibility.Visible,
			WordWrap = true,
			ShowLineNumbers = true,
		};
		private readonly Button _SpecificFileCloseButton = new MyButton { Content = "Close Menu", };

		private readonly Grid _GuildSearchLayout = new Grid { Background = UIModification.MakeBrush("#BF000000"), Visibility = Visibility.Collapsed };
		private readonly Grid _GuildSearchTextLayout = new Grid();
		private readonly Viewbox _GuildSearchNameHeader = UIModification.MakeStandardViewBox("Guild Name:");
		private readonly TextBox _GuildSearchNameInput = new MyTextBox { MaxLength = 100, };
		private readonly Viewbox _GuildSearchIDHeader = UIModification.MakeStandardViewBox("ID:");
		private readonly TextBox _GuildSearchIDInput = new MyNumberBox { MaxLength = 18, };
		private readonly ComboBox _GuildSearchFileComboBox = new MyComboBox { ItemsSource = UIModification.MakeComboBoxSourceOutOfEnum(typeof(FileType)), };
		private readonly Button _GuildSearchSearchButton = new MyButton { Content = "Search", };
		private readonly Button _GuildSearchCloseButton = new MyButton { Content = "Close", };
		#endregion

		#region DM Menu
		private readonly Grid _DMLayout = new Grid { Visibility = Visibility.Collapsed, };
		private readonly RichTextBox _DMOutput = new MyRichTextBox { IsReadOnly = true, IsDocumentEnabled = true, };
		private readonly TreeView _DMTreeView = new TreeView();
		private readonly Button _DMSearchButton = new MyButton { Content = "Search DMs", };

		private readonly Grid _SpecificDMLayout = new Grid { Visibility = Visibility.Collapsed, };
		private readonly TextEditor _SpecificDMDisplay = new TextEditor
		{
			Background = null,
			Foreground = null,
			BorderBrush = null,
			VerticalScrollBarVisibility = ScrollBarVisibility.Visible,
			WordWrap = true,
			ShowLineNumbers = true,
			IsReadOnly = true,
		};
		private readonly Button _SpecificDMCloseButton = new MyButton { Content = "Close Menu", };

		private readonly Grid _DMSearchLayout = new Grid { Background = UIModification.MakeBrush("#BF000000"), Visibility = Visibility.Collapsed };
		private readonly Grid _DMSearchTextLayout = new Grid();
		private readonly Viewbox _DMSearchNameHeader = UIModification.MakeStandardViewBox("Username:");
		private readonly TextBox _DMSearchNameInput = new MyTextBox { MaxLength = 32, };
		private readonly Viewbox _DMSearchDiscHeader = UIModification.MakeStandardViewBox("Disc:");
		private readonly TextBox _DMSearchDiscInput = new MyNumberBox { MaxLength = 4, };
		private readonly Viewbox _DMSearchIDHeader = UIModification.MakeStandardViewBox("ID:");
		private readonly TextBox _DMSearchIDInput = new MyNumberBox { MaxLength = 18, };
		private readonly Button _DMSearchSearchButton = new MyButton { Content = "Search", };
		private readonly Button _DMSearchCloseButton = new MyButton { Content = "Close", };
		#endregion

		#region System Info
		private readonly Grid _SysInfoLayout = new Grid();
		private readonly TextBox _SysInfoUnder = new MyTextBox { IsReadOnly = true, };
		private readonly Viewbox _Latency = new Viewbox { Child = UIModification.MakeSysInfoBox(), };
		private readonly Viewbox _Memory = new Viewbox { Child = UIModification.MakeSysInfoBox(), };
		private readonly Viewbox _Threads = new Viewbox { Child = UIModification.MakeSysInfoBox(), };
		private readonly Viewbox _Guilds = new Viewbox { Child = UIModification.MakeSysInfoBox(), };
		private readonly Viewbox _Users = new Viewbox { Child = UIModification.MakeSysInfoBox(), };
		private readonly ToolTip _MemHoverInfo = new ToolTip { Content = "This is not guaranteed to be 100% correct.", };
		#endregion

		public BotWindow(IServiceProvider provider)
		{
			FontFamily = new FontFamily("Courier New");

			var client = (IDiscordClient)provider.GetService(typeof(IDiscordClient));
			var botSettings = (IBotSettings)provider.GetService(typeof(IBotSettings));
			var logging = (ILogModule)provider.GetService(typeof(ILogModule));
			var uiSettings = UISettings.LoadUISettings(botSettings.Loaded);

			InitializeComponents(uiSettings, client, botSettings);
			Loaded += (sender, e) => RunApplication(sender, e, uiSettings, client, botSettings, logging);
		}
		private void InitializeComponents(UISettings uiSettings, IDiscordClient client, IBotSettings botSettings)  
		{
			//Main layout
			UIModification.AddRows(_Layout, 100);
			UIModification.AddCols(_Layout, 4);

			//Output
			UIModification.AddElement(_Layout, _Output, 0, 87, 0, 4);

			//System Info
			UIModification.AddElement(_Layout, _SysInfoLayout, 87, 3, 0, 3, 0, 5);
			UIModification.AddElement(_SysInfoLayout, _SysInfoUnder, 0, 1, 0, 5);
			UIModification.AddElement(_SysInfoLayout, _Latency, 0, 1, 0, 1);
			UIModification.AddElement(_SysInfoLayout, _Memory, 0, 1, 1, 1);
			UIModification.AddElement(_SysInfoLayout, _Threads, 0, 1, 2, 1);
			UIModification.AddElement(_SysInfoLayout, _Guilds, 0, 1, 3, 1);
			UIModification.AddElement(_SysInfoLayout, _Users, 0, 1, 4, 1);

			//Input
			UIModification.AddElement(_Layout, _InputLayout, 90, 10, 0, 3, 1, 10);
			UIModification.AddElement(_InputLayout, _Input, 0, 1, 0, 9);
			UIModification.AddElement(_InputLayout, _InputButton, 0, 1, 9, 1);

			//Buttons
			UIModification.AddElement(_Layout, _ButtonLayout, 87, 13, 3, 1, 2, 5);
			UIModification.AddElement(_ButtonLayout, _MainButton, 0, 2, 0, 1);
			UIModification.AddElement(_ButtonLayout, _InfoButton, 0, 2, 1, 1);
			UIModification.AddElement(_ButtonLayout, _SettingsButton, 0, 1, 2, 1);
			UIModification.AddElement(_ButtonLayout, _ColorsButton, 1, 1, 2, 1);
			UIModification.AddElement(_ButtonLayout, _DMButton, 0, 2, 3, 1);
			UIModification.AddElement(_ButtonLayout, _FileButton, 0, 2, 4, 1);

			//Main menu
			UIModification.AddElement(_Layout, _MainMenuLayout, 0, 87, 3, 1, 100, 3);
			UIModification.AddElement(_MainMenuLayout, _MainMenuOutput, 0, 95, 0, 3);
			UIModification.AddElement(_MainMenuLayout, _PauseButton, 95, 5, 0, 1);
			UIModification.AddElement(_MainMenuLayout, _RestartButton, 95, 5, 1, 1);
			UIModification.AddElement(_MainMenuLayout, _DisconnectButton, 95, 5, 2, 1);

			//Settings menu
			UIModification.AddElement(_Layout, _SettingsLayout, 0, 87, 3, 1, 100, 100);
			UIModification.AddPlaceHolderTB(_SettingsLayout, 0, 100, 0, 100);
			UIModification.AddCols((Grid)_TrustedUsersAdd.Setting, 10);
			UIModification.AddElement((Grid)_TrustedUsersAdd.Setting, _TrustedUsersAddBox, 0, 1, 0, 9);
			UIModification.AddElement((Grid)_TrustedUsersAdd.Setting, _TrustedUsersAddButton, 0, 1, 9, 1);
			UIModification.AddCols((Grid)_TrustedUsersRemove.Setting, 10);
			UIModification.AddElement((Grid)_TrustedUsersRemove.Setting, _TrustedUsersComboBox, 0, 1, 0, 9);
			UIModification.AddElement((Grid)_TrustedUsersRemove.Setting, _TrustedUsersRemoveButton, 0, 1, 9, 1);
			UIModification.AddElement(_SettingsLayout, _SettingsSaveButton, 95, 5, 0, 100);
			var _Settings = new[]
			{
				_DownloadUsersSetting,
				_PrefixSetting,
				_BotOwnerSetting,
				_GameSetting,
				_StreamSetting,
				_ShardSetting,
				_MessageCacheSetting,
				_UserGatherCountSetting,
				_MessageGatherSizeSetting,
				_LogLevelComboBox,
				_TrustedUsersAdd, 
				_TrustedUsersRemove,
			};
			for (int i = 0; i < _Settings.Length; ++i)
			{
				const int TITLE_START_COLUMN = 5;
				const int TITLE_COLUMN_LENGTH = 35;
				const int SETTING_START_COLUMN = 40;
				const int SETTING_COLUMN_LENGTH = 55;
				const int LENGTH_FOR_SETTINGS = 4;

				UIModification.AddElement(_SettingsLayout, _Settings[i].Title, (i * LENGTH_FOR_SETTINGS), LENGTH_FOR_SETTINGS, TITLE_START_COLUMN, TITLE_COLUMN_LENGTH);
				UIModification.AddElement(_SettingsLayout, _Settings[i].Setting, (i * LENGTH_FOR_SETTINGS), LENGTH_FOR_SETTINGS, SETTING_START_COLUMN, SETTING_COLUMN_LENGTH);
			}

			//Colors menu
			UIModification.AddElement(_Layout, _ColorsLayout, 0, 87, 3, 1, 100, 100);

			//Info menu
			UIModification.AddElement(_Layout, _InfoLayout, 0, 87, 3, 1, 1, 10);
			UIModification.AddPlaceHolderTB(_InfoLayout, 0, 1, 0, 10);
			UIModification.AddElement(_InfoLayout, _InfoOutput, 0, 1, 1, 8);

			//File menu
			UIModification.AddElement(_Layout, _FileLayout, 0, 87, 3, 1, 100, 1);
			UIModification.AddElement(_FileLayout, _FileOutput, 0, 95, 0, 1);
			UIModification.AddElement(_FileLayout, _FileSearchButton, 95, 5, 0, 1);
			UIModification.AddElement(_Layout, _SpecificFileLayout, 0, 100, 0, 4, 100, 4);
			UIModification.AddElement(_SpecificFileLayout, _SpecificFileDisplay, 0, 100, 0, 3);
			UIModification.AddElement(_SpecificFileLayout, _SpecificFileCloseButton, 95, 5, 3, 1);

			//DM menu
			UIModification.AddElement(_Layout, _DMLayout, 0, 87, 3, 1, 100, 1);
			UIModification.AddElement(_DMLayout, _DMOutput, 0, 95, 0, 1);
			UIModification.AddElement(_DMLayout, _DMSearchButton, 95, 5, 0, 1);
			UIModification.AddElement(_Layout, _SpecificDMLayout, 0, 100, 0, 4, 100, 4);
			UIModification.AddElement(_SpecificDMLayout, _SpecificDMDisplay, 0, 100, 0, 3);
			UIModification.AddElement(_SpecificDMLayout, _SpecificDMCloseButton, 95, 5, 3, 1);

			//Guild search
			UIModification.AddElement(_Layout, _GuildSearchLayout, 0, 100, 0, 4, 10, 10);
			UIModification.AddElement(_GuildSearchLayout, _GuildSearchTextLayout, 3, 4, 3, 4, 100, 100);
			UIModification.PutInBGWithMouseUpEvent(_GuildSearchLayout, _GuildSearchTextLayout, null, CloseFileSearch);
			UIModification.AddPlaceHolderTB(_GuildSearchTextLayout, 0, 100, 0, 100);
			UIModification.AddElement(_GuildSearchTextLayout, _GuildSearchNameHeader, 10, 10, 15, 70);
			UIModification.AddElement(_GuildSearchTextLayout, _GuildSearchNameInput, 20, 21, 15, 70);
			UIModification.AddElement(_GuildSearchTextLayout, _GuildSearchIDHeader, 41, 10, 15, 70);
			UIModification.AddElement(_GuildSearchTextLayout, _GuildSearchIDInput, 51, 10, 15, 70);
			UIModification.AddElement(_GuildSearchTextLayout, _GuildSearchFileComboBox, 63, 10, 20, 60);
			UIModification.AddElement(_GuildSearchTextLayout, _GuildSearchSearchButton, 75, 15, 20, 25);
			UIModification.AddElement(_GuildSearchTextLayout, _GuildSearchCloseButton, 75, 15, 55, 25);

			//Output search
			UIModification.AddElement(_Layout, _OutputSearchLayout, 0, 100, 0, 4, 10, 10);
			UIModification.AddElement(_OutputSearchLayout, _OutputSearchTextLayout, 1, 8, 1, 8, 100, 100);
			UIModification.PutInBGWithMouseUpEvent(_OutputSearchLayout, _OutputSearchTextLayout, null, CloseOutputSearch);
			UIModification.AddPlaceHolderTB(_OutputSearchTextLayout, 90, 10, 0, 100);
			UIModification.AddElement(_OutputSearchTextLayout, _OutputSearchResults, 0, 90, 0, 100);
			UIModification.AddElement(_OutputSearchTextLayout, _OutputSearchComboBox, 92, 6, 2, 30);
			UIModification.AddElement(_OutputSearchTextLayout, _OutputSearchButton, 92, 6, 66, 15);
			UIModification.AddElement(_OutputSearchTextLayout, _OutputSearchCloseButton, 92, 6, 83, 15);

			//DM search
			UIModification.AddElement(_Layout, _DMSearchLayout, 0, 100, 0, 4, 10, 10);
			UIModification.AddElement(_DMSearchLayout, _DMSearchTextLayout, 3, 4, 3, 4, 72, 100);
			UIModification.PutInBGWithMouseUpEvent(_DMSearchLayout, _DMSearchTextLayout, null, CloseDMSearch);
			UIModification.AddPlaceHolderTB(_DMSearchTextLayout, 0, 100, 0, 100);
			UIModification.AddElement(_DMSearchTextLayout, _DMSearchNameHeader, 10, 10, 15, 50);
			UIModification.AddElement(_DMSearchTextLayout, _DMSearchNameInput, 20, 10, 15, 50);
			UIModification.AddElement(_DMSearchTextLayout, _DMSearchDiscHeader, 10, 10, 65, 20);
			UIModification.AddElement(_DMSearchTextLayout, _DMSearchDiscInput, 20, 10, 65, 20);
			UIModification.AddElement(_DMSearchTextLayout, _DMSearchIDHeader, 30, 10, 15, 70);
			UIModification.AddElement(_DMSearchTextLayout, _DMSearchIDInput, 40, 10, 15, 70);
			UIModification.AddElement(_DMSearchTextLayout, _DMSearchSearchButton, 52, 10, 20, 25);
			UIModification.AddElement(_DMSearchTextLayout, _DMSearchCloseButton, 52, 10, 55, 25);

			//Font size properties
			UIModification.SetFontSizeProperties(.275, new UIElement[] { _Input, });
			UIModification.SetFontSizeProperties(.060, new UIElement[] { _GuildSearchNameInput, _GuildSearchIDInput, _DMSearchNameInput, _DMSearchDiscInput, _DMSearchIDInput });
			UIModification.SetFontSizeProperties(.035, new UIElement[] { _InfoOutput, });
			UIModification.SetFontSizeProperties(.022, new UIElement[] { _SpecificFileDisplay, _FileOutput, _OutputSearchComboBox, _DMOutput });
			UIModification.SetFontSizeProperties(.018, new UIElement[] { _MainMenuOutput, }, _Settings.Select(x => x.Title), _Settings.Select(x => x.Setting));

			//Context menus
			_Output.ContextMenu = new ContextMenu
			{
				ItemsSource = new[] { _OutputContextMenuSearch, _OutputContextMenuSave, _OutputContextMenuClear },
			};
			_SpecificFileDisplay.ContextMenu = new ContextMenu
			{
				ItemsSource = new[] { _SpecificFileContextMenuSave },
			};

			MakeInputEvents(client, botSettings);
			MakeOutputEvents();
			MakeMenuEvents(uiSettings, client, botSettings);
			MakeDMEvents();
			MakeGuildFileEvents();
			MakeOtherEvents(uiSettings, client, botSettings);

			//Set this panel as the content for this window and run the application
			this.Content = _Layout;
			this.WindowState = WindowState.Maximized;
		}

		private void RunApplication(object sender, RoutedEventArgs e, UISettings uiSettings, IDiscordClient client, IBotSettings botSettings, ILogModule logging)
		{
			//Make console output show on the output text block and box
			Console.SetOut(new UITextBoxStreamWriter(_Output));

			Task.Run(async () =>
			{
				if (SavingAndLoading.ValidatePath(Properties.Settings.Default.Path, botSettings.Windows, true))
				{
					botSettings.SetGotPath();
				}
				if (await SavingAndLoading.ValidateBotKey(client, Properties.Settings.Default.BotKey, true))
				{
					botSettings.SetGotKey();
				}
				await ClientActions.MaybeStartBot(client, botSettings);
			});

			uiSettings.InitializeColors();
			uiSettings.ActivateTheme();
			UIModification.SetColorMode(_Layout);
			UpdateSystemInformation(client, botSettings, logging);
		}

		private void MakeOtherEvents(UISettings uiSettings, IDiscordClient client, IBotSettings botSettings)
		{
			_PauseButton.Click += (sender, e) => Pause(sender, e, botSettings);
			_RestartButton.Click += Restart;
			_DisconnectButton.Click += Disconnect;

			_Memory.MouseEnter += ModifyMemHoverInfo;
			_Memory.MouseLeave += ModifyMemHoverInfo;

			_SettingsSaveButton.Click += (sender, e) => SaveSettings(sender, e, client, botSettings);
			_ColorsSaveButton.Click += (sender, e) => SaveColors(sender, e, uiSettings);
			_TrustedUsersRemoveButton.Click += RemoveTrustedUser;
			_TrustedUsersAddButton.Click += (sender, e) => AddTrustedUser(sender, e, client);
		}
		private void Pause(object sender, RoutedEventArgs e, IBotSettings botSettings)
		{
			if (botSettings.Pause)
			{
				ConsoleActions.WriteLine("The bot is now unpaused.");
				botSettings.TogglePause();
			}
			else
			{
				ConsoleActions.WriteLine("The bot is now paused.");
				botSettings.TogglePause();
			}
		}
		private void Restart(object sender, RoutedEventArgs e)
		{
			switch (MessageBox.Show("Are you sure you want to restart the bot?", Constants.PROGRAM_NAME, MessageBoxButton.OKCancel))
			{
				case MessageBoxResult.OK:
				{
					Misc.RestartBot();
					return;
				}
			}
		}
		private void Disconnect(object sender, RoutedEventArgs e)
		{
			switch (MessageBox.Show("Are you sure you want to disconnect the bot?", Constants.PROGRAM_NAME, MessageBoxButton.OKCancel))
			{
				case MessageBoxResult.OK:
				{
					Misc.DisconnectBot();
					return;
				}
			}
		}
		private void ModifyMemHoverInfo(object sender, RoutedEventArgs e)
		{
			UIModification.ToggleToolTip(_MemHoverInfo);
		}
		private async void SaveSettings(object sender, RoutedEventArgs e, IDiscordClient client, IBotSettings botSettings)
		{
			//Go through each setting and update them
			for (int i = 0; i < VisualTreeHelper.GetChildrenCount(_SettingsLayout); ++i)
			{
				var ele = VisualTreeHelper.GetChild(_SettingsLayout, i);
				var setting = (ele as FrameworkElement)?.Tag;
				if (setting is SettingOnBot)
				{
					var fuckYouForTellingMeToPatternMatch = setting as SettingOnBot?;
					var castSetting = (SettingOnBot)fuckYouForTellingMeToPatternMatch;

					if (!SaveSetting(ele, castSetting, botSettings))
					{
						ConsoleActions.WriteLine(String.Format("Failed to save: {0}", castSetting.EnumName()));
					}
				}
			}

			await ClientActions.SetGame(client, botSettings);
		}
		private void SaveColors(object sender, RoutedEventArgs e, UISettings uiSettings)
		{
			for (int i = 0; i < VisualTreeHelper.GetChildrenCount(_ColorsLayout); ++i)
			{
				var child = VisualTreeHelper.GetChild(_ColorsLayout, i);
				if (child is MyTextBox)
				{
					var castedChild = child as MyTextBox;

					if (!(castedChild.Tag is ColorTarget))
						continue;
					var target = (ColorTarget)castedChild.Tag;

					var childText = castedChild.Text;
					if (String.IsNullOrWhiteSpace(childText))
					{
						continue;
					}
					else if (!childText.StartsWith("#"))
					{
						childText = "#" + childText;
					}

					Brush brush = null;
					try
					{
						brush = UIModification.MakeBrush(childText);
					}
					catch
					{
						ConsoleActions.WriteLine(String.Format("Invalid color supplied for {0}.", target.EnumName()));
						continue;
					}

					if (!UIModification.CheckIfTwoBrushesAreTheSame(uiSettings.ColorTargets[target], brush))
					{
						uiSettings.ColorTargets[target] = brush;
						castedChild.Text = UIModification.FormatBrush(brush);
						ConsoleActions.WriteLine(String.Format("Successfully updated the color for {0}.", target.EnumName()));
					}
				}
				else if (child is ComboBox)
				{
					var selected = ((ComboBox)child).SelectedItem as MyTextBox;
					var tag = selected?.Tag as ColorTheme?;
					if (!tag.HasValue || tag == uiSettings.Theme)
						continue;

					uiSettings.SetTheme((ColorTheme)tag);
					ConsoleActions.WriteLine("Successfully updated the theme type.");
				}
			}

			uiSettings.SaveSettings();
			uiSettings.ActivateTheme();
			UIModification.SetColorMode(_Layout);
		}
		private async void AddTrustedUser(object sender, RoutedEventArgs e, IDiscordClient client)
		{
			var text = _TrustedUsersAddBox.Text;
			_TrustedUsersAddBox.Text = "";

			if (String.IsNullOrWhiteSpace(text))
			{
				return;
			}
			else if (ulong.TryParse(text, out ulong userID))
			{
				var currTBs = _TrustedUsersComboBox.Items.Cast<TextBox>().ToList();
				if (currTBs.Select(x => (ulong)x.Tag).Contains(userID))
					return;

				var tb = UIModification.MakeTextBoxFromUserID(await client.GetUserAsync(userID));
				if (tb == null)
				{
					return;
				}

				currTBs.Add(tb);
				_TrustedUsersComboBox.ItemsSource = currTBs;
			}
			else
			{
				ConsoleActions.WriteLine(String.Format("The given input '{0}' is not a valid ID.", text));
			}
		}
		private void RemoveTrustedUser(object sender, RoutedEventArgs e)
		{
			if (_TrustedUsersComboBox.SelectedItem == null)
				return;

			var userID = (ulong)((TextBox)_TrustedUsersComboBox.SelectedItem).Tag;
			var currTBs = _TrustedUsersComboBox.Items.Cast<TextBox>().ToList();
			if (!currTBs.Select(x => (ulong)x.Tag).Contains(userID))
				return;

			currTBs.RemoveAll(x => (ulong)x.Tag == userID);
			_TrustedUsersComboBox.ItemsSource = currTBs;
		}

		private void MakeInputEvents(IDiscordClient client, IBotSettings botSettings)
		{
			_Input.KeyUp += (sender, e) => AcceptInput(sender, e, client, botSettings);
			_InputButton.Click += (sender, e) => AcceptInput(sender, e, client, botSettings);
		}
		private async void AcceptInput(object sender, KeyEventArgs e, IDiscordClient client, IBotSettings botSettings)
		{
			var text = _Input.Text;
			if (String.IsNullOrWhiteSpace(text))
			{
				_InputButton.IsEnabled = false;
				return;
			}
			else
			{
				if (e.Key.Equals(Key.Enter) || e.Key.Equals(Key.Return))
				{
					await DoStuffWithInput(UICommandHandler.GatherInput(_Input, _InputButton), client, botSettings);
				}
				else
				{
					_InputButton.IsEnabled = true;
				}
			}
		}
		private async void AcceptInput(object sender, RoutedEventArgs e, IDiscordClient client, IBotSettings botSettings)
		{
			await DoStuffWithInput(UICommandHandler.GatherInput(_Input, _InputButton), client, botSettings);
		}
		private async Task DoStuffWithInput(string input, IDiscordClient client, IBotSettings botSettings)
		{
			//Make sure both the path and key are set
			if (!botSettings.GotPath || !botSettings.GotKey)
			{
				if (!botSettings.GotPath)
				{
					if (SavingAndLoading.ValidatePath(input, botSettings.Windows))
					{
						botSettings.SetGotPath();
					}
				}
				else if (!botSettings.GotKey)
				{
					if (await SavingAndLoading.ValidateBotKey(client, input))
					{
						botSettings.SetGotKey();
					}
				}
				await ClientActions.MaybeStartBot(client, botSettings);
			}
			else
			{
				UICommandHandler.HandleCommand(input, botSettings.Prefix);
			}
		}

		private void MakeOutputEvents()
		{
			_OutputContextMenuSave.Click += SaveOutput;
			_OutputContextMenuClear.Click += ClearOutput;
			_OutputContextMenuSearch.Click += OpenOutputSearch;
			_OutputSearchCloseButton.Click += CloseOutputSearch;
			_OutputSearchButton.Click += SearchOutput;
		}
		private void SaveOutput(object sender, RoutedEventArgs e)
		{
			//Make sure the path is valid
			var path = Gets.GetBaseBotDirectory("Output_Log_" + DateTime.UtcNow.ToString("MM-dd_HH-mm-ss") + Constants.GENERAL_FILE_EXTENSION);
			if (path == null)
			{
				ConsoleActions.WriteLine("Unable to save the output log.");
				return;
			}

			//Save the file
			using (StreamWriter writer = new StreamWriter(path))
			{
				writer.Write(_Output.Text);
			}

			//Write to the console telling the user that the console log was successfully saved
			ConsoleActions.WriteLine("Successfully saved the output log.");
		}
		private void ClearOutput(object sender, RoutedEventArgs e)
		{
			switch (MessageBox.Show("Are you sure you want to clear the output window?", Constants.PROGRAM_NAME, MessageBoxButton.OKCancel))
			{
				case MessageBoxResult.OK:
				{
					_Output.Text = "";
					return;
				}
			}
		}
		private void OpenOutputSearch(object sender, RoutedEventArgs e)
		{
			_OutputSearchComboBox.ItemsSource = UIModification.MakeComboBoxSourceOutOfStrings(ConsoleActions.WrittenLines.Keys);
			_OutputSearchLayout.Visibility = Visibility.Visible;
		}
		private void CloseOutputSearch(object sender, RoutedEventArgs e)
		{
			_OutputSearchComboBox.SelectedItem = null;
			_OutputSearchResults.Text = null;
			_OutputSearchLayout.Visibility = Visibility.Collapsed;
		}
		private void SearchOutput(object sender, RoutedEventArgs e)
		{
			var selectedItem = (TextBox)_OutputSearchComboBox.SelectedItem;
			if (selectedItem != null)
			{
				_OutputSearchResults.Text = null;
				ConsoleActions.WrittenLines[selectedItem.Text].ForEach(x => _OutputSearchResults.AppendText(x + Environment.NewLine));
			}
		}

		private void MakeGuildFileEvents()
		{
			_FileSearchButton.Click += OpenFileSearch;
			_GuildSearchSearchButton.Click += SearchForFile;
			_GuildSearchCloseButton.Click += CloseFileSearch;
			_SpecificFileCloseButton.Click += CloseSpecificFileLayout;
			_SpecificFileContextMenuSave.Click += SaveFile;
		}
		private void OpenFileSearch(object sender, RoutedEventArgs e)
		{
			_GuildSearchLayout.Visibility = Visibility.Visible;
		}
		private void CloseFileSearch(object sender, RoutedEventArgs e)
		{
			_GuildSearchFileComboBox.SelectedItem = null;
			_GuildSearchNameInput.Text = "";
			_GuildSearchIDInput.Text = "";
			_GuildSearchLayout.Visibility = Visibility.Collapsed;
		}
		private void SearchForFile(object sender, RoutedEventArgs e)
		{
			var tb = (TextBox)_GuildSearchFileComboBox.SelectedItem;
			if (tb == null)
				return;

			var nameStr = _GuildSearchNameInput.Text;
			var idStr = _GuildSearchIDInput.Text;
			if (String.IsNullOrWhiteSpace(nameStr) && String.IsNullOrWhiteSpace(idStr))
				return;

			var fileType = (FileType)tb.Tag;
			CloseFileSearch(sender, e);

			TreeViewItem guild = null;
			if (!String.IsNullOrWhiteSpace(idStr))
			{
				if (!ulong.TryParse(idStr, out ulong guildID))
				{
					ConsoleActions.WriteLine(String.Format("The ID '{0}' is not a valid number.", idStr));
					return;
				}
				else
				{
					guild = _FileTreeView.Items.Cast<TreeViewItem>().FirstOrDefault(x =>
					{
						return ((GuildFileInformation)x.Tag).ID == guildID;
					});

					if (guild == null)
					{
						ConsoleActions.WriteLine(String.Format("No guild could be found with the ID '{0}'.", guildID));
						return;
					}
				}
			}
			else if (!String.IsNullOrWhiteSpace(nameStr))
			{
				var guilds = _FileTreeView.Items.Cast<TreeViewItem>().Where(x =>
				{
					return ((GuildFileInformation)x.Tag).Name.CaseInsEquals(nameStr);
				});

				if (guilds.Count() == 0)
				{
					ConsoleActions.WriteLine(String.Format("No guild could be found with the name '{0}'.", nameStr));
					return;
				}
				else if (guilds.Count() == 1)
				{
					guild = guilds.FirstOrDefault();
				}
				else
				{
					ConsoleActions.WriteLine("More than one guild has the name '{0}'.", nameStr);
					return;
				}
			}

			if (guild != null)
			{
				var item = guild.Items.Cast<TreeViewItem>().FirstOrDefault(x =>
				{
					return ((FileInformation)x.Tag).FileType == fileType;
				});

				if (item != null)
				{
					OpenSpecificFileLayout(item, e);
				}
			}
		}
		private void OpenSpecificFileLayout(object sender, RoutedEventArgs e)
		{
			if (CheckIfTreeViewItemFileExists((TreeViewItem)sender))
			{
				UIModification.SetRowAndSpan(_FileLayout, 0, 100);
				_SpecificFileLayout.Visibility = Visibility.Visible;
				_FileSearchButton.Visibility = Visibility.Collapsed;
			}
			else
			{
				ConsoleActions.WriteLine("Unable to bring up the file.");
			}
		}
		private void CloseSpecificFileLayout(object sender, RoutedEventArgs e)
		{
			var result = MessageBox.Show("Are you sure you want to close the edit window?", Constants.PROGRAM_NAME, MessageBoxButton.OKCancel);

			switch (result)
			{
				case MessageBoxResult.OK:
				{
					UIModification.SetRowAndSpan(_FileLayout, 0, 87);
					_SpecificFileDisplay.Tag = null;
					_SpecificFileLayout.Visibility = Visibility.Collapsed;
					_FileSearchButton.Visibility = Visibility.Visible;
					break;
				}
			}
		}
		private void SaveFile(object sender, RoutedEventArgs e)
		{
			var fileLocation = _SpecificFileDisplay.Tag.ToString();
			if (String.IsNullOrWhiteSpace(fileLocation) || !File.Exists(fileLocation))
			{
				UIModification.MakeFollowingToolTip(_Layout, _ToolTip, "Unable to gather the path for this file.").Forget();
				return;
			}

			var fileAndExtension = fileLocation.Substring(fileLocation.LastIndexOf('\\') + 1);
			if (fileAndExtension.Equals(Constants.GUILD_SETTINGS_LOCATION))
			{
				//Make sure the guild info stays valid
				try
				{
					var throwaway = JsonConvert.DeserializeObject(_SpecificFileDisplay.Text, Constants.GUILDS_SETTINGS_TYPE);
				}
				catch (Exception exc)
				{
					ConsoleActions.ExceptionToConsole(exc);
					UIModification.MakeFollowingToolTip(_Layout, _ToolTip, "Failed to save the file.").Forget();
					return;
				}
			}

			//Save the file and give a notification
			using (var writer = new StreamWriter(fileLocation))
			{
				writer.WriteLine(_SpecificFileDisplay.Text);
			}
			UIModification.MakeFollowingToolTip(_Layout, _ToolTip, "Successfully saved the file.").Forget();
		}

		private void MakeDMEvents()
		{
			_DMSearchButton.Click += OpenDMSearch;
			_DMSearchSearchButton.Click += SearchForDM;
			_DMSearchCloseButton.Click += CloseDMSearch;
			_SpecificDMCloseButton.Click += CloseSpecificDMLayout;
		}
		private void OpenDMSearch(object sender, RoutedEventArgs e)
		{
			_DMSearchLayout.Visibility = Visibility.Visible;
		}
		private void CloseDMSearch(object sender, RoutedEventArgs e)
		{
			_DMSearchNameInput.Text = "";
			_DMSearchDiscInput.Text = "";
			_DMSearchIDInput.Text = "";
			_DMSearchLayout.Visibility = Visibility.Collapsed;
		}
		private void SearchForDM(object sender, RoutedEventArgs e)
		{
			var nameStr = _DMSearchNameInput.Text;
			var discStr = _DMSearchDiscInput.Text;
			var idStr = _DMSearchIDInput.Text;

			if (String.IsNullOrWhiteSpace(nameStr) && String.IsNullOrWhiteSpace(idStr))
				return;
			CloseDMSearch(sender, e);

			TreeViewItem DMChannel = null;
			if (!String.IsNullOrWhiteSpace(idStr))
			{
				if (!ulong.TryParse(idStr, out ulong userID))
				{
					ConsoleActions.WriteLine(String.Format("The ID '{0}' is not a valid number.", idStr));
					return;
				}
				else
				{
					DMChannel = _DMTreeView.Items.Cast<TreeViewItem>().FirstOrDefault(x => ((IDMChannel)x.Tag)?.Recipient?.Id == userID);
					if (DMChannel == null)
					{
						ConsoleActions.WriteLine(String.Format("No user could be found with the ID '{0}'.", userID));
						return;
					}
				}
			}
			else if (!String.IsNullOrWhiteSpace(nameStr))
			{
				var DMChannels = _DMTreeView.Items.Cast<TreeViewItem>().Where(x => (bool)((IDMChannel)x.Tag)?.Recipient?.Username.CaseInsEquals(nameStr));

				if (!String.IsNullOrWhiteSpace(discStr))
				{
					if (!ushort.TryParse(discStr, out ushort disc))
					{
						ConsoleActions.WriteLine(String.Format("The discriminator '{0}' is not a valid number.", discStr));
						return;
					}
					else
					{
						//Why are discriminators strings instead of ints????
						DMChannels = DMChannels.Where(x => (bool)((IDMChannel)x.Tag)?.Recipient?.Discriminator.CaseInsEquals(discStr));
					}
				}

				if (DMChannels.Count() == 0)
				{
					ConsoleActions.WriteLine(String.Format("No user could be found with the name '{0}'.", nameStr));
					return;
				}
				else if (DMChannels.Count() == 1)
				{
					DMChannel = DMChannels.FirstOrDefault();
				}
				else
				{
					ConsoleActions.WriteLine("More than one user has the name '{0}'.", nameStr);
					return;
				}
			}

			if (DMChannel != null)
			{
				OpenSpecificDMLayout(DMChannel, e);
			}
		}
		private async void OpenSpecificDMLayout(object sender, RoutedEventArgs e)
		{
			if (await CheckIfTreeViewItemDMExists((TreeViewItem)sender))
			{
				UIModification.SetRowAndSpan(_DMLayout, 0, 100);
				_SpecificDMLayout.Visibility = Visibility.Visible;
				_DMSearchButton.Visibility = Visibility.Collapsed;
			}
			else
			{
				ConsoleActions.WriteLine("Unable to bring up the DMs.");
			}
		}
		private void CloseSpecificDMLayout(object sender, RoutedEventArgs e)
		{
			UIModification.SetRowAndSpan(_DMLayout, 0, 87);
			_SpecificDMDisplay.Tag = null;
			_SpecificDMLayout.Visibility = Visibility.Collapsed;
			_DMSearchButton.Visibility = Visibility.Visible;
		}

		private void MakeMenuEvents(UISettings uiSettings, IDiscordClient client, IBotSettings botSettings)
		{
			_MainButton.Click += (sender, e) => OpenMenu(sender, e, uiSettings, client, botSettings);
			_SettingsButton.Click += (sender, e) => OpenMenu(sender, e, uiSettings, client, botSettings);
			_ColorsButton.Click += (sender, e) => OpenMenu(sender, e, uiSettings, client, botSettings);
			_InfoButton.Click += (sender, e) => OpenMenu(sender, e, uiSettings, client, botSettings);
			_FileButton.Click += (sender, e) => OpenMenu(sender, e, uiSettings, client, botSettings);
			_DMButton.Click += (sender, e) => OpenMenu(sender, e, uiSettings, client, botSettings);
		}
		private async void OpenMenu(object sender, RoutedEventArgs e, UISettings uiSettings, IDiscordClient client, IBotSettings botSettings)
		{
			if (!botSettings.Loaded)
				return;

			//Hide everything so stuff doesn't overlap
			_MainMenuLayout.Visibility = Visibility.Collapsed;
			_SettingsLayout.Visibility = Visibility.Collapsed;
			_ColorsLayout.Visibility = Visibility.Collapsed;
			_InfoLayout.Visibility = Visibility.Collapsed;
			_FileLayout.Visibility = Visibility.Collapsed;
			_DMLayout.Visibility = Visibility.Collapsed;

			//If clicking the same button then resize the output window to the regular size
			var type = (sender as Button)?.Tag as MenuType? ?? default(MenuType);
			if (type == _LastButtonClicked)
			{
				UIModification.SetColAndSpan(_Output, 0, 4);
				_LastButtonClicked = default(MenuType);
			}
			else
			{
				//Resize the regular output window and have the menubox appear
				UIModification.SetColAndSpan(_Output, 0, 3);
				_LastButtonClicked = type;

				switch (type)
				{
					case MenuType.Main:
					{
						_MainMenuLayout.Visibility = Visibility.Visible;
						return;
					}
					case MenuType.Info:
					{
						_InfoLayout.Visibility = Visibility.Visible;
						return;
					}
					case MenuType.Settings:
					{
						UpdateSettingsWhenOpened(client, botSettings);
						_SettingsLayout.Visibility = Visibility.Visible;
						return;
					}
					case MenuType.Colors:
					{
						UIModification.MakeColorDisplayer(uiSettings, _ColorsLayout, _ColorsSaveButton, .018);
						_ColorsLayout.Visibility = Visibility.Visible;
						return;
					}
					case MenuType.DMs:
					{
						var treeView = UIModification.MakeDMTreeView(_DMTreeView, await client.GetDMChannelsAsync());
						treeView.Items.Cast<TreeViewItem>().ToList().ForEach(x =>
						{
							x.MouseDoubleClick += OpenSpecificDMLayout;
						});
						_DMOutput.Document = new FlowDocument(new Paragraph(new InlineUIContainer(treeView)));
						_DMLayout.Visibility = Visibility.Visible;
						return;
					}
					case MenuType.Files:
					{
						var treeView = UIModification.MakeGuildTreeView(_FileTreeView, await client.GetGuildsAsync());
						treeView.Items.Cast<TreeViewItem>().SelectMany(x => x.Items.Cast<TreeViewItem>()).ToList().ForEach(x =>
						{
							x.MouseDoubleClick += OpenSpecificFileLayout;
						});
						_FileOutput.Document = new FlowDocument(new Paragraph(new InlineUIContainer(treeView)));
						_FileLayout.Visibility = Visibility.Visible;
						return;
					}
				}
			}
		}

		private void UpdateSystemInformation(IDiscordClient client, IBotSettings botSettings, ILogModule logging)
		{
			var timer = new DispatcherTimer() { Interval = new TimeSpan(0, 0, 0, 0, 500) };
			timer.Tick += async (sender, e) =>
			{
				var guilds = await client.GetGuildsAsync();
				var userIDs = new List<ulong>();
				foreach (var guild in guilds)
				{
					userIDs.AddRange((await guild.GetUsersAsync()).Select(x => x.Id));
				}

				((TextBox)_Latency.Child).Text = String.Format("Latency: {0}ms", ClientActions.GetLatency(client));
				((TextBox)_Memory.Child).Text = String.Format("Memory: {0}MB", Gets.GetMemory(botSettings.Windows).ToString("0.00"));
				((TextBox)_Threads.Child).Text = String.Format("Threads: {0}", Process.GetCurrentProcess().Threads.Count);
				((TextBox)_Guilds.Child).Text = String.Format("Guilds: {0}", guilds.Count);
				((TextBox)_Users.Child).Text = String.Format("Members: {0}", userIDs.Distinct().Count());
				_InfoOutput.Document = UIModification.MakeInfoMenu(Gets.GetUptime(botSettings), logging.FormatLoggedCommands(), logging.FormatLoggedActions());
			};
			timer.Start();
		}
		private async void UpdateSettingsWhenOpened(IDiscordClient client, IBotSettings botSettings)
		{
			((CheckBox)((Viewbox)_DownloadUsersSetting.Setting).Child).IsChecked = botSettings.AlwaysDownloadUsers;
			((TextBox)_PrefixSetting.Setting).Text = botSettings.Prefix;
			((TextBox)_BotOwnerSetting.Setting).Text = botSettings.BotOwnerID.ToString();
			((TextBox)_GameSetting.Setting).Text = botSettings.Game;
			((TextBox)_StreamSetting.Setting).Text = botSettings.Stream;
			((TextBox)_ShardSetting.Setting).Text = botSettings.ShardCount.ToString();
			((TextBox)_MessageCacheSetting.Setting).Text = botSettings.MessageCacheCount.ToString();
			((TextBox)_UserGatherCountSetting.Setting).Text = botSettings.MaxUserGatherCount.ToString();
			((TextBox)_MessageGatherSizeSetting.Setting).Text = botSettings.MaxMessageGatherSize.ToString();
			((ComboBox)_LogLevelComboBox.Setting).SelectedItem = ((ComboBox)_LogLevelComboBox.Setting).Items.OfType<TextBox>().FirstOrDefault(x => (LogSeverity)x.Tag == botSettings.LogLevel);
			var itemsSource = new List<TextBox>();
			foreach (var trustedUser in botSettings.TrustedUsers)
			{
				itemsSource.Add(UIModification.MakeTextBoxFromUserID(await client.GetUserAsync(trustedUser)));
			}
			_TrustedUsersComboBox.ItemsSource = itemsSource;
		}
		private bool CheckIfTreeViewItemFileExists(TreeViewItem treeItem)
		{
			var fileLocation = ((FileInformation)treeItem.Tag).FileLocation;
			if (fileLocation == null || fileLocation == ((string)_SpecificFileDisplay.Tag))
			{
				return false;
			}

			_SpecificFileDisplay.Clear();
			_SpecificFileDisplay.Tag = fileLocation;
			using (var reader = new StreamReader(fileLocation))
			{
				_SpecificFileDisplay.AppendText(reader.ReadToEnd());
			}
			return true;
		}
		private async Task<bool> CheckIfTreeViewItemDMExists(TreeViewItem treeItem)
		{
			var DMChannel = (IDMChannel)treeItem.Tag;
			if (DMChannel == null || DMChannel.Id == ((IDMChannel)_SpecificDMDisplay.Tag)?.Id)
			{
				return false;
			}

			_SpecificDMDisplay.Clear();
			_SpecificDMDisplay.Tag = DMChannel;

			var messages = Actions.Formatting.FormatDMs(await Messages.GetBotDMs(DMChannel));
			if (messages.Any())
			{
				foreach (var message in messages)
				{
					_SpecificDMDisplay.AppendText(String.Format("{0}{1}----------{1}", Actions.Formatting.RemoveMarkdownChars(message, true), Environment.NewLine));
				}
			}
			else
			{
				_SpecificDMDisplay.AppendText(String.Format("No DMs with this user exist; I am not sure why Discord says some do, but I will close the DMs with this person now."));
				await DMChannel.CloseAsync();
			}
			return true;
		}
		private bool SaveSetting(object obj, SettingOnBot setting, IBotSettings botSettings)
		{
			if (obj is Grid)
			{
				return SaveSetting(obj as Grid, setting, botSettings);
			}
			else if (obj is TextBox)
			{
				return SaveSetting(obj as TextBox, setting, botSettings);
			}
			else if (obj is Viewbox)
			{
				return SaveSetting(obj as Viewbox, setting, botSettings);
			}
			else if (obj is CheckBox)
			{
				return SaveSetting(obj as CheckBox, setting, botSettings);
			}
			else if (obj is ComboBox)
			{
				return SaveSetting(obj as ComboBox, setting, botSettings);
			}
			else
			{
				return true;
			}
		}
		private bool SaveSetting(Grid g, SettingOnBot setting, IBotSettings botSettings)
		{
			var children = g.Children;
			foreach (var child in children)
			{
				return SaveSetting(child, setting, botSettings);
			}
			return true;
		}
		private bool SaveSetting(TextBox tb, SettingOnBot setting, IBotSettings botSettings)
		{
			var text = tb.Text;
			switch (setting)
			{
				case SettingOnBot.Prefix:
				{
					if (String.IsNullOrWhiteSpace(text))
					{
						return false;
					}
					else if (botSettings.Prefix != text)
					{
						botSettings.Prefix = text;
					}
					return true;
				}
				case SettingOnBot.BotOwnerID:
				{
					if (!ulong.TryParse(text, out ulong id))
					{
						return false;
					}
					else if (botSettings.BotOwnerID != id)
					{
						botSettings.BotOwnerID = id;
					}
					return true;
				}
				case SettingOnBot.Game:
				{
					if (botSettings.Game != text)
					{
						botSettings.Game = text;
					}
					return true;
				}
				case SettingOnBot.Stream:
				{
					if (!Misc.MakeSureInputIsValidTwitchAccountName(text))
					{
						return false;
					}
					else if (botSettings.Stream != text)
					{
						botSettings.Stream = text;
					}
					return true;
				}
				case SettingOnBot.ShardCount:
				{
					if (!uint.TryParse(text, out uint num))
					{
						return false;
					}
					else if (botSettings.ShardCount != num)
					{
						botSettings.ShardCount = num;
					}
					return true;
				}
				case SettingOnBot.MessageCacheCount:
				{
					if (!uint.TryParse(text, out uint num))
					{
						return false;
					}
					else if (botSettings.MessageCacheCount != num)
					{
						botSettings.MessageCacheCount = num;
					}
					return true;
				}
				case SettingOnBot.MaxUserGatherCount:
				{
					if (!uint.TryParse(text, out uint num))
					{
						return false;
					}
					else if (botSettings.MaxUserGatherCount != num)
					{
						botSettings.MaxUserGatherCount = num;
					}
					return true;
				}
				case SettingOnBot.MaxMessageGatherSize:
				{
					if (!uint.TryParse(text, out uint num))
					{
						return false;
					}
					else if (botSettings.MaxMessageGatherSize != num)
					{
						botSettings.MaxMessageGatherSize = num;
					}
					return true;
				}
				default:
				{
					return true;
				}
			}
		}
		private bool SaveSetting(Viewbox vb, SettingOnBot setting, IBotSettings botSettings)
		{
			return SaveSetting(vb.Child, setting, botSettings);
		}
		private bool SaveSetting(CheckBox cb, SettingOnBot setting, IBotSettings botSettings)
		{
			var isChecked = cb.IsChecked.Value;
			switch (setting)
			{
				case SettingOnBot.AlwaysDownloadUsers:
				{
					if (botSettings.AlwaysDownloadUsers != isChecked)
					{
						botSettings.AlwaysDownloadUsers = isChecked;
					}
					return true;
				}
				default:
				{
					return true;
				}
			}
		}
		private bool SaveSetting(ComboBox cb, SettingOnBot setting, IBotSettings botSettings)
		{
			switch (setting)
			{
				case SettingOnBot.LogLevel:
				{
					var selectedLogLevel = (LogSeverity)(cb.SelectedItem as TextBox).Tag;
					if (botSettings.LogLevel != selectedLogLevel)
					{
						botSettings.LogLevel = selectedLogLevel;
					}
					return true;
				}
				case SettingOnBot.TrustedUsers:
				{
					var updatedTrustedUsers = cb.Items.OfType<TextBox>().Select(x => (ulong)x.Tag).ToList();
					var removedUsers = botSettings.TrustedUsers.Except(updatedTrustedUsers);
					var addedUsers = updatedTrustedUsers.Except(botSettings.TrustedUsers);
					if (removedUsers.Any() || addedUsers.Any())
					{
						botSettings.TrustedUsers = updatedTrustedUsers;
					}
					return true;
				}
				default:
				{
					return true;
				}
			}
		}
	}

	public class UIModification
	{
		private static CancellationTokenSource _ToolTipCancellationTokenSource;

		public static void AddRows(Grid grid, int amount)
		{
			for (int i = 0; i < amount; ++i)
			{
				grid.RowDefinitions.Add(new RowDefinition());
			}
		}
		public static void AddCols(Grid grid, int amount)
		{
			for (int i = 0; i < amount; ++i)
			{
				grid.ColumnDefinitions.Add(new ColumnDefinition());
			}
		}
		public static void SetRowAndSpan(UIElement item, int start = 0, int length = 1)
		{
			Grid.SetRow(item, Math.Max(0, start));
			Grid.SetRowSpan(item, Math.Max(1, length));
		}
		public static void SetColAndSpan(UIElement item, int start = 0, int length = 1)
		{
			Grid.SetColumn(item, Math.Max(0, start));
			Grid.SetColumnSpan(item, Math.Max(1, length));
		}
		public static void AddElement(Grid parent, Grid child, int rowStart, int rowLength, int columnStart, int columnLength, int setRows = 0, int setColumns = 0)
		{
			AddRows(child, setRows);
			AddCols(child, setColumns);
			parent.Children.Add(child);
			SetRowAndSpan(child, rowStart, rowLength);
			SetColAndSpan(child, columnStart, columnLength);
		}
		public static void AddElement(Grid parent, UIElement child, int rowStart, int rowLength, int columnStart, int columnLength)
		{
			parent.Children.Add(child);
			SetRowAndSpan(child, rowStart, rowLength);
			SetColAndSpan(child, columnStart, columnLength);
		}
		public static void AddPlaceHolderTB(Grid parent, int rowStart, int rowLength, int columnStart, int columnLength)
		{
			AddElement(parent, new MyTextBox { IsReadOnly = true, }, rowStart, rowLength, columnStart, columnLength);
		}

		public static void SetColorMode(DependencyObject parent)
		{
			for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); ++i)
			{
				var element = VisualTreeHelper.GetChild(parent, i) as DependencyObject;
				if (element is Control)
				{
					if (element is CheckBox || element is ComboBox)
					{
						continue;
					}
					if (element is MyButton)
					{
						SwitchElementColor((MyButton)element);
					}
					else
					{
						SwitchElementColor((Control)element);
					}
				}
				SetColorMode(element);
			}
		}
		public static void SwitchElementColor(Control element)
		{
			var eleBackground = element.Background as SolidColorBrush;
			if (eleBackground == null)
			{
				element.SetResourceReference(Control.BackgroundProperty, ColorTarget.Base_Background);
			}
			var eleForeground = element.Foreground as SolidColorBrush;
			if (eleForeground == null)
			{
				element.SetResourceReference(Control.ForegroundProperty, ColorTarget.Base_Foreground);
			}
			var eleBorder = element.BorderBrush as SolidColorBrush;
			if (eleBorder == null)
			{
				element.SetResourceReference(Control.BorderBrushProperty, ColorTarget.Base_Border);
			}
		}
		public static void SwitchElementColor(MyButton element)
		{
			var style = element.Style;
			if (style == null)
			{
				element.SetResourceReference(Button.StyleProperty, OtherTarget.Button_Style);
			}
			var eleForeground = element.Foreground as SolidColorBrush;
			if (eleForeground == null)
			{
				element.SetResourceReference(Control.ForegroundProperty, ColorTarget.Base_Foreground);
			}
		}
		public static void SwitchElementColor(object element) { }

		public static Style MakeButtonStyle(Brush regBG, Brush regFG, Brush regB, Brush disabledBG, Brush disabledFG, Brush disabledB, Brush mouseOverBG)
		{
			var templateContentPresenter = new FrameworkElementFactory
			{
				Type = typeof(ContentPresenter),
			};
			templateContentPresenter.SetValue(ContentPresenter.MarginProperty, new Thickness(2));
			templateContentPresenter.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
			templateContentPresenter.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
			templateContentPresenter.SetValue(ContentPresenter.RecognizesAccessKeyProperty, true);

			var templateBorder = new FrameworkElementFactory
			{
				Type = typeof(Border),
				Name = "Border",
			};
			templateBorder.SetValue(Border.BorderThicknessProperty, new Thickness(1));
			templateBorder.SetValue(Border.BackgroundProperty, regBG);
			templateBorder.SetValue(Border.BorderBrushProperty, regB);
			templateBorder.AppendChild(templateContentPresenter);

			//Create the template
			var template = new ControlTemplate
			{
				TargetType = typeof(Button),
				VisualTree = templateBorder,
			};
			//Add in the triggers
			MakeButtonTriggers(regBG, regFG, regB, disabledBG, disabledFG, disabledB, mouseOverBG).ForEach(x => template.Triggers.Add(x));

			var buttonFocusRectangle = new FrameworkElementFactory
			{
				Type = typeof(System.Windows.Shapes.Rectangle),
			};
			buttonFocusRectangle.SetValue(System.Windows.Shapes.Shape.MarginProperty, new Thickness(2));
			buttonFocusRectangle.SetValue(System.Windows.Shapes.Shape.StrokeThicknessProperty, 1.0);
			buttonFocusRectangle.SetValue(System.Windows.Shapes.Shape.StrokeProperty, UIModification.MakeBrush("#60000000"));
			buttonFocusRectangle.SetValue(System.Windows.Shapes.Shape.StrokeDashArrayProperty, new DoubleCollection { 1.0, 2.0 });

			var buttonFocusBorder = new FrameworkElementFactory
			{
				Type = typeof(Border),
			};
			buttonFocusBorder.AppendChild(buttonFocusRectangle);

			var buttonFocusVisual = new Style();
			new List<Setter>
			{
				new Setter
				{
					Property = Control.TemplateProperty,
					Value = new ControlTemplate
					{
						VisualTree = buttonFocusBorder,
					}
				},
			}.ForEach(x => buttonFocusVisual.Setters.Add(x));

			//Add in the template
			var buttonStyle = new Style();
			new List<Setter>
			{
				new Setter
				{
					Property = Button.SnapsToDevicePixelsProperty,
					Value = true,
				},
				new Setter
				{
					Property = Button.OverridesDefaultStyleProperty,
					Value = true,
				},
				new Setter
				{
					Property = Button.FocusVisualStyleProperty,
					Value = buttonFocusVisual,
				},
				new Setter
				{
					Property = Button.TemplateProperty,
					Value = template,
				},
			}.ForEach(x => buttonStyle.Setters.Add(x));

			return buttonStyle;
		}
		public static List<Trigger> MakeButtonTriggers(Brush regBG, Brush regFG, Brush regB, Brush disabledBG, Brush disabledFG, Brush disabledB, Brush mouseOverBG)
		{
			//This used to have 5 triggers until I realized how useless a lot of them were.
			var isMouseOverTrigger = new Trigger
			{
				Property = Button.IsMouseOverProperty,
				Value = true,
			};
			new List<Setter>
			{
				new Setter
				{
					TargetName = "Border",
					Property = Border.BackgroundProperty,
					Value = mouseOverBG,
				},
			}.ForEach(x => isMouseOverTrigger.Setters.Add(x));

			var isEnabledTrigger = new Trigger
			{
				Property = Button.IsEnabledProperty,
				Value = false,
			};
			new List<Setter>
			{
				new Setter
				{
					TargetName = "Border",
					Property = Border.BackgroundProperty,
					Value = disabledBG,
				},
				new Setter
				{
					TargetName = "Border",
					Property = Border.BorderBrushProperty,
					Value = disabledB,
				},
				new Setter
				{
					Property = Button.ForegroundProperty,
					Value = disabledFG,
				},
			}.ForEach(x => isEnabledTrigger.Setters.Add(x));

			return new List<Trigger> { isMouseOverTrigger, isEnabledTrigger };
		}
		public static Brush MakeBrush(string color)
		{
			return (SolidColorBrush)new BrushConverter().ConvertFrom(color);
		}
		public static bool CheckIfTwoBrushesAreTheSame(Brush b1, Brush b2)
		{
			var nullableColor1 = ((SolidColorBrush)b1)?.Color;
			var nullableColor2 = ((SolidColorBrush)b2)?.Color;
			var color1IsNull = !nullableColor1.HasValue;
			var color2IsNull = !nullableColor2.HasValue;
			if (color1IsNull || color2IsNull)
			{
				return color1IsNull && color2IsNull;
			}

			var color1 = nullableColor1.Value;
			var color2 = nullableColor2.Value;

			var a = color1.A == color2.A;
			var r = color1.R == color2.R;
			var g = color1.G == color2.G;
			var b = color1.B == color2.B;
			return a && r && g && b;
		}
		public static string FormatBrush(Brush b)
		{
			var color = ((SolidColorBrush)b)?.Color;
			if (!color.HasValue)
				return "";

			var c = color.Value;
			return String.Format("#{0}{1}{2}{3}", c.A.ToString("X2"), c.R.ToString("X2"), c.G.ToString("X2"), c.B.ToString("X2"));
		}

		public static void SetFontSizeProperties(double size, params IEnumerable<UIElement>[] elements)
		{
			foreach (var ele in elements.SelectMany(x => x))
			{
				SetFontSizeProperty(ele, size);
			}
		}
		public static void SetFontSizeProperty(UIElement element, double size)
		{
			if (element is Control)
			{
				(element as Control).SetBinding(Control.FontSizeProperty, new Binding
				{
					Path = new PropertyPath("ActualHeight"),
					RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Grid), 1),
					Converter = new UIFontResizer(size),
				});
			}
			else if (element is Grid)
			{
				foreach (var child in (element as Grid).Children.OfType<Control>())
				{
					SetFontSizeProperty(child, size);
				}
			}
		}

		public static void ToggleToolTip(ToolTip ttip)
		{
			ttip.IsOpen = !ttip.IsOpen;
		}
		public static async Task MakeFollowingToolTip(UIElement baseElement, ToolTip tt, string text, int timeInMS = 2500)
		{
			tt.Content = text ?? "Blank.";
			tt.IsOpen = true;
			baseElement.MouseMove += (sender, e) =>
			{
				var point = System.Windows.Forms.Control.MousePosition;
				tt.HorizontalOffset = point.X;
				tt.VerticalOffset = point.Y;
			};

			if (_ToolTipCancellationTokenSource != null)
			{
				_ToolTipCancellationTokenSource.Cancel();
			}
			_ToolTipCancellationTokenSource = new CancellationTokenSource();

			await baseElement.Dispatcher.InvokeAsync(async () =>
			{
				try
				{
					await Task.Delay(timeInMS, _ToolTipCancellationTokenSource.Token);
				}
				catch (TaskCanceledException)
				{
					return;
				}
				catch (Exception e)
				{
					ConsoleActions.ExceptionToConsole(e);
					return;
				}

				tt.IsOpen = false;
			});
		}

		public static int[][] FigureOutWhereToPutBG(Grid parent, UIElement child)
		{
			var rowTotal = parent.RowDefinitions.Count;
			var columnTotal = parent.ColumnDefinitions.Count;

			var rowStart = Grid.GetRow(child);
			var rowSpan = Grid.GetRowSpan(child);
			var columnStart = Grid.GetColumn(child);
			var columnSpan = Grid.GetColumnSpan(child);

			var start = 0;
			var temp = new int[4][];

			/* Example:
			 * Row start		  0		10		 90		10
			 * Row span			 10		80		 10		80
			 * Column start		  0		 0		  0		90
			 * Column span		100		10		100		10
			 */

			var a1p1 = start;
			var a1p2 = rowStart;
			var a1p3 = start;
			var a1p4 = columnTotal;
			temp[0] = new[] { a1p1, a1p2, a1p3, a1p4, };

			var a2p1 = rowStart;
			var a2p2 = rowSpan;
			var a2p3 = start;
			var a2p4 = columnStart;
			temp[1] = new[] { a2p1, a2p2, a2p3, a2p4, };

			var a3p1 = rowStart + rowSpan;
			var a3p2 = rowTotal - a3p1;
			var a3p3 = start;
			var a3p4 = columnTotal;
			temp[2] = new[] { a3p1, a3p2, a3p3, a3p4, };

			var a4p1 = rowStart;
			var a4p2 = rowSpan;
			var a4p3 = columnStart + columnSpan;
			var a4p4 = columnTotal - a4p3;
			temp[3] = new[] { a4p1, a4p2, a4p3, a4p4, };

			return temp;
		}
		public static void PutInBG(Grid parent, UIElement child, Brush brush)
		{
			PutInBGWithMouseUpEvent(parent, child, brush);
		}
		public static void PutInBGWithMouseUpEvent(Grid parent, UIElement child, Brush brush = null, MouseButtonEventHandler action = null)
		{
			//Because setting the entire layout with the MouseUp event meant the empty combobox when clicked would trigger it even when IsHitTestVisible = True. No idea why, but this is the workaround.
			var BGPoints = FigureOutWhereToPutBG(parent, child);
			for (int i = 0; i < BGPoints.GetLength(0); ++i)
			{
				var temp = new Grid { Background = brush ?? Brushes.Transparent, SnapsToDevicePixels = true, };
				if (action != null)
				{
					temp.MouseUp += action;
				}
				AddElement(parent, temp, BGPoints[i][0], BGPoints[i][1], BGPoints[i][2], BGPoints[i][3]);
			}
		}

		public static Hyperlink MakeHyperlink(string link, string name)
		{
			//Make sure the input is a valid link
			if (!Uploads.ValidateURL(link))
			{
				ConsoleActions.WriteLine(Actions.Formatting.ERROR("Invalid URL."));
				return null;
			}
			//Create the hyperlink
			var hyperlink = new Hyperlink(new Run(name))
			{
				NavigateUri = new Uri(link),
				IsEnabled = true,
			};
			//Make it work when clicked
			hyperlink.RequestNavigate += (sender, e) =>
			{
				Process.Start(e.Uri.ToString());
				e.Handled = true;
			};
			return hyperlink;
		}
		public static TextBox MakeTitle(string text, string summary)
		{
			ToolTip tt = null;
			if (!String.IsNullOrWhiteSpace(summary))
			{
				tt = new ToolTip
				{
					Content = summary,
				};
			}

			var tb = new MyTextBox
			{
				Text = text,
				IsReadOnly = true,
				BorderThickness = new Thickness(0),
				VerticalAlignment = VerticalAlignment.Center,
				HorizontalAlignment = HorizontalAlignment.Left,
				TextWrapping = TextWrapping.WrapWithOverflow,
			};

			if (tt != null)
			{
				tb.MouseEnter += (sender, e) =>
				{
					ToggleToolTip(tt);
				};
				tb.MouseLeave += (sender, e) =>
				{
					ToggleToolTip(tt);
				};
			}

			return tb;
		}
		public static TextBox MakeSetting(SettingOnBot setting, int length)
		{
			return new MyTextBox
			{
				VerticalContentAlignment = VerticalAlignment.Center,
				Tag = setting,
				MaxLength = length
			};
		}
		public static TextBox MakeSysInfoBox()
		{
			return new MyTextBox
			{
				IsReadOnly = true,
				BorderThickness = new Thickness(0, .5, 0, .5),
				Background = null,
			};
		}
		public static TextBox MakeTextBoxFromUserID(IUser user)
		{
			if (user == null)
			{
				return null;
			}

			return new MyTextBox
			{
				Text = String.Format("'{0}#{1}' ({2})", (user.Username.AllCharactersAreWithinUpperLimit(Constants.MAX_UTF16_VAL_FOR_NAMES) ? user.Username : "Non-Standard Name"), user.Discriminator, user.Id),
				Tag = user.Id,
				IsReadOnly = true,
				IsHitTestVisible = false,
				BorderThickness = new Thickness(0),
				Background = Brushes.Transparent,
				Foreground = Brushes.Black,
			};
		}
		public static Viewbox MakeStandardViewBox(string text)
		{
			return new Viewbox
			{
				Child = new MyTextBox
				{
					Text = text,
					VerticalAlignment = VerticalAlignment.Bottom,
					IsReadOnly = true,
					BorderThickness = new Thickness(0)
				},
				HorizontalAlignment = HorizontalAlignment.Left
			};
		}
		public static TreeView MakeGuildTreeView(TreeView tv, IEnumerable<IGuild> guilds)
		{
			//Get the directory
			var directory = Gets.GetBaseBotDirectory();
			if (directory == null || !Directory.Exists(directory))
				return tv;

			//Remove its parent so it can be added back to something
			var parent = tv.Parent;
			if (parent != null)
			{
				(parent as InlineUIContainer).Child = null;
			}

			tv.BorderThickness = new Thickness(0);
			tv.Background = (Brush)Application.Current.Resources[ColorTarget.Base_Background];
			tv.Foreground = (Brush)Application.Current.Resources[ColorTarget.Base_Foreground];
			tv.ItemsSource = Directory.GetDirectories(directory).Select(guildDir =>
			{
				//Separate the ID from the rest of the directory
				var strID = guildDir.Substring(guildDir.LastIndexOf('\\') + 1);
				//Make sure the ID is valid
				if (!ulong.TryParse(strID, out ulong ID))
					return null;

				var guild = guilds.FirstOrDefault(x => x.Id == ID);
				if (guild == null)
					return null;

				//Get all of the files
				var listOfFiles = new List<TreeViewItem>();
				Directory.GetFiles(guildDir).ToList().ForEach(fileLoc =>
				{
					var fileType = Gets.GetFileType(Path.GetFileNameWithoutExtension(fileLoc));
					if (!fileType.HasValue)
						return;

					var fileItem = new TreeViewItem
					{
						Header = Path.GetFileName(fileLoc),
						Tag = new FileInformation(fileType.Value, fileLoc),
						Background = (Brush)Application.Current.Resources[ColorTarget.Base_Background],
						Foreground = (Brush)Application.Current.Resources[ColorTarget.Base_Foreground],
					};
					listOfFiles.Add(fileItem);
				});

				//If no items then don't bother adding in the guild to the treeview
				if (!listOfFiles.Any())
					return null;

				//Create the guild item
				var guildItem = new TreeViewItem
				{
					Header = guild.FormatGuild(),
					Tag = new GuildFileInformation(ID, guild.Name, (guild as Discord.WebSocket.SocketGuild).MemberCount),
					Background = (Brush)Application.Current.Resources[ColorTarget.Base_Background],
					Foreground = (Brush)Application.Current.Resources[ColorTarget.Base_Foreground],
				};
				listOfFiles.ForEach(x =>
				{
					guildItem.Items.Add(x);
				});

				return guildItem;
			}).Where(x => x != null).OrderByDescending(x => ((GuildFileInformation)x.Tag).MemberCount);

			return tv;
		}
		public static TreeView MakeDMTreeView(TreeView tv, IEnumerable<IDMChannel> dms)
		{
			//Remove its parent so it can be added back to something
			var parent = tv.Parent;
			if (parent != null)
			{
				(parent as InlineUIContainer).Child = null;
			}

			tv.BorderThickness = new Thickness(0);
			tv.Background = (Brush)Application.Current.Resources[ColorTarget.Base_Background];
			tv.Foreground = (Brush)Application.Current.Resources[ColorTarget.Base_Foreground];
			tv.ItemsSource = dms.Select(x =>
			{
				var user = x.Recipient;
				if (user == null)
					return null;

				return new TreeViewItem
				{
					Header = String.Format("'{0}#{1}' ({2})", (user.Username.AllCharactersAreWithinUpperLimit(Constants.MAX_UTF16_VAL_FOR_NAMES) ? user.Username : "Non-Standard Name"), user.Discriminator, user.Id),
					Tag = x,
					Background = (Brush)Application.Current.Resources[ColorTarget.Base_Background],
					Foreground = (Brush)Application.Current.Resources[ColorTarget.Base_Foreground],
				};
			}).Where(x => x != null);

			if (tv.ItemsSource.Cast<object>().Count() == 0)
			{
				var temp = new TreeViewItem
				{
					Header = "No DMs",
					Background = (Brush)Application.Current.Resources[ColorTarget.Base_Background],
					Foreground = (Brush)Application.Current.Resources[ColorTarget.Base_Foreground],
				};
				tv.ItemsSource = new[] { temp };
			}

			return tv;
		}
		public static FlowDocument MakeMainMenu()
		{
			var defs1 = "Latency:\n\tTime it takes for a command to reach the bot.\nMemory:\n\tAmount of RAM the program is using.\n\t(This is wrong most of the time.)";
			var defs2 = "Threads:\n\tWhere all the actions in the bot happen.\nShards:\n\tHold all the guilds a bot has on its client.\n\tThere is a limit of 2500 guilds per shard.";
			var vers = String.Format("\nAPI Wrapper Version: {0}\nBot Version: {1}\nGitHub Repository: ", Constants.API_VERSION, Constants.BOT_VERSION);
			var help = "\n\nNeed additional help? Join the Discord server: ";
			var all = String.Join("\n", defs1, defs2, vers);

			var temp = new Paragraph();
			temp.Inlines.Add(new Run(all));
			temp.Inlines.Add(MakeHyperlink(Constants.REPO, "Advobot"));
			temp.Inlines.Add(new Run(help));
			temp.Inlines.Add(MakeHyperlink(Constants.DISCORD_INV, "Here"));

			return new FlowDocument(temp);
		}
		public static FlowDocument MakeInfoMenu(string botUptime, string formattedLoggedCommands, string formattedLoggedThings)
		{
			var uptime = String.Format("Uptime: {0}", botUptime);
			var cmds = String.Format("Logged Commands:\n{0}", formattedLoggedCommands);
			var logs = String.Format("Logged Actions:\n{0}", formattedLoggedThings);
			var str = Actions.Formatting.RemoveMarkdownChars(String.Format("{0}\r\r{1}\r\r{2}", uptime, cmds, logs), true);
			var paragraph = new Paragraph(new Run(str))
			{
				TextAlignment = TextAlignment.Center,
			};
			return new FlowDocument(paragraph);
		}
		public static Grid MakeColorDisplayer(UISettings UISettings, Grid child, Button button, double fontSizeProperty)
		{
			child.Children.Clear();
			AddPlaceHolderTB(child, 0, 100, 0, 100);

			var themeTitle = MakeTitle("Themes:", "");
			SetFontSizeProperty(themeTitle, fontSizeProperty);
			AddElement(child, themeTitle, 2, 5, 10, 55);

			var themeComboBox = new MyComboBox
			{
				VerticalContentAlignment = VerticalAlignment.Center,
				ItemsSource = MakeComboBoxSourceOutOfEnum(typeof(ColorTheme)),
			};
			themeComboBox.SelectedItem = themeComboBox.Items.Cast<TextBox>().FirstOrDefault(x => (ColorTheme)x.Tag == UISettings.Theme);
			AddElement(child, themeComboBox, 2, 5, 65, 25);

			var colorResourceKeys = Enum.GetValues(typeof(ColorTarget)).Cast<ColorTarget>().ToArray();
			for (int i = 0; i < colorResourceKeys.Length; ++i)
			{
				var key = colorResourceKeys[i];
				var value = FormatBrush(UISettings.ColorTargets[key]);

				var title = MakeTitle(String.Format("{0}:", key.EnumName()), "");
				var setting = new MyTextBox
				{
					VerticalContentAlignment = VerticalAlignment.Center,
					Tag = key,
					MaxLength = 10,
					Text = value,
				};
				AddElement(child, title, i * 5 + 7, 5, 10, 55);
				AddElement(child, setting, i * 5 + 7, 5, 65, 25);
				SetFontSizeProperties(fontSizeProperty, new[] { title, setting });
			}

			AddElement(child, button, 95, 5, 0, 100);
			SetColorMode(child);

			return child;
		}
		public static IEnumerable<TextBox> MakeComboBoxSourceOutOfEnum(Type type)
		{
			return Enum.GetValues(type).Cast<object>().Select(x =>
			{
				return new MyTextBox
				{
					Text = Enum.GetName(type, x),
					Tag = x,
					IsReadOnly = true,
					IsHitTestVisible = false,
					BorderThickness = new Thickness(0),
					Background = Brushes.Transparent,
					Foreground = Brushes.Black,
				};
			});
		}
		public static IEnumerable<TextBox> MakeComboBoxSourceOutOfStrings(IEnumerable<string> strings)
		{
			return strings.Select(x =>
			{
				return new MyTextBox
				{
					Text = x,
					Tag = x,
					IsReadOnly = true,
					IsHitTestVisible = false,
					BorderThickness = new Thickness(0),
					Background = Brushes.Transparent,
					Foreground = Brushes.Black,
				};
			});
		}
	}

	public class UICommandHandler
	{
		public static string GatherInput(TextBox tb, Button b)
		{
			var text = tb.Text.Trim(new[] { '\r', '\n' });
			if (text.Contains("﷽"))
			{
				text += "This program really doesn't like that long Arabic character for some reason. Whenever there are a lot of them it crashes the program completely.";
			}
			ConsoleActions.WriteLine(text);

			tb.Text = "";
			b.IsEnabled = false;

			return text;
		}
		public static void HandleCommand(string input, string prefix)
		{
			if (input.CaseInsStartsWith(prefix))
			{
				var inputArray = input.Substring(prefix.Length)?.Split(new[] { ' ' }, 2);
				if (!FindCommand(inputArray[0], inputArray.Length > 1 ? inputArray[1] : null))
				{
					ConsoleActions.WriteLine("No command could be found with that name.");
				}
			}
		}
		public static bool FindCommand(string cmd, string args)
		{
			//Find what command it belongs to
			if ("test".CaseInsEquals(cmd))
			{
				UITest();
			}
			else
			{
				return false;
			}
			return true;
		}
		public static void UITest()
		{
#if DEBUG
			var codeLen = true;
			if (codeLen)
			{
				var totalChars = 0;
				var totalLines = 0;
				foreach (var file in Directory.GetFiles(Path.GetFullPath(Path.Combine(System.Reflection.Assembly.GetExecutingAssembly().Location, @"..\..\..\"))))
				{
					if (".cs".CaseInsEquals(Path.GetExtension(file)))
					{
						totalChars += File.ReadAllText(file).Length;
						totalLines += File.ReadAllLines(file).Count();
					}
				}
				ConsoleActions.WriteLine(String.Format("Current Totals:{0}\t\t\t Chars: {1}{0}\t\t\t Lines: {2}", Environment.NewLine, totalChars, totalLines));
			}
			var resetInfo = false;
			if (resetInfo)
			{
				Properties.Settings.Default.BotKey = null;
				Properties.Settings.Default.Path = null;
				Properties.Settings.Default.BotName = null;
				Properties.Settings.Default.BotID = 0;
				Properties.Settings.Default.Save();
				Misc.DisconnectBot();
			}
#endif
		}
	}

	public class UITextBoxStreamWriter : TextWriter 
	{
		private TextBoxBase _Output;
		private bool _IgnoreNewLines;
		private string _CurrentLineText;

		public UITextBoxStreamWriter(TextBoxBase output)
		{
			_Output = output;
			_IgnoreNewLines = output is RichTextBox;
		}

		public override void Write(char value)
		{
			if (value.Equals('\n'))
			{
				Write(_CurrentLineText);
				_CurrentLineText = null;
			}
			//Done because crashes program without exception. Could not for the life of me figure out why; something in the .dlls themself.
			else if (value.Equals('﷽'))
			{
				return;
			}
			else
			{
				_CurrentLineText += value;
			}
		}
		public override void Write(string value)
		{
			if (value == null || (_IgnoreNewLines && value.Equals('\n')))
				return;

			_Output.Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new Action(() =>
			{
				_Output.AppendText(value);
			}));
		}
		public override Encoding Encoding
		{
			get { return Encoding.UTF8; }
		}
	}

	public class UIFontResizer : IValueConverter
	{
		private double _ConvertFactor;

		public UIFontResizer(double convertFactor)
		{
			_ConvertFactor = convertFactor;
		}

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var converted = (int)(System.Convert.ToInt16(value) * _ConvertFactor);
			return Math.Max(converted, -1);
		}
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	public struct SettingInMenu
	{
		public UIElement Setting;
		public TextBox Title;
	}

	public class MyRichTextBox : RichTextBox
	{
		public MyRichTextBox()
		{
			this.Background = null;
			this.Foreground = null;
			this.BorderBrush = null;
		}
	}

	public class MyTextBox : TextBox
	{
		public MyTextBox()
		{
			this.Background = null;
			this.Foreground = null;
			this.BorderBrush = null;
			this.TextWrapping = TextWrapping.Wrap;
		}
	}

	public class MyButton : Button
	{
		public MyButton()
		{
			this.Background = null;
			this.Foreground = null;
			this.BorderBrush = null;
		}
	}

	public class MyComboBox : ComboBox
	{
		public MyComboBox()
		{
			this.VerticalContentAlignment = VerticalAlignment.Center;
		}
	}

	public class MyNumberBox : MyTextBox
	{
		public MyNumberBox()
		{
			this.PreviewTextInput += MakeSureKeyIsNumber;
			DataObject.AddPastingHandler(this, MakeSurePasteIsNumbers);
		}

		private void MakeSureKeyIsNumber(object sender, TextCompositionEventArgs e)
		{
			e.Handled = !char.IsDigit(e.Text, e.Text.Length - 1);
		}
		private void MakeSurePasteIsNumbers(object sender, DataObjectPastingEventArgs e)
		{
			if (!e.SourceDataObject.GetDataPresent(DataFormats.UnicodeText, true))
			{
				return;
			}

			var textBeingPasted = e.SourceDataObject.GetData(DataFormats.UnicodeText).ToString();
			var onlyNums = System.Text.RegularExpressions.Regex.Replace(textBeingPasted, @"[^\d]", "", System.Text.RegularExpressions.RegexOptions.Compiled);
			this.Text = onlyNums.Substring(0, Math.Min(this.MaxLength, onlyNums.Length));
			e.CancelCommand();
		}
	}

	public class UISettings
	{
		[JsonIgnore]
		private static readonly Brush LightModeBackground = UIModification.MakeBrush("#FFFFFF");
		[JsonIgnore]
		private static readonly Brush LightModeForeground = UIModification.MakeBrush("#000000");
		[JsonIgnore]
		private static readonly Brush LightModeBorder = UIModification.MakeBrush("#ABADB3");
		[JsonIgnore]
		private static readonly Brush LightModeButtonBackground = UIModification.MakeBrush("#DDDDDD");
		[JsonIgnore]
		private static readonly Brush LightModeButtonBorder = UIModification.MakeBrush("#707070");
		[JsonIgnore]
		private static readonly Brush LightModeButtonDisabledBackground = UIModification.MakeBrush("#F4F4F4");
		[JsonIgnore]
		private static readonly Brush LightModeButtonDisabledForeground = UIModification.MakeBrush("#888888");
		[JsonIgnore]
		private static readonly Brush LightModeButtonDisabledBorder = UIModification.MakeBrush("#ADB2B5");
		[JsonIgnore]
		private static readonly Brush LightModeButtonMouseOver = UIModification.MakeBrush("#BEE6FD");
		[JsonIgnore]
		private static readonly Style LightModeButtonStyle = UIModification.MakeButtonStyle
			(
			LightModeButtonBackground,
			LightModeForeground,
			LightModeButtonBorder,
			LightModeButtonDisabledBackground,
			LightModeButtonDisabledForeground,
			LightModeButtonDisabledBorder,
			LightModeButtonMouseOver
			);

		[JsonIgnore]
		private static readonly Brush DarkModeBackground = UIModification.MakeBrush("#1C1C1C");
		[JsonIgnore]
		private static readonly Brush DarkModeForeground = UIModification.MakeBrush("#E1E1E1");
		[JsonIgnore]
		private static readonly Brush DarkModeBorder = UIModification.MakeBrush("#ABADB3");
		[JsonIgnore]
		private static readonly Brush DarkModeButtonBackground = UIModification.MakeBrush("#151515");
		[JsonIgnore]
		private static readonly Brush DarkModeButtonBorder = UIModification.MakeBrush("#ABADB3");
		[JsonIgnore]
		private static readonly Brush DarkModeButtonDisabledBackground = UIModification.MakeBrush("#343434");
		[JsonIgnore]
		private static readonly Brush DarkModeButtonDisabledForeground = UIModification.MakeBrush("#A0A0A0");
		[JsonIgnore]
		private static readonly Brush DarkModeButtonDisabledBorder = UIModification.MakeBrush("#ADB2B5");
		[JsonIgnore]
		private static readonly Brush DarkModeButtonMouseOver = UIModification.MakeBrush("#303333");
		[JsonIgnore]
		private static readonly Style DarkModeButtonStyle = UIModification.MakeButtonStyle
			(
			DarkModeButtonBackground,
			DarkModeForeground,
			DarkModeButtonBorder,
			DarkModeButtonDisabledBackground,
			DarkModeButtonDisabledForeground,
			DarkModeButtonDisabledBorder,
			DarkModeButtonMouseOver
			);

		[JsonProperty("Theme")]
		public ColorTheme Theme { get; private set; } = ColorTheme.Classic;
		[JsonProperty("ColorTargets")]
		public Dictionary<ColorTarget, Brush> ColorTargets { get; private set; } = new Dictionary<ColorTarget, Brush>();

		public UISettings()
		{
			foreach (var target in Enum.GetValues(typeof(ColorTarget)).Cast<ColorTarget>())
			{
				ColorTargets.Add(target, null);
			}
		}

		public void SetTheme(ColorTheme theme)
		{
			Theme = theme;
		}
		public void SaveSettings()
		{
			SavingAndLoading.OverWriteFile(Gets.GetBaseBotDirectory(Constants.UI_INFO_LOCATION), SavingAndLoading.Serialize(this));
		}
		public void InitializeColors()
		{
			var res = Application.Current.Resources;
			res.Add(ColorTarget.Base_Background, LightModeBackground);
			res.Add(ColorTarget.Base_Foreground, LightModeForeground);
			res.Add(ColorTarget.Base_Border, LightModeBorder);
			res.Add(ColorTarget.Button_Background, LightModeButtonBackground);
			res.Add(ColorTarget.Button_Border, LightModeButtonBorder);
			res.Add(ColorTarget.Button_Disabled_Background, LightModeButtonDisabledBackground);
			res.Add(ColorTarget.Button_Disabled_Foreground, LightModeButtonDisabledForeground);
			res.Add(ColorTarget.Button_Disabled_Border, LightModeButtonDisabledBorder);
			res.Add(ColorTarget.Button_Mouse_Over_Background, LightModeButtonMouseOver);
			res.Add(OtherTarget.Button_Style, LightModeButtonStyle);
		}
		public void ActivateTheme()
		{
			switch (Theme)
			{
				case ColorTheme.Classic:
				{
					ActivateClassic();
					return;
				}
				case ColorTheme.Dark_Mode:
				{
					ActivateDarkMode();
					return;
				}
				case ColorTheme.User_Made:
				{
					ActivateUserMade();
					return;
				}
			}
		}
		private void ActivateClassic()
		{
			var res = Application.Current.Resources;
			res[ColorTarget.Base_Background] = LightModeBackground;
			res[ColorTarget.Base_Foreground] = LightModeForeground;
			res[ColorTarget.Base_Border] = LightModeBorder;
			res[ColorTarget.Button_Background] = LightModeButtonBackground;
			res[ColorTarget.Button_Border] = LightModeButtonBorder;
			res[ColorTarget.Button_Disabled_Background] = LightModeButtonDisabledBackground;
			res[ColorTarget.Button_Disabled_Foreground] = LightModeButtonDisabledForeground;
			res[ColorTarget.Button_Disabled_Border] = LightModeButtonDisabledBorder;
			res[ColorTarget.Button_Mouse_Over_Background] = LightModeButtonMouseOver;
			res[OtherTarget.Button_Style] = LightModeButtonStyle;
		}
		private void ActivateDarkMode()
		{
			var res = Application.Current.Resources;
			res[ColorTarget.Base_Background] = DarkModeBackground;
			res[ColorTarget.Base_Foreground] = DarkModeForeground;
			res[ColorTarget.Base_Border] = DarkModeBorder;
			res[ColorTarget.Button_Background] = DarkModeButtonBackground;
			res[ColorTarget.Button_Border] = DarkModeButtonBorder;
			res[ColorTarget.Button_Disabled_Background] = DarkModeButtonDisabledBackground;
			res[ColorTarget.Button_Disabled_Foreground] = DarkModeButtonDisabledForeground;
			res[ColorTarget.Button_Disabled_Border] = DarkModeButtonDisabledBorder;
			res[ColorTarget.Button_Mouse_Over_Background] = DarkModeButtonMouseOver;
			res[OtherTarget.Button_Style] = DarkModeButtonStyle;
		}
		private void ActivateUserMade()
		{
			var res = Application.Current.Resources;
			foreach (var kvp in ColorTargets)
			{
				res[kvp.Key] = kvp.Value;
			}
			res[OtherTarget.Button_Style] = UIModification.MakeButtonStyle
				(
				(Brush)res[ColorTarget.Base_Background],
				(Brush)res[ColorTarget.Base_Foreground],
				(Brush)res[ColorTarget.Base_Border],
				(Brush)res[ColorTarget.Button_Disabled_Background],
				(Brush)res[ColorTarget.Button_Disabled_Foreground],
				(Brush)res[ColorTarget.Button_Disabled_Border],
				(Brush)res[ColorTarget.Button_Mouse_Over_Background]
				);
		}

		public static UISettings LoadUISettings(bool loaded)
		{
			var UISettings = new UISettings();
			var path = Gets.GetBaseBotDirectory(Constants.UI_INFO_LOCATION);
			if (!File.Exists(path))
			{
				if (loaded)
				{
					ConsoleActions.WriteLine("The bot UI information file does not exist.");
				}
				return UISettings;
			}

			try
			{
				using (var reader = new StreamReader(path))
				{
					UISettings = JsonConvert.DeserializeObject<UISettings>(reader.ReadToEnd());
				}
			}
			catch (Exception e)
			{
				ConsoleActions.ExceptionToConsole(e);
			}
			return UISettings;
		}
	}

	public struct BrushTargetAndValue
	{
		[JsonProperty]
		public ColorTarget Target { get; private set; }
		[JsonProperty]
		public Brush Brush { get; private set; }

		public BrushTargetAndValue(ColorTarget target, string colorString)
		{
			Target = target;
			Brush = UIModification.MakeBrush(colorString);
		}
	}

	[Flags]
	public enum ColorTheme : uint
	{
		Classic								= (1U << 0),
		Dark_Mode							= (1U << 1),
		User_Made							= (1U << 2),
	}

	[Flags]
	public enum ColorTarget : uint
	{
		Base_Background						= (1U << 0),
		Base_Foreground						= (1U << 1),
		Base_Border							= (1U << 2),
		Button_Background					= (1U << 3),
		Button_Border						= (1U << 4),
		Button_Disabled_Background			= (1U << 5),
		Button_Disabled_Foreground			= (1U << 6),
		Button_Disabled_Border				= (1U << 7),
		Button_Mouse_Over_Background		= (1U << 8),
	}

	[Flags]
	public enum OtherTarget : uint
	{
		Button_Style						= (1U << 0),
	}

	[Flags]
	public enum MenuType : uint
	{
		Main								= (1U << 1),
		Info								= (1U << 2),
		Settings							= (1U << 3),
		Colors								= (1U << 4),
		DMs									= (1U << 5),
		Files								= (1U << 6),
	}
}
