using Advobot.Core;
using Advobot.Core.Actions;
using Advobot.UILauncher.Classes.Controls;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Advobot.UILauncher.Windows
{
	/// <summary>
	/// Interaction logic for FileSearch.xaml
	/// </summary>
	internal partial class FileSearchWindow : ModalWindow
	{
		private string[] _Files = new[]
		{
			Constants.GUILD_SETTINGS_LOCATION.Split('.')[0],
		};

		public FileSearchWindow() : this(null) { }
		public FileSearchWindow(Window mainWindow) : base(mainWindow)
		{
			InitializeComponent();
			this.FileTypeComboBox.ItemsSource = AdvobotComboBox.CreateComboBoxSourceOutOfStrings(this._Files);
		}

		private void Search(object sender, RoutedEventArgs e)
		{
			var selected = this.FileTypeComboBox.SelectedItem;
			var name = this.GuildNameInput.Text;
			var id = this.GuildIdInput.Text;
			if (!(this.Owner is AdvobotWindow win) ||
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
					ConsoleActions.WriteLine($"No guild could be found with the ID '{id}'.");
				}
			}
			else if (!String.IsNullOrWhiteSpace(name))
			{
				var guilds = items.Where(x => x.Guild.Name.CaseInsEquals(name));
				if (guilds.Count() == 0)
				{
					ConsoleActions.WriteLine($"No guild could be found with the name '{name}'.");
				}
				else if (guilds.Count() == 1)
				{
					item = guilds.FirstOrDefault();
				}
				else
				{
					ConsoleActions.WriteLine($"More than one guild has the name '{name}'.");
				}
			}
			else
			{
				return;
			}

			if (item != null)
			{
				this.DialogResult = true;
				this.Hide();
				item.Items.OfType<AdvobotTreeViewFile>().FirstOrDefault(x => x.FileInfo.Name.CaseInsContains(s))?.OpenFile();
			}

			//Close the modal
			Close(sender, e);
		}
	}
}
