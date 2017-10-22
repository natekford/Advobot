using Advobot.Core.Actions;
using Advobot.UILauncher.Enums;
using Advobot.Core.Interfaces;
using Discord;
using ICSharpCode.AvalonEdit;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using Advobot.UILauncher.Classes;
using Advobot.Core.Actions.Formatting;
using Advobot.Commands;

namespace Advobot.UILauncher.Actions
{
	internal class UIBotWindowLogic
	{
		private static Dictionary<ToolTipReason, string> _ToolTipReasons = new Dictionary<ToolTipReason, string>
		{
			{ ToolTipReason.FileSavingFailure, "Failed to save the file." },
			{ ToolTipReason.FileSavingSuccess, "Successfully saved the file." },
			{ ToolTipReason.InvalidFilePath, "Unable to gather the path for this file." },
		};

		public static async Task SaveSettings(Grid parent, IDiscordClient client, IBotSettings botSettings)
		{
			//Go through each setting and update them
			for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); ++i)
			{
				var ele = VisualTreeHelper.GetChild(parent, i);
				var settingName = (ele as FrameworkElement)?.Tag;
				if (settingName is string && !SaveSetting(ele, settingName.ToString(), botSettings))
				{
					ConsoleActions.WriteLine($"Failed to save: {settingName.ToString()}");
				}
			}

