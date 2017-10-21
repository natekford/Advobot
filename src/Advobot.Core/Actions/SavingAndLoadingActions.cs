using Advobot.Core.Actions.Formatting;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.IO;

namespace Advobot.Core.Actions
{
	public static class SavingAndLoadingActions
	{
		/// <summary>
		/// Creates a file if it does not already exist.
		/// </summary>
		/// <param name="fileInfo"></param>
		public static void CreateFile(FileInfo fileInfo)
		{
			if (!fileInfo.Exists)
			{
				Directory.CreateDirectory(fileInfo.DirectoryName);
				fileInfo.Create().Close();
			}
		}
		/// <summary>
		/// Creates a file if it does not already exist, then writes over it.
		/// </summary>
		/// <param name="fileInfo"></param>
		/// <param name="text"></param>
		public static void OverWriteFile(FileInfo fileInfo, string text)
		{
			CreateFile(fileInfo);
			using (var writer = new StreamWriter(fileInfo.FullName))
			{
				writer.Write(text);
			}
		}
		/// <summary>
		/// Attempts to delete the supplied file.
		/// </summary>
		/// <param name="fileInfo"></param>
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

		/// <summary>
		/// Converts the object to JSON.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public static string Serialize(object obj)
		{
			return JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.Indented, new StringEnumConverter());
		}
		/// <summary>
		/// Creates an object of type <typeparamref name="T"/> with the supplied string and type.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="value"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public static T Deserialize<T>(string value, Type type)
		{
			return (T)JsonConvert.DeserializeObject(value, type, new StringEnumConverter());
		}

		/// <summary>
		/// Writes an uncaught exception to a log file.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
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