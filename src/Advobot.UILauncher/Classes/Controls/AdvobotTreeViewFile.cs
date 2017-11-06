using Advobot.Core;
using Advobot.Core.Actions;
using Advobot.UILauncher.Actions;
using Advobot.UILauncher.Enums;
using Advobot.UILauncher.Interfaces;
using Advobot.UILauncher.Windows;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Advobot.UILauncher.Classes.Controls
{
	internal class AdvobotTreeViewFile : TreeViewItem, IAdvobotControl
	{
		private FileInfo _FI;
		public FileInfo FileInfo => _FI;
		private AdvobotWindow _W;

		public AdvobotTreeViewFile(FileInfo fileInfo)
		{
			this._FI = fileInfo;
			EntityActions.TryGetTopMostParent(this, out AdvobotWindow _W, out var ancestorLevel);
			this.Header = _FI.Name;
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

		private void OpenFile(object sender, RoutedEventArgs e)
		{
			if (SavingActions.TryGetFileText(_FI, out var text, out var fileInfo))
			{
				_W.OpenSpecificFileLayout(text, fileInfo);
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

		}
	}
}
