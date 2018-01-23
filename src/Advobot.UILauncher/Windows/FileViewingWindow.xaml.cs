using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Advobot.Core;
using Advobot.UILauncher.Classes.Controls;
using Advobot.UILauncher.Utilities;

namespace Advobot.UILauncher.Windows
{
	/// <summary>
	/// Interaction logic for FileViewingWindow.xaml
	/// </summary>
	internal partial class FileViewingWindow : ModalWindow
	{
		private AdvobotWindow _AdvoWin;
		private AdvobotTreeViewFile _TreeViewFile;

		public FileViewingWindow() : this(null, null) { }
		public FileViewingWindow(AdvobotWindow mainWindow, AdvobotTreeViewFile treeViewFile) : base(mainWindow)
		{
			InitializeComponent();
			_AdvoWin = mainWindow;
			_TreeViewFile = treeViewFile;
			if (SavingUtils.TryGetFileText(_TreeViewFile, out var text, out var fileInfo))
			{
				SpecificFileOutput.Tag = fileInfo;
				SpecificFileOutput.Clear();
				SpecificFileOutput.AppendText(text);
			}
		}

		private void CopyFile(object sender, RoutedEventArgs e)
		{
			_TreeViewFile.CopyFile();
		}

		private void DeleteFile(object sender, RoutedEventArgs e)
		{
			_TreeViewFile.DeleteFile();
		}

		private void SaveFile(object sender, RoutedEventArgs e)
		{
			ToolTipUtils.EnableTimedToolTip(Layout, SavingUtils.SaveFile(SpecificFileOutput).GetReason());
		}

		private void SaveFileWithCtrlS(object sender, KeyEventArgs e)
		{
			if (SavingUtils.IsCtrlS(e))
			{
				SaveFile(sender, e);
			}
		}
		private void CloseFile(object sender, RoutedEventArgs e)
		{
			switch (MessageBox.Show("Are you sure you want to close the file window?", Constants.PROGRAM_NAME, MessageBoxButton.YesNo))
			{
				case MessageBoxResult.Yes:
				{
					Close(sender, e);
					return;
				}
			}
		}
		private void MoveToolTip(object sender, MouseEventArgs e)
		{
			if (!(sender is FrameworkElement fe) || !(fe.ToolTip is ToolTip tt))
			{
				return;
			}

			var pos = e.GetPosition(fe);
			tt.HorizontalOffset = pos.X + 10;
			tt.VerticalOffset = pos.Y + 10;
		}
	}
}
