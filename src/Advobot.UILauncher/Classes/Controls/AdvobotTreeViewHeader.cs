using Advobot.Core.Utilities;
using Advobot.UILauncher.Enums;
using Advobot.UILauncher.Interfaces;
using Discord.WebSocket;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Advobot.UILauncher.Classes.Controls
{
	internal class AdvobotTreeViewHeader : TreeViewItem, IAdvobotControl, IDisposable
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
				var directories = IOUtils.GetBaseBotDirectory().GetDirectories();
				var guildDir = directories.SingleOrDefault(x => x.Name == _G.Id.ToString());
				if (!guildDir.Exists)
				{
					Directory.CreateDirectory(guildDir.FullName);
				}

				//Use the correct directory and files
				_DI = guildDir;
				_Files.Clear();
				foreach (var file in _DI.GetFiles())
				{
					_Files.Add(new AdvobotTreeViewFile(file));
				}

				//If any files get updated or deleted then modify the guild files in the treeview
				_FSW?.Dispose();
				_FSW = new FileSystemWatcher(_DI.FullName);
				_FSW.Deleted += OnFileChangeInGuildDirectory;
				_FSW.Renamed += OnFileChangeInGuildDirectory;
				_FSW.Created += OnFileChangeInGuildDirectory;
				_FSW.EnableRaisingEvents = true;
			}
		}
		private ObservableCollection<AdvobotTreeViewFile> _Files = new ObservableCollection<AdvobotTreeViewFile>();

		public AdvobotTreeViewHeader(SocketGuild guild)
		{
			Header = guild.Format();
			Guild = guild;
			Tag = new CompGuild(guild);
			ItemsSource = _Files;
			HorizontalContentAlignment = HorizontalAlignment.Left;
			VerticalContentAlignment = VerticalAlignment.Center;
			SetResourceReferences();
		}
		public void SetResourceReferences()
		{
			SetResourceReference(BackgroundProperty, ColorTarget.BaseBackground);
			SetResourceReference(ForegroundProperty, ColorTarget.BaseForeground);
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

			Dispatcher.Invoke(() =>
			{
				switch (e.ChangeType)
				{
					case WatcherChangeTypes.Created:
						_Files.Add(new AdvobotTreeViewFile(new FileInfo(e.FullPath)));
						break;
					case WatcherChangeTypes.Deleted:
						_Files.Remove(_Files.FirstOrDefault(x => x.FileInfo.FullName == e.FullPath));
						break;
					case WatcherChangeTypes.Renamed:
						var renamed = (RenamedEventArgs)e;
						_Files.FirstOrDefault(x => x.FileInfo.FullName == renamed.OldFullPath)?.Update(renamed);
						break;
				}
				Items.SortDescriptions.Clear();
				Items.SortDescriptions.Add(new SortDescription("Header", ListSortDirection.Ascending));
			});
		}

		public void Dispose()
		{
			((IDisposable)_FSW).Dispose();
		}
	}
}