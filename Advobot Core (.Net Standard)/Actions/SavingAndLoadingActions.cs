using Advobot.Interfaces;
using Discord;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Advobot.Actions
{
	public static class SavingAndLoadingActions
	{
		private static bool _Loaded = false;

		public static async Task DoStartupActions(IDiscordClient client, IBotSettings botSettings)
		{
			if (_Loaded)
			{
				return;
			}

			if (Config.Configuration[ConfigKeys.Bot_Id] != client.CurrentUser.Id.ToString())
			{
				Config.Configuration[ConfigKeys.Bot_Id] = client.CurrentUser.Id.ToString();
				Config.Save();
				ConsoleActions.WriteLine("The bot needs to be restarted in order for the config to be loaded correctly.");
				ClientActions.RestartBot();
			}

			await ClientActions.UpdateGameAsync(client, botSettings);

			ConsoleActions.WriteLine("The current bot prefix is: " + botSettings.Prefix);
			ConsoleActions.WriteLine($"Bot took {DateTime.UtcNow.Subtract(Process.GetCurrentProcess().StartTime.ToUniversalTime()).TotalMilliseconds:n} milliseconds to load everything.");
			_Loaded = true;
		}

		public static string Serialize(object obj)
		{
			return JsonConvert.SerializeObject(obj, Formatting.Indented, new Newtonsoft.Json.Converters.StringEnumConverter());
		}
		public static void CreateFile(FileInfo fileInfo)
		{
			if (!fileInfo.Exists)
			{
				Directory.CreateDirectory(fileInfo.DirectoryName);
				fileInfo.Create().Close();
			}
		}
		public static void OverWriteFile(FileInfo fileInfo, string text)
		{
			CreateFile(fileInfo);
			using (var writer = new StreamWriter(fileInfo.FullName))
			{
				writer.Write(text);
			}
		}
		public static void DeleteFile(FileInfo fileInfo)
		{
			try
			{
				fileInfo.Delete();
			}
			catch (Exception e)
			{
				ConsoleActions.ExceptionToConsole(e);
			}
		}

		public static void LogUncaughtException(object sender, UnhandledExceptionEventArgs e)
		{
			var crashLogPath = GetActions.GetBaseBotDirectoryFile(Constants.CRASH_LOG_LOCATION);
			CreateFile(crashLogPath);
			//Use File.AppendText instead of new StreamWriter so the text doesn't get overwritten.
			using (var writer = crashLogPath.AppendText())
			{
				writer.WriteLine($"{FormattingActions.FormatReadableDateTime(DateTime.UtcNow)}: {e.ExceptionObject.ToString()}\n");
			}
		}
	}
}