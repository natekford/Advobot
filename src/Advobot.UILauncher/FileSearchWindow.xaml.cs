using Advobot.Core.Actions;
using Advobot.UILauncher.Actions;
using Advobot.UILauncher.Classes;
using Advobot.UILauncher.Classes.Converters;
using Advobot.UILauncher.Enums;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Advobot.UILauncher
{
    /// <summary>
    /// Interaction logic for FileSearch.xaml
    /// </summary>
    public partial class FileSearchWindow : Window
	{
		public FileSearchWindow(Window mainWindow) : this()
		{
			this.Owner = mainWindow;
			this.Resources = mainWindow.Resources;
			this.Height = mainWindow.Height / 2;
			this.Width = mainWindow.Width / 2;
		}
        public FileSearchWindow()
        {
            InitializeComponent();
			ColorSettings.SwitchElementColorOfChildren(this.Layout);
		}

		private void WindowClosed(object sender, EventArgs e)
		{
			//Restore opacity and return false, indicating the user did not search
			this.Owner.Opacity = 100;
			if (this.DialogResult == null)
			{
				this.DialogResult = false;
			}
		}
		private void Close(object sender, RoutedEventArgs e)
		{
			this.Close();
		}
		private void Search(object sender, RoutedEventArgs e)
		{
			var selected = this.FileTypeComboBox.SelectedItem;
			var name = this.GuildNameInput.Text;
			var id = this.GuildIdInput.Text;
			if (false
				|| !(this.Owner is AdvobotWindow win)
				|| !(selected is TextBox tb)
				|| !(tb.Tag is FileType ft))
			{
				return;
			}

			var items = win.FilesTreeView.Items.OfType<TreeViewItem>();
			TreeViewItem item = null;
			if (!String.IsNullOrWhiteSpace(id))
			{
				item = items.SingleOrDefault(x => x.Tag is SocketGuild g && g.Id.ToString() == id);
				if (item == null)
				{
					ConsoleActions.WriteLine($"No guild could be found with the ID '{id}'.");
				}
			}
			else if (!String.IsNullOrWhiteSpace(name))
			{
				var guilds = items.Where(x => x.Tag is SocketGuild g && g.Name.CaseInsEquals(name));
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
				var file = item.Items.OfType<TreeViewItem>().FirstOrDefault(x => x.Tag is FileInformation fi && fi.FileType == ft);
				if (item != null && UIModification.TryGetFileText(file, out var text, out var fileInfo))
				{
					//Open the result in the window and set dialogresult to true indicating a file was found
					win.OpenSpecificFileLayout(text, fileInfo);
					this.DialogResult = true;
				}
				else
				{
					ConsoleActions.WriteLine($"Unable to open the file.");
				}
			}

			//Close the modal
			Close(sender, e);
		}
	}
}
