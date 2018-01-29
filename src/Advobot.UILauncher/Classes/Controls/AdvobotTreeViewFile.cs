using Advobot.Core.Utilities;
using Advobot.UILauncher.Enums;
using Advobot.UILauncher.Interfaces;
using Advobot.UILauncher.Utilities;
using Advobot.UILauncher.Windows;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.IO;
using System.Reflection;
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
			_FI = fileInfo;
			Header = _FI.Name;
			Tag = _FI;
			ContextMenu = CreateContextMenu();
			HorizontalContentAlignment = HorizontalAlignment.Left;
			VerticalContentAlignment = VerticalAlignment.Center;
			MouseDoubleClick += OpenFile;
			SetResourceReferences();
		}
		private ContextMenu CreateContextMenu()
		{
			var delete = new MenuItem
			{
				Header = "Delete File",
				HorizontalContentAlignment = HorizontalAlignment.Center,
				VerticalContentAlignment = VerticalAlignment.Center
			};
			delete.Click += DeleteFile;
			var copy = new MenuItem
			{
				Header = "Copy File",
				HorizontalContentAlignment = HorizontalAlignment.Center,
				VerticalContentAlignment = VerticalAlignment.Center
			};
			copy.Click += CopyFile;
			return new ContextMenu { ItemsSource = new[] { delete, copy } };
		}
		public void SetResourceReferences()
		{
			SetResourceReference(BackgroundProperty, ColorTarget.BaseBackground);
			SetResourceReference(ForegroundProperty, ColorTarget.BaseForeground);
		}
		public void Update(RenamedEventArgs e)
		{
			_FI = new FileInfo(e.FullPath);
			Header = _FI.Name;
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
			if (!this.TryGetTopMostParent(out AdvobotWindow window, out var ancestorLevel))
			{
				throw new ArgumentException("unable to get a parent", nameof(AdvobotWindow));
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
						if (!this.TryGetTopMostParent(out AdvobotWindow window, out var ancestorLevel))
						{
							throw new ArgumentException("unable to get a parent", nameof(AdvobotWindow));
						}
						_FI.CopyTo(Path.Combine(dialog.FileName, _FI.Name), true);
						ToolTipUtils.EnableTimedToolTip(window.Layout, $"Successfully copied {_FI.Name} to {dialog.FileName}.");
						return;
				}
			}
		}
		private void DeleteFile(object sender, RoutedEventArgs e)
		{
			var text = $"Are you sure you want to delete the file {_FI.Name}?";
			var caption = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyProductAttribute>().Product;
			switch (MessageBox.Show(text, caption, MessageBoxButton.YesNo))
			{
				case MessageBoxResult.Yes:
					IOUtils.DeleteFile(_FI);
					return;
			}
		}
	}
}
