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

namespace Advobot
{
	public class BotWindow : Window
	{
		private readonly Grid mLayout = new Grid();
		private readonly ToolTip mToolTip = new ToolTip { Placement = PlacementMode.Relative };
		private readonly BotUIInfo mUIInfo = BotUIInfo.LoadBotUIInfo();

		#region Input
		private readonly Grid mInputLayout = new Grid();
		//Max height has to be set here as a large number to a) not get in the way and b) not crash when resized small. I don't want to use a RTB for input.
		private readonly TextBox mInput = new MyTextBox { TextWrapping = TextWrapping.Wrap, MaxLength = 250, MaxLines = 5, MaxHeight = 1000, };
		private readonly Button mInputButton = new MyButton { Content = "Enter", IsEnabled = false, };
		#endregion

		#region Output
		private readonly MenuItem mOutputContextMenuSearch = new MenuItem { Header = "Search For...", };
		private readonly MenuItem mOutputContextMenuSave = new MenuItem { Header = "Save Output Log", };
		private readonly MenuItem mOutputContextMenuClear = new MenuItem { Header = "Clear Output Log", };
		private readonly MyTextBox mOutput = new MyTextBox
		{
			VerticalScrollBarVisibility = ScrollBarVisibility.Visible,
			TextWrapping = TextWrapping.Wrap,
			IsReadOnly = true,
		};

		private readonly Grid mOutputSearchLayout = new Grid { Background = UIModification.MakeBrush("#BF000000"), Visibility = Visibility.Collapsed, };
		private readonly Grid mOutputSearchTextLayout = new Grid();
		private readonly TextBox mOutputSearchResults = new MyTextBox { VerticalScrollBarVisibility = ScrollBarVisibility.Visible, IsReadOnly = true, };
		private readonly ComboBox mOutputSearchComboBox = new MyComboBox { IsEditable = true, };
		private readonly Button mOutputSearchButton = new MyButton { Content = "Search", };
		private readonly Button mOutputSearchCloseButton = new MyButton { Content = "Close", };
		#endregion

		#region Buttons
		private readonly Grid mButtonLayout = new Grid();
		private readonly Button mMainButton = new MyButton { Content = "Main", Tag = MenuType.Main, };
		private readonly Button mInfoButton = new MyButton { Content = "Info", Tag = MenuType.Info, };
		private readonly Button mSettingsButton = new MyButton { Content = "Settings", Tag = MenuType.Settings, };
		private readonly Button mColorsButton = new MyButton { Content = "Colors", Tag = MenuType.Colors, };
		private readonly Button mDMButton = new MyButton { Content = "DMs", Tag = MenuType.DMs, };
		private readonly Button mFileButton = new MyButton { Content = "Files", Tag = MenuType.Files, };
		private MenuType mLastButtonClicked;
		#endregion

		#region Main Menu
		private readonly Grid mMainMenuLayout = new Grid { Visibility = Visibility.Collapsed, };
		private readonly RichTextBox mMainMenuOutput = new MyRichTextBox
		{
			Document = UIModification.MakeMainMenu(),
			IsReadOnly = true,
			IsDocumentEnabled = true,
		};
		private readonly Button mDisconnectButton = new MyButton { Content = "Disconnect", };
		private readonly Button mRestartButton = new MyButton { Content = "Restart", };
		private readonly Button mPauseButton = new MyButton { Content = "Pause",};
		#endregion

		#region Settings Menu
		private readonly Grid mSettingsLayout = new Grid { Visibility = Visibility.Collapsed, };
		private readonly Button mSettingsSaveButton = new MyButton { Content = "Save Settings" };

		private readonly SettingInMenu mDownloadUsersSetting = new SettingInMenu
		{
			Setting = new Viewbox
			{
				Child = new CheckBox
				{
					IsChecked = ((bool)Variables.BotInfo.GetSetting(SettingOnBot.AlwaysDownloadUsers)),
					Tag = SettingOnBot.AlwaysDownloadUsers,
				},
				HorizontalAlignment = HorizontalAlignment.Left,
				VerticalAlignment = VerticalAlignment.Center,
				Tag = SettingOnBot.AlwaysDownloadUsers,
			},
			Title = UIModification.MakeTitle("Download Users:", "This automatically puts users in the bots cache. With it off, many commands will not work since I haven't added in a manual way to download users."),
		};
		private readonly SettingInMenu mPrefixSetting = new SettingInMenu
		{
			Setting = UIModification.MakeSetting(SettingOnBot.Prefix, 10),
			Title = UIModification.MakeTitle("Prefix:", "The prefix which is needed to be said before commands."),
		};
		private readonly SettingInMenu mBotOwnerSetting = new SettingInMenu
		{
			Setting = UIModification.MakeSetting(SettingOnBot.BotOwnerID, 18),
			Title = UIModification.MakeTitle("Bot Owner:", "The number here is the ID of a user. The bot owner can use some additional commands."),
		};
		private readonly SettingInMenu mGameSetting = new SettingInMenu
		{
			Setting = UIModification.MakeSetting(SettingOnBot.Game, 100),
			Title = UIModification.MakeTitle("Game:", "Changes what the bot says it's playing."),
		};
		private readonly SettingInMenu mStreamSetting = new SettingInMenu
		{
			Setting = UIModification.MakeSetting(SettingOnBot.Stream, 50),
			Title = UIModification.MakeTitle("Stream:", "Can set whatever stream you want as long as it's a valid Twitch.tv stream."),
		};
		private readonly SettingInMenu mShardSetting = new SettingInMenu
		{
			Setting = UIModification.MakeSetting(SettingOnBot.ShardCount, 3),
			Title = UIModification.MakeTitle("Shard Count:", "Each shard can hold up to 2500 guilds."),
		};
		private readonly SettingInMenu mMessageCacheSetting = new SettingInMenu
		{
			Setting = UIModification.MakeSetting(SettingOnBot.MessageCacheCount, 6),
			Title = UIModification.MakeTitle("Message Cache:", "The amount of messages the bot will hold in its cache."),
		};
		private readonly SettingInMenu mUserGatherCountSetting = new SettingInMenu
		{
			Setting = UIModification.MakeSetting(SettingOnBot.MaxUserGatherCount, 5),
			Title = UIModification.MakeTitle("Max User Gather:", "Limits the amount of users a command can modify at once."),
		};
		private readonly SettingInMenu mMessageGatherSizeSetting = new SettingInMenu
		{
			Setting = UIModification.MakeSetting(SettingOnBot.MaxMessageGatherSize, 7),
			Title = UIModification.MakeTitle("Max Msg Gather:", "This is in bytes, which to be very basic is roughly two bytes per character."),
		};
		private readonly SettingInMenu mLogLevelComboBox = new SettingInMenu
		{
			Setting = new MyComboBox { ItemsSource = UIModification.MakeComboBoxSourceOutOfEnum(typeof(Discord.LogSeverity)), Tag = SettingOnBot.LogLevel, },
			Title = UIModification.MakeTitle("Log Level:", "Certain events in the Discord library used in this bot have a required log level to be said in the console."),
		};
		private readonly SettingInMenu mTrustedUsersAdd = new SettingInMenu
		{
			Setting = new Grid() { Tag = SettingOnBot.TrustedUsers, },
			Title = UIModification.MakeTitle("Trusted Users:", "Some commands can only be run by the bot owner or user IDs that they have designated as trust worthy."),
		};
		private readonly TextBox mTrustedUsersAddBox = UIModification.MakeSetting(SettingOnBot.TrustedUsers, 18);
		private readonly Button mTrustedUsersAddButton = new MyButton { Content = "+", };
		private readonly SettingInMenu mTrustedUsersRemove = new SettingInMenu
		{
			Setting = new Grid() { Tag = SettingOnBot.TrustedUsers, },
			Title = UIModification.MakeTitle("", ""),
		};
		private readonly ComboBox mTrustedUsersComboBox = new MyComboBox { Tag = SettingOnBot.TrustedUsers, };
		private readonly Button mTrustedUsersRemoveButton = new MyButton { Content = "-", };
		#endregion

		#region Colors Menu
		private readonly Grid mColorsLayout = new Grid { Visibility = Visibility.Collapsed, };
		private readonly Button mColorsSaveButton = new MyButton { Content = "Save Colors", };
		#endregion

