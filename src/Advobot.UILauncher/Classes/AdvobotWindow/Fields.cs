using Advobot.Core.Interfaces;
using Advobot.UILauncher.Actions;
using Advobot.UILauncher.Enums;
using Discord;
using ICSharpCode.AvalonEdit;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;

namespace Advobot.UILauncher.Classes.AdvobotWindow
{
	public partial class AdvobotWindow : Window
	{
		#region Variables
		private IDiscordClient _Client;
		private IBotSettings _BotSettings;
		private ILogService _Logging;
		private ColorSettings _UISettings;
		private readonly DispatcherTimer _UpdateTimer = new DispatcherTimer
		{
			Interval = new TimeSpan(0, 0, 0, 0, 500)
		};
		#endregion

		#region Top Most Items
		private readonly Grid _Layout = new Grid();
		private readonly ToolTip _ToolTip = new ToolTip
		{
			Placement = PlacementMode.Relative
		};
		#endregion

		#region Input
		private readonly Grid _InputLayout = new Grid();
		//Max height has to be set here as a large number to a) not get in the way and b) not crash when resized small. I don't want to use a RTB for input.
		private readonly TextBox _Input = new AdvobotTextBox
		{
			TextWrapping = TextWrapping.Wrap,
			MaxLength = 250,
			MaxLines = 5,
			MaxHeight = 1000,
			FontResizeValue = .275
		};
		private readonly Button _InputButton = new AdvobotButton
		{
			Content = "Enter",
			IsEnabled = false,
		};
		#endregion

		#region Output
		private readonly MenuItem _OutputContextMenuSearch = new MenuItem
		{
			Header = "Search For...",
		};
		private readonly MenuItem _OutputContextMenuSave = new MenuItem
		{
			Header = "Save Output Log",
		};
		private readonly MenuItem _OutputContextMenuClear = new MenuItem
		{
			Header = "Clear Output Log",
		};
		private readonly AdvobotTextBox _Output = new AdvobotTextBox
		{
			VerticalScrollBarVisibility = ScrollBarVisibility.Visible,
			TextWrapping = TextWrapping.Wrap,
			IsReadOnly = true,
		};

		private readonly Grid _OutputSearchLayout = new Grid
		{
			Background = UIModification.MakeBrush("#BF000000"),
			Visibility = Visibility.Collapsed,
		};
		private readonly Grid _OutputSearchTextLayout = new Grid();
		private readonly TextBox _OutputSearchResults = new AdvobotTextBox
		{
			VerticalScrollBarVisibility = ScrollBarVisibility.Visible,
			IsReadOnly = true,
		};
		private readonly ComboBox _OutputSearchComboBox = new AdvobotComboBox
		{
			IsEditable = true,
			FontResizeValue = .022,
		};
		private readonly Button _OutputSearchButton = new AdvobotButton
		{
			Content = "Search",
		};
		private readonly Button _OutputSearchCloseButton = new AdvobotButton
		{
			Content = "Close",
		};
		#endregion

		#region Buttons
		private readonly Grid _ButtonLayout = new Grid();
		private readonly Button _MainButton = AdvobotButton.CreateButtonFromEnum(MenuType.Main);
		private readonly Button _InfoButton = AdvobotButton.CreateButtonFromEnum(MenuType.Info);
		private readonly Button _SettingsButton = AdvobotButton.CreateButtonFromEnum(MenuType.Settings);
		private readonly Button _ColorsButton = AdvobotButton.CreateButtonFromEnum(MenuType.Colors);
		private readonly Button _FileButton = AdvobotButton.CreateButtonFromEnum(MenuType.Files);
		private MenuType _LastButtonClicked;
		#endregion

		#region Main Menu
		private readonly Grid _MainMenuLayout = new Grid
		{
			Visibility = Visibility.Collapsed,
		};
		private readonly RichTextBox _MainMenuOutput = new AdvobotRichTextBox
		{
			Document = UIModification.MakeMainMenu(),
			IsReadOnly = true,
			IsDocumentEnabled = true,
			FontResizeValue = .018,
		};
		private readonly Button _DisconnectButton = new AdvobotButton
		{
			Content = "Disconnect",
		};
		private readonly Button _RestartButton = new AdvobotButton
		{
			Content = "Restart",
		};
		private readonly Button _PauseButton = new AdvobotButton
		{
			Content = "Pause",
		};
		#endregion

