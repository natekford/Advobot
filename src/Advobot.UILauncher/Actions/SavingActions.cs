using Advobot.Core;
using Advobot.Core.Actions;
using Advobot.Core.Actions.Formatting;
using Advobot.Core.Enums;
using Advobot.Core.Interfaces;
using Advobot.UILauncher.Enums;
using Discord;
using ICSharpCode.AvalonEdit;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Advobot.UILauncher.Actions
{
	internal static class SavingActions
	{
		/// <summary>
		/// Saves the text of <paramref name="editor"/> to file.
		/// </summary>
		/// <param name="editor"></param>
		/// <returns></returns>
		public static ToolTipReason SaveFile(TextEditor editor)
		{
			return SaveFile(editor, editor.Text);
		}
		/// <summary>
		/// Saves the text of <paramref name="tb"/> to file.
		/// </summary>
		/// <param name="tb"></param>
		/// <returns></returns>
		public static ToolTipReason SaveFile(TextBox tb)
		{
			return SaveFile(tb, tb.Text);
		}
		private static ToolTipReason SaveFile(Control control, string text)
		{
			//If no valid tag just save to a new file with its name being the control's name
			var tag = control.Tag ?? CreateFileInfo(control);
			if (!(tag is FileInfo fi))
			{
				return ToolTipReason.InvalidFilePath;
			}
			else if (fi.Name == Constants.GUILD_SETTINGS_LOCATION)
			{
				//Make sure the guild info stays valid
				try
				{
					var throwaway = JsonConvert.DeserializeObject(text, Constants.GUILD_SETTINGS_TYPE);
				}
				catch (Exception e)
				{
					ConsoleActions.ExceptionToConsole(e);
					return ToolTipReason.FileSavingFailure;
				}
			}

			try
			{
				SavingAndLoadingActions.OverWriteFile(fi, text);
				return ToolTipReason.FileSavingSuccess;
			}
			catch
			{
				return ToolTipReason.FileSavingFailure;
			}
		}
		private static FileInfo CreateFileInfo(Control control)
		{
			var baseDir = GetActions.GetBaseBotDirectory().FullName;
			var fileName = $"{control.Name}_{TimeFormatting.FormatDateTimeForSaving()}{Constants.GENERAL_FILE_EXTENSION}";
			return new FileInfo(Path.Combine(baseDir, fileName));
		}

		/// <summary>
		/// Saves every setting that is a child of <paramref name="parent"/>.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="botSettings"></param>
		public static void SaveSettings(Grid parent, IBotSettings botSettings)
		{
			foreach (var child in parent.GetChildren().OfType<FrameworkElement>())
			{
				if (!SaveSetting(child, botSettings))
				{
					ConsoleActions.WriteLine($"Failed to save: {child.Name}");
				}
			}
		}
		private static bool SaveSetting(FrameworkElement ele, IBotSettings botSettings)
		{
			//Go through children and not the actual object
			if (ele is Grid g)
			{
				return !g.Children.OfType<FrameworkElement>().Select(x => SaveSetting(x, botSettings)).Any(x => !x);
			}
			else if (ele is Viewbox vb && vb.Child is FrameworkElement vbc)
			{
				return SaveSetting(vbc, botSettings);
			}
			else if (ele is TextBox tb && tb.Tag is BotSetting tbs)
			{
				if (tb.IsReadOnly)
				{
					return true;
				}

				var text = tb.Text;
				switch (tbs)
				{
					case BotSetting.Prefix:
					{
						if (String.IsNullOrWhiteSpace(text))
						{
							return false;
						}
						else if (botSettings.Prefix != text)
						{
							botSettings.Prefix = text;
						}
						return true;
					}
					case BotSetting.Game:
					{
						if (botSettings.Game != text)
						{
							botSettings.Game = text;
						}
						return true;
					}
					case BotSetting.Stream:
					{
						if (!RegexActions.CheckIfInputIsAValidTwitchName(text))
						{
							return false;
						}
						else if (botSettings.Stream != text)
						{
							botSettings.Stream = text;
						}
						return true;
					}
					case BotSetting.ShardCount:
					{
						if (!uint.TryParse(text, out uint num))
						{
							return false;
						}
						else if (botSettings.ShardCount != num)
						{
							botSettings.ShardCount = (int)num;
						}
						return true;
					}
					case BotSetting.MessageCacheCount:
					{
						if (!uint.TryParse(text, out uint num))
						{
							return false;
						}
						else if (botSettings.MessageCacheCount != num)
						{
							botSettings.MessageCacheCount = (int)num;
						}
						return true;
					}
					case BotSetting.MaxUserGatherCount:
					{
						if (!uint.TryParse(text, out uint num))
						{
							return false;
						}
						else if (botSettings.MaxUserGatherCount != num)
						{
							botSettings.MaxUserGatherCount = (int)num;
						}
						return true;
					}
					case BotSetting.MaxMessageGatherSize:
					{
						if (!uint.TryParse(text, out uint num))
						{
							return false;
						}
						else if (botSettings.MaxMessageGatherSize != num)
						{
							botSettings.MaxMessageGatherSize = (int)num;
						}
						return true;
					}
					case BotSetting.TrustedUsers:
					{
						return true;
					}
				}
			}
			else if (ele is CheckBox cb && cb.Tag is BotSetting cbs)
			{
				var isChecked = cb.IsChecked.Value;
				switch (cbs)
				{
					case BotSetting.AlwaysDownloadUsers:
					{
						if (botSettings.AlwaysDownloadUsers != isChecked)
						{
							botSettings.AlwaysDownloadUsers = isChecked;
						}
						return true;
					}
				}
			}
			else if (ele is ComboBox cmb && cmb.Tag is BotSetting cmbs)
			{
				switch (cmbs)
				{
					case BotSetting.LogLevel:
					{
						if (cmb.SelectedItem is TextBox cmbtb && cmbtb.Tag is LogSeverity ls && botSettings.LogLevel != ls)
						{
							botSettings.LogLevel = ls;
						}
						return true;
					}
					case BotSetting.TrustedUsers:
					{
						var updated = cmb.Items.OfType<TextBox>().Select(x => x?.Tag as ulong? ?? 0).Where(x => x != 0);
						if (botSettings.TrustedUsers.Except(updated).Any() || updated.Except(botSettings.TrustedUsers).Any())
						{
							botSettings.TrustedUsers = updated.ToList();
						}
						return true;
					}
				}
			}
			else
			{
				return true;
			}

			throw new ArgumentException($"Invalid object provided when attempting to save settings for a {ele.Name ?? ele.GetType().Name}.");
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
		/// Attempts to get a text file from an element's tag.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="text"></param>
		/// <param name="fileInfo"></param>
		/// <returns></returns>
		public static bool TryGetFileText(object sender, out string text, out FileInfo fileInfo)
		{
			text = null;
			fileInfo = null;
			if (sender is FrameworkElement element && element.Tag is FileInfo fi && fi.Exists)
			{
				using (var reader = new StreamReader(fi.FullName))
				{
					text = reader.ReadToEnd();
					fileInfo = fi;
				}
				return true;
			}

			ConsoleActions.WriteLine("Unable to bring up the file.");
			return false;
		}
	}
}
