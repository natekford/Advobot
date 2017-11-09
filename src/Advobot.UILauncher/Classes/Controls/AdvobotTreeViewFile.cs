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
			var export = new MenuItem
			{
				Header = "Export File",
				HorizontalContentAlignment = HorizontalAlignment.Center,
				VerticalContentAlignment = VerticalAlignment.Center,
			};
			export.Click += ExportFile;
			return new ContextMenu { ItemsSource = new[] { delete, export } };
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

		private void OpenFile(object sender, RoutedEventArgs e)
		{
			if (!EntityActions.TryGetTopMostParent(this, out AdvobotWindow window, out var ancestorLevel))
			{
				throw new ArgumentException($"Unable to get a parent {nameof(AdvobotWindow)}.");
			}
			else if (SavingActions.TryGetFileText(this, out var text, out var fileInfo))
			{
				window.OpenSpecificFileLayout(text, fileInfo);
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
		private void ExportFile(object sender, RoutedEventArgs e)
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

						this._FI.CopyTo(Path.Combine(dialog.FileName, _FI.Name));
						ToolTipActions.EnableTimedToolTip(window.Layout, $"Successfully copied {this._FI.Name} to {dialog.FileName}.");
						break;
					}
				}
			}
		}
	}
}
