using Advobot.UILauncher.Actions;
using Advobot.UILauncher.Interfaces;
using System.Windows;
using System.Windows.Controls;

namespace Advobot.UILauncher.Classes.AdvobotWindow
{
	public partial class AdvobotWindow : Window
	{
		//This part is effectively XAML in code.
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

				var t = _Settings[i].Title;
				var s = _Settings[i].Setting;
				if (t is IFontResizeValue tfrv)
				{
					tfrv.FontResizeValue = .018;
				}
				if (s is IFontResizeValue sfrv)
				{
					sfrv.FontResizeValue = .018;
				}
				UIModification.AddElement(_SettingsLayout, t, (i * LENGTH_FOR_SETTINGS), LENGTH_FOR_SETTINGS, TITLE_START_COLUMN, TITLE_COLUMN_LENGTH);
				UIModification.AddElement(_SettingsLayout, s, (i * LENGTH_FOR_SETTINGS), LENGTH_FOR_SETTINGS, SETTING_START_COLUMN, SETTING_COLUMN_LENGTH);
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

			_Output.ContextMenu = new ContextMenu
			{
				ItemsSource = new[] { _OutputContextMenuSearch, _OutputContextMenuSave, _OutputContextMenuClear },
			};
			_SpecificFileDisplay.ContextMenu = new ContextMenu
			{
				ItemsSource = new[] { _SpecificFileContextMenuSave },
			};

			HookUpEvents();

			//Set this panel as the content for this window and run the application
			this.Content = _Layout;
			this.WindowState = WindowState.Maximized;
		}
		private void HookUpEvents()
		{
			//Bot status
			_PauseButton.Click += Pause;
			_RestartButton.Click += Restart;
			_DisconnectButton.Click += Disconnect;

			//Settings
			_SettingsSaveButton.Click += SaveSettings;
			_ColorsSaveButton.Click += SaveColors;
			_TrustedUsersRemoveButton.Click += RemoveTrustedUser;
			_TrustedUsersAddButton.Click += AddTrustedUser;

			//Input
			_Input.KeyUp += AcceptInput;
			_InputButton.Click += AcceptInput;

			//Output
			_OutputContextMenuSave.Click += SaveOutput;
			_OutputContextMenuClear.Click += ClearOutput;
			_OutputContextMenuSearch.Click += OpenOutputSearch;

			//Output search
			_OutputSearchCloseButton.Click += CloseOutputSearch;
			_OutputSearchButton.Click += SearchOutput;

			//File
			_FileSearchButton.Click += OpenFileSearch;
			_GuildSearchSearchButton.Click += SearchForFile;
			_GuildSearchCloseButton.Click += CloseFileSearch;

			//Specific file
			_SpecificFileCloseButton.Click += CloseSpecificFileLayout;
			_SpecificFileContextMenuSave.Click += SaveSpecificFile;

			//Menu
			/*
			_MainButton.Click += OpenMenu;
			_SettingsButton.Click += OpenMenu;
			_ColorsButton.Click += OpenMenu;
			_InfoButton.Click += OpenMenu;
			_FileButton.Click += OpenMenu;*/
		}
	}
}
