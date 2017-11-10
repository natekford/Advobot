using Advobot.Core;
using Advobot.Core.Actions;
using Advobot.UILauncher.Actions;
using Advobot.UILauncher.Enums;
using Advobot.UILauncher.Interfaces;
using Advobot.UILauncher.Windows;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Advobot.UILauncher.Classes.Controls
{
	internal class AdvobotTreeViewFile : TreeViewItem, IAdvobotControl
	{
		private FileInfo _FI;
		public FileInfo FileInfo => _FI;

		public AdvobotTreeViewFile(FileInfo fileInfo)
		{
			this._FI = fileInfo;
			this.Header = _FI.Name;
			this.Tag = _FI;
			this.ContextMenu = CreateContextMenu();
			this.HorizontalContentAlignment = HorizontalAlignment.Left;
			this.VerticalContentAlignment = VerticalAlignment.Center;
			this.MouseDoubleClick += OpenFile;
			SetResourceReferences();
		}
		private ContextMenu CreateContextMenu()
		{
			var delete = new MenuItem
			{
				Header = "Delete File",
				HorizontalContentAlignment = HorizontalAlignment.Center,
				VerticalContentAlignment = VerticalAlignment.Center,
			};
			delete.Click += DeleteFile;
			var copy = new MenuItem
			{
				Header = "Copy File",
				HorizontalContentAlignment = HorizontalAlignment.Center,
				VerticalContentAlignment = VerticalAlignment.Center,
			};
			copy.Click += CopyFile;
			return new ContextMenu { ItemsSource = new[] { delete, copy } };
		}
		public void SetResourceReferences()
		{
			this.SetResourceReference(TreeViewItem.BackgroundProperty, ColorTarget.BaseBackground);
			this.SetResourceReference(TreeViewItem.ForegroundProperty, ColorTarget.BaseForeground);
		}
		public void Update(RenamedEventArgs e)
		{
			this._FI = new FileInfo(e.FullPath);
			this.Header = _FI.Name;
		}

		public void OpenFile()
		{
			OpenFile(null, null);
		}
		public void CopyFile()
		{
			CopyFile(null, null);
		}
		public void DeleteFile()
		{
			DeleteFile(null, null);
		}
		private void OpenFile(object sender, RoutedEventArgs e)
		{
			if (!EntityActions.TryGetTopMostParent(this, out AdvobotWindow window, out var ancestorLevel))
			{
				throw new ArgumentException($"Unable to get a parent {nameof(AdvobotWindow)}.");
			}
			new FileViewingWindow(window, this).ShowDialog();
		}
		private void CopyFile(object sender, RoutedEventArgs e)
		{
			using (var dialog = new CommonOpenFileDialog { IsFolderPicker = true })
			{
				switch (dialog.ShowDialog())
				{
					case CommonFileDialogResult.Ok:
					{
						if (!EntityActions.TryGetTopMostParent(this, out AdvobotWindow window, out var ancestorLevel))
						{
							throw new ArgumentException($"Unable to get a parent {nameof(AdvobotWindow)}.");
						}

						this._FI.CopyTo(Path.Combine(dialog.FileName, _FI.Name), true);
						ToolTipActions.EnableTimedToolTip(window.Layout, $"Successfully copied {this._FI.Name} to {dialog.FileName}.");
						break;
					}
				}
			}
		}
		private void DeleteFile(object sender, RoutedEventArgs e)
		{
			var text = $"Are you sure you want to delete the file {_FI.Name}?";
			switch (MessageBox.Show(text, Constants.PROGRAM_NAME, MessageBoxButton.YesNo))
			{
				case MessageBoxResult.Yes:
				{
					SavingAndLoadingActions.DeleteFile(_FI);
					return;
				}
			}
		}
	}
}
