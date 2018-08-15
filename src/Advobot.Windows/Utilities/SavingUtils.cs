using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Advobot.Interfaces;
using Advobot.Utilities;
using Advobot.Windows.Classes;
using Advobot.Windows.Classes.Controls;
using Advobot.Windows.Enums;
using AdvorangesUtils;
using ICSharpCode.AvalonEdit;
using Newtonsoft.Json;

namespace Advobot.Windows.Utilities
{
	internal static class SavingUtils
	{
		/// <summary>
		/// Saves the text of <paramref name="editor"/> to file.
		/// </summary>
		/// <param name="accessor"></param>
		/// <param name="editor"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public static ToolTipReason SaveFile(IBotDirectoryAccessor accessor, TextEditor editor, Type type = null)
		{
			return SaveFile(accessor, editor, editor.Text, type);
		}
		/// <summary>
		/// Saves the text of <paramref name="tb"/> to file.
		/// </summary>
		/// <param name="accessor"></param>
		/// <param name="tb"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public static ToolTipReason SaveFile(IBotDirectoryAccessor accessor, TextBox tb, Type type = null)
		{
			return SaveFile(accessor, tb, tb.Text, type);
		}
		/// <summary>
		/// Attempts to save a file and returns a value indicating the result.
		/// </summary>
		/// <param name="accessor"></param>
		/// <param name="control"></param>
		/// <param name="text"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		private static ToolTipReason SaveFile(IBotDirectoryAccessor accessor, Control control, string text, Type type)
		{
			//If no valid tag just save to a new file with its name being the control's name
			var tag = control.Tag ?? CreateFileInfo(accessor, control);
			if (!(tag is FileInfo fi))
			{
				return ToolTipReason.InvalidFilePath;
			}
			if (type != null)
			{
				try
				{
					var throwaway = JsonConvert.DeserializeObject(text, type);
				}
				catch (JsonReaderException jre)
				{
					jre.Write();
					return ToolTipReason.FileSavingFailure;
				}
			}

			try
			{
				IOUtils.SafeWriteAllText(fi, text);
				return ToolTipReason.FileSavingSuccess;
			}
			catch
			{
				return ToolTipReason.FileSavingFailure;
			}
		}
		/// <summary>
		/// Creates a <see cref="FileInfo"/> based off of <paramref name="control"/> name.
		/// </summary>
		/// <param name="accessor"></param>
		/// <param name="control"></param>
		/// <returns></returns>
		private static FileInfo CreateFileInfo(IBotDirectoryAccessor accessor, Control control)
		{
			return accessor.GetBaseBotDirectoryFile($"{control.Name}_{FormattingUtils.ToSaving()}.txt");
		}
		/// <summary>
		/// Returns true if the key from <paramref name="e"/> is <see cref="Key.S"/> and control is pressed.
		/// </summary>
		/// <param name="e"></param>
		/// <returns></returns>
		public static bool IsCtrlS(KeyEventArgs e)
		{
			return e.Key == Key.S && e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Control);
		}
		/// <summary>
		/// Attempts to get a text file from a path.
		/// </summary>
		/// <param name="path"></param>
		/// <param name="text"></param>
		/// <param name="fileInfo"></param>
		/// <returns></returns>
		public static bool TryGetFileText(string path, out string text, out FileInfo fileInfo)
		{
			text = null;
			fileInfo = null;
			if (File.Exists(path))
			{
				try
				{
					using (var reader = new StreamReader(path))
					{
						text = reader.ReadToEnd();
						fileInfo = new FileInfo(path);
					}
					return true;
				}
				catch (Exception e)
				{
					e.Write();
					return false;
				}
			}

			ConsoleUtils.WriteLine("Unable to bring up the file.");
			return false;
		}
	}
}
