using ICSharpCode.AvalonEdit;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
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
using System.Collections.Generic;


namespace Advobot
{
	//Create the UI
	public class BotWindow : Window
	{
		private static Grid mLayout = new Grid();

		#region Input
		private static Grid mInputLayout = new Grid();
		//Max height has to be set here as a large number to a) not get in the way and b) not crash when resized small. I don't want to use a RTB for input.
		private static TextBox mInputBox = new TextBox
		{
			MaxLength = 250,
			MaxLines = 5,
			MaxHeight = 1000,
			TextWrapping = TextWrapping.Wrap,
		};
		private static Button mInputButton = new Button
		{
			IsEnabled = false,
			Content = "Enter",
			Tag = "Enter",
		};
		#endregion

		#region Output
		private static MenuItem mOutputContextMenuSave = new MenuItem
		{
			Header = "Save Output Log",
		};
		private static MenuItem mOutputContextMenuClear = new MenuItem
		{
			Header = "Clear Output Log",
		};
		private static ContextMenu mOutputContextMenu = new ContextMenu
		{
			ItemsSource = new[] { mOutputContextMenuSave, mOutputContextMenuClear },
		};
		private static RichTextBox mOutputBox = new RichTextBox
		{
			ContextMenu = mOutputContextMenu,
			IsReadOnly = true,
			IsDocumentEnabled = true,
			VerticalScrollBarVisibility = ScrollBarVisibility.Visible,
			Background = null,
		};
		#endregion

		#region Menu
		private static Grid mMenuLayout = new Grid
		{
			Visibility = Visibility.Collapsed,
		};

		private static Paragraph mHelpParagraph = UIMakeElement.MakeHelpParagraph();
		private static Paragraph mInfoParagraph = new Paragraph(new Run(Actions.FormatLoggedThings(true)));

		private static TreeView mFileTreeView = new TreeView();
		private static Paragraph mFileParagraph = new Paragraph();

		private static RichTextBox mMenuOutput = new RichTextBox
		{
			IsReadOnly = true,
			IsDocumentEnabled = true,
			Background = null,
		};

		private static string mLastButtonClicked;
		private static Grid mButtonLayout = new Grid();
		private static Button mHelpButton = new Button
		{
			Content = "Help",
		};
		private static Button mSettingsButton = new Button
		{
			Content = "Settings",
		};
		private static Button mInfoButton = new Button
		{
			Content = "Info",
		};
		private static Button mFileButton = new Button
		{
			Content = "Files",
		};
		private static Button mFileSearchButton = new Button
		{
			Content = "Search",
			Visibility = Visibility.Collapsed,
		};
		#endregion

		#region Settings
		private static Grid mSettingsLayout = new Grid
		{
			Visibility = Visibility.Collapsed,
		};
		private static TextBox mSettingsPlaceholderTB = new TextBox
		{
			IsReadOnly = true,
		};
		private const int TITLE_START_COLUMN = 5;
		private const int TITLE_COLUMN_LENGTH = 35;
		private const int TB_START_COLUMN = 40;
		private const int TB_COLUMN_LENGTH = 55;

		private static TextBox mDarkModeTitle = UIMakeElement.MakeTitle("Darkmode:");
		private static Viewbox mDarkModeSetting = new Viewbox
		{
			Child = new CheckBox
			{
				IsChecked = Variables.BotInfo.DarkMode,
				Tag = SettingOnBot.DarkMode,
			},
			HorizontalAlignment = HorizontalAlignment.Left,
			VerticalAlignment = VerticalAlignment.Center,
			Tag = SettingOnBot.DarkMode,
		};

		private static TextBox mDownloadUsersTitle = UIMakeElement.MakeTitle("Download Users:");
		private static Viewbox mDownloadUsersSetting = new Viewbox
		{
			Child = new CheckBox
			{
				IsChecked = Variables.BotInfo.AlwaysDownloadUsers,
				Tag = SettingOnBot.AlwaysDownloadUsers,
			},
			HorizontalAlignment = HorizontalAlignment.Left,
			VerticalAlignment = VerticalAlignment.Center,
			Tag = SettingOnBot.AlwaysDownloadUsers,
		};

		private static TextBox mPrefixTitle = UIMakeElement.MakeTitle("Prefix:");
		private static TextBox mPrefixSetting = UIMakeElement.MakeSetting(SettingOnBot.Prefix, 10);

		private static TextBox mBotOwnerTitle = UIMakeElement.MakeTitle("Bot Owner:");
		private static TextBox mBotOwnerSetting = UIMakeElement.MakeSetting(SettingOnBot.BotOwner, 18);

		private static TextBox mGameTitle = UIMakeElement.MakeTitle("Game:");
		private static TextBox mGameSetting = UIMakeElement.MakeSetting(SettingOnBot.Game, 100);

		private static TextBox mStreamTitle = UIMakeElement.MakeTitle("Stream:");
		private static TextBox mStreamSetting = UIMakeElement.MakeSetting(SettingOnBot.Stream, 50);

		private static TextBox mShardTitle = UIMakeElement.MakeTitle("Shard Count:");
		private static TextBox mShardSetting = UIMakeElement.MakeSetting(SettingOnBot.ShardCount, 3);

		private static TextBox mMessageCacheTitle = UIMakeElement.MakeTitle("Message Cache:");
		private static TextBox mMessageCacheSetting = UIMakeElement.MakeSetting(SettingOnBot.MessageCacheSize, 6);

		private static TextBox mLogLevelTitle = UIMakeElement.MakeTitle("Log Level:");
		private static ComboBox mLogLevelComboBox = new ComboBox
		{
			VerticalContentAlignment = VerticalAlignment.Center,
			Tag = SettingOnBot.LogLevel,
			ItemsSource = UIMakeElement.MakeComboBoxSourceOutOfEnum(typeof(Discord.LogSeverity)),
		};

