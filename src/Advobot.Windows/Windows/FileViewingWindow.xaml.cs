using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Advobot.Interfaces;
using Advobot.Windows.Classes.Controls;
using Advobot.Windows.Utilities;
using AdvorangesUtils;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace Advobot.Windows.Windows
{
	/// <summary>
	/// Interaction logic for FileViewingWindow.xaml
	/// </summary>
	internal partial class FileViewingWindow : ModalWindow
	{
		private FileInfo _File;
		private Type _FileType;

		public FileViewingWindow() : this(null, null, null, null, null) { }
		public FileViewingWindow(AdvobotWindow mainWindow, ILowLevelConfig config, Type fileType, FileInfo fileInfo, string text)
			: base(mainWindow, config)
		{
			InitializeComponent();
			_File = fileInfo ?? throw new ArgumentNullException(nameof(fileInfo));
			_FileType = fileType ?? throw new ArgumentNullException(nameof(fileType));
			SpecificFileOutput.Tag = fileInfo;
			SpecificFileOutput.Clear();
			SpecificFileOutput.AppendText(text);
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
						_File.CopyTo(Path.Combine(dialog.FileName, _File.Name), true);
						ToolTipUtils.EnableTimedToolTip(window.Layout, $"Successfully copied {_File.Name} to {dialog.FileName}.");
						return;
				}
			}
		}
		private void DeleteFile(object sender, RoutedEventArgs e)
		{
			var text = $"Are you sure you want to delete the file {_File.Name}?";
			var caption = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyProductAttribute>().Product;
			switch (MessageBox.Show(text, caption, MessageBoxButton.YesNo))
			{
				case MessageBoxResult.Yes:
					try
					{
						_File.Delete();
					}
					catch (Exception ex)
					{
						ex.Write();
					}
					return;
			}
		}
		private void SaveFile(object sender, RoutedEventArgs e)
		{
			ToolTipUtils.EnableTimedToolTip(Layout, SavingUtils.SaveFile(Config, SpecificFileOutput, _FileType).GetReason());
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
			var caption = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyProductAttribute>().Product;
			switch (MessageBox.Show("Are you sure you want to close the file window?", caption, MessageBoxButton.YesNo))
			{
				case MessageBoxResult.Yes:
					Close(sender, e);
					return;
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
