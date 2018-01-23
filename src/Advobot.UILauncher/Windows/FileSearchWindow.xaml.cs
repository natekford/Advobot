using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Advobot.Core;
using Advobot.Core.Utilities;
using Advobot.UILauncher.Classes.Controls;

namespace Advobot.UILauncher.Windows
{
	/// <summary>
	/// Interaction logic for FileSearch.xaml
	/// </summary>
	internal partial class FileSearchWindow : ModalWindow
	{
		private string[] _Files = {
			Constants.GUILD_SETTINGS_LOC.Split('.')[0]
		};

		public FileSearchWindow() : this(null) { }
		public FileSearchWindow(Window mainWindow) : base(mainWindow)
		{
			InitializeComponent();
			FileTypeComboBox.ItemsSource = AdvobotComboBox.CreateComboBoxSourceOutOfStrings(_Files);
		}

		private void Search(object sender, RoutedEventArgs e)
		{
			var selected = FileTypeComboBox.SelectedItem;
			var name = GuildNameInput.Text;
			var id = GuildIdInput.Text;
			if (!(Owner is AdvobotWindow win) ||
				!(selected is TextBox tb) ||
				!(tb.Tag is string s))
			{
				return;
			}

			var items = win.FilesTreeView.Items.OfType<AdvobotTreeViewHeader>();
			TreeViewItem item = null;
			if (!String.IsNullOrWhiteSpace(id))
			{
				item = items.SingleOrDefault(x => x.Guild.Id.ToString() == id);
				if (item == null)
				{
					ConsoleUtils.WriteLine($"No guild could be found with the ID '{id}'.");
				}
			}
			else if (!String.IsNullOrWhiteSpace(name))
			{
				var guilds = items.Where(x => x.Guild.Name.CaseInsEquals(name));
				if (guilds.Count() == 0)
				{
					ConsoleUtils.WriteLine($"No guild could be found with the name '{name}'.");
				}
				else if (guilds.Count() == 1)
				{
					item = guilds.FirstOrDefault();
				}
				else
				{
					ConsoleUtils.WriteLine($"More than one guild has the name '{name}'.");
				}
			}
			else
			{
				return;
			}

			if (item != null)
			{
				DialogResult = true;
				Hide();
				item.Items.OfType<AdvobotTreeViewFile>().FirstOrDefault(x => x.FileInfo.Name.CaseInsContains(s))?.OpenFile();
			}

			//Close the modal
			Close(sender, e);
		}
	}
}