		private static TextBox mTrustedUsersTitle = UIMakeElement.MakeTitle("Trusted Users:");
		private static TextBox mTrustedUsersAddBox = UIMakeElement.MakeSetting(SettingOnBot.TrustedUsers, 18);
		private static Button mTrustedUsersAddButton = new Button
		{
			Content = "+",
		};
		private static Grid mTrustedUsersAddGrid = new Grid
		{
			Tag = SettingOnBot.TrustedUsers,
		};

		private static TextBox mPlaceholderTitle = UIMakeElement.MakeTitle("");
		private static ComboBox mTrustedUsersComboBox = new ComboBox
		{
			VerticalContentAlignment = VerticalAlignment.Center,
			Tag = SettingOnBot.TrustedUsers,
			ItemsSource = Variables.BotInfo.TrustedUsers,
		};
		private static Button mTrustedUsersRemoveButton = new Button
		{
			Content = "-",
		};
		private static Grid mTrustedUsersRemoveGrid = new Grid
		{
			Tag = SettingOnBot.TrustedUsers,
		};

		private static Control[] mTitleBoxes = new[] 
		{
			mDarkModeTitle,
			mDownloadUsersTitle,
			mPrefixTitle,
			mBotOwnerTitle,
			mGameTitle,
			mStreamTitle,
			mShardTitle,
			mMessageCacheTitle,
			mLogLevelTitle,
			mTrustedUsersTitle,
			mPlaceholderTitle,
		};
		private static UIElement[] mSettings = new UIElement[] 
		{
			mDarkModeSetting,
			mDownloadUsersSetting,
			mPrefixSetting,
			mBotOwnerSetting,
			mGameSetting,
			mStreamSetting,
			mShardSetting,
			mMessageCacheSetting,
			mLogLevelComboBox,
			mTrustedUsersAddGrid,
			mTrustedUsersRemoveGrid,
		};

		private static Button mSettingsSaveButton = new Button
		{
			Content = "Save Settings"
		};
		#endregion

		#region Edit
		private static Grid mEditLayout = new Grid
		{
			Visibility = Visibility.Collapsed,
		};
		private static Grid mEditButtonLayout = new Grid();
		private static TextEditor mEditBox = new TextEditor
		{
			WordWrap = true,
			VerticalScrollBarVisibility = ScrollBarVisibility.Visible,
			ShowLineNumbers = true,
		};
		private static TextBox mEditSaveBox = new TextBox
		{
			Text = "Successfully saved the file.",
			Visibility = Visibility.Collapsed,
			TextAlignment = TextAlignment.Center,
			VerticalContentAlignment = VerticalAlignment.Center,
			IsReadOnly = true,
		};
		private static Button mEditSaveButton = new Button
		{
			Content = "Save",
		};
		private static Button mEditCloseButton = new Button
		{
			Content = "Close",
		};
		#endregion

		#region Guild Search
		private static Grid mSearchLayout = new Grid
		{
			Background = (Brush)new BrushConverter().ConvertFrom("#BF000000"),
			Visibility = Visibility.Collapsed
		};
		private static Grid mSearchTextLayout = new Grid();

		private static TextBox mSearchPlaceholderTB = new TextBox
		{
			IsReadOnly = true,
		};
		private static Viewbox mNameHeader = UIMakeElement.MakeStandardViewBox("Guild Name:");
		private static TextBox mNameInput = new TextBox
		{
			MaxLength = 100,
			TextWrapping = TextWrapping.Wrap,
		};
		private static Viewbox mIDHeader = UIMakeElement.MakeStandardViewBox("ID:");
		private static TextBox mIDInput = new TextBox
		{
			MaxLength = 18,
			TextWrapping = TextWrapping.Wrap,
		};
		private static ComboBox mFileComboBox = new ComboBox
		{
			VerticalContentAlignment = VerticalAlignment.Center,
			Tag = SettingOnBot.LogLevel,
			ItemsSource = UIMakeElement.MakeComboBoxSourceOutOfEnum(typeof(FileType)),
		};

		private static Button mSearchButton = new Button
		{
			Content = "Search",
		};
		private static Button mSearchCloseButton = new Button
		{
			Content = "Close",
		};
		#endregion

		#region System Info
		private static Grid mSysInfoLayout = new Grid();
		private static TextBox mSysInfoUnder = new TextBox();
		private static Viewbox mLatency = new Viewbox
		{
			Child = UIMakeElement.MakeSysInfoBox(),
		};
		private static Viewbox mMemory = new Viewbox
		{
			Child = UIMakeElement.MakeSysInfoBox(),
		};
		private static Viewbox mThreads = new Viewbox
		{
			Child = UIMakeElement.MakeSysInfoBox(),
		};
		private static Viewbox mGuilds = new Viewbox
		{
			Child = UIMakeElement.MakeSysInfoBox(),
		};
		private static Viewbox mUsers = new Viewbox
		{
			Child = UIMakeElement.MakeSysInfoBox(),
		};
		#endregion

		#region Misc
		private static ToolTip mMemHoverInfo = new ToolTip
		{
			Content = "This is not guaranteed to be 100% correct.",
		};
		private static ToolTip mSaveToolTip = new ToolTip
		{
			Content = "Successfully saved the file.",
		};
		public const double LARGE_TEXT = .275;
		public const double MEDIUM_TEXT = .06;
		public const double SMALL_TEXT = .022;
		public const double TINY_TEXT = .019;
		#endregion