		#region Info Menu
		private readonly Grid mInfoLayout = new Grid { Visibility = Visibility.Collapsed, };
		private readonly RichTextBox mInfoOutput = new MyRichTextBox
		{
			Document = UIModification.MakeInfoMenu(),
			BorderThickness = new Thickness(0, 1, 0, 1),
			IsReadOnly = true,
			IsDocumentEnabled = true,
		};
		#endregion

		#region Guild Menu
		private readonly Grid mFileLayout = new Grid { Visibility = Visibility.Collapsed, };
		private readonly RichTextBox mFileOutput = new MyRichTextBox { IsReadOnly = true, IsDocumentEnabled = true, };
		private readonly TreeView mFileTreeView = new TreeView();
		private readonly Button mFileSearchButton = new MyButton { Content = "Search Guilds", };

		private readonly Grid mSpecificFileLayout = new Grid { Visibility = Visibility.Collapsed, };
		private readonly MenuItem mSpecificFileContextMenuSave = new MenuItem { Header = "Save File", };
		private readonly TextEditor mSpecificFileDisplay = new TextEditor
		{
			Background = null,
			Foreground = null,
			BorderBrush = null,
			VerticalScrollBarVisibility = ScrollBarVisibility.Visible,
			WordWrap = true,
			ShowLineNumbers = true,
		};
		private readonly Button mSpecificFileCloseButton = new MyButton { Content = "Close Menu", };

		private readonly Grid mGuildSearchLayout = new Grid { Background = UIModification.MakeBrush("#BF000000"), Visibility = Visibility.Collapsed };
		private readonly Grid mGuildSearchTextLayout = new Grid();
		private readonly Viewbox mGuildSearchNameHeader = UIModification.MakeStandardViewBox("Guild Name:");
		private readonly TextBox mGuildSearchNameInput = new MyTextBox { MaxLength = 100, };
		private readonly Viewbox mGuildSearchIDHeader = UIModification.MakeStandardViewBox("ID:");
		private readonly TextBox mGuildSearchIDInput = new MyNumberBox { MaxLength = 18, };
		private readonly ComboBox mGuildSearchFileComboBox = new MyComboBox { ItemsSource = UIModification.MakeComboBoxSourceOutOfEnum(typeof(FileType)), };
		private readonly Button mGuildSearchSearchButton = new MyButton { Content = "Search", };
		private readonly Button mGuildSearchCloseButton = new MyButton { Content = "Close", };
		#endregion

		#region DM Menu
		private readonly Grid mDMLayout = new Grid { Visibility = Visibility.Collapsed, };
		private readonly RichTextBox mDMOutput = new MyRichTextBox { IsReadOnly = true, IsDocumentEnabled = true, };
		private readonly TreeView mDMTreeView = new TreeView();
		private readonly Button mDMSearchButton = new MyButton { Content = "Search DMs", };

		private readonly Grid mSpecificDMLayout = new Grid { Visibility = Visibility.Collapsed, };
		private readonly TextEditor mSpecificDMDisplay = new TextEditor
		{
			Background = null,
			Foreground = null,
			BorderBrush = null,
			VerticalScrollBarVisibility = ScrollBarVisibility.Visible,
			WordWrap = true,
			ShowLineNumbers = true,
			IsReadOnly = true,
		};
		private readonly Button mSpecificDMCloseButton = new MyButton { Content = "Close Menu", };

		private readonly Grid mDMSearchLayout = new Grid { Background = UIModification.MakeBrush("#BF000000"), Visibility = Visibility.Collapsed };
		private readonly Grid mDMSearchTextLayout = new Grid();
		private readonly Viewbox mDMSearchNameHeader = UIModification.MakeStandardViewBox("Username:");
		private readonly TextBox mDMSearchNameInput = new MyTextBox { MaxLength = 32, };
		private readonly Viewbox mDMSearchDiscHeader = UIModification.MakeStandardViewBox("Disc:");
		private readonly TextBox mDMSearchDiscInput = new MyNumberBox { MaxLength = 4, };
		private readonly Viewbox mDMSearchIDHeader = UIModification.MakeStandardViewBox("ID:");
		private readonly TextBox mDMSearchIDInput = new MyNumberBox { MaxLength = 18, };
		private readonly Button mDMSearchSearchButton = new MyButton { Content = "Search", };
		private readonly Button mDMSearchCloseButton = new MyButton { Content = "Close", };
		#endregion

		#region System Info
		private readonly Grid mSysInfoLayout = new Grid();
		private readonly TextBox mSysInfoUnder = new MyTextBox { IsReadOnly = true, };
		private readonly Viewbox mLatency = new Viewbox { Child = UIModification.MakeSysInfoBox(), };
		private readonly Viewbox mMemory = new Viewbox { Child = UIModification.MakeSysInfoBox(), };
		private readonly Viewbox mThreads = new Viewbox { Child = UIModification.MakeSysInfoBox(), };
		private readonly Viewbox mGuilds = new Viewbox { Child = UIModification.MakeSysInfoBox(), };
		private readonly Viewbox mUsers = new Viewbox { Child = UIModification.MakeSysInfoBox(), };
		private readonly ToolTip mMemHoverInfo = new ToolTip { Content = "This is not guaranteed to be 100% correct.", };
		#endregion

