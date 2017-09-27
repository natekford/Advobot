using Advobot.Actions.Formatting;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.IO;

namespace Advobot.Actions
{
	public static class SavingAndLoadingActions
	{
		public static string Serialize(object obj)
		{
			return JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.Indented, new StringEnumConverter());
		}
		public static T Deserialize<T>(string value, Type type)
		{
			return (T)JsonConvert.DeserializeObject(value, type, new StringEnumConverter());
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
				writer.WriteLine($"{TimeFormatting.FormatReadableDateTime(DateTime.UtcNow)}: {e.ExceptionObject.ToString()}\n");
			}
		}
	}
}