		public BotWindow()
		{
			FontFamily = new FontFamily("Courier New");
			InitializeComponent();
			Loaded += BotWindow_LoadedEvent;
		}
		private void InitializeComponent()
		{
			//Main layout
			UILayoutModification.AddRows(mLayout, 100);
			UILayoutModification.AddCols(mLayout, 4);

			//Output
			UILayoutModification.AddElement(mLayout, mOutputBox, 0, 87, 0, 4);

			//System Info
			UILayoutModification.AddElement(mLayout, mSysInfoLayout, 87, 3, 0, 3, 0, 5);
			UILayoutModification.AddElement(mSysInfoLayout, mSysInfoUnder, 0, 1, 0, 5);
			UILayoutModification.AddElement(mSysInfoLayout, mLatency, 0, 1, 0, 1);
			UILayoutModification.AddElement(mSysInfoLayout, mMemory, 0, 1, 1, 1);
			UILayoutModification.AddElement(mSysInfoLayout, mThreads, 0, 1, 2, 1);
			UILayoutModification.AddElement(mSysInfoLayout, mGuilds, 0, 1, 3, 1);
			UILayoutModification.AddElement(mSysInfoLayout, mUsers, 0, 1, 4, 1);

			//Input
			UILayoutModification.AddElement(mLayout, mInputLayout, 90, 10, 0, 3, 1, 10);
			UILayoutModification.AddElement(mInputLayout, mInputBox, 0, 1, 0, 9);
			UILayoutModification.AddElement(mInputLayout, mInputButton, 0, 1, 9, 1);

			//Buttons
			UILayoutModification.AddElement(mLayout, mButtonLayout, 87, 13, 3, 1, 1, 4);
			UILayoutModification.AddElement(mButtonLayout, mHelpButton, 0, 1, 0, 1);
			UILayoutModification.AddElement(mButtonLayout, mSettingsButton, 0, 1, 1, 1);
			UILayoutModification.AddElement(mButtonLayout, mInfoButton, 0, 1, 2, 1);
			UILayoutModification.AddElement(mButtonLayout, mFileButton, 0, 1, 3, 1);

			//Menu
			UILayoutModification.AddElement(mLayout, mMenuLayout, 0, 87, 3, 1, 100, 1);
			UILayoutModification.AddElement(mMenuLayout, mMenuOutput, 0, 100, 3, 1);
			UILayoutModification.AddElement(mMenuLayout, mFileSearchButton, 96, 4, 3, 1);

			//Settings
			UILayoutModification.AddElement(mLayout, mSettingsLayout, 0, 87, 3, 1, 250, 100);
			UILayoutModification.AddElement(mSettingsLayout, mSettingsPlaceholderTB, 0, 250, 0, 100);
			UILayoutModification.AddCols(mTrustedUsersAddGrid, 10);
			UILayoutModification.AddElement(mTrustedUsersAddGrid, mTrustedUsersAddBox, 0, 1, 0, 9);
			UILayoutModification.AddElement(mTrustedUsersAddGrid, mTrustedUsersAddButton, 0, 1, 9, 1);
			UILayoutModification.AddCols(mTrustedUsersRemoveGrid, 10);
			UILayoutModification.AddElement(mTrustedUsersRemoveGrid, mTrustedUsersComboBox, 0, 1, 0, 9);
			UILayoutModification.AddElement(mTrustedUsersRemoveGrid, mTrustedUsersRemoveButton, 0, 1, 9, 1);
			for (int i = 0; i < mTitleBoxes.Length; i++)
			{
				var title = mTitleBoxes[i];
				dynamic setting = mSettings[i];
				UILayoutModification.AddElement(mSettingsLayout, title, (i * 10), 10, TITLE_START_COLUMN, TITLE_COLUMN_LENGTH);
				UILayoutModification.SetFontSizeProperty(title, SMALL_TEXT);
				UILayoutModification.AddElement(mSettingsLayout, setting, (i * 10), 10, TB_START_COLUMN, TB_COLUMN_LENGTH);
				UILayoutModification.SetFontSizeProperty(setting, SMALL_TEXT);
			}
			UILayoutModification.AddElement(mSettingsLayout, mSettingsSaveButton, 240, 10, 0, 100);

			//Edit
			UILayoutModification.AddElement(mLayout, mEditLayout, 0, 100, 0, 4, 100, 4);
			UILayoutModification.AddElement(mEditLayout, mEditBox, 0, 100, 0, 3);
			UILayoutModification.AddElement(mEditLayout, mEditSaveBox, 84, 3, 3, 1);
			UILayoutModification.AddElement(mEditLayout, mEditButtonLayout, 87, 13, 3, 1, 1, 2);
			UILayoutModification.AddElement(mEditButtonLayout, mEditSaveButton, 0, 1, 0, 1);
			UILayoutModification.AddElement(mEditButtonLayout, mEditCloseButton, 0, 1, 1, 1);

			//Search
			UILayoutModification.AddElement(mLayout, mSearchLayout, 0, 100, 0, 4, 10, 10);
			UILayoutModification.AddElement(mSearchLayout, mSearchTextLayout, 3, 4, 3, 4, 100, 100);
			UILayoutModification.AddElement(mSearchTextLayout, mSearchPlaceholderTB, 0, 100, 0, 100);
			UILayoutModification.AddElement(mSearchTextLayout, mNameHeader, 10, 10, 20, 60);
			UILayoutModification.AddElement(mSearchTextLayout, mNameInput, 20, 15, 20, 60);
			UILayoutModification.AddElement(mSearchTextLayout, mIDHeader, 35, 10, 20, 60);
			UILayoutModification.AddElement(mSearchTextLayout, mIDInput, 45, 10, 20, 60);
			UILayoutModification.AddElement(mSearchTextLayout, mFileComboBox, 57, 10, 20, 60);
			UILayoutModification.AddElement(mSearchTextLayout, mSearchButton, 69, 15, 20, 25);
			UILayoutModification.AddElement(mSearchTextLayout, mSearchCloseButton, 69, 15, 55, 25);

			//Font size properties
			UILayoutModification.SetFontSizeProperties(new Control[] { mInputBox }, LARGE_TEXT);
			UILayoutModification.SetFontSizeProperties(new Control[] { mNameInput, mIDInput }, MEDIUM_TEXT);
			UILayoutModification.SetFontSizeProperties(new Control[] { mEditBox, mEditSaveBox }, SMALL_TEXT);

			//Paragraphs
			mFileParagraph.Inlines.Add(mFileTreeView);

			//Events
			mInputBox.KeyUp += AcceptInput;
			mMemory.MouseEnter += ModifyMemHoverInfo;
			mMemory.MouseLeave += ModifyMemHoverInfo;
			mInputButton.Click += AcceptInput;
			mOutputContextMenuSave.Click += SaveOutput;
			mOutputContextMenuClear.Click += ClearOutput;
			mTrustedUsersRemoveButton.Click += RemoveTrustedUser;
			mTrustedUsersAddButton.Click += AddTrustedUser;
			mHelpButton.Click += BringUpMenu;
			mSettingsButton.Click += BringUpMenu;
			mInfoButton.Click += BringUpMenu;
			mFileButton.Click += BringUpMenu;
			mEditCloseButton.Click += CloseEditScreen;
			mEditSaveButton.Click += SaveEditScreen;
			mSettingsSaveButton.Click += SaveSettings;
			mFileSearchButton.Click += BringUpFileSearch;
			mSearchButton.Click += FileSearch;
			mSearchCloseButton.Click += CloseSearch;

			//Set this panel as the content for this window and run the application
			Content = mLayout;
			RunApplication();
		}
		private void RunApplication()
		{
			//Make console output show on the output text block and box
			Console.SetOut(new UITextBoxStreamWriter(mOutputBox));

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

			//Make sure the system information stays updated
			UpdateSystemInformation();
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
				mInfoParagraph.Inlines.Clear();
				mInfoParagraph.Inlines.Add(new Run(Actions.FormatLoggedThings(true) + "\n\nCharacter Count: ~540,000\nLine Count: ~15,500"));
			};
			timer.Start();
		}
		private void BotWindow_LoadedEvent(object sender, RoutedEventArgs e)
		{
			SwitchColorMode();
		}

