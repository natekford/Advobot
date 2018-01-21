using Advobot.Core.Utilities.Formatting;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Advobot.Core.Utilities
{
	/// <summary>
	/// Actions involving saving and loading.
	/// </summary>
	public static class IOUtils
	{
		internal static JsonSerializerSettings DefaultSerializingSettings = new JsonSerializerSettings
		{
			//Ignores errors parsing specific invalid properties instead of throwing exceptions making the entire object null
			//Will still make the object null if the property's type is changed to something not creatable from the text
			//Won't make the entire object null though, just the 
			Error = (sender, e) =>
			{
#if DEBUG
				ConsoleUtils.WriteLine(e.ErrorContext.Error.Message, color: ConsoleColor.Red);
#endif
				e.ErrorContext.Handled = false;
			},
			Converters = new[] { new StringEnumConverter() },
		};

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
		{
			return Directory.CreateDirectory(Path.Combine(GetBaseBotDirectory().FullName, guildId.ToString()));
		}
		/// <summary>
		/// Assuming the save path is C:\Users\User\AppData\Roaming, returns C:\Users\User\AppData\Roaming\Discord_Servers_BotId\ServerId\File
		/// </summary>
		/// <param name="guildId"></param>
		/// <param name="fileName"></param>
		/// <returns></returns>
		public static FileInfo GetServerDirectoryFile(ulong guildId, string fileName)
		{
			return new FileInfo(Path.Combine(GetServerDirectory(guildId).FullName, fileName));
		}
		/// <summary>
		/// Assuming the save path is C:\Users\User\AppData\Roaming, returns C:\Users\User\AppData\Roaming\Discord_Servers_BotId
		/// </summary>
		/// <returns></returns>
		public static DirectoryInfo GetBaseBotDirectory()
		{
			return Directory.CreateDirectory(Path.Combine(Config.Configuration[Config.ConfigDict.ConfigKey.SavePath],
				$"{Constants.SERVER_FOLDER}_{Config.Configuration[Config.ConfigDict.ConfigKey.BotId]}"));
		}
		/// <summary>
		/// Assuming the save path is C:\Users\User\AppData\Roaming, returns C:\Users\User\AppData\Roaming\Discord_Servers_BotId\File
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>
		public static FileInfo GetBaseBotDirectoryFile(string fileName)
		{
			return new FileInfo(Path.Combine(GetBaseBotDirectory().FullName, fileName));
		}
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
		public static void OverwriteFile(FileInfo fileInfo, string text)
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
				e.Write();
			}
		}
		/// <summary>
		/// Converts the object to JSON.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public static string Serialize(object obj, JsonSerializerSettings settings = null)
		{
			return JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.Indented, settings ?? DefaultSerializingSettings);
		}
		/// <summary>
		/// Creates an object of type <typeparamref name="T"/> with the supplied string and type.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="value"></param>
		/// <param name="type"></param>
		/// <param name="settings"></param>
		/// <returns></returns>
		public static T Deserialize<T>(string value, Type type, JsonSerializerSettings settings = null)
		{
			return (T)JsonConvert.DeserializeObject(value, type, settings ?? DefaultSerializingSettings);
		}
		/// <summary>
		/// Creates an object from JSON stored in a file.
		/// By default will ignore any fields/propties deserializing with errors and parses enums as strings.
		/// </summary>
		/// <typeparam name="T">The general type to deserialize. Can be an abstraction of <paramref name="type"/> but has to be a type where it can be converted to <typeparamref name="T"/>.</typeparam>
		/// <param name="file">The file to read from.</param>
		/// <param name="type">The type of object to create.</param>
		/// <param name="settings">The json settings to use. If null, uses settings that parse enums as strings and ignores errors.</param>
		/// <param name="create">If true, unable to deserialize an object from the file, and the type has a parameterless constructor, then uses that constructor.</param>
		/// <param name="callback">An action to do after the object has been deserialized.</param>
		/// <returns></returns>
		public static T DeserializeFromFile<T>(FileInfo file, Type type, JsonSerializerSettings settings = null, bool create = false)
		{
			T obj = default;
			var stillDef = true;

			if (file.Exists)
			{
				try
				{
					using (var reader = new StreamReader(file.FullName))
					{
						obj = Deserialize<T>(reader.ReadToEnd(), type, settings ?? DefaultSerializingSettings);
						stillDef = false;
					}
					ConsoleUtils.WriteLine($"The {type.Name} file has successfully been loaded.");
				}
				catch (JsonReaderException jre)
				{
					jre.Write();
				}
			}
			else
			{
				ConsoleUtils.WriteLine($"The {type.Name} file could not be found; using default.");
			}

			//If want an object no matter what and the object is still default and there is a parameterless constructor then create one
			return create && stillDef && type.GetConstructors().Any(x => !x.GetParameters().Any()) ? (T)Activator.CreateInstance(type) : obj;
		}
		/// <summary>
		/// Writes an uncaught exception to a log file.
		/// </summary>
		/// <param name="exeption"></param>
		public static void LogUncaughtException(object exception)
		{
			var crashLogPath = GetBaseBotDirectoryFile(Constants.CRASH_LOG_LOC);
			CreateFile(crashLogPath);
			//Use File.AppendText instead of new StreamWriter so the text doesn't get overwritten.
			using (var writer = crashLogPath.AppendText())
			{
				writer.WriteLine($"{DateTime.UtcNow.Readable()}: {exception.ToString()}\n");
			}
		}
	}
}