			await ClientActions.UpdateGameAsync(client, botSettings);
		}
		private static bool SaveSetting(object obj, string settingName, IBotSettings botSettings)
		{
			if (obj is Grid)
			{
				return SaveSetting(obj as Grid, settingName, botSettings);
			}
			else if (obj is TextBox)
			{
				return SaveSetting(obj as TextBox, settingName, botSettings);
			}
			else if (obj is Viewbox)
			{
				return SaveSetting(obj as Viewbox, settingName, botSettings);
			}
			else if (obj is CheckBox)
			{
				return SaveSetting(obj as CheckBox, settingName, botSettings);
			}
			else if (obj is ComboBox)
			{
				return SaveSetting(obj as ComboBox, settingName, botSettings);
			}
			else
			{
				throw new ArgumentException("Invalid object provided when attempting to save settings.");
			}
		}
		private static bool SaveSetting(Grid g, string settingName, IBotSettings botSettings)
		{
			var children = g.Children;
			foreach (var child in children)
			{
				return SaveSetting(child, settingName, botSettings);
			}
			return true;
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
					var selectedLogLevel = (LogSeverity)(cb.SelectedItem as TextBox).Tag;
					if (botSettings.LogLevel != selectedLogLevel)
					{
						botSettings.LogLevel = selectedLogLevel;
					}
					return true;
				}
				case nameof(IBotSettings.TrustedUsers):
				{
					var updatedTrustedUsers = cb.Items.OfType<TextBox>().Select(x => (ulong)x.Tag).ToList();
					var removedUsers = botSettings.TrustedUsers.Except(updatedTrustedUsers);
					var addedUsers = updatedTrustedUsers.Except(botSettings.TrustedUsers);
					if (removedUsers.Any() || addedUsers.Any())
					{
						botSettings.TrustedUsers = updatedTrustedUsers;
					}
					return true;
				}
				default:
				{
					throw new ArgumentException($"Invalid object provided when attempting to save settings for a {cb.GetType().Name}.");
				}
			}
		}

		public static string GetReasonTextFromToolTipReason(ToolTipReason reason)
		{
			return _ToolTipReasons[reason];
		}
		public static ToolTipReason SaveFile(TextEditor tb)
		{
			var fileInfo = ((FileInformation)tb.Tag).FileInfo;
			if (fileInfo == null || !fileInfo.Exists)
			{
				return ToolTipReason.InvalidFilePath;
			}
			else if (fileInfo.Name.Equals(Constants.GUILD_SETTINGS_LOCATION))
			{
				//Make sure the guild info stays valid
				try
				{
					var throwaway = JsonConvert.DeserializeObject(tb.Text, Constants.GUILD_SETTINGS_TYPE);
				}
				catch (Exception exc)
				{
					ConsoleActions.ExceptionToConsole(exc);
					return ToolTipReason.FileSavingFailure;
				}
			}

			try
			{
				SavingAndLoadingActions.OverWriteFile(fileInfo, tb.Text);
				return ToolTipReason.FileSavingSuccess;
			}
			catch
			{
				return ToolTipReason.FileSavingFailure;
			}
		}
		public static ToolTipReason SaveOutput(TextBox tb)
		{
			var fileName = $"Output_Log_{TimeFormatting.FormatDateTimeForSaving()}{Constants.GENERAL_FILE_EXTENSION}";
			var fileInfo = GetActions.GetBaseBotDirectoryFile(fileName);
			try
			{
				SavingAndLoadingActions.OverWriteFile(fileInfo, tb.Text);
				return ToolTipReason.FileSavingSuccess;
			}
			catch
			{
				return ToolTipReason.FileSavingFailure;
			}
		}

		public static FileType? GetFileType(string file)
		{
			return Enum.TryParse(file, true, out FileType type) ? type as FileType? : null;
		}

		public static void PauseBot(IBotSettings botSettings)
		{
			if (botSettings.Pause)
			{
				ConsoleActions.WriteLine("The bot is now unpaused.");
				botSettings.TogglePause();
			}
			else
			{
				ConsoleActions.WriteLine("The bot is now paused.");
				botSettings.TogglePause();
			}
		}

		public static bool AppendTextToTextEditorIfPathExists(TextEditor display, TreeViewItem treeItem)
		{
			var fileInfo = ((FileInformation)treeItem.Tag).FileInfo;
			if (fileInfo != null && fileInfo.Exists)
			{
				display.Clear();
				display.Tag = fileInfo;
				using (var reader = new StreamReader(fileInfo.FullName))
				{
					display.AppendText(reader.ReadToEnd());
				}
				return true;
			}
			else
			{
				ConsoleActions.WriteLine("Unable to bring up the file.");
				return false;
			}
		}

		public static async Task<IServiceProvider> GetPath(string path, bool startup)
		{
			if (Config.ValidatePath(path, startup))
			{
				var provider = await CreationActions.CreateServiceProvider().CAF();
				CommandHandler.Install(provider);
				return provider;
			}
			return null;
		}
		public static async Task<bool> GetKey(IDiscordClient client, string key, bool startup)
		{
			return await Config.ValidateBotKey(client, key, startup);
		}

		public static async Task AddTrustedUserToComboBox(ComboBox cb, IDiscordClient client, string input)
		{
			if (String.IsNullOrWhiteSpace(input))
			{
				return;
			}
			else if (ulong.TryParse(input, out ulong userID))
			{
				var currTBs = cb.Items.Cast<TextBox>().ToList();
				if (currTBs.Any(x => (ulong)x.Tag == userID))
					return;

				var tb = UIModification.MakeTextBoxFromUserID(await client.GetUserAsync(userID));
				if (tb != null)
				{
					currTBs.Add(tb);
					cb.ItemsSource = currTBs;
				}
			}
			else
			{
				ConsoleActions.WriteLine($"The given input '{input}' is not a valid ID.");
			}
		}
		public static void RemoveTrustedUserFromComboBox(ComboBox cb)
		{
			if (cb.SelectedItem == null)
				return;

			cb.ItemsSource = cb.Items.Cast<TextBox>().Where(x => (ulong)x.Tag != (ulong)((TextBox)cb.SelectedItem).Tag).ToList();
		}
	}

	internal class UICommandHandler
	{
		public static string GatherInput(TextBox tb, Button b)
		{
			var text = tb.Text.Trim(new[] { '\r', '\n' });
			if (text.Contains("﷽"))
			{
				text += "This program really doesn't like that long Arabic character for some reason. Whenever there are a lot of them it crashes the program completely.";
			}

			ConsoleActions.WriteLine(text);

			tb.Text = "";
			b.IsEnabled = false;

			return text;
		}
		public static void HandleCommand(string input, string prefix)
		{
			if (input.CaseInsStartsWith(prefix))
			{
				var inputArray = input.Substring(prefix.Length)?.Split(new[] { ' ' }, 2);
				if (!FindCommand(inputArray[0], inputArray.Length > 1 ? inputArray[1] : null))
				{
					ConsoleActions.WriteLine("No command could be found with that name.");
				}
			}
		}
		public static bool FindCommand(string cmd, string args)
		{
			return false;
		}
	}
}