		private void AcceptInput(object sender, KeyEventArgs e)
		{
			var text = mInputBox.Text;
			if (String.IsNullOrWhiteSpace(text))
			{
				mInputButton.IsEnabled = false;
				return;
			}
			else
			{
				if (e.Key.Equals(Key.Enter) || e.Key.Equals(Key.Return))
				{
					UICommandHandler.GatherInput();
				}
				else
				{
					mInputButton.IsEnabled = true;
				}
			}
		}
		private void AcceptInput(object sender, RoutedEventArgs e)
		{
			UICommandHandler.GatherInput();
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
			using (FileStream stream = new FileStream(path, FileMode.Create))
			{
				new TextRange(mOutputBox.Document.ContentStart, mOutputBox.Document.ContentEnd).Save(stream, DataFormats.Text, true);
			}

			//Write to the console telling the user that the console log was successfully saved
			Actions.WriteLine("Successfully saved the output log.");
		}
		private void ClearOutput(object sender, RoutedEventArgs e)
		{
			var result = MessageBox.Show("Are you sure you want to clear the output window?", Variables.BotName, MessageBoxButton.OKCancel);

			switch (result)
			{
				case MessageBoxResult.OK:
				{
					mOutputBox.Document.Blocks.Clear();
					break;
				}
			}
		}

		private void BringUpMenu(object sender, RoutedEventArgs e)
		{
			//Make sure everything is loaded first
			if (!Variables.Loaded)
				return;

			var name = (sender as Button).Content.ToString();

			//Hide everything so stuff doesn't overlap
			mMenuOutput.Document.Blocks.Clear();
			mMenuLayout.Visibility = Visibility.Collapsed;
			mSettingsLayout.Visibility = Visibility.Collapsed;
			mFileSearchButton.Visibility = Visibility.Collapsed;

			//If clicking the same button then resize the output window to the regular size
			if (Actions.CaseInsEquals(name, mLastButtonClicked))
			{
				UILayoutModification.SetColAndSpan(mOutputBox, 0, 4);
				mLastButtonClicked = null;
			}
			else
			{
				//Resize the regular output window and have the menubox appear
				UILayoutModification.SetColAndSpan(mOutputBox, 0, 3);
				mLastButtonClicked = name;

				//Show the text for help
				if (Actions.CaseInsEquals(name, mHelpButton.Content.ToString()))
				{
					UILayoutModification.SetFontSizeProperty(mMenuOutput, TINY_TEXT);
					mMenuOutput.Document.Blocks.Add(mHelpParagraph);
					mMenuLayout.Visibility = Visibility.Visible;
				}
				//Show the text for settings
				else if (Actions.CaseInsEquals(name, mSettingsButton.Content.ToString()))
				{
					UpdateSettingsWhenOpened();
					mSettingsLayout.Visibility = Visibility.Visible;
				}
				//Show the text for info
				else if (Actions.CaseInsEquals(name, mInfoButton.Content.ToString()))
				{
					UILayoutModification.SetFontSizeProperty(mMenuOutput, SMALL_TEXT);
					mMenuOutput.Document.Blocks.Add(mInfoParagraph);
					mMenuLayout.Visibility = Visibility.Visible;
				}
				//Show the text for files
				else if (Actions.CaseInsEquals(name, mFileButton.Content.ToString()))
				{
					mFileParagraph.Inlines.Clear();
					mFileParagraph.Inlines.Add(mFileTreeView = UIMakeElement.MakeGuildTreeView());
					mFileSearchButton.Visibility = Visibility.Visible;

					UILayoutModification.SetFontSizeProperty(mMenuOutput, SMALL_TEXT);
					mMenuOutput.Document.Blocks.Add(mFileParagraph);
					mMenuLayout.Visibility = Visibility.Visible;
				}
			}
		}
		private void ModifyMemHoverInfo(object sender, RoutedEventArgs e)
		{
			UILayoutModification.ToggleToolTip(mMemHoverInfo);
		}
		private void RemoveTrustedUser(object sender, RoutedEventArgs e)
		{
			if (mTrustedUsersComboBox.SelectedItem == null)
				return;

			var userID = (ulong)mTrustedUsersComboBox.SelectedItem;
			var curr = mTrustedUsersComboBox.Items.Cast<ulong>().ToList();
			if (!curr.Contains(userID))
				return;

			curr.Remove(userID);
			mTrustedUsersComboBox.ItemsSource = curr.Distinct();
		}
		private void AddTrustedUser(object sender, RoutedEventArgs e)
		{
			var text = mTrustedUsersAddBox.Text;
			mTrustedUsersAddBox.Text = "";

			if (ulong.TryParse(text, out ulong userID))
			{
				var curr = mTrustedUsersComboBox.Items.Cast<ulong>().ToList();
				if (curr.Contains(userID))
					return;

				curr.Add(userID);
				mTrustedUsersComboBox.ItemsSource = curr.Distinct();
			}
			else
			{
				Actions.WriteLine(String.Format("The given input '{0}' is not a valid ID.", text));
			}
		}

