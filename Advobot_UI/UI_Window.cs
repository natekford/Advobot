using Advobot.Actions;
using Advobot.Enums;
using Advobot.Graphics.Colors;
using Advobot.Graphics.HelperFunctions;
using Advobot.Interfaces;
using Advobot.Structs;
using Discord;
using ICSharpCode.AvalonEdit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

//This entire file *should* be able to be removed from the solution with no adverse effects aside from the single error in Advobot_Program.cs
namespace Advobot
{
	namespace Graphics
	{
		namespace UserInterface
		{
			//Trying to split this up into separate classes = worse than kicking a wall with toothpicks under your toenail
			//All I did instead was remove most logic from it into a separate class. Shortened from about 1300 lines to 800.
			public class MyWindow : Window
			{
				private readonly IDiscordClient _Client;
				private readonly IBotSettings _BotSettings;
				private readonly ILogModule _Logging;
				private readonly UISettings _UISettings;

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
				private readonly Button _PauseButton = new MyButton { Content = "Pause", };
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

				#region System Info
				private readonly Grid _SysInfoLayout = new Grid();
				private readonly TextBox _SysInfoUnder = new MyTextBox { IsReadOnly = true, };
				private readonly Viewbox _Latency = new Viewbox { Child = UIModification.MakeSysInfoBox(), };
				private readonly Viewbox _Memory = new Viewbox { Child = UIModification.MakeSysInfoBox(), };
				private readonly Viewbox _Threads = new Viewbox { Child = UIModification.MakeSysInfoBox(), };
				private readonly Viewbox _Guilds = new Viewbox { Child = UIModification.MakeSysInfoBox(), };
				private readonly Viewbox _Users = new Viewbox { Child = UIModification.MakeSysInfoBox(), };
				#endregion

				public MyWindow(IServiceProvider provider)
				{
					FontFamily = new FontFamily("Courier New");

					_Client = (IDiscordClient)provider.GetService(typeof(IDiscordClient));
					_BotSettings = (IBotSettings)provider.GetService(typeof(IBotSettings));
					_Logging = (ILogModule)provider.GetService(typeof(ILogModule));
					_UISettings = UISettings.LoadUISettings(_BotSettings.Loaded);

					InitializeComponents();
					Loaded += RunApplication;
				}
				private void InitializeComponents()
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
					UIModification.AddElement(_Layout, _ButtonLayout, 87, 13, 3, 1, 2, 4);
					UIModification.AddElement(_ButtonLayout, _MainButton, 0, 2, 0, 1);
					UIModification.AddElement(_ButtonLayout, _InfoButton, 0, 2, 1, 1);
					UIModification.AddElement(_ButtonLayout, _FileButton, 0, 2, 2, 1);
					UIModification.AddElement(_ButtonLayout, _SettingsButton, 0, 1, 3, 1);
					UIModification.AddElement(_ButtonLayout, _ColorsButton, 1, 1, 3, 1);

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

					//Specific File
					UIModification.AddElement(_Layout, _SpecificFileLayout, 0, 100, 0, 4, 100, 4);
					UIModification.AddElement(_SpecificFileLayout, _SpecificFileDisplay, 0, 100, 0, 3);
					UIModification.AddElement(_SpecificFileLayout, _SpecificFileCloseButton, 95, 5, 3, 1);

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

					UIModification.SetFontSizeProperties(.275, new UIElement[] { _Input, });
					UIModification.SetFontSizeProperties(.060, new UIElement[] { _GuildSearchNameInput, _GuildSearchIDInput, });
					UIModification.SetFontSizeProperties(.035, new UIElement[] { _InfoOutput, });
					UIModification.SetFontSizeProperties(.022, new UIElement[] { _SpecificFileDisplay, _FileOutput, _OutputSearchComboBox, });
					UIModification.SetFontSizeProperties(.018, new UIElement[] { _MainMenuOutput, }, _Settings.Select(x => x.Title), _Settings.Select(x => x.Setting));

					_Output.ContextMenu = new ContextMenu { ItemsSource = new[] { _OutputContextMenuSearch, _OutputContextMenuSave, _OutputContextMenuClear }, };
					_SpecificFileDisplay.ContextMenu = new ContextMenu { ItemsSource = new[] { _SpecificFileContextMenuSave }, };

					HookUpEvents();

