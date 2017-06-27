using ICSharpCode.AvalonEdit;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
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
		private static readonly Grid mLayout = new Grid();
		private static readonly BotUIInfo mUIInfo = BotUIInfo.LoadBotUIInfo();

		#region Input
		private static readonly Grid mInputLayout = new Grid();
		//Max height has to be set here as a large number to a) not get in the way and b) not crash when resized small. I don't want to use a RTB for input.
		private static readonly TextBox mInput = new MyTextBox
		{
			MaxLength = 250,
			MaxLines = 5,
			MaxHeight = 1000,
			TextWrapping = TextWrapping.Wrap,
		};
		private static readonly Button mInputButton = new MyButton
		{
			IsEnabled = false,
			Content = "Enter",
		};
		#endregion

		#region Output
		private static readonly MenuItem mOutputContextMenuSearch = new MenuItem
		{
			Header = "Search For...",
		};
		private static readonly MenuItem mOutputContextMenuSave = new MenuItem
		{
			Header = "Save Output Log",
		};
		private static readonly MenuItem mOutputContextMenuClear = new MenuItem
		{
			Header = "Clear Output Log",
		};
		private static readonly MyTextBox mOutput = new MyTextBox
		{
			ContextMenu = new ContextMenu
			{
				ItemsSource = new[] { mOutputContextMenuSearch, mOutputContextMenuSave, mOutputContextMenuClear },
			},
			IsReadOnly = true,
			VerticalScrollBarVisibility = ScrollBarVisibility.Visible,
			TextWrapping = TextWrapping.Wrap,
		};
		#endregion

		#region Buttons
		private static readonly Grid mButtonLayout = new Grid();

		private static readonly Button mHelpButton = new MyButton
		{
			Content = "Main",
		};
		private static readonly Button mSettingsButton = new MyButton
		{
			Content = "Settings",
		};
		private static readonly Button mColorsButton = new MyButton
		{
			Content = "Colors",
		};
		private static readonly Button mInfoButton = new MyButton
		{
			Content = "Info",
		};
		private static readonly Button mFileButton = new MyButton
		{
			Content = "Files",
		};

		private static string mLastButtonClicked;
		#endregion

		#region Main Menu
		private static readonly Grid mMainMenuLayout = new Grid
		{
			Visibility = Visibility.Collapsed,
		};
		private static readonly RichTextBox mMainMenuOutput = new MyRichTextBox
		{
			IsReadOnly = true,
			IsDocumentEnabled = true,
			Document = UIModification.MakeMainMenu(),
		};

		private static readonly Button mDisconnectButton = new MyButton
		{
			Content = "Disconnect",
		};
		private static readonly Button mRestartButton = new MyButton
		{
			Content = "Restart",
		};
		private static readonly Button mPauseButton = new MyButton
		{
			Content = "Pause",
		};
		#endregion

		#region Settings Menu
		private static readonly Grid mSettingsLayout = new Grid
		{
			Visibility = Visibility.Collapsed,
		};
		private static readonly TextBox mSettingsPlaceholderTB = new MyTextBox
		{
			IsReadOnly = true,
		};
		private const int TITLE_START_COLUMN = 5;
		private const int TITLE_COLUMN_LENGTH = 35;
		private const int TB_START_COLUMN = 40;
		private const int TB_COLUMN_LENGTH = 55;

		private static readonly TextBox mDownloadUsersTitle = UIModification.MakeTitle("Download Users:",
			"When checked the bot will download users the instant it joins the guild. Having this off means that a lot of commands will not work correctly since I haven't bothered to put a way to manually download users.");
		private static readonly Viewbox mDownloadUsersSetting = new Viewbox
		{
			Child = new CheckBox
			{
				IsChecked = ((bool)Variables.BotInfo.GetSetting(SettingOnBot.AlwaysDownloadUsers)),
				Tag = SettingOnBot.AlwaysDownloadUsers,
			},
			HorizontalAlignment = HorizontalAlignment.Left,
			VerticalAlignment = VerticalAlignment.Center,
			Tag = SettingOnBot.AlwaysDownloadUsers,
		};

		private static readonly TextBox mPrefixTitle = UIModification.MakeTitle("Prefix:", 
			"The prefix which is needed to be said before commands.");
		private static readonly TextBox mPrefixSetting = UIModification.MakeSetting(SettingOnBot.Prefix, 10);

		private static readonly TextBox mBotOwnerTitle = UIModification.MakeTitle("Bot Owner:",
			"The number here is the ID of a user. The bot owner can use some additional commands.");
		private static readonly TextBox mBotOwnerSetting = UIModification.MakeSetting(SettingOnBot.BotOwnerID, 18);

		private static readonly TextBox mGameTitle = UIModification.MakeTitle("Game:",
			"Changes what the bot says it's playing.");
		private static readonly TextBox mGameSetting = UIModification.MakeSetting(SettingOnBot.Game, 100);

		private static readonly TextBox mStreamTitle = UIModification.MakeTitle("Stream:", 
			"Can set whatever stream you want as long as it's a valid Twitch.tv stream.");
		private static readonly TextBox mStreamSetting = UIModification.MakeSetting(SettingOnBot.Stream, 50);

		private static readonly TextBox mShardTitle = UIModification.MakeTitle("Shard Count:",
			"Each shard can hold up to 2500 guilds.");
		private static readonly TextBox mShardSetting = UIModification.MakeSetting(SettingOnBot.ShardCount, 3);

		private static readonly TextBox mMessageCacheTitle = UIModification.MakeTitle("Message Cache:",
			"The amount of messages the bot will hold in its cache.");
		private static readonly TextBox mMessageCacheSetting = UIModification.MakeSetting(SettingOnBot.MessageCacheCount, 6);

		private static readonly TextBox mUserGatherCountTitle = UIModification.MakeTitle("Max User Gather:",
			"Limits the amount of users a command can modify at once.");
		private static readonly TextBox mUserGatherCountSetting = UIModification.MakeSetting(SettingOnBot.MaxUserGatherCount, 5);

		private static readonly TextBox mMessageGatherSizeTitle = UIModification.MakeTitle("Max Msg Gather:",
			"For the download messages command. This is in bytes, which to be very basic is roughly two bytes per character.");
		private static readonly TextBox mMessageGatherSizeSetting = UIModification.MakeSetting(SettingOnBot.MaxMessageGatherSize, 7);

		private static readonly TextBox mLogLevelTitle = UIModification.MakeTitle("Log Level:",
			"Certain events in the Discord library used in this bot have a required log level to be said in the console.");
		private static readonly ComboBox mLogLevelComboBox = new MyAutoFillComboBox(UIModification.MakeComboBoxSourceOutOfEnum(typeof(Discord.LogSeverity)))
		{
			VerticalContentAlignment = VerticalAlignment.Center,
			Tag = SettingOnBot.LogLevel,
		};

		private static readonly TextBox mTrustedUsersTitle = UIModification.MakeTitle("Trusted Users:",
			"Some commands can only be run by the bot owner or user IDs that they have designated as trust worthy.");
		private static readonly TextBox mTrustedUsersAddBox = UIModification.MakeSetting(SettingOnBot.TrustedUsers, 18);
		private static readonly Button mTrustedUsersAddButton = new MyButton
		{
			Content = "+",
		};
		private static readonly Grid mTrustedUsersAddGrid = new Grid
		{
			Tag = SettingOnBot.TrustedUsers,
		};

		private static readonly TextBox mPlaceholderTitle = UIModification.MakeTitle("", "");
		private static readonly ComboBox mTrustedUsersComboBox = new MyAutoFillComboBox
		{
			VerticalContentAlignment = VerticalAlignment.Center,
			Tag = SettingOnBot.TrustedUsers,
		};
		private static readonly Button mTrustedUsersRemoveButton = new MyButton
		{
			Content = "-",
		};
		private static readonly Grid mTrustedUsersRemoveGrid = new Grid
		{
			Tag = SettingOnBot.TrustedUsers,
		};

		private static readonly UIElement[] mTitleBoxes = new UIElement[]
		{
			mDownloadUsersTitle,
			mPrefixTitle,
			mBotOwnerTitle,
			mGameTitle,
			mStreamTitle,
			mShardTitle,
			mMessageCacheTitle,
			mUserGatherCountTitle,
			mMessageGatherSizeTitle,
			mLogLevelTitle,
			mTrustedUsersTitle,
			mPlaceholderTitle,
		};
		private static readonly UIElement[] mSettingBoxes = new UIElement[]
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
			mTrustedUsersAddGrid,
			mTrustedUsersRemoveGrid,
		};

		private static readonly Button mSettingsSaveButton = new MyButton
		{
			Content = "Save Settings"
		};
		#endregion

		#region Colors Menu
		private static readonly Grid mColorsLayout = new Grid
		{
			Visibility = Visibility.Collapsed,
		};

		private static readonly Button mColorsSaveButton = new MyButton
		{
			Content = "Save Colors",
		};
		#endregion

		#region Info Menu
		private static readonly Grid mInfoLayout = new Grid
		{
			Visibility = Visibility.Collapsed,
		};
		private static readonly TextBox mInfoPlaceholderTB = new MyTextBox
		{
			IsReadOnly = true,
		};
		private static readonly RichTextBox mInfoOutput = new MyRichTextBox
		{
			IsReadOnly = true,
			IsDocumentEnabled = true,
			Document = UIModification.MakeInfoMenu(),
			BorderThickness = new Thickness(0, 1, 0, 1),
		};
		#endregion

		#region File Menu
		private static readonly Grid mFileLayout = new Grid
		{
			Visibility = Visibility.Collapsed,
		};
		private static readonly RichTextBox mFileOutput = new MyRichTextBox
		{
			IsReadOnly = true,
			IsDocumentEnabled = true,
		};

		private static readonly TreeView mFileTreeView = new TreeView();
		private static readonly Button mFileSearchButton = new MyButton
		{
			Content = "Search",
		};
		#endregion

		#region Edit Menu
		private static readonly Grid mEditLayout = new Grid
		{
			Visibility = Visibility.Collapsed,
		};
		private static readonly Grid mEditButtonLayout = new Grid();
		private static readonly TextEditor mEditBox = new TextEditor
		{
			Background = null,
			Foreground = null,
			BorderBrush = null,
			VerticalScrollBarVisibility = ScrollBarVisibility.Visible,
			WordWrap = true,
			ShowLineNumbers = true,
		};
		private static readonly TextBox mEditSaveBox = new MyTextBox
		{
			Text = "Successfully saved the file.",
			Visibility = Visibility.Collapsed,
			TextAlignment = TextAlignment.Center,
			VerticalContentAlignment = VerticalAlignment.Center,
			IsReadOnly = true,
		};
		private static readonly Button mEditSaveButton = new MyButton
		{
			Content = "Save",
		};
		private static readonly Button mEditCloseButton = new MyButton
		{
			Content = "Close",
		};
		#endregion

		#region Guild Search
		private static readonly Grid mGuildSearchLayout = new Grid
		{
			Background = UIModification.MakeBrush("#BF000000"),
			Visibility = Visibility.Collapsed
		};
		private static readonly Grid mGuildSearchTextLayout = new Grid();

		private static readonly TextBox mGuildSearchPlaceholderTB = new MyTextBox
		{
			IsReadOnly = true,
		};
		private static readonly Viewbox mGuildSearchNameHeader = UIModification.MakeStandardViewBox("Guild Name:");
		private static readonly TextBox mGuildSearchNameInput = new MyTextBox
		{
			MaxLength = 100,
			TextWrapping = TextWrapping.Wrap,
		};
		private static readonly Viewbox mGuildSearchIDHeader = UIModification.MakeStandardViewBox("ID:");
		private static readonly TextBox mGuildSearchIDInput = new MyTextBox
		{
			MaxLength = 18,
			TextWrapping = TextWrapping.Wrap,
		};
		private static readonly ComboBox mGuildSearchFileComboBox = new MyAutoFillComboBox
		{
			VerticalContentAlignment = VerticalAlignment.Center,
			Tag = SettingOnBot.LogLevel,
			ItemsSource = UIModification.MakeComboBoxSourceOutOfEnum(typeof(FileType)),
		};

		private static readonly Button mGuildSearchButton = new MyButton
		{
			Content = "Search",
		};
		private static readonly Button mGuildSearchCloseButton = new MyButton
		{
			Content = "Close",
		};
		#endregion

		#region Output Search
		private static readonly Grid mOutputSearchLayout = new Grid
		{
			Background = UIModification.MakeBrush("#BF000000"),
			Visibility = Visibility.Collapsed
		};
		private static readonly Grid mOutputSearchTextLayout = new Grid();

		private static readonly TextBox mOutputSearchResults = new MyTextBox
		{
			IsReadOnly = true,
			VerticalScrollBarVisibility = ScrollBarVisibility.Visible,
			TextWrapping = TextWrapping.Wrap,
		};
		private static readonly TextBox mOutputSearchPlaceholderTB = new MyTextBox
		{
			IsReadOnly = true,
		};

		private static readonly ComboBox mOutputSearchComboBox = new MyAutoFillComboBox
		{
			VerticalContentAlignment = VerticalAlignment.Center,
			Tag = SettingOnBot.LogLevel,
		};

		private static readonly Button mOutputSearchButton = new MyButton
		{
			Content = "Search",
		};
		private static readonly Button mOutputSearchCloseButton = new MyButton
		{
			Content = "Close",
		};
		#endregion

		#region System Info
		private static readonly Grid mSysInfoLayout = new Grid();
		private static readonly TextBox mSysInfoUnder = new MyTextBox()
		{
			IsReadOnly = true,
		};

		private static readonly Viewbox mLatency = new Viewbox
		{
			Child = UIModification.MakeSysInfoBox(),
		};
		private static readonly Viewbox mMemory = new Viewbox
		{
			Child = UIModification.MakeSysInfoBox(),
		};
		private static readonly Viewbox mThreads = new Viewbox
		{
			Child = UIModification.MakeSysInfoBox(),
		};
		private static readonly Viewbox mGuilds = new Viewbox
		{
			Child = UIModification.MakeSysInfoBox(),
		};
		private static readonly Viewbox mUsers = new Viewbox
		{
			Child = UIModification.MakeSysInfoBox(),
		};

		private static readonly ToolTip mMemHoverInfo = new ToolTip
		{
			Content = "This is not guaranteed to be 100% correct.",
		};
		#endregion

		public BotWindow()
		{
			FontFamily = new FontFamily("Courier New");
			InitializeComponent();
			Loaded += RunApplication;
		}
		private void InitializeComponent()
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
			UIModification.AddElement(mLayout, mButtonLayout, 87, 13, 3, 1, 1, 5);
			UIModification.AddElement(mButtonLayout, mHelpButton, 0, 1, 0, 1);
			UIModification.AddElement(mButtonLayout, mSettingsButton, 0, 1, 1, 1);
			UIModification.AddElement(mButtonLayout, mColorsButton, 0, 1, 2, 1);
			UIModification.AddElement(mButtonLayout, mInfoButton, 0, 1, 3, 1);
			UIModification.AddElement(mButtonLayout, mFileButton, 0, 1, 4, 1);

			//Main Menu
			UIModification.AddElement(mLayout, mMainMenuLayout, 0, 87, 3, 1, 100, 3);
			UIModification.AddElement(mMainMenuLayout, mMainMenuOutput, 0, 95, 0, 3);
			UIModification.AddElement(mMainMenuLayout, mPauseButton, 95, 5, 0, 1);
			UIModification.AddElement(mMainMenuLayout, mRestartButton, 95, 5, 1, 1);
			UIModification.AddElement(mMainMenuLayout, mDisconnectButton, 95, 5, 2, 1);

			//Settings Menu
			UIModification.AddElement(mLayout, mSettingsLayout, 0, 87, 3, 1, 100, 100);
			UIModification.AddElement(mSettingsLayout, mSettingsPlaceholderTB, 0, 100, 0, 100);
			UIModification.AddCols(mTrustedUsersAddGrid, 10);
			UIModification.AddElement(mTrustedUsersAddGrid, mTrustedUsersAddBox, 0, 1, 0, 9);
			UIModification.AddElement(mTrustedUsersAddGrid, mTrustedUsersAddButton, 0, 1, 9, 1);
			UIModification.AddCols(mTrustedUsersRemoveGrid, 10);
			UIModification.AddElement(mTrustedUsersRemoveGrid, mTrustedUsersComboBox, 0, 1, 0, 9);
			UIModification.AddElement(mTrustedUsersRemoveGrid, mTrustedUsersRemoveButton, 0, 1, 9, 1);
			for (int i = 0; i < mTitleBoxes.Length; i++)
			{
				dynamic title = mTitleBoxes[i];
				dynamic setting = mSettingBoxes[i];
				UIModification.AddElement(mSettingsLayout, title, (i * 4), 4, TITLE_START_COLUMN, TITLE_COLUMN_LENGTH);
				UIModification.AddElement(mSettingsLayout, setting, (i * 4), 4, TB_START_COLUMN, TB_COLUMN_LENGTH);
			}
			UIModification.AddElement(mSettingsLayout, mSettingsSaveButton, 95, 5, 0, 100);

			//Colors Menu
			UIModification.AddElement(mLayout, mColorsLayout, 0, 87, 3, 1, 100, 100);

			//Info Menu
			UIModification.AddElement(mLayout, mInfoLayout, 0, 87, 3, 1, 1, 10);
			UIModification.AddElement(mInfoLayout, mInfoPlaceholderTB, 0, 1, 0, 10);
			UIModification.AddElement(mInfoLayout, mInfoOutput, 0, 1, 1, 8);

			//File Menu
			UIModification.AddElement(mLayout, mFileLayout, 0, 87, 3, 1, 100, 1);
			UIModification.AddElement(mFileLayout, mFileOutput, 0, 100, 0, 1);
			UIModification.AddElement(mFileLayout, mFileSearchButton, 95, 5, 0, 1);

			//Edit
			UIModification.AddElement(mLayout, mEditLayout, 0, 100, 0, 4, 100, 4);
			UIModification.AddElement(mEditLayout, mEditBox, 0, 100, 0, 3);
			UIModification.AddElement(mEditLayout, mEditSaveBox, 84, 3, 3, 1);
			UIModification.AddElement(mEditLayout, mEditButtonLayout, 87, 13, 3, 1, 1, 2);
			UIModification.AddElement(mEditButtonLayout, mEditSaveButton, 0, 1, 0, 1);
			UIModification.AddElement(mEditButtonLayout, mEditCloseButton, 0, 1, 1, 1);

			//Guild Search
			UIModification.AddElement(mLayout, mGuildSearchLayout, 0, 100, 0, 4, 10, 10);
			UIModification.AddElement(mGuildSearchLayout, mGuildSearchTextLayout, 3, 4, 3, 4, 100, 100);
			UIModification.AddElement(mGuildSearchTextLayout, mGuildSearchPlaceholderTB, 0, 100, 0, 100);
			UIModification.AddElement(mGuildSearchTextLayout, mGuildSearchNameHeader, 10, 10, 20, 60);
			UIModification.AddElement(mGuildSearchTextLayout, mGuildSearchNameInput, 20, 15, 20, 60);
			UIModification.AddElement(mGuildSearchTextLayout, mGuildSearchIDHeader, 35, 10, 20, 60);
			UIModification.AddElement(mGuildSearchTextLayout, mGuildSearchIDInput, 45, 10, 20, 60);
			UIModification.AddElement(mGuildSearchTextLayout, mGuildSearchFileComboBox, 57, 10, 20, 60);
			UIModification.AddElement(mGuildSearchTextLayout, mGuildSearchButton, 69, 15, 20, 25);
			UIModification.AddElement(mGuildSearchTextLayout, mGuildSearchCloseButton, 69, 15, 55, 25);

			//Output Search
			UIModification.AddElement(mLayout, mOutputSearchLayout, 0, 100, 0, 4, 100, 100);
			UIModification.AddElement(mOutputSearchLayout, mOutputSearchResults, 10, 75, 10, 80);
			UIModification.AddElement(mOutputSearchLayout, mOutputSearchPlaceholderTB, 85, 5, 10, 80);
			UIModification.AddElement(mOutputSearchLayout, mOutputSearchComboBox, 86, 3, 12, 25);
			UIModification.AddElement(mOutputSearchLayout, mOutputSearchButton, 86, 3, 76, 5);
			UIModification.AddElement(mOutputSearchLayout, mOutputSearchCloseButton, 86, 3, 83, 5);  

			//Font size properties
			UIModification.SetFontSizeProperties(.275, new UIElement[] { mInput, });
			UIModification.SetFontSizeProperties(.060, new UIElement[] { mGuildSearchNameInput, mGuildSearchIDInput, });
			UIModification.SetFontSizeProperties(.035, new UIElement[] { mInfoOutput, });
			UIModification.SetFontSizeProperties(.022, new UIElement[] { mEditBox, mEditSaveBox, mFileOutput, });
			UIModification.SetFontSizeProperties(.018, new UIElement[] { mMainMenuOutput, }, mTitleBoxes, mSettingBoxes);

			mPauseButton.Click += Pause;
			mRestartButton.Click += Restart;
			mDisconnectButton.Click += Disconnect;
			mInput.KeyUp += AcceptInput;
			mInputButton.Click += AcceptInput;
			mMemory.MouseEnter += ModifyMemHoverInfo;
			mMemory.MouseLeave += ModifyMemHoverInfo;
			mHelpButton.Click += BringUpMenu;
			mSettingsButton.Click += BringUpMenu;
			mColorsButton.Click += BringUpMenu;
			mInfoButton.Click += BringUpMenu;
			mFileButton.Click += BringUpMenu;
			mOutputContextMenuSave.Click += SaveOutput;
			mOutputContextMenuClear.Click += ClearOutput;
			mTrustedUsersRemoveButton.Click += RemoveTrustedUser;
			mTrustedUsersAddButton.Click += AddTrustedUser;
			mSettingsSaveButton.Click += SaveSettings;
			mColorsSaveButton.Click += SaveColors;
			mEditCloseButton.Click += CloseEditLayout;
			mEditSaveButton.Click += SaveEditLayout;
			mFileSearchButton.Click += BringUpFileSearch;
			mGuildSearchCloseButton.Click += CloseFileSearch;
			mGuildSearchLayout.MouseUp += CloseFileSearch;
			mGuildSearchButton.Click += FileSearch;
			mOutputContextMenuSearch.Click += BringUpOutputSearch;
			mOutputSearchCloseButton.Click += CloseOutputSearch;
			mOutputSearchLayout.MouseUp += CloseOutputSearch;
			mOutputSearchButton.Click += OutputSearch;

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
				//Check if valid path at startup
				Variables.GotPath = Actions.ValidatePath(Properties.Settings.Default.Path, true);
				//Check if valid key at startup
				Variables.GotKey = Variables.GotPath && await Actions.ValidateBotKey(Variables.Client, Properties.Settings.Default.BotKey, true);
				//Try to start the bot
				Actions.MaybeStartBot();
			});

			mUIInfo.InitializeColors();
			mUIInfo.ActivateTheme();
			UIModification.SetColorMode(mLayout);
			UpdateSystemInformation();
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
		private void BringUpOutputSearch(object sender, RoutedEventArgs e)
		{
			((MyAutoFillComboBox)mOutputSearchComboBox).SetItemsSource(UIModification.MakeComboBoxSourceOutOfStrings(Variables.WrittenLines.Keys));
			mOutputSearchLayout.Visibility = Visibility.Visible;
		}
		private void CloseOutputSearch(object sender, RoutedEventArgs e)
		{
			mOutputSearchComboBox.SelectedItem = null;
			mOutputSearchResults.Text = null;
			mOutputSearchLayout.Visibility = Visibility.Collapsed;
		}
		private void OutputSearch(object sender, RoutedEventArgs e)
		{
			var selectedItem = (TextBox)mOutputSearchComboBox.SelectedItem;
			if (selectedItem != null)
			{
				mOutputSearchResults.Text = null;
				Variables.WrittenLines[selectedItem.Text].ForEach(x => mOutputSearchResults.AppendText(x + Environment.NewLine));
			}
		}
		private void BringUpFileSearch(object sender, RoutedEventArgs e)
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
		private void FileSearch(object sender, RoutedEventArgs e)
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
				}
				else
				{
					guild = mFileTreeView.Items.Cast<TreeViewItem>().FirstOrDefault(x =>
					{
						var info = (GuildFileInformation)x.Tag;
						return info.ID == guildID;
					});

					if (guild == null)
					{
						Actions.WriteLine(String.Format("No guild could be found with the ID '{0}'.", guildID));
					}
				}
			}
			else if (!String.IsNullOrWhiteSpace(nameStr))
			{
				var guilds = mFileTreeView.Items.Cast<TreeViewItem>().Where(x =>
				{
					var info = (GuildFileInformation)x.Tag;
					return Actions.CaseInsEquals(info.Name, nameStr);
				});

				if (guilds.Count() == 0)
				{
					Actions.WriteLine(String.Format("No guild could be found with the name '{0}'.", nameStr));
				}
				else if (guilds.Count() == 1)
				{
					guild = guilds.FirstOrDefault();
				}
				else
				{
					Actions.WriteLine("More than one guild has the name '{0}'.", nameStr);
				}
			}

			if (guild != null)
			{
				var item = guild.Items.Cast<TreeViewItem>().FirstOrDefault(x =>
				{
					var info = (FileInformation)x.Tag;
					return info.FileType == fileType;
				});

				if (item != null)
				{
					if (CheckIfTreeViewItemFileExists(item))
					{
						mEditLayout.Visibility = Visibility.Visible;
						mFileSearchButton.Visibility = Visibility.Collapsed;
						return;
					}
				}

				Actions.WriteLine("Unable to bring up the file.");
			}
		}
		private void BringUpEditLayout(object sender, RoutedEventArgs e)
		{
			if (CheckIfTreeViewItemFileExists((TreeViewItem)sender))
			{
				mEditLayout.Visibility = Visibility.Visible;
				mFileSearchButton.Visibility = Visibility.Collapsed;
			}
		}
		private void CloseEditLayout(object sender, RoutedEventArgs e)
		{
			var result = MessageBox.Show("Are you sure you want to close the edit window?", Variables.BotName, MessageBoxButton.OKCancel);

			switch (result)
			{
				case MessageBoxResult.OK:
				{
					mEditLayout.Visibility = Visibility.Collapsed;
					mFileSearchButton.Visibility = Visibility.Visible;
					break;
				}
			}
		}
		private void SaveEditLayout(object sender, RoutedEventArgs e)
		{
			var fileLocation = mEditBox.Tag.ToString();
			if (String.IsNullOrWhiteSpace(fileLocation) || !File.Exists(fileLocation))
			{
				MessageBox.Show("Unable to gather the path for this file.", Variables.BotName);
			}
			else
			{
				var fileAndExtension = fileLocation.Substring(fileLocation.LastIndexOf('\\') + 1);
				if (fileAndExtension.Equals(Constants.GUILD_INFO_LOCATION))
				{
					//Make sure the guild info stays valid
					try
					{
						var throwaway = JsonConvert.DeserializeObject<BotGuildInfo>(mEditBox.Text);
					}
					catch (Exception exc)
					{
						Actions.ExceptionToConsole(exc);
						MessageBox.Show("Failed to save the file.", Variables.BotName);
						return;
					}
				}

				//Save the file and give a notification
				using (var writer = new StreamWriter(fileLocation))
				{
					writer.WriteLine(mEditBox.Text);
				}
				UIModification.ToggleAndRetoggleElement(mEditSaveBox);
			}
		}
		private void BringUpMenu(object sender, RoutedEventArgs e)
		{
			if (!Variables.Loaded)
				return;

			//Hide everything so stuff doesn't overlap
			mMainMenuLayout.Visibility = Visibility.Collapsed;
			mSettingsLayout.Visibility = Visibility.Collapsed;
			mColorsLayout.Visibility = Visibility.Collapsed;
			mInfoLayout.Visibility = Visibility.Collapsed;
			mFileLayout.Visibility = Visibility.Collapsed;

			//If clicking the same button then resize the output window to the regular size
			var name = (sender as Button).Content.ToString();
			if (Actions.CaseInsEquals(name, mLastButtonClicked))
			{
				UIModification.SetColAndSpan(mOutput, 0, 4);
				mLastButtonClicked = null;
			}
			else
			{
				//Resize the regular output window and have the menubox appear
				UIModification.SetColAndSpan(mOutput, 0, 3);
				mLastButtonClicked = name;

				//Show the text for help
				if (Actions.CaseInsEquals(name, mHelpButton.Content.ToString()))
				{
					mMainMenuLayout.Visibility = Visibility.Visible;
				}
				//Show the text for settings
				else if (Actions.CaseInsEquals(name, mSettingsButton.Content.ToString()))
				{
					UpdateSettingsWhenOpened();
					mSettingsLayout.Visibility = Visibility.Visible;
				}
				else if (Actions.CaseInsEquals(name, mColorsButton.Content.ToString()))
				{
					UIModification.MakeColorDisplayer(mUIInfo, mColorsLayout, mColorsSaveButton, .018);
					mColorsLayout.Visibility = Visibility.Visible;
				}
				//Show the text for info
				else if (Actions.CaseInsEquals(name, mInfoButton.Content.ToString()))
				{
					mInfoLayout.Visibility = Visibility.Visible;
				}
				//Show the text for files
				else if (Actions.CaseInsEquals(name, mFileButton.Content.ToString()))
				{
					var treeView = UIModification.MakeGuildTreeView(mFileTreeView);
					treeView.Items.Cast<TreeViewItem>().SelectMany(x => x.Items.Cast<TreeViewItem>()).ToList().ForEach(x =>
					{
						x.MouseDoubleClick += BringUpEditLayout;
					});
					mFileOutput.Document = new FlowDocument(new Paragraph(new InlineUIContainer(treeView)));
					mFileLayout.Visibility = Visibility.Visible;
				}
			}
		}
		private void SaveSettings(object sender, RoutedEventArgs e)
		{
			var botInfo = Variables.BotInfo;
			var success = new List<string>();
			var failure = new List<string>();

			//Go through each setting and update them
			foreach (dynamic ele in mSettingBoxes)
			{
				var setting = ele.Tag as SettingOnBot?;
				if (setting == null)
					continue;

				ReturnedSetting response = SaveSetting(ele, (SettingOnBot)setting, botInfo);
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
						Actions.WriteLine(String.Format("Invalid color supplied for {0}.", Enum.GetName(typeof(ColorTarget), target)));
						continue;
					}

					if (!UIModification.CheckIfTwoBrushesAreTheSame(mUIInfo.ColorTargets[target], brush))
					{
						mUIInfo.ColorTargets[target] = brush;
						castedChild.Text = UIModification.FormatBrush(brush);
						Actions.WriteLine(String.Format("Successfully updated the color for {0}.", Enum.GetName(typeof(ColorTarget), target)));
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
		private void ModifyMemHoverInfo(object sender, RoutedEventArgs e)
		{
			UIModification.ToggleToolTip(mMemHoverInfo);
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
			((CheckBox)mDownloadUsersSetting.Child).IsChecked = ((bool)botInfo.GetSetting(SettingOnBot.AlwaysDownloadUsers));
			mPrefixSetting.Text = ((string)botInfo.GetSetting(SettingOnBot.Prefix));
			mBotOwnerSetting.Text = ((ulong)botInfo.GetSetting(SettingOnBot.BotOwnerID)).ToString();
			mGameSetting.Text = ((string)botInfo.GetSetting(SettingOnBot.Game));
			mStreamSetting.Text = ((string)botInfo.GetSetting(SettingOnBot.Stream));
			mShardSetting.Text = ((int)botInfo.GetSetting(SettingOnBot.ShardCount)).ToString();
			mMessageCacheSetting.Text = ((int)botInfo.GetSetting(SettingOnBot.MessageCacheCount)).ToString();
			mUserGatherCountSetting.Text = ((int)botInfo.GetSetting(SettingOnBot.MaxUserGatherCount)).ToString();
			mMessageGatherSizeSetting.Text = ((int)botInfo.GetSetting(SettingOnBot.MaxMessageGatherSize)).ToString();
			mLogLevelComboBox.SelectedItem = mLogLevelComboBox.Items.OfType<TextBox>().FirstOrDefault(x => x.Tag != null && (Discord.LogSeverity)x.Tag == ((Discord.LogSeverity)Variables.BotInfo.GetSetting(SettingOnBot.LogLevel)));
			mTrustedUsersComboBox.ItemsSource = ((List<ulong>)Variables.BotInfo.GetSetting(SettingOnBot.TrustedUsers)).Select(x => UIModification.MakeTextBoxFromUserID(x));
		}
		private bool CheckIfTreeViewItemFileExists(TreeViewItem treeItem)
		{
			//Get the path from the tag
			var fileLocation = ((FileInformation)treeItem.Tag).FileLocation;
			if (fileLocation == null)
				return false;

			//Change the text in the bot and make it visible
			using (var reader = new StreamReader(fileLocation))
			{
				mEditBox.Text = reader.ReadToEnd();
			}
			mEditBox.Tag = fileLocation;
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
			if (Actions.CaseInsEquals(botInfo.GetSetting(setting)?.ToString(), text))
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
			Grid.SetRow(item, start < 0 ? 0 : start);
			Grid.SetRowSpan(item, length < 1 ? 1 : length);
		}

		public static void SetColAndSpan(UIElement item, int start = 0, int length = 1)
		{
			Grid.SetColumn(item, start < 0 ? 0 : start);
			Grid.SetColumnSpan(item, length < 1 ? 1 : length);
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

		public static void SetFontSizeProperties(double size, params IEnumerable<UIElement>[] elements)
		{
			foreach (dynamic ele in elements.SelectMany(x => x))
			{
				SetFontSizeProperty(ele, size);
			}
		}

		public static void SetFontSizeProperty(Control element, double size)
		{
			element.SetBinding(Control.FontSizeProperty, MakeTextSizeBinding(size));
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

		public static void ToggleAndRetoggleElement(UIElement element)
		{
			element.Dispatcher.InvokeAsync(async () =>
			{
				element.Visibility = element.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
				await Task.Delay(2500);
				element.Visibility = element.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
			});
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
				Text = String.Format("'{0}#{1}' ({2})", (Actions.GetIfValidUnicode(user.Username, 127) ? user.Username : "Non-Standard Name"), user.Discriminator, user.Id),
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

			//Format the treeviewitems
			var guildItems = new List<TreeViewItem>();
			Directory.GetDirectories(directory).ToList().ForEach(guildDir =>
			{
				//Separate the ID from the rest of the directory
				var strID = guildDir.Substring(guildDir.LastIndexOf('\\') + 1);
				//Make sure the ID is valid
				if (!ulong.TryParse(strID, out ulong ID))
					return;

				var guild = Variables.Client.GetGuild(ID);
				if (guild == null)
					return;

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
					return;

				//Create the guild item
				var guildItem = new TreeViewItem
				{
					Header = String.Format("({0}) {1}", strID, guild.Name),
					Tag = new GuildFileInformation(ID, guild.Name, guild.MemberCount),
					Background = (Brush)Application.Current.Resources[ColorTarget.Base_Background],
					Foreground = (Brush)Application.Current.Resources[ColorTarget.Base_Foreground],
				};
				listOfFiles.ForEach(x =>
				{
					guildItem.Items.Add(x);
				});

				guildItems.Add(guildItem);
			});

			tv.ItemsSource = guildItems.OrderByDescending(x => ((GuildFileInformation)x.Tag).MemberCount);
			tv.BorderThickness = new Thickness(0);
			tv.Background = (Brush)Application.Current.Resources[ColorTarget.Base_Background];
			tv.Foreground = (Brush)Application.Current.Resources[ColorTarget.Base_Foreground];

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
			var uptime = Actions.GetUptime();
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

			var placeHolderTB = new MyTextBox
			{
				IsReadOnly = true,
			};
			AddElement(child, placeHolderTB, 0, 100, 0, 100);

			var themeTitle = MakeTitle("Themes:", "");
			SetFontSizeProperty(themeTitle, fontSizeProperty);
			AddElement(child, themeTitle, 2, 5, 10, 55);
			var themeComboBox = new MyAutoFillComboBox
			{
				ItemsSource = MakeComboBoxSourceOutOfEnum(typeof(ColorTheme)),
				VerticalContentAlignment = VerticalAlignment.Center,
			};
			themeComboBox.SelectedItem = themeComboBox.Items.Cast<MyTextBox>().FirstOrDefault(x => (ColorTheme)x.Tag == UIInfo.Theme);
			AddElement(child, themeComboBox, 2, 5, 65, 25);

			var colorResourceKeys = Enum.GetValues(typeof(ColorTarget)).Cast<ColorTarget>().ToArray();
			for (int i = 0; i < colorResourceKeys.Length; i++)
			{
				var key = colorResourceKeys[i];
				var value = FormatBrush(UIInfo.ColorTargets[key]);

				var title = MakeTitle(String.Format("{0}:", Enum.GetName(typeof(ColorTarget), key)), "");
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

		public static Brush MakeBrush(string color)
		{
			return (SolidColorBrush)new BrushConverter().ConvertFrom(color);
		}

		public static Binding MakeTextSizeBinding(double val)
		{
			return new Binding
			{
				Path = new PropertyPath("ActualHeight"),
				RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Grid), 1),
				Converter = new UIFontResizer(val),
			};
		}

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
						if (Variables.GotPath = Actions.ValidatePath(text))
						{
							Variables.GotKey = await Actions.ValidateBotKey(Variables.Client, Properties.Settings.Default.BotKey, true);
						}
					}
					else if (!Variables.GotKey)
					{
						Variables.GotKey = await Actions.ValidateBotKey(Variables.Client, text);
					}
					Actions.MaybeStartBot();
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
			if (Actions.CaseInsStartsWith(input, prefix))
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
			if (Actions.CaseInsEquals(cmd, "test"))
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
					if (Actions.CaseInsEquals(Path.GetExtension(file), ".cs"))
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
			return Math.Max((int)(System.Convert.ToDouble(value) * mConvertFactor), -1);
		}
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
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

	//TODO: Make this not a piece of shit
	public class MyAutoFillComboBox : ComboBox
	{
		private List<TextBox> mItemsSource;
		private ToolTip mToolTip = new ToolTip();
		private string mSearchWord;

		public MyAutoFillComboBox()
		{
			mItemsSource = new List<TextBox>();
			Init();
		}
		public MyAutoFillComboBox(IEnumerable<string> itemSource)
		{
			mItemsSource = itemSource.Select(x =>
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
				} as TextBox;
			}).ToList();
			Init();
		}
		public MyAutoFillComboBox(IEnumerable<TextBox> itemSource)
		{
			mItemsSource = itemSource.ToList();
			Init();
		}

		private void Init()
		{
			this.ItemsSource = mItemsSource;
			this.ToolTip = mToolTip;
			this.PreviewTextInput += MyAutoFillComboBox_TextInput;
			this.DropDownClosed += MyAutoFillComboBox_DropDownClosed;
		}
		private void MyAutoFillComboBox_TextInput(object sender, TextCompositionEventArgs e)
		{
			var typedChar = e.Text.FirstOrDefault().ToString();
			if (!String.IsNullOrWhiteSpace(typedChar))
			{
				mSearchWord += typedChar;
				var itemsWithCharAtIndex = this.ItemsSource.Cast<TextBox>().Where(x => Actions.CaseInsStartsWith(x.Text, mSearchWord)).ToArray();
				if (itemsWithCharAtIndex.Any())
				{
					mToolTip.Content = mSearchWord;
					mToolTip.IsOpen = true;
					this.ItemsSource = itemsWithCharAtIndex;
					return;
				}
			}

			{
				Reset();
			}
		}
		private void MyAutoFillComboBox_DropDownClosed(object sender, EventArgs e)
		{
			Reset();
		}
		private void Reset()
		{
			mSearchWord = null;
			mToolTip.IsOpen = false;
			this.ItemsSource = mItemsSource;
		}
		public void SetItemsSource(IEnumerable<TextBox> itemsSource)
		{
			mItemsSource = itemsSource.ToList();
			this.ItemsSource = mItemsSource;
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

		[JsonConstructor]
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
		Classic = 1,
		Dark_Mode = 2,
		User_Made = 3,
	}

	public enum ColorTarget
	{
		Base_Background = 1,
		Base_Foreground = 2,
		Base_Border = 3,
		Button_Background = 4,
		Button_Border = 5,
		Button_Disabled_Background = 6,
		Button_Disabled_Foreground = 7,
		Button_Disabled_Border = 8,
		Button_Mouse_Over_Background = 9,
	}

	public enum OtherTarget
	{
		Button_Style = 1,
	}
}
