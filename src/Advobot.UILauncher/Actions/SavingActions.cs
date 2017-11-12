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
		public static ToolTipReason SaveFile(TextEditor editor) => SaveFile(editor, editor.Text);
		/// <summary>
		/// Saves the text of <paramref name="tb"/> to file.
		/// </summary>
		/// <param name="tb"></param>
		/// <returns></returns>
		public static ToolTipReason SaveFile(TextBox tb) => SaveFile(tb, tb.Text);
		/// <summary>
		/// Attempts to save a file and returns a value indicating the result.
		/// </summary>
		/// <param name="control"></param>
		/// <param name="text"></param>
		/// <returns></returns>
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
		/// <summary>
		/// Creates a <see cref="FileInfo"/> based off of <paramref name="control"/> name.
		/// </summary>
		/// <param name="control"></param>
		/// <returns></returns>
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
			else if (ele is Viewbox vb)
			{
				return vb.Child is FrameworkElement vbc ? SaveSetting(vbc, botSettings) : true;
			}

			object value = null;
			if (!(ele.Tag is BotSetting setting))
			{
				return false;
			}
			else if (ele is TextBox tb)
			{
				var text = tb.Text;
				switch (setting)
				{
					case BotSetting.Prefix:
					{
						if (String.IsNullOrWhiteSpace(text))
						{
							return false;
						}
						value = text;
						break;
					}
					case BotSetting.Game:
					{
						value = text ?? "";
						break;
					}
					case BotSetting.Stream:
					{
						if (!RegexActions.CheckIfInputIsAValidTwitchName(text))
						{
							return false;
						}
						value = text;
						break;
					}
					case BotSetting.ShardCount:
					{
						if (!uint.TryParse(text, out uint num))
						{
							return false;
						}
						value = (int)num;
						break;
					}
					case BotSetting.MessageCacheCount:
					{
						if (!uint.TryParse(text, out uint num))
						{
							return false;
						}
						value = (int)num;
						break;
					}
					case BotSetting.MaxUserGatherCount:
					{
						if (!uint.TryParse(text, out uint num))
						{
							return false;
						}
						value = (int)num;
						break;
					}
					case BotSetting.MaxMessageGatherSize:
					{
						if (!uint.TryParse(text, out uint num))
						{
							return false;
						}
						value = (int)num;
						break;
					}
				}
			}
			else if (ele is CheckBox cb)
			{
				value = cb.IsChecked.Value;
			}
			else if (ele is ComboBox cmb)
			{
				switch (setting)
				{
					case BotSetting.LogLevel:
					{
						if (cmb.SelectedItem is TextBox cmbtb && cmbtb.Tag is LogSeverity ls)
						{
							value = ls;
						}
						break;
					}
					case BotSetting.TrustedUsers:
					{
						var updated = cmb.Items.OfType<TextBox>().Select(x => x?.Tag as ulong? ?? 0).Where(x => x != 0);
						if (botSettings.TrustedUsers.Except(updated).Any() || updated.Except(botSettings.TrustedUsers).Any())
						{
							value = updated.ToList();
						}
						break;
					}
				}
			}
			else
			{
				var name = ele.Name ?? ele.GetType().Name;
				throw new ArgumentException($"Invalid object provided when attempting to save settings for a {name}.");
			}

			var field = typeof(IBotSettings).GetProperty(setting.EnumName());
			//Make sure value isn't null 
			if (value != null && value.GetType() == field.PropertyType && !field.GetValue(botSettings).Equals(value))
			{
				field.SetValue(botSettings, value);
			}
			return true;
		}

		/// <summary>
		/// Returns true if the key from <paramref name="e"/> is <see cref="Key.S"/> and control is pressed.
		/// </summary>
		/// <param name="e"></param>
		/// <returns></returns>
		public static bool IsCtrlS(KeyEventArgs e) => e.Key == Key.S && e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Control);
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
