using Advobot.Core.Actions;
using Advobot.Core.Interfaces;
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
				var success = true;
				foreach (var child in g.Children)
				{
					success = success && SaveSetting(child, settingName, botSettings);
				}
				return success;
			}
			else if (obj is TextBox tb)
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
				}
			}
			else if (obj is Viewbox vb)
			{
				return SaveSetting(vb.Child, settingName, botSettings);
			}
			else if (obj is CheckBox cb)
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
				}
			}
			else if (obj is ComboBox cmb)
			{
				switch (settingName)
				{
					case nameof(IBotSettings.LogLevel):
					{
						if (cmb.SelectedItem is TextBox cmbtb && cmbtb.Tag is LogSeverity ls && botSettings.LogLevel != ls)
						{
							botSettings.LogLevel = ls;
						}
						return true;
					}
					case nameof(IBotSettings.TrustedUsers):
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