		public BotWindow()
		{
			FontFamily = new FontFamily("Courier New");
			InitializeComponents();
			Loaded += RunApplication;
		}
		private void InitializeComponents()  
		{
			//Main layout
			UIModification.AddRows(mLayout, 100);
			UIModification.AddCols(mLayout, 4);

			//Output
			UIModification.AddElement(mLayout, mOutput, 0, 87, 0, 4);

			//System Info
			UIModification.AddElement(mLayout, mSysInfoLayout, 87, 3, 0, 3, 0, 5);
			UIModification.AddElement(mSysInfoLayout, mSysInfoUnder, 0, 1, 0, 5);
			UIModification.AddElement(mSysInfoLayout, mLatency, 0, 1, 0, 1);
			UIModification.AddElement(mSysInfoLayout, mMemory, 0, 1, 1, 1);
			UIModification.AddElement(mSysInfoLayout, mThreads, 0, 1, 2, 1);
			UIModification.AddElement(mSysInfoLayout, mGuilds, 0, 1, 3, 1);
			UIModification.AddElement(mSysInfoLayout, mUsers, 0, 1, 4, 1);

			//Input
			UIModification.AddElement(mLayout, mInputLayout, 90, 10, 0, 3, 1, 10);
			UIModification.AddElement(mInputLayout, mInput, 0, 1, 0, 9);
			UIModification.AddElement(mInputLayout, mInputButton, 0, 1, 9, 1);

			//Buttons
			UIModification.AddElement(mLayout, mButtonLayout, 87, 13, 3, 1, 2, 5);
			UIModification.AddElement(mButtonLayout, mMainButton, 0, 2, 0, 1);
			UIModification.AddElement(mButtonLayout, mInfoButton, 0, 2, 1, 1);
			UIModification.AddElement(mButtonLayout, mSettingsButton, 0, 1, 2, 1);
			UIModification.AddElement(mButtonLayout, mColorsButton, 1, 1, 2, 1);
			UIModification.AddElement(mButtonLayout, mDMButton, 0, 2, 3, 1);
			UIModification.AddElement(mButtonLayout, mFileButton, 0, 2, 4, 1);

			//Main menu
			UIModification.AddElement(mLayout, mMainMenuLayout, 0, 87, 3, 1, 100, 3);
			UIModification.AddElement(mMainMenuLayout, mMainMenuOutput, 0, 95, 0, 3);
			UIModification.AddElement(mMainMenuLayout, mPauseButton, 95, 5, 0, 1);
			UIModification.AddElement(mMainMenuLayout, mRestartButton, 95, 5, 1, 1);
			UIModification.AddElement(mMainMenuLayout, mDisconnectButton, 95, 5, 2, 1);

			//Settings menu
			UIModification.AddElement(mLayout, mSettingsLayout, 0, 87, 3, 1, 100, 100);
			UIModification.AddPlaceHolderTB(mSettingsLayout, 0, 100, 0, 100);
			UIModification.AddCols((Grid)mTrustedUsersAdd.Setting, 10);
			UIModification.AddElement((Grid)mTrustedUsersAdd.Setting, mTrustedUsersAddBox, 0, 1, 0, 9);
			UIModification.AddElement((Grid)mTrustedUsersAdd.Setting, mTrustedUsersAddButton, 0, 1, 9, 1);
			UIModification.AddCols((Grid)mTrustedUsersRemove.Setting, 10);
			UIModification.AddElement((Grid)mTrustedUsersRemove.Setting, mTrustedUsersComboBox, 0, 1, 0, 9);
			UIModification.AddElement((Grid)mTrustedUsersRemove.Setting, mTrustedUsersRemoveButton, 0, 1, 9, 1);
			UIModification.AddElement(mSettingsLayout, mSettingsSaveButton, 95, 5, 0, 100);
			var mSettings = new[]
			{
				mDownloadUsersSetting,
				mPrefixSetting,
				mBotOwnerSetting,
				mGameSetting,
				mStreamSetting,
				mShardSetting,
				mMessageCacheSetting,
				mUserGatherCountSetting,
				mMessageGatherSizeSetting,
				mLogLevelComboBox,
				mTrustedUsersAdd, 
				mTrustedUsersRemove,
			};
			for (int i = 0; i < mSettings.Length; i++)
			{
				const int TITLE_START_COLUMN = 5;
				const int TITLE_COLUMN_LENGTH = 35;
				const int SETTING_START_COLUMN = 40;
				const int SETTING_COLUMN_LENGTH = 55;
				const int LENGTH_FOR_SETTINGS = 4;

				UIModification.AddElement(mSettingsLayout, mSettings[i].Title, (i * LENGTH_FOR_SETTINGS), LENGTH_FOR_SETTINGS, TITLE_START_COLUMN, TITLE_COLUMN_LENGTH);
				UIModification.AddElement(mSettingsLayout, mSettings[i].Setting, (i * LENGTH_FOR_SETTINGS), LENGTH_FOR_SETTINGS, SETTING_START_COLUMN, SETTING_COLUMN_LENGTH);
			}

			//Colors menu
			UIModification.AddElement(mLayout, mColorsLayout, 0, 87, 3, 1, 100, 100);

			//Info menu
			UIModification.AddElement(mLayout, mInfoLayout, 0, 87, 3, 1, 1, 10);
			UIModification.AddPlaceHolderTB(mInfoLayout, 0, 1, 0, 10);
			UIModification.AddElement(mInfoLayout, mInfoOutput, 0, 1, 1, 8);

			//File menu
			UIModification.AddElement(mLayout, mFileLayout, 0, 87, 3, 1, 100, 1);
			UIModification.AddElement(mFileLayout, mFileOutput, 0, 95, 0, 1);
			UIModification.AddElement(mFileLayout, mFileSearchButton, 95, 5, 0, 1);
			UIModification.AddElement(mLayout, mSpecificFileLayout, 0, 100, 0, 4, 100, 4);
			UIModification.AddElement(mSpecificFileLayout, mSpecificFileDisplay, 0, 100, 0, 3);
			UIModification.AddElement(mSpecificFileLayout, mSpecificFileCloseButton, 95, 5, 3, 1);

			//DM menu
			UIModification.AddElement(mLayout, mDMLayout, 0, 87, 3, 1, 100, 1);
			UIModification.AddElement(mDMLayout, mDMOutput, 0, 95, 0, 1);
			UIModification.AddElement(mDMLayout, mDMSearchButton, 95, 5, 0, 1);
			UIModification.AddElement(mLayout, mSpecificDMLayout, 0, 100, 0, 4, 100, 4);
			UIModification.AddElement(mSpecificDMLayout, mSpecificDMDisplay, 0, 100, 0, 3);
			UIModification.AddElement(mSpecificDMLayout, mSpecificDMCloseButton, 95, 5, 3, 1);

			//Guild search
			UIModification.AddElement(mLayout, mGuildSearchLayout, 0, 100, 0, 4, 10, 10);
			UIModification.AddElement(mGuildSearchLayout, mGuildSearchTextLayout, 3, 4, 3, 4, 100, 100);
			UIModification.PutInBGWithMouseUpEvent(mGuildSearchLayout, mGuildSearchTextLayout, null, CloseFileSearch);
			UIModification.AddPlaceHolderTB(mGuildSearchTextLayout, 0, 100, 0, 100);
			UIModification.AddElement(mGuildSearchTextLayout, mGuildSearchNameHeader, 10, 10, 15, 70);
			UIModification.AddElement(mGuildSearchTextLayout, mGuildSearchNameInput, 20, 21, 15, 70);
			UIModification.AddElement(mGuildSearchTextLayout, mGuildSearchIDHeader, 41, 10, 15, 70);
			UIModification.AddElement(mGuildSearchTextLayout, mGuildSearchIDInput, 51, 10, 15, 70);
			UIModification.AddElement(mGuildSearchTextLayout, mGuildSearchFileComboBox, 63, 10, 20, 60);
			UIModification.AddElement(mGuildSearchTextLayout, mGuildSearchSearchButton, 75, 15, 20, 25);
			UIModification.AddElement(mGuildSearchTextLayout, mGuildSearchCloseButton, 75, 15, 55, 25);

			//Output search
			UIModification.AddElement(mLayout, mOutputSearchLayout, 0, 100, 0, 4, 10, 10);
			UIModification.AddElement(mOutputSearchLayout, mOutputSearchTextLayout, 1, 8, 1, 8, 100, 100);
			UIModification.PutInBGWithMouseUpEvent(mOutputSearchLayout, mOutputSearchTextLayout, null, CloseOutputSearch);
			UIModification.AddPlaceHolderTB(mOutputSearchTextLayout, 90, 10, 0, 100);
			UIModification.AddElement(mOutputSearchTextLayout, mOutputSearchResults, 0, 90, 0, 100);
			UIModification.AddElement(mOutputSearchTextLayout, mOutputSearchComboBox, 92, 6, 2, 30);
			UIModification.AddElement(mOutputSearchTextLayout, mOutputSearchButton, 92, 6, 66, 15);
			UIModification.AddElement(mOutputSearchTextLayout, mOutputSearchCloseButton, 92, 6, 83, 15);

			//DM search
			UIModification.AddElement(mLayout, mDMSearchLayout, 0, 100, 0, 4, 10, 10);
			UIModification.AddElement(mDMSearchLayout, mDMSearchTextLayout, 3, 4, 3, 4, 72, 100);
			UIModification.PutInBGWithMouseUpEvent(mDMSearchLayout, mDMSearchTextLayout, null, CloseDMSearch);
			UIModification.AddPlaceHolderTB(mDMSearchTextLayout, 0, 100, 0, 100);
			UIModification.AddElement(mDMSearchTextLayout, mDMSearchNameHeader, 10, 10, 15, 50);
			UIModification.AddElement(mDMSearchTextLayout, mDMSearchNameInput, 20, 10, 15, 50);
			UIModification.AddElement(mDMSearchTextLayout, mDMSearchDiscHeader, 10, 10, 65, 20);
			UIModification.AddElement(mDMSearchTextLayout, mDMSearchDiscInput, 20, 10, 65, 20);
			UIModification.AddElement(mDMSearchTextLayout, mDMSearchIDHeader, 30, 10, 15, 70);
			UIModification.AddElement(mDMSearchTextLayout, mDMSearchIDInput, 40, 10, 15, 70);
			UIModification.AddElement(mDMSearchTextLayout, mDMSearchSearchButton, 52, 10, 20, 25);
			UIModification.AddElement(mDMSearchTextLayout, mDMSearchCloseButton, 52, 10, 55, 25);

			//Font size properties
			UIModification.SetFontSizeProperties(.275, new UIElement[] { mInput, });
			UIModification.SetFontSizeProperties(.060, new UIElement[] { mGuildSearchNameInput, mGuildSearchIDInput, mDMSearchNameInput, mDMSearchDiscInput, mDMSearchIDInput });
			UIModification.SetFontSizeProperties(.035, new UIElement[] { mInfoOutput, });
			UIModification.SetFontSizeProperties(.022, new UIElement[] { mSpecificFileDisplay, mFileOutput, mOutputSearchComboBox, mDMOutput });
			UIModification.SetFontSizeProperties(.018, new UIElement[] { mMainMenuOutput, }, mSettings.Select(x => x.Title), mSettings.Select(x => x.Setting));

			//Context menus
			mOutput.ContextMenu = new ContextMenu
			{
				ItemsSource = new[] { mOutputContextMenuSearch, mOutputContextMenuSave, mOutputContextMenuClear },
			};
			mSpecificFileDisplay.ContextMenu = new ContextMenu
			{
				ItemsSource = new[] { mSpecificFileContextMenuSave },
			};

			MakeInputEvents();
			MakeOutputEvents();
			MakeMenuEvents();
			MakeDMEvents();
			MakeGuildFileEvents();
			MakeOtherEvents();

			//Set this panel as the content for this window and run the application
			this.Content = mLayout;
			this.WindowState = WindowState.Maximized;
		}

