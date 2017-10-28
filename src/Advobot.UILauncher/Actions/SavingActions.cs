using Advobot.Core;
using Advobot.Core.Actions;
using Advobot.Core.Actions.Formatting;
using Advobot.Core.Enums;
using Advobot.Core.Interfaces;
using Advobot.UILauncher.Classes;
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

namespace Advobot.UILauncher.Actions
{
	internal static class SavingActions
	{
		public static ToolTipReason SaveFile(TextEditor editor)
		{
			return SaveFile(editor, editor.Text);
		}
		public static ToolTipReason SaveFile(TextBox tb)
		{
			return SaveFile(tb, tb.Text);
		}
		private static ToolTipReason SaveFile(Control control, string text)
		{
			//If no valid tag just save to a new file with its name being the control's name
			var tag = control.Tag
				?? new FileInfo($"{control.Name}_{TimeFormatting.FormatDateTimeForSaving()}{Constants.GENERAL_FILE_EXTENSION}");
			if (!(tag is FileInformation fi))
			{
				return ToolTipReason.InvalidFilePath;
			}
			else if (fi.FileInfo.Name == Constants.GUILD_SETTINGS_LOCATION)
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
				SavingAndLoadingActions.OverWriteFile(fi.FileInfo, text);
				return ToolTipReason.FileSavingSuccess;
			}
			catch
			{
				return ToolTipReason.FileSavingFailure;
			}
		}
		public static FileType GetFileType(string file)
		{
			return Enum.TryParse(file, true, out FileType type) ? type : default;
		}

		public static async Task SaveSettings(Grid parent, IDiscordClient client, IBotSettings botSettings)
		{
			foreach (var child in parent.GetChildren())
			{
				if (child is FrameworkElement ele && !SaveSetting(ele, botSettings))
				{
					ConsoleActions.WriteLine($"Failed to save: {ele.Name}");
				}
			}
			await ClientActions.UpdateGameAsync(client, botSettings);
		}
		private static bool SaveSetting(object obj, IBotSettings botSettings)
		{
			//Go through children and not the actual object
			if (obj is Grid g)
			{
				return !g.Children.OfType<FrameworkElement>().Select(x => SaveSetting(x, botSettings)).Any(x => !x);
			}
			else if (obj is Viewbox vb)
			{
				return SaveSetting(vb.Child, botSettings);
			}
			//If object isn't a frameworkele or has no tag then it's not a setting
			else if (!(obj is FrameworkElement f) || f.Tag == null)
			{
				return true;
			}
			else if (obj is TextBox tb && tb.Tag is BotSetting tbs)
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
			else if (obj is CheckBox cb && cb.Tag is BotSetting cbs)
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
			else if (obj is ComboBox cmb && cmb.Tag is BotSetting cmbs)
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

			throw new ArgumentException($"Invalid object provided when attempting to save settings for a {obj.GetType().Name}.");
		}
	}
}