					//Set this panel as the content for this window and run the application
					this.Content = _Layout;
					this.WindowState = WindowState.Maximized;
				}
				private void RunApplication(object sender, RoutedEventArgs e)
				{
					//Make console output show on the output text block and box
					Console.SetOut(new UITextBoxStreamWriter(_Output));

					Task.Run(async () =>
					{
						if (SavingAndLoadingActions.ValidatePath(GetActions.GetSavePath(), _BotSettings.Windows, true))
						{
							_BotSettings.SetGotPath();
						}
						if (await SavingAndLoadingActions.ValidateBotKey(_Client, GetActions.GetBotKey(), true))
						{
							_BotSettings.SetGotKey();
						}
						await ClientActions.MaybeStartBot(_Client, _BotSettings);
					});

					_UISettings.ActivateTheme();
					UIModification.SetColorMode(_Layout);

					var timer = new DispatcherTimer() { Interval = new TimeSpan(0, 0, 0, 0, 500) };
					timer.Tick += async (s, ea) =>
					{
						var guilds = await _Client.GetGuildsAsync();

						IEnumerable<ulong> userIDs = new List<ulong>();
						foreach (var guild in guilds)
						{
							userIDs = (await guild.GetUsersAsync()).Select(x => x.Id);
						}

						((TextBox)_Latency.Child).Text = String.Format("Latency: {0}ms", ClientActions.GetLatency(_Client));
						((TextBox)_Memory.Child).Text = String.Format("Memory: {0}MB", GetActions.GetMemory(_BotSettings.Windows).ToString("0.00"));
						((TextBox)_Threads.Child).Text = String.Format("Threads: {0}", Process.GetCurrentProcess().Threads.Count);
						((TextBox)_Guilds.Child).Text = String.Format("Guilds: {0}", guilds.Count);
						((TextBox)_Users.Child).Text = String.Format("Members: {0}", userIDs.Distinct().Count());
						_InfoOutput.Document = UIModification.MakeInfoMenu(GetActions.GetUptime(_BotSettings), _Logging.FormatLoggedCommands(), _Logging.FormatLoggedActions());
					};
					timer.Start();
				}

				private void HookUpEvents()
				{
					//Bot status
					_PauseButton.Click								+= Pause;
					_RestartButton.Click							+= Restart;
					_DisconnectButton.Click							+= Disconnect;

					//Settings
					_SettingsSaveButton.Click						+= SaveSettings;
					_ColorsSaveButton.Click							+= SaveColors;
					_TrustedUsersRemoveButton.Click					+= RemoveTrustedUser;
					_TrustedUsersAddButton.Click					+= AddTrustedUser;

					//Input
					_Input.KeyUp									+= AcceptInput;
					_InputButton.Click								+= AcceptInput;

					//Output
					_OutputContextMenuSave.Click					+= SaveOutput;
					_OutputContextMenuClear.Click					+= ClearOutput;
					_OutputContextMenuSearch.Click					+= OpenOutputSearch;

					//Output search
					_OutputSearchCloseButton.Click					+= CloseOutputSearch;
					_OutputSearchButton.Click						+= SearchOutput;

					//File
					_FileSearchButton.Click							+= OpenFileSearch;
					_GuildSearchSearchButton.Click					+= SearchForFile;
					_GuildSearchCloseButton.Click					+= CloseFileSearch;

					//Specific file
					_SpecificFileCloseButton.Click					+= CloseSpecificFileLayout;
					_SpecificFileContextMenuSave.Click				+= SaveSpecificFile;

					//Menu
					_MainButton.Click								+= OpenMenu;
					_SettingsButton.Click							+= OpenMenu;
					_ColorsButton.Click								+= OpenMenu;
					_InfoButton.Click								+= OpenMenu;
					_FileButton.Click								+= OpenMenu;
				}
				private void Pause(object sender, RoutedEventArgs e)
				{
					UIBotWindowLogic.PauseBot(_BotSettings);
				}
				private void Restart(object sender, RoutedEventArgs e)
				{
					switch (MessageBox.Show("Are you sure you want to restart the bot?", Constants.PROGRAM_NAME, MessageBoxButton.OKCancel))
					{
						case MessageBoxResult.OK:
						{
							MiscActions.RestartBot();
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
							MiscActions.DisconnectBot();
							return;
						}
					}
				}

				private async void SaveSettings(object sender, RoutedEventArgs e)
				{
					await UIBotWindowLogic.SaveSettings(_Layout, _Client, _BotSettings);
				}
				private void SaveColors(object sender, RoutedEventArgs e)
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

