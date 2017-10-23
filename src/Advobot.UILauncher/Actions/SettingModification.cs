using Advobot.Core.Actions;
using Advobot.Core.Interfaces;
using Advobot.UILauncher.Classes;
using Discord;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Advobot.UILauncher.Actions
{
	internal static class SettingModification
	{
		public static async Task SaveSettings(Grid parent, IDiscordClient client, IBotSettings botSettings)
		{
			foreach (var child in parent.GetChildren())
			{
				if (child is FrameworkElement ele && ele.Tag is string name && !SaveSetting(ele, name, botSettings))
				{
					ConsoleActions.WriteLine($"Failed to save: {name}");
				}
			}
			await ClientActions.UpdateGameAsync(client, botSettings);
		}
		private static bool SaveSetting(object obj, string settingName, IBotSettings botSettings)
		{
			if (obj is Grid g)
			{
				return SaveSetting(g, settingName, botSettings);
			}
			else if (obj is TextBox tb)
			{
				return SaveSetting(tb, settingName, botSettings);
			}
			else if (obj is Viewbox vb)
			{
				return SaveSetting(vb, settingName, botSettings);
			}
			else if (obj is CheckBox cb)
			{
				return SaveSetting(cb, settingName, botSettings);
			}
			else if (obj is ComboBox cmb)
			{
				return SaveSetting(cmb, settingName, botSettings);
			}
			else
			{
				throw new ArgumentException("Invalid object provided when attempting to save settings.");
			}
		}
		private static bool SaveSetting(Grid g, string settingName, IBotSettings botSettings)
		{
			var success = true;
			foreach (var child in g.Children)
			{
				success = success && SaveSetting(child, settingName, botSettings);
			}
			return success;
		}
		private static bool SaveSetting(TextBox tb, string settingName, IBotSettings botSettings)
		{
			if (tb.IsReadOnly)
			{
				return true;
			}

			var text = tb.Text;
			switch (settingName)
			{
				case nameof(IBotSettings.Prefix):
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
				case nameof(IBotSettings.Game):
				{
					if (botSettings.Game != text)
					{
						botSettings.Game = text;
					}
					return true;
				}
				case nameof(IBotSettings.Stream):
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
				case nameof(IBotSettings.ShardCount):
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
				case nameof(IBotSettings.MessageCacheCount):
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
				case nameof(IBotSettings.MaxUserGatherCount):
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
				case nameof(IBotSettings.MaxMessageGatherSize):
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
				case nameof(IBotSettings.TrustedUsers):
				{
					return true;
				}
				default:
				{
					throw new ArgumentException($"Invalid object provided when attempting to save settings for a {tb.GetType().Name}.");
				}
			}
		}
		private static bool SaveSetting(Viewbox vb, string settingName, IBotSettings botSettings)
		{
			return SaveSetting(vb.Child, settingName, botSettings);
		}
		private static bool SaveSetting(CheckBox cb, string settingName, IBotSettings botSettings)
		{
			var isChecked = cb.IsChecked.Value;
			switch (settingName)
			{
				case nameof(IBotSettings.AlwaysDownloadUsers):
				{
					if (botSettings.AlwaysDownloadUsers != isChecked)
					{
						botSettings.AlwaysDownloadUsers = isChecked;
					}
					return true;
				}
				default:
				{
					throw new ArgumentException($"Invalid object provided when attempting to save settings for a {cb.GetType().Name}.");
				}
			}
		}
		private static bool SaveSetting(ComboBox cb, string settingName, IBotSettings botSettings)
		{
			switch (settingName)
			{
				case nameof(IBotSettings.LogLevel):
				{
					if (cb.SelectedItem is TextBox tb && tb.Tag is LogSeverity ls && botSettings.LogLevel != ls)
					{
						botSettings.LogLevel = ls;
					}
					return true;
				}
				case nameof(IBotSettings.TrustedUsers):
				{
					var updated = cb.Items.OfType<TextBox>().Select(x => x?.Tag as ulong? ?? 0).Where(x => x != 0);
					if (botSettings.TrustedUsers.Except(updated).Any() || updated.Except(botSettings.TrustedUsers).Any())
					{
						botSettings.TrustedUsers = updated.ToList();
					}
					return true;
				}
				default:
				{
					throw new ArgumentException($"Invalid object provided when attempting to save settings for a {cb.GetType().Name}.");
				}
			}
		}

		public static async Task AddTrustedUserToComboBox(ComboBox cb, IDiscordClient client, string input)
		{
			if (!ulong.TryParse(input, out ulong userId))
			{
				ConsoleActions.WriteLine($"The given input '{input}' is not a valid ID.");
			}
			else if (cb.Items.OfType<TextBox>().Any(x => x?.Tag is ulong id && id == userId))
			{
				return;
			}

			var tb = AdvobotTextBox.CreateUserBox(await client.GetUserAsync(userId));
			if (tb != null)
			{
				cb.ItemsSource = cb.ItemsSource.OfType<TextBox>().Concat(new[] { tb }).Where(x => x != null);
			}
		}
		public static void RemoveTrustedUserFromComboBox(ComboBox cb)
		{
			if (cb.SelectedItem != null)
			{
				cb.ItemsSource = cb.ItemsSource.OfType<TextBox>().Except(new[] { cb.SelectedItem }).Where(x => x != null);
			}
		}
	}
}
