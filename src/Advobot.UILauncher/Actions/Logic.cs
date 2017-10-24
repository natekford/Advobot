using Advobot.Commands;
using Advobot.Core;
using Advobot.Core.Actions;
using Advobot.Core.Actions.Formatting;
using Advobot.Core.Interfaces;
using Advobot.UILauncher.Classes;
using Advobot.UILauncher.Enums;
using Discord;
using ICSharpCode.AvalonEdit;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Advobot.UILauncher.Actions
{
	internal static class UIBotWindowLogic
	{
		private static Dictionary<ToolTipReason, string> _ToolTipReasons = new Dictionary<ToolTipReason, string>
		{
			{ ToolTipReason.FileSavingFailure, "Failed to save the file." },
			{ ToolTipReason.FileSavingSuccess, "Successfully saved the file." },
			{ ToolTipReason.InvalidFilePath, "Unable to gather the path for this file." },
		};

		public static string GetReason(this ToolTipReason reason)
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
		public static FileType GetFileType(string file)
		{
			return Enum.TryParse(file, true, out FileType type) ? type : default;
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

		public static IEnumerable<FrameworkElement> GetChildren(this DependencyObject parent)
		{
			for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); ++i)
			{
				yield return (FrameworkElement)VisualTreeHelper.GetChild(parent, i);
			}
		}
	}
}