							if (!UIModification.CheckIfTwoBrushesAreTheSame(_UISettings.ColorTargets[target], brush))
							{
								_UISettings.ColorTargets[target] = brush;
								castedChild.Text = UIModification.FormatBrush(brush);
								ConsoleActions.WriteLine(String.Format("Successfully updated the color for {0}.", target.EnumName()));
							}
						}
						else if (child is ComboBox)
						{
							var selected = ((ComboBox)child).SelectedItem as MyTextBox;
							var tag = selected?.Tag as ColorTheme?;
							if (!tag.HasValue || tag == _UISettings.Theme)
								continue;

							_UISettings.SetTheme((ColorTheme)tag);
							ConsoleActions.WriteLine("Successfully updated the theme type.");
						}
					}

					_UISettings.SaveSettings();
					_UISettings.ActivateTheme();
					UIModification.SetColorMode(_Layout);
				}
				private async void AddTrustedUser(object sender, RoutedEventArgs e)
				{
					await UIBotWindowLogic.AddTrustedUserToComboBox(_TrustedUsersComboBox, _Client, _TrustedUsersAddBox.Text);
					_TrustedUsersAddBox.Text = null;
				}
				private void RemoveTrustedUser(object sender, RoutedEventArgs e)
				{
					UIBotWindowLogic.RemoveTrustedUserFromComboBox(_TrustedUsersComboBox);
				}

				private async void AcceptInput(object sender, KeyEventArgs e)
				{
					var text = _Input.Text;
					if (String.IsNullOrWhiteSpace(text))
					{
						_InputButton.IsEnabled = false;
						return;
					}

					if (e.Key.Equals(Key.Enter) || e.Key.Equals(Key.Return))
					{
						await UIBotWindowLogic.DoStuffWithInput(UICommandHandler.GatherInput(_Input, _InputButton), _Client, _BotSettings);
					}
					else
					{
						_InputButton.IsEnabled = true;
					}
				}
				private async void AcceptInput(object sender, RoutedEventArgs e)
				{
					await UIBotWindowLogic.DoStuffWithInput(UICommandHandler.GatherInput(_Input, _InputButton), _Client, _BotSettings);
				}

				private void SaveOutput(object sender, RoutedEventArgs e)
				{
					SayToolTipReason(UIBotWindowLogic.SaveOutput(_Output));
				}
				private void ClearOutput(object sender, RoutedEventArgs e)
				{
					switch (MessageBox.Show("Are you sure you want to clear the output window?", Constants.PROGRAM_NAME, MessageBoxButton.OKCancel))
					{
						case MessageBoxResult.OK:
						{
							_Output.Text = null;
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

				private void OpenFileSearch(object sender, RoutedEventArgs e)
				{
					_GuildSearchLayout.Visibility = Visibility.Visible;
				}
				private void CloseFileSearch(object sender, RoutedEventArgs e)
				{
					_GuildSearchFileComboBox.SelectedItem = null;
					_GuildSearchNameInput.Text = null;
					_GuildSearchIDInput.Text = null;
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

						guild = _FileTreeView.Items.Cast<TreeViewItem>().FirstOrDefault(x => ((GuildFileInformation)x.Tag).Id == guildID);
						if (guild == null)
						{
							ConsoleActions.WriteLine(String.Format("No guild could be found with the ID '{0}'.", guildID));
							return;
						}
					}
					else if (!String.IsNullOrWhiteSpace(nameStr))
					{
						var guilds = _FileTreeView.Items.Cast<TreeViewItem>().Where(x => ((GuildFileInformation)x.Tag).Name.CaseInsEquals(nameStr));
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
						var item = guild.Items.Cast<TreeViewItem>().FirstOrDefault(x => ((FileInformation)x.Tag).FileType == fileType);
						if (item != null)
						{
							OpenSpecificFileLayout(item, e);
						}
					}
				}

				private void OpenSpecificFileLayout(object sender, RoutedEventArgs e)
				{
					if (UIBotWindowLogic.AppendTextToTextEditorIfPathExistsAndReturnIfHappened(_SpecificFileDisplay, (TreeViewItem)sender))
					{
						UIModification.SetRowAndSpan(_FileLayout, 0, 100);
						_SpecificFileLayout.Visibility = Visibility.Visible;
						_FileSearchButton.Visibility = Visibility.Collapsed;
					}
				}
				private void CloseSpecificFileLayout(object sender, RoutedEventArgs e)
				{
					switch (MessageBox.Show("Are you sure you want to close the edit window?", Constants.PROGRAM_NAME, MessageBoxButton.OKCancel))
					{
						case MessageBoxResult.OK:
						{
							UIModification.SetRowAndSpan(_FileLayout, 0, 87);
							_SpecificFileDisplay.Tag = null;
							_SpecificFileLayout.Visibility = Visibility.Collapsed;
							_FileSearchButton.Visibility = Visibility.Visible;
							return;
						}
					}
				}
				private void SaveSpecificFile(object sender, RoutedEventArgs e)
				{
					SayToolTipReason(UIBotWindowLogic.SaveFile(_SpecificFileDisplay));
				}

				private async void OpenMenu(object sender, RoutedEventArgs e)
				{
					if (!_BotSettings.Loaded)
						return;

					//Hide everything so stuff doesn't overlap
					_MainMenuLayout.Visibility = Visibility.Collapsed;
					_SettingsLayout.Visibility = Visibility.Collapsed;
					_ColorsLayout.Visibility = Visibility.Collapsed;
					_InfoLayout.Visibility = Visibility.Collapsed;
					_FileLayout.Visibility = Visibility.Collapsed;

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
								UpdateSettingsWhenOpened();
								_SettingsLayout.Visibility = Visibility.Visible;
								return;
							}
							case MenuType.Colors:
							{
								UIModification.MakeColorDisplayer(_UISettings, _ColorsLayout, _ColorsSaveButton, .018);
								_ColorsLayout.Visibility = Visibility.Visible;
								return;
							}
							case MenuType.Files:
							{
								var treeView = UIModification.MakeGuildTreeView(_FileTreeView, await _Client.GetGuildsAsync());
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
				private async void UpdateSettingsWhenOpened()
				{
					((CheckBox)((Viewbox)_DownloadUsersSetting.Setting).Child).IsChecked	= _BotSettings.AlwaysDownloadUsers;
					((TextBox)_PrefixSetting.Setting).Text									= _BotSettings.Prefix;
					((TextBox)_GameSetting.Setting).Text									= _BotSettings.Game;
					((TextBox)_StreamSetting.Setting).Text									= _BotSettings.Stream;
					((TextBox)_ShardSetting.Setting).Text									= _BotSettings.ShardCount.ToString();
					((TextBox)_MessageCacheSetting.Setting).Text							= _BotSettings.MessageCacheCount.ToString();
					((TextBox)_UserGatherCountSetting.Setting).Text							= _BotSettings.MaxUserGatherCount.ToString();
					((TextBox)_MessageGatherSizeSetting.Setting).Text						= _BotSettings.MaxMessageGatherSize.ToString();
					((ComboBox)_LogLevelComboBox.Setting).SelectedItem						= ((ComboBox)_LogLevelComboBox.Setting).Items.OfType<TextBox>().FirstOrDefault(x => (LogSeverity)x.Tag == _BotSettings.LogLevel);
					var itemsSource = new List<TextBox>();
					foreach (var trustedUser in _BotSettings.TrustedUsers)
					{
						itemsSource.Add(UIModification.MakeTextBoxFromUserID(await _Client.GetUserAsync(trustedUser)));
					}
					_TrustedUsersComboBox.ItemsSource = itemsSource;
				}

				private async void SayToolTipReason(ToolTipReason reason)
				{
					await UIModification.MakeFollowingToolTip(_Layout, _ToolTip, UIBotWindowLogic.GetReasonTextFromToolTipReason(reason));
				}
			}

			internal struct SettingInMenu
			{
				public UIElement Setting;
				public TextBox Title;
			}
			
			internal class MyRichTextBox : RichTextBox
			{
				public MyRichTextBox()
				{
					this.Background = null;
					this.Foreground = null;
					this.BorderBrush = null;
				}
			}
			internal class MyTextBox : TextBox
			{
				public MyTextBox()
				{
					this.Background = null;
					this.Foreground = null;
					this.BorderBrush = null;
					this.TextWrapping = TextWrapping.Wrap;
				}
			}
			internal class MyButton : Button
			{
				public MyButton()
				{
					this.Background = null;
					this.Foreground = null;
					this.BorderBrush = null;
				}
			}
			internal class MyComboBox : ComboBox
			{
				public MyComboBox()
				{
					this.VerticalContentAlignment = VerticalAlignment.Center;
				}
			}
			internal class MyNumberBox : MyTextBox
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

			[Flags]
			internal enum MenuType : uint
			{
				Main							= (1U << 0),
				Info							= (1U << 1),
				Settings						= (1U << 2),
				Colors							= (1U << 3),
				Files							= (1U << 4),
			}

			[Flags]
			internal enum ToolTipReason : uint
			{
				FileSavingFailure				= (1U << 0),
				FileSavingSuccess				= (1U << 1),
				InvalidFilePath					= (1U << 2),
			}
		}
	}
}