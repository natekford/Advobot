using Advobot.Core.Interfaces;
using Advobot.Core.Utilities.Formatting;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Advobot.Core.Utilities
{
	/// <summary>
	/// Actions involving saving and loading.
	/// </summary>
	public static class IOUtils
	{
		//Has to be manually set, but that shouldn't be a problem since the break would have been manually created anyways
		public static JsonFix[] Fixes =
		{
			#region January 20, 2018: Text Fix
			new JsonFix
			{
				Type = typeof(IGuildSettings),
				Path = "WelcomeMessage.Title",
				ErrorValues = new[] { new Regex(@"\[.*\]") },
				NewValue = null
			}
			#endregion
		};
		private static JsonSerializerSettings _DefaultSerializingSettings = GenerateDefaultSerializerSettings();

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
				$"Discord_Servers_{Config.Configuration[Config.ConfigDict.ConfigKey.BotId]}"));
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
		private static void CreateFile(FileInfo fileInfo)
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
			//let the last character get written twice which would mess up json
			using (var writer = new StreamWriter(fileInfo.Open(FileMode.Truncate)))
			{
				writer.Write(text);
			}
		}
		/// <summary>
		/// Converts the object to json.
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="settings"></param>
		/// <returns></returns>
		public static string Serialize(object obj, JsonSerializerSettings settings = null)
		{
			return JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.Indented, settings ?? _DefaultSerializingSettings);
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
			//Only use fixes specified for the class
			var json = value;
			var fixes = Fixes.Where(f => f.Type == type || f.Type.IsAssignableFrom(type));
			if (fixes.Any())
			{
				var jObject = JObject.Parse(value);
				foreach (var fix in fixes)
				{
					if (jObject.SelectToken(fix.Path)?.Parent is JProperty jProp && fix.ErrorValues.Any(x => x.IsMatch(jProp.Value.ToString())))
					{
						jProp.Value = fix.NewValue;
					}
				}
				json = jObject.ToString();
			}

			return (T)JsonConvert.DeserializeObject(json, type, settings ?? _DefaultSerializingSettings);
		}
		/// <summary>
		/// Creates an object from json stored in a file.
		/// By default will ignore any fields/propties deserializing with errors and parses enums as strings.
		/// </summary>
		/// <typeparam name="T">The general type to deserialize. Can be an abstraction of <paramref name="type"/> but has to be a type where it can be converted to <typeparamref name="T"/>.</typeparam>
		/// <param name="file">The file to read from.</param>
		/// <param name="type">The type of object to create.</param>
		/// <param name="settings">The json settings to use. If null, uses settings that parse enums as strings and ignores errors.</param>
		/// <param name="create">If true, unable to deserialize an object from the file, and the type has a parameterless constructor, then uses that constructor.</param>
		/// <param name="callback">An action to do after the object has been deserialized.</param>
		/// <returns></returns>
		public static T DeserializeFromFile<T>(FileInfo file, Type type, bool create = false, JsonSerializerSettings settings = null, Action<T> callback = null)
		{
			T obj = default;
			var stillDef = true;

			if (file.Exists)
			{
				try
				{
					using (var reader = new StreamReader(file.FullName))
					{
						obj = Deserialize<T>(reader.ReadToEnd(), type, settings ?? _DefaultSerializingSettings);
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
			var result = create && stillDef && type.GetConstructors().Any(x => !x.GetParameters().Any()) ? (T)Activator.CreateInstance(type) : obj;
			callback?.Invoke(result);
			return result;
		}
		/// <summary>
		/// Generates json serializer settings which ignore most errors, and has a string enum converter.
		/// </summary>
		/// <returns></returns>
		public static JsonSerializerSettings GenerateDefaultSerializerSettings()
		{
			return new JsonSerializerSettings
			{
				//Ignores errors parsing specific invalid properties instead of throwing exceptions making the entire object null
				//Will still make the object null if the property's type is changed to something not creatable from the text
				//Won't make the entire object null though, just the 
				Error = (sender, e) =>
				{
					ConsoleUtils.WriteLine(e.ErrorContext.Error.Message, color: ConsoleColor.Red);
					e.ErrorContext.Handled = false;
				},
				Converters = new JsonConverter[] { new StringEnumConverter() }
			};
		}
		/// <summary>
		/// Writes an uncaught exception to a log file.
		/// </summary>
		/// <param name="exception"></param>
		public static void LogUncaughtException(object exception)
		{
			var crashLogPath = GetBaseBotDirectoryFile("CrashLog.txt");
			CreateFile(crashLogPath);
			//Use File.AppendText instead of new StreamWriter so the text doesn't get overwritten.
			using (var writer = crashLogPath.AppendText())
			{
				writer.WriteLine($"{DateTime.UtcNow.ToReadable()}: {exception}\n");
			}
		}

		public struct JsonFix
		{
			public Type Type;
			public string Path;
			public Regex[] ErrorValues;
			public string NewValue;
		}
	}
}