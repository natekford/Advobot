using Advobot.Core.Utilities.Formatting;
using Advobot.Core.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Diagnostics;
using System.IO;

namespace Advobot.Core.Utilities
{
	/// <summary>
	/// Actions involving saving and loading.
	/// </summary>
	public static class IOUtils
	{
		/// <summary>
		/// Returns the <see cref="Process.WorkingSet64"/> value divided by a MB.
		/// </summary>
		/// <returns></returns>
		public static double GetMemory()
		{
			using (var process = Process.GetCurrentProcess())
			{
				process.Refresh();
				return process.PrivateMemorySize64 / (1024.0 * 1024.0);
			}
		}

		/// <summary>
		/// Assuming the save path is C:\Users\User\AppData\Roaming, returns C:\Users\User\AppData\Roaming\Discord_Servers_BotId\ServerId
		/// </summary>
		/// <param name="guildId"></param>
		/// <returns></returns>
		public static DirectoryInfo GetServerDirectory(ulong guildId)
			=> Directory.CreateDirectory(Path.Combine(GetBaseBotDirectory().FullName, guildId.ToString()));
		/// <summary>
		/// Assuming the save path is C:\Users\User\AppData\Roaming, returns C:\Users\User\AppData\Roaming\Discord_Servers_BotId\ServerId\File
		/// </summary>
		/// <param name="guildId"></param>
		/// <param name="fileName"></param>
		/// <returns></returns>
		public static FileInfo GetServerDirectoryFile(ulong guildId, string fileName)
			=> new FileInfo(Path.Combine(GetServerDirectory(guildId).FullName, fileName));
		/// <summary>
		/// Assuming the save path is C:\Users\User\AppData\Roaming, returns C:\Users\User\AppData\Roaming\Discord_Servers_BotId
		/// </summary>
		/// <returns></returns>
		public static DirectoryInfo GetBaseBotDirectory()
			=> Directory.CreateDirectory(Path.Combine(Config.Configuration[ConfigKey.SavePath],
				$"{Constants.SERVER_FOLDER}_{Config.Configuration[ConfigKey.BotId]}"));
		/// <summary>
		/// Assuming the save path is C:\Users\User\AppData\Roaming, returns C:\Users\User\AppData\Roaming\Discord_Servers_BotId\File
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>
		public static FileInfo GetBaseBotDirectoryFile(string fileName)
			=> new FileInfo(Path.Combine(GetBaseBotDirectory().FullName, fileName));

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
			//Have to use this open method because fileInfo.OpenWrite() occasionally
			//let the last character get written twice which would mess up JSON
			using (var writer = new StreamWriter(fileInfo.Open(FileMode.Truncate)))
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
				ConsoleUtils.ExceptionToConsole(e);
			}
		}

		/// <summary>
		/// Converts the object to JSON.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public static string Serialize(object obj)
			=> JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.Indented, new StringEnumConverter());
		/// <summary>
		/// Creates an object of type <typeparamref name="T"/> with the supplied string and type.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="value"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public static T Deserialize<T>(string value, Type type)
			=> (T)JsonConvert.DeserializeObject(value, type, new StringEnumConverter());

		/// <summary>
		/// Writes an uncaught exception to a log file.
		/// </summary>
		/// <param name="exeption"></param>
		public static void LogUncaughtException(object exception)
		{
			var crashLogPath = GetBaseBotDirectoryFile(Constants.CRASH_LOG_LOCATION);
			CreateFile(crashLogPath);
			//Use File.AppendText instead of new StreamWriter so the text doesn't get overwritten.
			using (var writer = crashLogPath.AppendText())
			{
				writer.WriteLine($"{TimeFormatting.FormatReadableDateTime(DateTime.UtcNow)}: {exception.ToString()}\n");
			}
		}
	}
}