		private void CloseSearch(object sender, RoutedEventArgs e)
		{
			HideSearchMenu();
		}
		private void BringUpFileSearch(object sender, RoutedEventArgs e)
		{
			mSearchLayout.Visibility = Visibility.Visible;
		}
		private void FileSearch(object sender, RoutedEventArgs e)
		{
			var tb = (TextBox)mFileComboBox.SelectedItem;
			if (tb == null)
				return;

			var nameStr = mNameInput.Text;
			var idStr = mIDInput.Text;
			if (String.IsNullOrWhiteSpace(nameStr) && String.IsNullOrWhiteSpace(idStr))
				return;

			var fileType = (FileType)tb.Tag;
			HideSearchMenu();

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
					if (BringUpEditLayout(item))
					{
						mEditLayout.Visibility = Visibility.Visible;
						mFileSearchButton.Visibility = Visibility.Collapsed;
						return;
					}
				}

				Actions.WriteLine("Unable to bring up the file.");
			}
		}
		private void HideSearchMenu()
		{
			mFileComboBox.SelectedItem = null;
			mNameInput.Text = "";
			mIDInput.Text = "";
			mSearchLayout.Visibility = Visibility.Collapsed;
		}

		private void CloseEditScreen(object sender, RoutedEventArgs e)
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
		private void SaveEditScreen(object sender, RoutedEventArgs e)
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
						var throwaway = Newtonsoft.Json.JsonConvert.DeserializeObject<BotGuildInfo>(mEditBox.Text);
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
				UILayoutModification.ToggleAndUntoggleUIEle(mEditSaveBox);
			}
		}
		private static bool BringUpEditLayout(TreeViewItem treeItem)
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
		public static void GuildFilesDoubleClick(object sender, RoutedEventArgs e)
		{
			//Get the double clicked item
			var treeItem = sender as TreeViewItem;
			if (treeItem == null)
				return;

			if (BringUpEditLayout(treeItem))
			{
				mEditLayout.Visibility = Visibility.Visible;
				mFileSearchButton.Visibility = Visibility.Collapsed;
			}
		}

		public static void SwitchColorMode()
		{
			UIColors.SwitchActive(Variables.BotInfo.DarkMode);
			var elements = GetAllElements();
			foreach (var element in elements)
			{
				UILayoutModification.SwitchColorMode((dynamic)element);
			}
		}
		public static List<DependencyObject> GetAllElements()
		{
			var elements = new List<DependencyObject>();
			var currentElements = mLayout.Children.Cast<DependencyObject>().ToList();
			while (true)
			{
				currentElements.RemoveAll(x => x == null);
				var tempArray = currentElements.ToArray();
				if (tempArray.Length == 0)
					break;

				currentElements.Clear();
				for (int i = 0; i < tempArray.Length; i++)
				{
					var tempEle = tempArray[i];
					elements.Add(tempEle);

					var childrenCount = VisualTreeHelper.GetChildrenCount(tempEle);
					if (childrenCount == 0)
					{
						continue;
					}

					for (int c = 0; c < childrenCount; c++)
					{
						currentElements.Add(VisualTreeHelper.GetChild(tempEle, c) as DependencyObject);
					}
				}
			}
			return elements.Distinct().ToList();
		}

		private void UpdateSettingsWhenOpened()
		{
			var botInfo = Variables.BotInfo;
			((CheckBox)mDarkModeSetting.Child).IsChecked = botInfo.DarkMode;
			((CheckBox)mDownloadUsersSetting.Child).IsChecked = botInfo.AlwaysDownloadUsers;
			mPrefixSetting.Text = botInfo.Prefix;
			mBotOwnerSetting.Text = botInfo.BotOwner.ToString();
			mGameSetting.Text = botInfo.Game;
			mStreamSetting.Text = botInfo.Stream;
			mShardSetting.Text = botInfo.ShardCount.ToString();
			mMessageCacheSetting.Text = botInfo.MessageCacheSize.ToString();
			mLogLevelComboBox.SelectedItem = GetSelectedLogLevel();
		}
		private void SaveSettings(object sender, RoutedEventArgs e)
		{
			var botInfo = Variables.BotInfo;
			var success = new List<string>();
			var failure = new List<string>();

			//Go through each setting and update them
			foreach (dynamic ele in mSettings)
			{
				SettingOnBot setting;
				try
				{
					setting = (SettingOnBot)ele.Tag;
				}
				catch
				{
					continue;
				}

				ReturnedSetting response = SaveSetting(ele, setting, botInfo);
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
				Actions.UpdateGame().Forget();
				Actions.SaveBotInfo();
			}
			if (failure.Any())
			{
				Actions.WriteLine(String.Format("Failed to save: {0}", String.Join(", ", failure)));
			}
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
			if (Actions.CaseInsEquals(botInfo.GetSetting(setting), text))
				return new ReturnedSetting(setting, NSF.Nothing);

			switch (setting)
			{
				case SettingOnBot.Prefix:
				{
					botInfo.SetPrefix(text);
					return new ReturnedSetting(setting, NSF.Success);
				}
				case SettingOnBot.BotOwner:
				{
					if (ulong.TryParse(text, out ulong id))
					{
						botInfo.SetBotOwner(id);
						return new ReturnedSetting(setting, NSF.Success);
					}
					return new ReturnedSetting(setting, NSF.Failure);
				}
				case SettingOnBot.Game:
				{
					botInfo.SetGame(text);
					return new ReturnedSetting(setting, NSF.Success);
				}
				case SettingOnBot.Stream:
				{
					botInfo.SetStream(text);
					return new ReturnedSetting(setting, NSF.Success);
				}
				case SettingOnBot.ShardCount:
				{
					if (int.TryParse(text, out int shardCount))
					{
						botInfo.SetShardCount(shardCount);
						return new ReturnedSetting(setting, NSF.Success);
					}
					return new ReturnedSetting(setting, NSF.Failure);
				}
				case SettingOnBot.MessageCacheSize:
				{
					if (int.TryParse(text, out int cacheSize))
					{
						botInfo.SetCacheSize(cacheSize);
						return new ReturnedSetting(setting, NSF.Success);
					}
					return new ReturnedSetting(setting, NSF.Failure);
				}
			}
			return new ReturnedSetting(setting, NSF.Nothing);
		}
		private ReturnedSetting SaveSetting(Viewbox vb, SettingOnBot setting, BotGlobalInfo botInfo)
		{
			var child = (dynamic)vb.Child;
			return SaveSetting(child, setting, botInfo);
		}
		private ReturnedSetting SaveSetting (CheckBox cb, SettingOnBot setting, BotGlobalInfo botInfo)
		{
			if (!cb.IsChecked.HasValue)
				return new ReturnedSetting(setting, NSF.Nothing);

			switch (setting)
			{
				case SettingOnBot.AlwaysDownloadUsers:
				{
					if (cb.IsChecked.Value != botInfo.AlwaysDownloadUsers)
					{
						botInfo.SetAlwaysDownloadUsers(!botInfo.AlwaysDownloadUsers);
						return new ReturnedSetting(setting, NSF.Success);
					}
					break;
				}
				case SettingOnBot.DarkMode:
				{
					if (cb.IsChecked.Value != botInfo.DarkMode)
					{
						botInfo.SetDarkMode(!botInfo.DarkMode);
						SwitchColorMode();
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
					var logLevel = (Discord.LogSeverity)((TextBox)cb.SelectedItem).Tag;
					if (logLevel != botInfo.LogLevel)
					{
						botInfo.SetLogLevel(logLevel);
						return new ReturnedSetting(setting, NSF.Success);
					}
					break;
				}
				case SettingOnBot.TrustedUsers:
				{
					var trustedUsers = cb.Items.Cast<ulong>().ToList();
					var diffUsers = botInfo.TrustedUsers.Except(trustedUsers);
					if (trustedUsers.Count != botInfo.TrustedUsers.Count || diffUsers.Any())
					{
						botInfo.SetTrustedUsers(trustedUsers);
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
		private static TextBox GetSelectedLogLevel()
		{
			return mLogLevelComboBox.Items.OfType<TextBox>().FirstOrDefault(x => (Discord.LogSeverity)x.Tag == Variables.BotInfo.LogLevel);
		}

		public static RichTextBox Output { get { return mOutputBox; } }
		public static RichTextBox Menu { get { return mMenuOutput; } }
		public static TextBox Input { get { return mInputBox; } }
		public static Button InputButton { get { return mInputButton; } }
	}

	//For darkmode or not
	public class UIColors
	{
		private static readonly Brush DarkModeBackground = (Brush)new BrushConverter().ConvertFrom("#1C1C1C");
		private static readonly Brush DarkModeForeground = (Brush)new BrushConverter().ConvertFrom("#9E9E9E");
		private static readonly Brush DarkModeBorder = (Brush)new BrushConverter().ConvertFrom("#FF000000"); //TODO:
		private static readonly Brush LightModeBackground = (Brush)new BrushConverter().ConvertFrom("#FFFFFFFF");
		private static readonly Brush LightModeForeground = (Brush)new BrushConverter().ConvertFrom("#FF000000");
		private static readonly Brush LightModeBorder = (Brush)new BrushConverter().ConvertFrom("#FF000000"); //TODO:

		private static Brush mActiveBackground = Variables.BotInfo.DarkMode ? DarkModeBackground : LightModeBackground;
		private static Brush mActiveForeground = Variables.BotInfo.DarkMode ? DarkModeForeground : LightModeForeground;
		private static Brush mActiveBorder = Variables.BotInfo.DarkMode ? DarkModeBorder : LightModeBorder;
		private static Brush mDisabledBackground = Variables.BotInfo.DarkMode ? LightModeBackground : DarkModeBackground;
		private static Brush mDisabledForeground = Variables.BotInfo.DarkMode ? LightModeForeground : DarkModeForeground;
		private static Brush mDisabledBorder = Variables.BotInfo.DarkMode ? LightModeBorder : DarkModeBorder;

		public static void SwitchActive(bool darkModeActive)
		{
			if (darkModeActive)
			{
				mActiveBackground = DarkModeBackground;
				mActiveForeground = DarkModeForeground;
				mActiveBorder = DarkModeBorder;
				mDisabledBackground = LightModeBackground;
				mDisabledForeground = LightModeForeground;
				mDisabledBorder = LightModeBorder;
			}
			else
			{
				mActiveBackground = LightModeBackground;
				mActiveForeground = LightModeForeground;
				mActiveBorder = LightModeBorder;
				mDisabledBackground = DarkModeBackground;
				mDisabledForeground = DarkModeForeground;
				mDisabledBorder = DarkModeBorder;
			}
		}
		public static Brush ActiveBackground
		{
			get { return mActiveBackground; }
		}
		public static Brush ActiveForeground
		{
			get { return mActiveForeground; }
		}
		public static Brush ActiveBorder
		{
			get { return mActiveBorder; }
		}
		public static Brush DisabledBackground
		{
			get { return mDisabledBackground; }
		}
		public static Brush DisabledForeground
		{
			get { return mDisabledForeground; }
		}
		public static Brush DisabledBorder
		{
			get { return mDisabledBorder; }
		}
	}

	//Modify the UI
	public class UILayoutModification
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

		public static void SwitchColorMode(Control element)
		{
			var eleBackground = element?.Background as SolidColorBrush;
			if (eleBackground == null || CheckIfSameBrush(eleBackground, UIColors.DisabledBackground))
			{
				element.Background = UIColors.ActiveBackground;
			}
			var eleForeground = element?.Foreground as SolidColorBrush;
			if (eleForeground == null || CheckIfSameBrush(eleForeground, UIColors.DisabledForeground))
			{
				element.Foreground = UIColors.ActiveForeground;
			}
		}
		public static void SwitchColorMode(Grid element) { }
		public static void SwitchColorMode(Viewbox element) { }
		public static void SwitchColorMode(CheckBox element) { }
		public static void SwitchColorMode(Button element) { }
		public static void SwitchColorMode(object element) { }
		private static bool CheckIfSameBrush(Brush firstBrush, Brush secondBrush)
		{
			if (firstBrush == null)
			{
				return secondBrush == null;
			}
			else if (secondBrush == null)
			{
				return false;
			}

			var firstColor = ((SolidColorBrush)firstBrush).Color;
			var secondColor = ((SolidColorBrush)secondBrush).Color;

			var a = firstColor.A == secondColor.A;
			var r = firstColor.R == secondColor.R;
			var g = firstColor.G == secondColor.G;
			var b = firstColor.B == secondColor.B;
			return a && r && g && b;
		}

		public static void SetFontSizeProperties(IEnumerable<Control> elements, double size)
		{
			foreach (var ele in elements)
			{
				SetFontSizeProperty(ele, size);
			}
		}
		public static void SetFontSizeProperty(Control element, double size)
		{
			element.SetBinding(Control.FontSizeProperty, CreateBinding(size));
		}
		public static void SetFontSizeProperty(Grid element, double size)
		{
			var children = element.Children;
			foreach (var child in children)
			{
				SetFontSizeProperty((dynamic)child, size);
			}
		}
		public static void SetFontSizeProperty(object element, double size)
		{
			return;
		}
		private static Binding CreateBinding(double val)
		{
			return new Binding
			{
				Path = new PropertyPath("ActualHeight"),
				RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Grid), 1),
				Converter = new UIFontResizer(val),
			};
		}

		public static void ToggleToolTip(ToolTip ttip)
		{
			ttip.IsOpen = !ttip.IsOpen;
		}
		public static void ToggleUIElement(UIElement ele)
		{
			ele.Visibility = ele.Visibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
		}
		public static void ToggleAndUntoggleUIEle(UIElement ele)
		{
			ele.Dispatcher.InvokeAsync(async () =>
			{
				ToggleUIElement(ele);
				await Task.Delay(2500);
				ToggleUIElement(ele);
			});
		}

		public static void AddHyperlink(RichTextBox output, string link, string name, string beforeText = null, string afterText = null)
		{
			//Create the hyperlink
			var hyperlink = UIMakeElement.MakeHyperlink(link, name);
			if (hyperlink == null)
			{
				return;
			}
			//Check if the paragraph is valid
			var para = BotWindow.Output.Document.Blocks.LastBlock as Paragraph;
			if (para == null)
			{
				Actions.WriteLine(link);
				return;
			}
			//Format the text before the hyperlink
			if (String.IsNullOrWhiteSpace(beforeText))
			{
				para.Inlines.Add(new Run(DateTime.Now.ToString("HH:mm:ss") + ": "));
			}
			else
			{
				para.Inlines.Add(new Run(beforeText));
			}
			//Add in the hyperlink
			para.Inlines.Add(hyperlink);
			//Format the text after the hyperlink
			if (String.IsNullOrWhiteSpace(beforeText))
			{
				para.Inlines.Add(new Run("\r"));
			}
			else
			{
				para.Inlines.Add(new Run(afterText));
			}
			//Add the paragraph to the ouput
			output.Document.Blocks.Add(para);
		}
	}

	//Make certain elements
	public class UIMakeElement
	{
		public static IEnumerable<TextBox> MakeComboBoxSourceOutOfEnum(Type type)
		{
			var values = Enum.GetValues(type);
			var tbs = new List<TextBox>();
			foreach (var value in values)
			{
				var tempTB = new TextBox
				{
					Text = Enum.GetName(type, value),
					Tag = value,
					IsReadOnly = true,
					IsHitTestVisible = false,
					BorderThickness = new Thickness(0),
					Background = Brushes.Transparent,
				};
				tbs.Add(tempTB);
			}
			return tbs;
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

		public static TreeView MakeGuildTreeView()
		{
			//Get the directory
			var directory = Actions.GetBaseBotDirectory();
			if (directory == null || !Directory.Exists(directory))
				return null;

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
						Background = UIColors.ActiveBackground,
						Foreground = UIColors.ActiveForeground,
					};
					fileItem.MouseDoubleClick += BotWindow.GuildFilesDoubleClick;
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
					Background = UIColors.ActiveBackground,
					Foreground = UIColors.ActiveForeground,
				};
				listOfFiles.ForEach(x =>
				{
					guildItem.Items.Add(x);
				});
				
				guildItems.Add(guildItem);
			});

			return new TreeView
			{
				ItemsSource = guildItems.OrderBy(x => ((GuildFileInformation)x.Tag).MemberCount).Reverse(),
				BorderThickness = new Thickness(0),
				Background = UIColors.ActiveBackground,
				Foreground = UIColors.ActiveForeground,
			};
		}

		public static Paragraph MakeHelpParagraph()
		{
			var cmd = String.Format("{0}Aliases:\n{1}", "Commands:".PadRight(Constants.PAD_RIGHT), UICommandNames.FormatStringForUse());
			var syntax = "\nCommand Syntax:\n\t[] means required\n\t<> means optional\n\t| means or";
			var defs1 = "\nLatency:\n\tTime it takes for a command to reach the bot.\nMemory:\n\tAmount of RAM the program is using.\n\t(This is wrong most of the time.)";
			var defs2 = "Threads:\n\tWhere all the actions in the bot happen.\nShards:\n\tHold all the guilds a bot has on its client.\n\tThere is a limit of 2500 guilds per shard.";
			var vers = String.Format("\nAPI Wrapper Version: {0}\nBot Version: {1}\nGitHub Repository: ", Constants.API_VERSION, Constants.BOT_VERSION);
			var help = "\n\nNeed additional help? Join the Discord server: ";
			var all = String.Join("\n", cmd, syntax, defs1, defs2, vers);

			var temp = new Paragraph();
			temp.Inlines.Add(new Run(all));
			temp.Inlines.Add(MakeHyperlink(Constants.REPO, "Advobot"));
			temp.Inlines.Add(new Run(help));
			temp.Inlines.Add(MakeHyperlink(Constants.DISCORD_INV, "Here"));

			return temp;
		}

		public static Viewbox MakeStandardViewBox(string text)
		{
			return new Viewbox
			{
				Child = new TextBox
				{
					Text = text,
					VerticalAlignment = VerticalAlignment.Bottom,
					IsReadOnly = true,
					BorderThickness = new Thickness(0)
				},
				HorizontalAlignment = HorizontalAlignment.Left
			};
		}

		public static TextBox MakeTitle(string text)
		{
			return new TextBox
			{
				Text = text,
				IsReadOnly = true,
				BorderThickness = new Thickness(0),
				VerticalAlignment = VerticalAlignment.Center
			};
		}

		public static TextBox MakeSetting(SettingOnBot setting, int length)
		{
			return new TextBox
			{
				VerticalContentAlignment = VerticalAlignment.Center,
				Tag = setting,
				MaxLength = length
			};
		}

		public static TextBox MakeSysInfoBox()
		{
			return new TextBox
			{
				IsReadOnly = true,
				BorderThickness = new Thickness(0, 1, 0, 1),
				Background = null,
			};
		}
	}

	//New class to handle commands
	public class UICommandHandler
	{
		public static void GatherInput()
		{
			//Get the current text
			var text = BotWindow.Input.Text.Trim(new[] { '\r', '\n' });
			BotWindow.Input.Text = "";
			BotWindow.InputButton.IsEnabled = false;
			Console.WriteLine(text);
			//Make sure both the path and key are set
			if (!Variables.GotPath || !Variables.GotKey)
			{
				Task.Run(async () =>
				{
					if (!Variables.GotPath)
					{
						Variables.GotPath = Actions.ValidatePath(text);
						Variables.GotKey = Variables.GotPath && await Actions.ValidateBotKey(Variables.Client, Properties.Settings.Default.BotKey, true);
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
			if (Actions.CaseInsStartsWith(input, Variables.BotInfo.Prefix))
			{
				//Remove the prefix
				input = input.Substring(Variables.BotInfo.Prefix.Length);
				//Split the input
				var inputArray = input.Split(new[] { ' ' }, 2);
				//Get the command
				var cmd = inputArray[0];
				//Get the args
				var args = inputArray.Length > 1 ? inputArray[1] : null;
				//Find the command with the given name
				if (FindCommand(cmd, args))
					return;
				//If no command, give an error message
				Actions.WriteLine("No command could be found with that name.");
			}
		}

		public static bool FindCommand(string cmd, string args)
		{
			//Find what command it belongs to
			if (UICommandNames.GetNameAndAliases(UICommandEnum.Pause).CaseInsContains(cmd))
			{
				UICommands.PAUSE(args);
			}
			else if (UICommandNames.GetNameAndAliases(UICommandEnum.ListGuilds).CaseInsContains(cmd))
			{
				UICommands.UIListGuilds();
			}
			else if (Actions.CaseInsEquals(cmd, "test"))
			{
				UICommands.UITest();
			}
			else
			{
				return false;
			}
			return true;
		}
	}

	//Commands the bot can do through the 'console'
	public class UICommands
	{
		public static void PAUSE(string input)
		{
			if (Variables.Pause)
			{
				Variables.Pause = false;
				Actions.WriteLine("Successfully unpaused the bot.");
			}
			else
			{
				Variables.Pause = true;
				Actions.WriteLine("Successfully paused the bot.");
			}
		}

		public static void UIListGuilds()
		{
			var guilds = Variables.Client.GetGuilds().ToList();
			var countWidth = guilds.Count.ToString().Length;
			var count = 1;
			guilds.ForEach(x =>
			{
				Actions.WriteLine(String.Format("{0}. {1} Owner: {2}", count++.ToString().PadLeft(countWidth, '0'), x.FormatGuild(), x.Owner.FormatUser()));
			});
		}

		public static void UITest()
		{
#if DEBUG
			var programLoc = System.Reflection.Assembly.GetExecutingAssembly().Location;
			var newPath = Path.GetFullPath(Path.Combine(programLoc, @"..\..\..\"));
			var totalChars = 0;
			var totalLines = 0;
			foreach (var file in Directory.GetFiles(newPath))
			{
				if (Actions.CaseInsEquals(Path.GetExtension(file), ".cs"))
				{
					totalChars += File.ReadAllText(file).Length;
					totalLines += File.ReadAllLines(file).Count();
				}
			}
			Actions.WriteLine(String.Format("Current Totals:{0}\t\t\t Chars: {1}{0}\t\t\t Lines: {2}", Environment.NewLine, totalChars, totalLines));
#endif
		}
	}

	//Write the console output into the UI
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
			else
			{
				mCurrentLineText += value;
			}
		}

		public override void Write(string value)
		{
			if (mIgnoreNewLines && value.Equals('\n'))
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

	//Resize font
	public class UIFontResizer : IValueConverter
	{
		double convertFactor;
		public UIFontResizer(double convertFactor)
		{
			this.convertFactor = convertFactor;
		}

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return Math.Max((int)(System.Convert.ToDouble(value) * convertFactor), -1);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