		#region Settings Menu
		private readonly Grid _SettingsLayout = new Grid
		{
			Visibility = Visibility.Collapsed,
		};
		private readonly Button _SettingsSaveButton = new AdvobotButton
		{
			Content = "Save Settings"
		};

		private readonly SettingInMenu _DownloadUsersSetting = new SettingInMenu
		{
			Setting = new Viewbox
			{
				Child = new CheckBox
				{
					Tag = nameof(IBotSettings.AlwaysDownloadUsers),
				},
				HorizontalAlignment = HorizontalAlignment.Left,
				VerticalAlignment = VerticalAlignment.Center,
				Tag = nameof(IBotSettings.AlwaysDownloadUsers),
			},
			Title = AdvobotTextBox.CreateTitleBox("Download Users:", "This automatically puts users in the bots cache. With it off, many commands will not work since I haven't added in a manual way to download users."),
		};

		private readonly SettingInMenu _PrefixSetting = new SettingInMenu
		{
			Setting = AdvobotTextBox.CreateSettingBox(nameof(IBotSettings.Prefix), 10),
			Title = AdvobotTextBox.CreateTitleBox("Prefix:", "The prefix which is needed to be said before commands."),
		};
		private readonly SettingInMenu _GameSetting = new SettingInMenu
		{
			Setting = AdvobotTextBox.CreateSettingBox(nameof(IBotSettings.Game), 100),
			Title = AdvobotTextBox.CreateTitleBox("Game:", "Changes what the bot says it's playing."),
		};
		private readonly SettingInMenu _StreamSetting = new SettingInMenu
		{
			Setting = AdvobotTextBox.CreateSettingBox(nameof(IBotSettings.Stream), 50),
			Title = AdvobotTextBox.CreateTitleBox("Stream:", "Can set whatever stream you want as long as it's a valid Twitch.tv stream."),
		};
		private readonly SettingInMenu _ShardSetting = new SettingInMenu
		{
			Setting = AdvobotTextBox.CreateSettingBox(nameof(IBotSettings.ShardCount), 3),
			Title = AdvobotTextBox.CreateTitleBox("Shard Count:", "Each shard can hold up to 2500 guilds."),
		};
		private readonly SettingInMenu _MessageCacheSetting = new SettingInMenu
		{
			Setting = AdvobotTextBox.CreateSettingBox(nameof(IBotSettings.MessageCacheCount), 6),
			Title = AdvobotTextBox.CreateTitleBox("Message Cache:", "The amount of messages the bot will hold in its cache."),
		};
		private readonly SettingInMenu _UserGatherCountSetting = new SettingInMenu
		{
			Setting = AdvobotTextBox.CreateSettingBox(nameof(IBotSettings.MaxUserGatherCount), 5),
			Title = AdvobotTextBox.CreateTitleBox("Max User Gather:", "Limits the amount of users a command can modify at once."),
		};
		private readonly SettingInMenu _MessageGatherSizeSetting = new SettingInMenu
		{
			Setting = AdvobotTextBox.CreateSettingBox(nameof(IBotSettings.MaxMessageGatherSize), 7),
			Title = AdvobotTextBox.CreateTitleBox("Max Msg Gather:", "This is in bytes, which to be very basic is roughly two bytes per character."),
		};

		private readonly SettingInMenu _LogLevelComboBox = new SettingInMenu
		{
			Setting = AdvobotComboBox.CreateEnumComboBox<LogSeverity>(nameof(IBotSettings.LogLevel)),
			Title = AdvobotTextBox.CreateTitleBox("Log Level:", "Certain events in the Discord library used in this bot have a required log level to be said in the console."),
		};
		private readonly SettingInMenu _TrustedUsersAdd = new SettingInMenu
		{
			Setting = new Grid() { Tag = nameof(IBotSettings.TrustedUsers), },
			Title = AdvobotTextBox.CreateTitleBox("Trusted Users:", "Some commands can only be run by the bot owner or user IDs that they have designated as trust worthy."),
		};
		private readonly TextBox _TrustedUsersAddBox = AdvobotTextBox.CreateSettingBox(nameof(IBotSettings.TrustedUsers), 18);
		private readonly Button _TrustedUsersAddButton = new AdvobotButton
		{
			Content = "+",
		};
		private readonly SettingInMenu _TrustedUsersRemove = new SettingInMenu
		{
			Setting = new Grid() { Tag = nameof(IBotSettings.TrustedUsers), },
			Title = AdvobotTextBox.CreateTitleBox("", ""),
		};
		private readonly ComboBox _TrustedUsersComboBox = new AdvobotComboBox
		{
			Tag = nameof(IBotSettings.TrustedUsers),
		};
		private readonly Button _TrustedUsersRemoveButton = new AdvobotButton
		{
			Content = "-",
		};
		#endregion

