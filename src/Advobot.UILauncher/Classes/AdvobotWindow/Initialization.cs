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
			//File menu
			//UIModification.AddElement(_Layout, _FileLayout, 0, 87, 3, 1, 100, 1);
			UIModification.AddElement(_FileLayout, _FileOutput, 0, 95, 0, 1);
			UIModification.AddElement(_FileLayout, _FileSearchButton, 95, 5, 0, 1);

			//Specific File
			//UIModification.AddElement(_Layout, _SpecificFileLayout, 0, 100, 0, 4, 100, 4);
			UIModification.AddElement(_SpecificFileLayout, _SpecificFileDisplay, 0, 100, 0, 3);
			UIModification.AddElement(_SpecificFileLayout, _SpecificFileCloseButton, 95, 5, 3, 1);

			//Guild search
			//UIModification.AddElement(_Layout, _GuildSearchLayout, 0, 100, 0, 4, 10, 10);
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
			//UIModification.AddElement(_Layout, _OutputSearchLayout, 0, 100, 0, 4, 10, 10);
			UIModification.AddElement(_OutputSearchLayout, _OutputSearchTextLayout, 1, 8, 1, 8, 100, 100);
			UIModification.PutInBGWithMouseUpEvent(_OutputSearchLayout, _OutputSearchTextLayout, null, CloseOutputSearch);
			UIModification.AddPlaceHolderTB(_OutputSearchTextLayout, 90, 10, 0, 100);
			UIModification.AddElement(_OutputSearchTextLayout, _OutputSearchResults, 0, 90, 0, 100);
			UIModification.AddElement(_OutputSearchTextLayout, _OutputSearchComboBox, 92, 6, 2, 30);
			UIModification.AddElement(_OutputSearchTextLayout, _OutputSearchButton, 92, 6, 66, 15);
			UIModification.AddElement(_OutputSearchTextLayout, _OutputSearchCloseButton, 92, 6, 83, 15);

			_SpecificFileDisplay.ContextMenu = new ContextMenu
			{
				ItemsSource = new[] { _SpecificFileContextMenuSave },
			};
		}
		private void HookUpEvents()
		{
			//_OutputContextMenuSearch.Click += OpenOutputSearch;

			//Output search
			_OutputSearchCloseButton.Click += CloseOutputSearch;
			_OutputSearchButton.Click += SearchOutput;

			//File
			_FileSearchButton.Click += OpenFileSearch;
			_GuildSearchSearchButton.Click += SearchForFile;
			_GuildSearchCloseButton.Click += CloseFileSearch;

			//Specific file
			_SpecificFileCloseButton.Click += CloseSpecificFileLayout;
			//_SpecificFileContextMenuSave.Click += SaveSpecificFile;
		}
	}
}