		private void RunApplication(object sender, RoutedEventArgs e)
		{
			//Make console output show on the output text block and box
			Console.SetOut(new UITextBoxStreamWriter(mOutput));

			//Validate path/botkey after the UI has launched to have them logged
			Task.Run(async () =>
			{
				Actions.ValidatePath(Properties.Settings.Default.Path, true);
				await Actions.ValidateBotKey(Variables.Client, Properties.Settings.Default.BotKey, true);
				await Actions.MaybeStartBot();
			});

			mUIInfo.InitializeColors();
			mUIInfo.ActivateTheme();
			UIModification.SetColorMode(mLayout);
			UpdateSystemInformation();
		}

		private void MakeOtherEvents()
		{
			mPauseButton.Click += Pause;
			mRestartButton.Click += Restart;
			mDisconnectButton.Click += Disconnect;

			mMemory.MouseEnter += ModifyMemHoverInfo;
			mMemory.MouseLeave += ModifyMemHoverInfo;

			mSettingsSaveButton.Click += SaveSettings;
			mColorsSaveButton.Click += SaveColors;
			mTrustedUsersRemoveButton.Click += RemoveTrustedUser;
			mTrustedUsersAddButton.Click += AddTrustedUser;
		}
		private void Pause(object sender, RoutedEventArgs e)
		{
			if (Variables.Pause)
			{
				Actions.WriteLine("The bot is now unpaused.");
				Variables.Pause = false;
			}
			else
			{
				Actions.WriteLine("The bot is now paused.");
				Variables.Pause = true;
			}
		}
		private void Restart(object sender, RoutedEventArgs e)
		{
			switch (MessageBox.Show("Are you sure you want to restart the bot?", Variables.BotName, MessageBoxButton.OKCancel))
			{
				case MessageBoxResult.OK:
				{
					Actions.RestartBot();
					return;
				}
			}
		}
		private void Disconnect(object sender, RoutedEventArgs e)
		{
			switch (MessageBox.Show("Are you sure you want to disconnect the bot?", Variables.BotName, MessageBoxButton.OKCancel))
			{
				case MessageBoxResult.OK:
				{
					Actions.DisconnectBot();
					return;
				}
			}
		}
		private void ModifyMemHoverInfo(object sender, RoutedEventArgs e)
		{
			UIModification.ToggleToolTip(mMemHoverInfo);
		}
		private void SaveSettings(object sender, RoutedEventArgs e)
		{
			var botInfo = Variables.BotInfo;
			var success = new List<string>();
			var failure = new List<string>();

			//Go through each setting and update them
			for (int i = 0; i < VisualTreeHelper.GetChildrenCount(mSettingsLayout); i++)
			{
				var ele = VisualTreeHelper.GetChild(mSettingsLayout, i);
				var setting = (ele as Control)?.Tag;
				if (setting is SettingOnBot)
				{
					var response = SaveSetting((dynamic)ele, (SettingOnBot)setting, botInfo);
					switch (response.Status)
					{
						case NSF.Success:
						{
							success.Add(response.Setting);
							break;
						}
						case NSF.Failure:
						{
							failure.Add(response.Setting);
							break;
						}
					}
				}
			}

			//Notify what was saved
			if (success.Any())
			{
				Actions.WriteLine(String.Format("Successfully saved: {0}", String.Join(", ", success)));
				Actions.DontWaitForResultOfBigUnimportantFunction(null, async () =>
				{
					await Actions.UpdateGame();
				});
			}
			if (failure.Any())
			{
				Actions.WriteLine(String.Format("Failed to save: {0}", String.Join(", ", failure)));
			}
		}
		private void SaveColors(object sender, RoutedEventArgs e)
		{
			for (int i = 0; i < VisualTreeHelper.GetChildrenCount(mColorsLayout); i++)
			{
				var child = VisualTreeHelper.GetChild(mColorsLayout, i);
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
						Actions.WriteLine(String.Format("Invalid color supplied for {0}.", target.EnumName()));
						continue;
					}

					if (!UIModification.CheckIfTwoBrushesAreTheSame(mUIInfo.ColorTargets[target], brush))
					{
						mUIInfo.ColorTargets[target] = brush;
						castedChild.Text = UIModification.FormatBrush(brush);
						Actions.WriteLine(String.Format("Successfully updated the color for {0}.", target.EnumName()));
					}
				}
				else if (child is ComboBox)
				{
					var selected = ((ComboBox)child).SelectedItem as MyTextBox;
					var tag = selected?.Tag as ColorTheme?;
					if (!tag.HasValue || tag == mUIInfo.Theme)
						continue;

					mUIInfo.SetTheme((ColorTheme)tag);
					Actions.WriteLine("Successfully updated the theme type.");
				}
			}