		#region Colors Menu
		private readonly Grid _ColorsLayout = new Grid
		{
			Visibility = Visibility.Collapsed,
		};
		private readonly Button _ColorsSaveButton = new AdvobotButton
		{
			Content = "Save Colors",
		};
		#endregion

		#region Info Menu
		private readonly Grid _InfoLayout = new Grid
		{
			Visibility = Visibility.Collapsed,
		};
		private readonly RichTextBox _InfoOutput = new AdvobotRichTextBox
		{
			BorderThickness = new Thickness(0, 1, 0, 1),
			IsReadOnly = true,
			IsDocumentEnabled = true,
			FontResizeValue = .035,
		};
		#endregion

		#region Guild Menu
		private readonly Grid _FileLayout = new Grid
		{
			Visibility = Visibility.Collapsed,
		};
		private readonly RichTextBox _FileOutput = new AdvobotRichTextBox
		{
			IsReadOnly = true,
			IsDocumentEnabled = true,
			FontResizeValue = .022,
		};
		private readonly TreeView _FileTreeView = new TreeView();
		private readonly Button _FileSearchButton = new AdvobotButton
		{
			Content = "Search Guilds",
		};

		private readonly Grid _SpecificFileLayout = new Grid
		{
			Visibility = Visibility.Collapsed,
		};
		private readonly MenuItem _SpecificFileContextMenuSave = new MenuItem
		{
			Header = "Save File",
		};
		private readonly TextEditor _SpecificFileDisplay = new AdvobotTextEditor
		{
			Background = null,
			Foreground = null,
			BorderBrush = null,
			VerticalScrollBarVisibility = ScrollBarVisibility.Visible,
			WordWrap = true,
			ShowLineNumbers = true,
			FontResizeValue = .022,
		};
		private readonly Button _SpecificFileCloseButton = new AdvobotButton
		{
			Content = "Close Menu",
		};

		private readonly Grid _GuildSearchLayout = new Grid { Background = UIModification.MakeBrush("#BF000000"), Visibility = Visibility.Collapsed };
		private readonly Grid _GuildSearchTextLayout = new Grid();
		private readonly Viewbox _GuildSearchNameHeader = UIModification.MakeStandardViewBox("Guild Name:");
		private readonly TextBox _GuildSearchNameInput = new AdvobotTextBox
		{
			MaxLength = 100,
			FontResizeValue = .060,
		};
		private readonly Viewbox _GuildSearchIDHeader = UIModification.MakeStandardViewBox("ID:");
		private readonly TextBox _GuildSearchIDInput = new AdvobotNumberBox
		{
			MaxLength = 18,
			FontResizeValue = .060,
		};
		private readonly ComboBox _GuildSearchFileComboBox = AdvobotComboBox.CreateEnumComboBox<FileType>(null);
		private readonly Button _GuildSearchSearchButton = new AdvobotButton
		{
			Content = "Search",
		};
		private readonly Button _GuildSearchCloseButton = new AdvobotButton
		{
			Content = "Close",
		};
		#endregion

		#region System Info
		private readonly Grid _SysInfoLayout = new Grid();
		private readonly TextBox _SysInfoUnder = new AdvobotTextBox
		{
			IsReadOnly = true,
		};
		private readonly Viewbox _Latency = new Viewbox
		{
			Child = AdvobotTextBox.CreateSystemInfoBox(),
		};
		private readonly Viewbox _Memory = new Viewbox
		{
			Child = AdvobotTextBox.CreateSystemInfoBox(),
		};
		private readonly Viewbox _Threads = new Viewbox
		{
			Child = AdvobotTextBox.CreateSystemInfoBox(),
		};
		private readonly Viewbox _Guilds = new Viewbox
		{
			Child = AdvobotTextBox.CreateSystemInfoBox(),
		};
		private readonly Viewbox _Users = new Viewbox
		{
			Child = AdvobotTextBox.CreateSystemInfoBox(),
		};
		#endregion
	}
}
