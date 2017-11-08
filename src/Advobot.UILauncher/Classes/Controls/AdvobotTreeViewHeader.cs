using Advobot.Core.Actions;
using Advobot.Core.Actions.Formatting;
using Advobot.UILauncher.Enums;
using Advobot.UILauncher.Interfaces;
using Discord.WebSocket;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Controls;

namespace Advobot.UILauncher.Classes.Controls
{
	internal class AdvobotTreeViewHeader : TreeViewItem, IAdvobotControl
	{
		private FileSystemWatcher _FSW;
		public FileSystemWatcher FileSystemWatcher => _FSW;
		private DirectoryInfo _DI;
		public DirectoryInfo GuildDirectory => _DI;
		private SocketGuild _G;
		public SocketGuild Guild
		{
			get => _G;
			set
			{
				_G = value;
				//Make sure the guild currently has a directory. If not, create it
				var directories = GetActions.GetBaseBotDirectory().GetDirectories();
				var guildDir = directories.SingleOrDefault(x => x.Name == _G.Id.ToString());
				if (!guildDir.Exists)
				{
					Directory.CreateDirectory(guildDir.FullName);
				}
				_DI = guildDir;

				//If any files get updated or deleted then modify the guild files in the treeview
				_FSW?.Dispose();
				_FSW = new FileSystemWatcher(_DI.FullName);
				_FSW.Deleted += OnFileChangeInGuildDirectory;
				_FSW.Renamed += OnFileChangeInGuildDirectory;
				_FSW.Created += OnFileChangeInGuildDirectory;
				_FSW.EnableRaisingEvents = true;
			}
		}
		private ObservableCollection<AdvobotTreeViewFile> _Files;

		public AdvobotTreeViewHeader(SocketGuild guild)
		{
			this.Guild = guild;
			this.Header = guild.FormatGuild();
			this.ItemsSource = _Files = new ObservableCollection<AdvobotTreeViewFile>(_DI.GetFiles().Select(x => new AdvobotTreeViewFile(x)));
			//Sort alphabetically
			this.Items.SortDescriptions.Add(new SortDescription("Header", ListSortDirection.Ascending));
			this.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Left;
			this.VerticalContentAlignment = System.Windows.VerticalAlignment.Center;
			SetResourceReferences();
		}

		public void SetResourceReferences()
		{
			this.SetResourceReference(Control.BackgroundProperty, ColorTarget.BaseBackground);
			this.SetResourceReference(Control.ForegroundProperty, ColorTarget.BaseForeground);
		}

		private void OnFileChangeInGuildDirectory(object sender, FileSystemEventArgs e)
		{
			//Only allow basic text files to be shown
			//If someone is determined, they could get any file in here by renaming the extension
			//But they know what they're getting into if they do that, so no worries.
			if (!new[] { ".json", ".txt", ".config" }.Contains(Path.GetExtension(e.FullPath)))
			{
				return;
			}

			this.Dispatcher.Invoke(() =>
			{
				switch (e.ChangeType)
				{
					case WatcherChangeTypes.Created:
					{
						_Files.Add(new AdvobotTreeViewFile(new FileInfo(e.FullPath)));
						break;
					}
					case WatcherChangeTypes.Deleted:
					{
						_Files.Remove(_Files.FirstOrDefault(x => x.FileInfo.FullName == e.FullPath));
						break;
					}
					case WatcherChangeTypes.Renamed:
					{
						_Files.Remove(_Files.FirstOrDefault(x => x.FileInfo.FullName == ((RenamedEventArgs)e).OldFullPath));
						_Files.Add(new AdvobotTreeViewFile(new FileInfo(e.FullPath)));
						break;
					}
				}
			});
		}
	}
}