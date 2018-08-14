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
		/// Saves the bot settings inside the parent grid.
		/// </summary>
		/// <param name="accessor"></param>
		/// <param name="parent"></param>
		/// <param name="settings"></param>
		public static void SaveBotSettings(IBotDirectoryAccessor accessor, Grid parent, IBotSettings settings)
		{
			foreach (var child in parent.GetChildren().OfType<FrameworkElement>())
			{
				if (!(SaveSetting(child, settings) is bool value))
				{
					continue;
				}
				ConsoleUtils.WriteLine(value ? $"Successfully updated {child.Name}." : $"Failed to save {child.Name}.");
			}
			settings.SaveSettings(accessor);
		}
		/// <summary>
		/// Saves the color settings inside the parent grid.
		/// </summary>
		/// <param name="accessor"></param>
		/// <param name="parent"></param>
		/// <param name="settings"></param>
		public static void SaveColorSettings(IBotDirectoryAccessor accessor, Grid parent, ColorSettings settings)
		{
			var children = parent.GetChildren();
			foreach (var tb in children.OfType<AdvobotTextBox>())
			{
				if (!(tb.Tag is ColorTarget target))
				{
					continue;
				}

				var childText = tb.Text;
				var name = target.ToString().FormatTitle().ToLower();
				//Removing a brush
				if (String.IsNullOrWhiteSpace(childText))
				{
					if (settings[target] != null)
					{
						settings[target] = null;
						ConsoleUtils.WriteLine($"Successfully removed the custom color for {name}.");
					}
				}
				//Failed to add a brush
				else if (!BrushUtils.TryCreateBrush(childText, out var brush))
				{
					ConsoleUtils.WriteLine($"Invalid custom color supplied for {name}: '{childText}'.");
				}
				//Succeeding in adding a brush
				else if (!BrushUtils.CheckIfSameBrush(settings[target], brush))
				{
					settings[target] = brush;
					ConsoleUtils.WriteLine($"Successfully updated the custom color for {name}: '{childText} ({settings[target].ToString()})'.");

					//Update the text here because if someone has the hex value for yellow but they put in Yellow as a string 
					//It won't update in the above if statement since they produce the same value
					tb.Text = settings[target].ToString();
				}
			}
			//Has to go after the textboxes so the theme will be applied
			foreach (var cb in children.OfType<AdvobotComboBox>())
			{
				if (!(cb.SelectedItem is ColorTheme theme))
				{
					continue;
				}
				if (settings.Theme == theme)
				{
					continue;
				}

				settings.Theme = theme;
				ConsoleUtils.WriteLine($"Successfully updated the theme to {settings.Theme.ToString().FormatTitle().ToLower()}.");
			}

			settings.SaveSettings(accessor);
		}
		private static bool? SaveSetting(FrameworkElement ele, IBotSettings botSettings)
		{
			//Go through children and not the actual object
			switch (ele)
			{
				case Grid g:
					var children = g.Children.OfType<FrameworkElement>();
					var results = children.Select(x => SaveSetting(x, botSettings)).Where(x => x != null).Cast<bool>();
					return results.Any() ? !results.Any(x => !x) : (bool?)null;
				case Viewbox vb:
					return vb.Child is FrameworkElement vbc ? SaveSetting(vbc, botSettings) : null;
			}

			object value = null;
			if (!(ele.Tag is string settingName))
			{
				return null;
			}
			switch (ele)
			{
				case AdvobotNumberBox nb:
					value = nb.StoredValue;
					break;
				case TextBox tb:
					var text = tb.Text;
					switch (settingName)
					{
						case nameof(IBotSettings.Prefix):
							if (String.IsNullOrWhiteSpace(text))
							{
								return false;
							}
							value = text;
							break;
						case nameof(IBotSettings.Game):
							value = text ?? "";
							break;
						case nameof(IBotSettings.Stream):
							if (!RegexUtils.IsValidTwitchName(text))
							{
								return false;
							}
							value = text;
							break;
					}
					break;
				case CheckBox cb:
					value = cb.IsChecked.Value;
					break;
				case ComboBox cmb:
					switch (settingName)
					{
						case nameof(IBotSettings.TrustedUsers):
							var updated = cmb.Items.OfType<TextBox>().Select(x => x?.Tag as ulong?).Where(x => x != null).Cast<ulong>();
							if (botSettings.TrustedUsers.Except(updated).Any() || updated.Except(botSettings.TrustedUsers).Any())
							{
								value = updated.ToList();
								break;
							}
							return null;
					}
					break;
				default:
					throw new ArgumentException("Invalid object when attempting to save settings.", ele.Name ?? ele.GetType().Name);
			}
			if (value == null)
			{
				return null;
			}

			var property = typeof(IBotSettings).GetProperty(settingName);
			if (value.GetType() != property.PropertyType)
			{
				return false;
			}
			else if (property.GetValue(botSettings).Equals(value))
			{
				return null;
			}
			property.SetValue(botSettings, value);
			return true;
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