			mUIInfo.SaveBotUIInfo();
			mUIInfo.ActivateTheme();
			UIModification.SetColorMode(mLayout);
		}
		private void AddTrustedUser(object sender, RoutedEventArgs e)
		{
			var text = mTrustedUsersAddBox.Text;
			mTrustedUsersAddBox.Text = "";

			if (String.IsNullOrWhiteSpace(text))
			{
				return;
			}
			else if (ulong.TryParse(text, out ulong userID))
			{
				var currTBs = mTrustedUsersComboBox.Items.Cast<TextBox>().ToList();
				if (currTBs.Select(x => (ulong)x.Tag).Contains(userID))
					return;

				var tb = UIModification.MakeTextBoxFromUserID(userID);
				if (tb == null)
				{
					return;
				}

				currTBs.Add(tb);
				mTrustedUsersComboBox.ItemsSource = currTBs;
			}
			else
			{
				Actions.WriteLine(String.Format("The given input '{0}' is not a valid ID.", text));
			}
		}
		private void RemoveTrustedUser(object sender, RoutedEventArgs e)
		{
			if (mTrustedUsersComboBox.SelectedItem == null)
				return;

			var userID = (ulong)((TextBox)mTrustedUsersComboBox.SelectedItem).Tag;
			var currTBs = mTrustedUsersComboBox.Items.Cast<TextBox>().ToList();
			if (!currTBs.Select(x => (ulong)x.Tag).Contains(userID))
				return;

			currTBs.RemoveAll(x => (ulong)x.Tag == userID);
			mTrustedUsersComboBox.ItemsSource = currTBs;
		}

		private void MakeInputEvents()
		{
			mInput.KeyUp += AcceptInput;
			mInputButton.Click += AcceptInput;
		}
		private void AcceptInput(object sender, KeyEventArgs e)
		{
			var text = mInput.Text;
			if (String.IsNullOrWhiteSpace(text))
			{
				mInputButton.IsEnabled = false;
				return;
			}
			else
			{
				if (e.Key.Equals(Key.Enter) || e.Key.Equals(Key.Return))
				{
					UICommandHandler.GatherInput(mInput, mInputButton);
				}
				else
				{
					mInputButton.IsEnabled = true;
				}
			}
		}
		private void AcceptInput(object sender, RoutedEventArgs e)
		{
			UICommandHandler.GatherInput(mInput, mInputButton);
		}

		private void MakeOutputEvents()
		{
			mOutputContextMenuSave.Click += SaveOutput;
			mOutputContextMenuClear.Click += ClearOutput;
			mOutputContextMenuSearch.Click += OpenOutputSearch;
			mOutputSearchCloseButton.Click += CloseOutputSearch;
			mOutputSearchButton.Click += SearchOutput;
		}
		private void SaveOutput(object sender, RoutedEventArgs e)
		{
			//Make sure the path is valid
			var path = Actions.GetBaseBotDirectory("Output_Log_" + DateTime.UtcNow.ToString("MM-dd_HH-mm-ss") + Constants.GENERAL_FILE_EXTENSION);
			if (path == null)
			{
				Actions.WriteLine("Unable to save the output log.");
				return;
			}

			//Save the file
			using (StreamWriter writer = new StreamWriter(path))
			{
				writer.Write(mOutput.Text);
			}

			//Write to the console telling the user that the console log was successfully saved
			Actions.WriteLine("Successfully saved the output log.");
		}
		private void ClearOutput(object sender, RoutedEventArgs e)
		{
			switch (MessageBox.Show("Are you sure you want to clear the output window?", Variables.BotName, MessageBoxButton.OKCancel))
			{
				case MessageBoxResult.OK:
				{
					mOutput.Text = "";
					return;
				}
			}
		}
		private void OpenOutputSearch(object sender, RoutedEventArgs e)
		{
			mOutputSearchComboBox.ItemsSource = UIModification.MakeComboBoxSourceOutOfStrings(Variables.WrittenLines.Keys);
			mOutputSearchLayout.Visibility = Visibility.Visible;
		}
		private void CloseOutputSearch(object sender, RoutedEventArgs e)
		{
			mOutputSearchComboBox.SelectedItem = null;
			mOutputSearchResults.Text = null;
			mOutputSearchLayout.Visibility = Visibility.Collapsed;
		}
		private void SearchOutput(object sender, RoutedEventArgs e)
		{
			var selectedItem = (TextBox)mOutputSearchComboBox.SelectedItem;
			if (selectedItem != null)
			{
				mOutputSearchResults.Text = null;
				Variables.WrittenLines[selectedItem.Text].ForEach(x => mOutputSearchResults.AppendText(x + Environment.NewLine));
			}
		}

		private void MakeGuildFileEvents()
		{
			mFileSearchButton.Click += OpenFileSearch;
			mGuildSearchSearchButton.Click += SearchForFile;
			mGuildSearchCloseButton.Click += CloseFileSearch;
			mSpecificFileCloseButton.Click += CloseSpecificFileLayout;
			mSpecificFileContextMenuSave.Click += SaveFile;
		}
		private void OpenFileSearch(object sender, RoutedEventArgs e)
		{
			mGuildSearchLayout.Visibility = Visibility.Visible;
		}
		private void CloseFileSearch(object sender, RoutedEventArgs e)
		{
			mGuildSearchFileComboBox.SelectedItem = null;
			mGuildSearchNameInput.Text = "";
			mGuildSearchIDInput.Text = "";
			mGuildSearchLayout.Visibility = Visibility.Collapsed;
		}
		private void SearchForFile(object sender, RoutedEventArgs e)
		{
			var tb = (TextBox)mGuildSearchFileComboBox.SelectedItem;
			if (tb == null)
				return;

			var nameStr = mGuildSearchNameInput.Text;
			var idStr = mGuildSearchIDInput.Text;
			if (String.IsNullOrWhiteSpace(nameStr) && String.IsNullOrWhiteSpace(idStr))
				return;

			var fileType = (FileType)tb.Tag;
			CloseFileSearch(sender, e);

			TreeViewItem guild = null;
			if (!String.IsNullOrWhiteSpace(idStr))
			{
				if (!ulong.TryParse(idStr, out ulong guildID))
				{
					Actions.WriteLine(String.Format("The ID '{0}' is not a valid number.", idStr));
					return;
				}
				else
				{
					guild = mFileTreeView.Items.Cast<TreeViewItem>().FirstOrDefault(x =>
					{
						return ((GuildFileInformation)x.Tag).ID == guildID;
					});

					if (guild == null)
					{
						Actions.WriteLine(String.Format("No guild could be found with the ID '{0}'.", guildID));
						return;
					}
				}
			}
			else if (!String.IsNullOrWhiteSpace(nameStr))
			{
				var guilds = mFileTreeView.Items.Cast<TreeViewItem>().Where(x =>
				{
					return ((GuildFileInformation)x.Tag).Name.CaseInsEquals(nameStr);
				});

				if (guilds.Count() == 0)
				{
					Actions.WriteLine(String.Format("No guild could be found with the name '{0}'.", nameStr));
					return;
				}
				else if (guilds.Count() == 1)
				{
					guild = guilds.FirstOrDefault();
				}
				else
				{
					Actions.WriteLine("More than one guild has the name '{0}'.", nameStr);
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
				UIModification.SetRowAndSpan(mFileLayout, 0, 100);
				mSpecificFileLayout.Visibility = Visibility.Visible;
				mFileSearchButton.Visibility = Visibility.Collapsed;
			}
			else
			{
				Actions.WriteLine("Unable to bring up the file.");
			}
		}
		private void CloseSpecificFileLayout(object sender, RoutedEventArgs e)
		{
			var result = MessageBox.Show("Are you sure you want to close the edit window?", Variables.BotName, MessageBoxButton.OKCancel);

			switch (result)
			{
				case MessageBoxResult.OK:
				{
					UIModification.SetRowAndSpan(mFileLayout, 0, 87);
					mSpecificFileDisplay.Tag = null;
					mSpecificFileLayout.Visibility = Visibility.Collapsed;
					mFileSearchButton.Visibility = Visibility.Visible;
					break;
				}
			}
		}
		private void SaveFile(object sender, RoutedEventArgs e)
		{
			var fileLocation = mSpecificFileDisplay.Tag.ToString();
			if (String.IsNullOrWhiteSpace(fileLocation) || !File.Exists(fileLocation))
			{
				UIModification.MakeFollowingToolTip(mLayout, mToolTip, "Unable to gather the path for this file.").Forget();
				return;
			}

			var fileAndExtension = fileLocation.Substring(fileLocation.LastIndexOf('\\') + 1);
			if (fileAndExtension.Equals(Constants.GUILD_INFO_LOCATION))
			{
				//Make sure the guild info stays valid
				try
				{
					var throwaway = JsonConvert.DeserializeObject<BotGuildInfo>(mSpecificFileDisplay.Text);
				}
				catch (Exception exc)
				{
					Actions.ExceptionToConsole(exc);
					UIModification.MakeFollowingToolTip(mLayout, mToolTip, "Failed to save the file.").Forget();
					return;
				}
			}

			//Save the file and give a notification
			using (var writer = new StreamWriter(fileLocation))
			{
				writer.WriteLine(mSpecificFileDisplay.Text);
			}
			UIModification.MakeFollowingToolTip(mLayout, mToolTip, "Successfully saved the file.").Forget();
		}

		private void MakeDMEvents()
		{
			mDMSearchButton.Click += OpenDMSearch;
			mDMSearchSearchButton.Click += SearchForDM;
			mDMSearchCloseButton.Click += CloseDMSearch;
			mSpecificDMCloseButton.Click += CloseSpecificDMLayout;
		}
		private void OpenDMSearch(object sender, RoutedEventArgs e)
		{
			mDMSearchLayout.Visibility = Visibility.Visible;
		}
		private void CloseDMSearch(object sender, RoutedEventArgs e)
		{
			mDMSearchNameInput.Text = "";
			mDMSearchDiscInput.Text = "";
			mDMSearchIDInput.Text = "";
			mDMSearchLayout.Visibility = Visibility.Collapsed;
		}
		private void SearchForDM(object sender, RoutedEventArgs e)
		{
			var nameStr = mDMSearchNameInput.Text;
			var discStr = mDMSearchDiscInput.Text;
			var idStr = mDMSearchIDInput.Text;

			if (String.IsNullOrWhiteSpace(nameStr) && String.IsNullOrWhiteSpace(idStr))
				return;
			CloseDMSearch(sender, e);

			TreeViewItem DMChannel = null;
			if (!String.IsNullOrWhiteSpace(idStr))
			{
				if (!ulong.TryParse(idStr, out ulong userID))
				{
					Actions.WriteLine(String.Format("The ID '{0}' is not a valid number.", idStr));
					return;
				}
				else
				{
					DMChannel = mDMTreeView.Items.Cast<TreeViewItem>().FirstOrDefault(x =>
					{
						return ((Discord.IDMChannel)x.Tag)?.Recipient?.Id == userID;
					});

					if (DMChannel == null)
					{
						Actions.WriteLine(String.Format("No user could be found with the ID '{0}'.", userID));
						return;
					}
				}
			}
			else if (!String.IsNullOrWhiteSpace(nameStr))
			{
				var DMChannels = mDMTreeView.Items.Cast<TreeViewItem>().Where(x =>
				{
					var username = ((Discord.IDMChannel)x.Tag)?.Recipient?.Username;
					return username.CaseInsEquals(nameStr);
				});

				if (!String.IsNullOrWhiteSpace(discStr))
				{
					if (!ushort.TryParse(discStr, out ushort disc))
					{
						Actions.WriteLine(String.Format("The discriminator '{0}' is not a valid number.", discStr));
						return;
					}
					else
					{
						DMChannels = DMChannels.Where(x =>
						{
							//Why are discriminators strings instead of ints????
							return (bool)((Discord.IDMChannel)x.Tag)?.Recipient?.Discriminator.Equals(discStr);
						});
					}
				}

				if (DMChannels.Count() == 0)
				{
					Actions.WriteLine(String.Format("No user could be found with the name '{0}'.", nameStr));
					return;
				}
				else if (DMChannels.Count() == 1)
				{
					DMChannel = DMChannels.FirstOrDefault();
				}
				else
				{
					Actions.WriteLine("More than one user has the name '{0}'.", nameStr);
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
				UIModification.SetRowAndSpan(mDMLayout, 0, 100);
				mSpecificDMLayout.Visibility = Visibility.Visible;
				mDMSearchButton.Visibility = Visibility.Collapsed;
			}
			else
			{
				Actions.WriteLine("Unable to bring up the DMs.");
			}
		}
		private void CloseSpecificDMLayout(object sender, RoutedEventArgs e)
		{
			UIModification.SetRowAndSpan(mDMLayout, 0, 87);
			mSpecificDMDisplay.Tag = null;
			mSpecificDMLayout.Visibility = Visibility.Collapsed;
			mDMSearchButton.Visibility = Visibility.Visible;
		}

		private void MakeMenuEvents()
		{
			mMainButton.Click += OpenMenu;
			mSettingsButton.Click += OpenMenu;
			mColorsButton.Click += OpenMenu;
			mInfoButton.Click += OpenMenu;
			mFileButton.Click += OpenMenu;
			mDMButton.Click += OpenMenu;
		}
		private async void OpenMenu(object sender, RoutedEventArgs e)
		{
			if (!Variables.Loaded)
				return;

			//Hide everything so stuff doesn't overlap
			mMainMenuLayout.Visibility = Visibility.Collapsed;
			mSettingsLayout.Visibility = Visibility.Collapsed;
			mColorsLayout.Visibility = Visibility.Collapsed;
			mInfoLayout.Visibility = Visibility.Collapsed;
			mFileLayout.Visibility = Visibility.Collapsed;
			mDMLayout.Visibility = Visibility.Collapsed;

			//If clicking the same button then resize the output window to the regular size
			var type = (sender as Button)?.Tag as MenuType? ?? MenuType.Nothing;
			if (type == mLastButtonClicked)
			{
				UIModification.SetColAndSpan(mOutput, 0, 4);
				mLastButtonClicked = MenuType.Nothing;
			}
			else
			{
				//Resize the regular output window and have the menubox appear
				UIModification.SetColAndSpan(mOutput, 0, 3);
				mLastButtonClicked = type;

				switch (type)
				{
					case MenuType.Main:
					{
						mMainMenuLayout.Visibility = Visibility.Visible;
						return;
					}
					case MenuType.Info:
					{
						mInfoLayout.Visibility = Visibility.Visible;
						return;
					}
					case MenuType.Settings:
					{
						UpdateSettingsWhenOpened();
						mSettingsLayout.Visibility = Visibility.Visible;
						return;
					}
					case MenuType.Colors:
					{
						UIModification.MakeColorDisplayer(mUIInfo, mColorsLayout, mColorsSaveButton, .018);
						mColorsLayout.Visibility = Visibility.Visible;
						return;
					}
					case MenuType.DMs:
					{
						var treeView = await UIModification.MakeDMTreeView(mDMTreeView);
						treeView.Items.Cast<TreeViewItem>().ToList().ForEach(x =>
						{
							x.MouseDoubleClick += OpenSpecificDMLayout;
						});
						mDMOutput.Document = new FlowDocument(new Paragraph(new InlineUIContainer(treeView)));
						mDMLayout.Visibility = Visibility.Visible;
						return;
					}
					case MenuType.Files:
					{
						var treeView = UIModification.MakeGuildTreeView(mFileTreeView);
						treeView.Items.Cast<TreeViewItem>().SelectMany(x => x.Items.Cast<TreeViewItem>()).ToList().ForEach(x =>
						{
							x.MouseDoubleClick += OpenSpecificFileLayout;
						});
						mFileOutput.Document = new FlowDocument(new Paragraph(new InlineUIContainer(treeView)));
						mFileLayout.Visibility = Visibility.Visible;
						return;
					}
				}
			}
		}

		private void UpdateSystemInformation()
		{
			var timer = new DispatcherTimer() { Interval = new TimeSpan(0, 0, 0, 0, 500) };
			timer.Tick += (sender, e) =>
			{
				var client = Variables.Client;
				((TextBox)mLatency.Child).Text = String.Format("Latency: {0}ms", client.GetLatency());
				((TextBox)mMemory.Child).Text = String.Format("Memory: {0}MB", Actions.GetMemory().ToString("0.00"));
				((TextBox)mThreads.Child).Text = String.Format("Threads: {0}", Process.GetCurrentProcess().Threads.Count);
				((TextBox)mGuilds.Child).Text = String.Format("Guilds: {0}", client.GetGuilds().Count);
				((TextBox)mUsers.Child).Text = String.Format("Members: {0}", client.GetGuilds().SelectMany(x => x.Users).Select(x => x.Id).Distinct().Count());
				mInfoOutput.Document = UIModification.MakeInfoMenu();
			};
			timer.Start();
		}
		private void UpdateSettingsWhenOpened()
		{
			var botInfo = Variables.BotInfo;
			((CheckBox)((Viewbox)mDownloadUsersSetting.Setting).Child).IsChecked = ((bool)botInfo.GetSetting(SettingOnBot.AlwaysDownloadUsers));
			((TextBox)mPrefixSetting.Setting).Text = ((string)botInfo.GetSetting(SettingOnBot.Prefix));
			((TextBox)mBotOwnerSetting.Setting).Text = ((ulong)botInfo.GetSetting(SettingOnBot.BotOwnerID)).ToString();
			((TextBox)mGameSetting.Setting).Text = ((string)botInfo.GetSetting(SettingOnBot.Game));
			((TextBox)mStreamSetting.Setting).Text = ((string)botInfo.GetSetting(SettingOnBot.Stream));
			((TextBox)mShardSetting.Setting).Text = ((int)botInfo.GetSetting(SettingOnBot.ShardCount)).ToString();
			((TextBox)mMessageCacheSetting.Setting).Text = ((int)botInfo.GetSetting(SettingOnBot.MessageCacheCount)).ToString();
			((TextBox)mUserGatherCountSetting.Setting).Text = ((int)botInfo.GetSetting(SettingOnBot.MaxUserGatherCount)).ToString();
			((TextBox)mMessageGatherSizeSetting.Setting).Text = ((int)botInfo.GetSetting(SettingOnBot.MaxMessageGatherSize)).ToString();
			((ComboBox)mLogLevelComboBox.Setting).SelectedItem = ((ComboBox)mLogLevelComboBox.Setting).Items.OfType<TextBox>().FirstOrDefault(x =>
			{
				return (Discord.LogSeverity)x.Tag == (Discord.LogSeverity)Variables.BotInfo.GetSetting(SettingOnBot.LogLevel);
			});
			mTrustedUsersComboBox.ItemsSource = ((List<ulong>)Variables.BotInfo.GetSetting(SettingOnBot.TrustedUsers)).Select(x => UIModification.MakeTextBoxFromUserID(x));
		}
		private bool CheckIfTreeViewItemFileExists(TreeViewItem treeItem)
		{
			var fileLocation = ((FileInformation)treeItem.Tag).FileLocation;
			if (fileLocation == null || fileLocation == ((string)mSpecificFileDisplay.Tag))
			{
				return false;
			}

			mSpecificFileDisplay.Clear();
			mSpecificFileDisplay.Tag = fileLocation;
			using (var reader = new StreamReader(fileLocation))
			{
				mSpecificFileDisplay.AppendText(reader.ReadToEnd());
			}
			return true;
		}
		private async Task<bool> CheckIfTreeViewItemDMExists(TreeViewItem treeItem)
		{
			var DMChannel = (Discord.IDMChannel)treeItem.Tag;
			if (DMChannel == null || DMChannel.Id == ((Discord.IDMChannel)mSpecificDMDisplay.Tag)?.Id)
			{
				return false;
			}

			mSpecificDMDisplay.Clear();
			mSpecificDMDisplay.Tag = DMChannel;

			var messages = Actions.FormatDMs(await Actions.GetBotDMs(DMChannel));
			if (messages.Any())
			{
				foreach (var message in messages)
				{
					mSpecificDMDisplay.AppendText(String.Format("{0}{1}----------{1}", Actions.ReplaceMarkdownChars(message, true), Environment.NewLine));
				}
			}
			else
			{
				mSpecificDMDisplay.AppendText(String.Format("No DMs with this user exist; I am not sure why Discord says some do, but I will close the DMs with this person now."));
				await DMChannel.CloseAsync();
			}
			return true;
		}
		private ReturnedSetting SaveSetting(Grid g, SettingOnBot setting, BotGlobalInfo botInfo)
		{
			var children = g.Children;
			foreach (var child in children)
			{
				var saved = (ReturnedSetting)SaveSetting((dynamic)child, setting, botInfo);
				if (saved.Status != NSF.Nothing)
				{
					return saved;
				}
			}
			return new ReturnedSetting(setting, NSF.Nothing);
		}
		private ReturnedSetting SaveSetting(TextBox tb, SettingOnBot setting, BotGlobalInfo botInfo)
		{
			var text = tb.Text;
			var settingText = botInfo.GetSetting(setting)?.ToString();
			if (settingText.CaseInsEquals(text))
				return new ReturnedSetting(setting, NSF.Nothing);

			var nothingSuccessFailure = NSF.Nothing;
			switch (setting)
			{
				case SettingOnBot.Prefix:
				{
					nothingSuccessFailure = botInfo.SetSetting(setting, text) ? NSF.Success : NSF.Failure;
					break;
				}
				case SettingOnBot.BotOwnerID:
				{
					nothingSuccessFailure = ulong.TryParse(text, out ulong num) && botInfo.SetSetting(setting, num) ? NSF.Success : NSF.Failure;
					break;
				}
				case SettingOnBot.Game:
				{
					nothingSuccessFailure = botInfo.SetSetting(setting, text) ? NSF.Success : NSF.Failure;
					break;
				}
				case SettingOnBot.Stream:
				{
					nothingSuccessFailure = botInfo.SetSetting(setting, text) ? NSF.Success : NSF.Failure;
					break;
				}
				case SettingOnBot.ShardCount:
				{
					nothingSuccessFailure = int.TryParse(text, out int num) && botInfo.SetSetting(setting, num) ? NSF.Success : NSF.Failure;
					break;
				}
				case SettingOnBot.MessageCacheCount:
				{
					nothingSuccessFailure = int.TryParse(text, out int num) && botInfo.SetSetting(setting, num) ? NSF.Success : NSF.Failure;
					break;
				}
				case SettingOnBot.MaxUserGatherCount:
				{
					nothingSuccessFailure = int.TryParse(text, out int num) && botInfo.SetSetting(setting, num) ? NSF.Success : NSF.Failure;
					break;
				}
				case SettingOnBot.MaxMessageGatherSize:
				{
					nothingSuccessFailure = int.TryParse(text, out int num) && botInfo.SetSetting(setting, num) ? NSF.Success : NSF.Failure;
					break;
				}
			}

			return new ReturnedSetting(setting, nothingSuccessFailure);
		}
		private ReturnedSetting SaveSetting(Viewbox vb, SettingOnBot setting, BotGlobalInfo botInfo)
		{
			return SaveSetting((dynamic)vb.Child, setting, botInfo);
		}
		private ReturnedSetting SaveSetting(CheckBox cb, SettingOnBot setting, BotGlobalInfo botInfo)
		{
			if (!cb.IsChecked.HasValue)
				return new ReturnedSetting(setting, NSF.Nothing);

			switch (setting)
			{
				case SettingOnBot.AlwaysDownloadUsers:
				{
					var alwaysDLUsers = ((bool)botInfo.GetSetting(SettingOnBot.AlwaysDownloadUsers));
					if (cb.IsChecked.Value != alwaysDLUsers)
					{
						botInfo.SetSetting(SettingOnBot.AlwaysDownloadUsers, !alwaysDLUsers);
						return new ReturnedSetting(setting, NSF.Success);
					}
					break;
				}
			}
			return new ReturnedSetting(setting, NSF.Nothing);
		}
		private ReturnedSetting SaveSetting(ComboBox cb, SettingOnBot setting, BotGlobalInfo botInfo)
		{
			switch (setting)
			{
				case SettingOnBot.LogLevel:
				{
					var logLevel = (Discord.LogSeverity)(cb.SelectedItem as TextBox).Tag;
					var currLogLevel = ((Discord.LogSeverity)botInfo.GetSetting(SettingOnBot.LogLevel));
					if (logLevel != currLogLevel)
					{
						botInfo.SetSetting(SettingOnBot.LogLevel, logLevel);
						return new ReturnedSetting(setting, NSF.Success);
					}
					break;
				}
				case SettingOnBot.TrustedUsers:
				{
					var trustedUsers = cb.Items.OfType<TextBox>().Select(x => (ulong)x.Tag).ToList();
					var currTrustedUsers = ((List<ulong>)Variables.BotInfo.GetSetting(SettingOnBot.TrustedUsers));
					var diffUsers = currTrustedUsers.Except(trustedUsers);
					if (trustedUsers.Count != currTrustedUsers.Count || diffUsers.Any())
					{
						botInfo.SetSetting(SettingOnBot.TrustedUsers, trustedUsers);
						return new ReturnedSetting(setting, NSF.Success);
					}
					break;
				}
			}
			return new ReturnedSetting(setting, NSF.Nothing);
		}
		private ReturnedSetting SaveSetting(object obj, SettingOnBot setting, BotGlobalInfo botInfo)
		{
			return new ReturnedSetting(setting, NSF.Nothing);
		}
	}

	public class UIModification
	{
		private static CancellationTokenSource mToolTipCancellationTokenSource;

		public static void AddRows(Grid grid, int amount)
		{
			for (int i = 0; i < amount; i++)
			{
				grid.RowDefinitions.Add(new RowDefinition());
			}
		}
		public static void AddCols(Grid grid, int amount)
		{
			for (int i = 0; i < amount; i++)
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
			for (int c = 0; c < VisualTreeHelper.GetChildrenCount(parent); c++)
			{
				var element = VisualTreeHelper.GetChild(parent, c) as DependencyObject;
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
			foreach (dynamic ele in elements.SelectMany(x => x))
			{
				SetFontSizeProperty(ele, size);
			}
		}
		public static void SetFontSizeProperty(Control element, double size)
		{
			element.SetBinding(Control.FontSizeProperty, new Binding
			{
				Path = new PropertyPath("ActualHeight"),
				RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Grid), 1),
				Converter = new UIFontResizer(size),
			});
		}
		public static void SetFontSizeProperty(Grid element, double size)
		{
			var children = element.Children;
			foreach (var child in children)
			{
				SetFontSizeProperty((dynamic)child, size);
			}
		}
		public static void SetFontSizeProperty(object element, double size) { }

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

			if (mToolTipCancellationTokenSource != null)
			{
				mToolTipCancellationTokenSource.Cancel();
			}
			mToolTipCancellationTokenSource = new CancellationTokenSource();

			await baseElement.Dispatcher.InvokeAsync(async () =>
			{
				try
				{
					await Task.Delay(timeInMS, mToolTipCancellationTokenSource.Token);
				}
				catch (TaskCanceledException)
				{
					return;
				}
				catch (Exception e)
				{
					Actions.ExceptionToConsole(e);
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
			for (int i = 0; i < BGPoints.GetLength(0); i++)
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
			if (!Actions.ValidateURL(link))
			{
				Actions.WriteLine(Actions.ERROR("Invalid URL."));
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
		public static TextBox MakeTextBoxFromUserID(ulong userID)
		{
			var user = Actions.GetGlobalUser(userID);
			if (user == null)
			{
				return null;
			}

			return new MyTextBox
			{
				Text = String.Format("'{0}#{1}' ({2})", (user.Username.AllCharactersAreWithinUpperLimit(Constants.MAX_UTF16_VAL_FOR_NAMES) ? user.Username : "Non-Standard Name"), user.Discriminator, user.Id),
				Tag = userID,
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
		public static TreeView MakeGuildTreeView(TreeView tv)
		{
			//Get the directory
			var directory = Actions.GetBaseBotDirectory();
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

				var guild = Variables.Client.GetGuild(ID);
				if (guild == null)
					return null;

				//Get all of the files
				var listOfFiles = new List<TreeViewItem>();
				Directory.GetFiles(guildDir).ToList().ForEach(fileLoc =>
				{
					var fileType = Actions.GetFileType(Path.GetFileNameWithoutExtension(fileLoc));
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
					Tag = new GuildFileInformation(ID, guild.Name, guild.MemberCount),
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
		public static async Task<TreeView> MakeDMTreeView(TreeView tv)
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
			tv.ItemsSource = (await Variables.Client.GetDMChannelsAsync()).Select(x =>
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

			if (tv.ItemsSource.Cast<dynamic>().Count() == 0)
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
		public static FlowDocument MakeInfoMenu()
		{
			var uptime = String.Format("Uptime: {0}", Actions.GetUptime());
			var cmds = String.Format("Logged Commands:\n{0}", Actions.FormatLoggedCommands());
			var logs = String.Format("Logged Actions:\n{0}", Actions.FormatLoggedThings());
			var str = Actions.ReplaceMarkdownChars(String.Format("{0}\r\r{1}\r\r{2}", uptime, cmds, logs), true);
			var paragraph = new Paragraph(new Run(str))
			{
				TextAlignment = TextAlignment.Center,
			};
			return new FlowDocument(paragraph);
		}
		public static Grid MakeColorDisplayer(BotUIInfo UIInfo, Grid child, Button button, double fontSizeProperty)
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
			themeComboBox.SelectedItem = themeComboBox.Items.Cast<TextBox>().FirstOrDefault(x => (ColorTheme)x.Tag == UIInfo.Theme);
			AddElement(child, themeComboBox, 2, 5, 65, 25);

			var colorResourceKeys = Enum.GetValues(typeof(ColorTarget)).Cast<ColorTarget>().ToArray();
			for (int i = 0; i < colorResourceKeys.Length; i++)
			{
				var key = colorResourceKeys[i];
				var value = FormatBrush(UIInfo.ColorTargets[key]);

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
			return Enum.GetValues(type).Cast<dynamic>().Select(x =>
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
		public static void GatherInput(TextBox tb, Button b)
		{
			//Get the current text
			var text = tb.Text.Trim(new[] { '\r', '\n' });
			if (text.Contains("﷽"))
			{
				text += "This program really doesn't like that long Arabic character for some reason. Whenever there are a lot of them it crashes the program completely.";
			}
			Actions.WriteLine(text);

			tb.Text = "";
			b.IsEnabled = false;

			//Make sure both the path and key are set
			if (!Variables.GotPath || !Variables.GotKey)
			{
				Task.Run(async () =>
				{
					if (!Variables.GotPath)
					{
						Actions.ValidatePath(text);
					}
					else if (!Variables.GotKey)
					{
						await Actions.ValidateBotKey(Variables.Client, text);
					}
					await Actions.MaybeStartBot();
				});
			}
			else
			{
				HandleCommand(text);
			}
		}
		public static void HandleCommand(string input)
		{
			var prefix = ((string)Variables.BotInfo.GetSetting(SettingOnBot.Prefix));
			if (input.CaseInsStartsWith(prefix))
			{
				var inputArray = input.Substring(prefix.Length)?.Split(new[] { ' ' }, 2);
				var cmd = inputArray[0];
				var args = inputArray.Length > 1 ? inputArray[1] : null;
				if (!FindCommand(cmd, args))
				{
					Actions.WriteLine("No command could be found with that name.");
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
				Actions.WriteLine(String.Format("Current Totals:{0}\t\t\t Chars: {1}{0}\t\t\t Lines: {2}", Environment.NewLine, totalChars, totalLines));
			}
			var resetKey = false;
			if (resetKey)
			{
				Properties.Settings.Default.BotKey = "";
				Properties.Settings.Default.Save();
				Actions.DisconnectBot();
			}
#endif
		}
	}

	public class UITextBoxStreamWriter : TextWriter 
	{
		private TextBoxBase mOutput;
		private bool mIgnoreNewLines;
		private string mCurrentLineText;

		public UITextBoxStreamWriter(TextBoxBase output)
		{
			mOutput = output;
			mIgnoreNewLines = output is RichTextBox;
		}

		public override void Write(char value)
		{
			if (value.Equals('\n'))
			{
				Write(mCurrentLineText);
				mCurrentLineText = null;
			}
			//Done because crashes program without exception. Could not for the life of me figure out why; something in the .dlls themself.
			else if (value.Equals('﷽'))
			{
				return;
			}
			else
			{
				mCurrentLineText += value;
			}
		}
		public override void Write(string value)
		{
			if (value == null || (mIgnoreNewLines && value.Equals('\n')))
				return;

			mOutput.Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new Action(() =>
			{
				mOutput.AppendText(value);
			}));
		}
		public override Encoding Encoding
		{
			get { return Encoding.UTF8; }
		}
	}

	public class UIFontResizer : IValueConverter
	{
		private double mConvertFactor;

		public UIFontResizer(double convertFactor)
		{
			mConvertFactor = convertFactor;
		}

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var converted = (int)(System.Convert.ToInt16(value) * mConvertFactor);
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

	public class BotUIInfo
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

		public BotUIInfo()
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
		public void SaveBotUIInfo()
		{
			Actions.OverWriteFile(Actions.GetBaseBotDirectory(Constants.UI_INFO_LOCATION), Actions.Serialize(this));
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

		public static BotUIInfo LoadBotUIInfo()
		{
			var botInfo = new BotUIInfo();
			var path = Actions.GetBaseBotDirectory(Constants.UI_INFO_LOCATION);
			if (!File.Exists(path))
			{
				if (Variables.Loaded)
				{
					Actions.WriteLine("The bot UI information file does not exist.");
				}
				return botInfo;
			}

			try
			{
				using (var reader = new StreamReader(path))
				{
					botInfo = JsonConvert.DeserializeObject<BotUIInfo>(reader.ReadToEnd());
				}
			}
			catch (Exception e)
			{
				Actions.ExceptionToConsole(e);
			}
			return botInfo;
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

	public enum ColorTheme
	{
		Classic								= 0,
		Dark_Mode							= 1,
		User_Made							= 2,
	}

	public enum ColorTarget
	{
		Base_Background						= 0,
		Base_Foreground						= 1,
		Base_Border							= 2,
		Button_Background					= 3,
		Button_Border						= 4,
		Button_Disabled_Background			= 5,
		Button_Disabled_Foreground			= 6,
		Button_Disabled_Border				= 7,
		Button_Mouse_Over_Background		= 8,
	}

	public enum OtherTarget
	{
		Button_Style						= 0,
	}

	public enum MenuType
	{
		Nothing								= -1,
		Main								= 0,
		Info								= 1,
		Settings							= 2,
		Colors								= 3,
		DMs									= 4,
		Files								= 5,
	}
}
