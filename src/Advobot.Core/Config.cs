using Advobot.Core.Actions;
using Advobot.Core.Enums;
using Discord;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace Advobot.Core
{
	/// <summary>
	/// Low level configuration that is necessary for the bot to run. Holds the bot key, bot id, and save path.
	/// </summary>
	public static class Config
	{
		/// <summary>
		/// The path to save the config file to. Does not save any other files here.
		/// </summary>
		[JsonIgnore]
		private static string _SavePath = CeateSavePath();
		[JsonProperty("Config")]
		public static ConfigDict Configuration = LoadConfigDictionary();

		/// <summary>
		/// Attempts to set the save path with the given input. Returns a boolean signifying whether the save path is valid or not.
		/// </summary>
		/// <param name="windows"></param>
		/// <param name="input"></param>
		/// <param name="startup"></param>
		/// <returns></returns>
		public static bool ValidatePath(string input, bool startup)
		{
			var path = input ?? Configuration[ConfigKey.SavePath];

			if (startup)
			{
				if (!String.IsNullOrWhiteSpace(path) && Directory.Exists(path))
				{
					return true;
				}

				ConsoleActions.WriteLine("Please enter a valid directory path in which to save files or say 'AppData':");
				return false;
			}

			if ("appdata".CaseInsEquals(path))
			{
				path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			}

			if (Directory.Exists(path))
			{
				ConsoleActions.WriteLine("Successfully set the save path as " + path);
				Configuration[ConfigKey.SavePath] = path;
				Save();
				return true;
			}

			ConsoleActions.WriteLine("Invalid directory. Please enter a valid directory:");
			return false;
		}
		/// <summary>
		/// Attempts to login with the given input. Returns a boolean signifying whether the login was successful or not.
		/// </summary>
		/// <param name="client">The client to login.</param>
		/// <param name="input">The bot key.</param>
		/// <param name="startup">Whether or not this should be treated as the first attempt at logging in.</param>
		/// <returns>A boolean signifying whether the login was successful or not.</returns>
		public static async Task<bool> ValidateBotKey(IDiscordClient client, string input, bool startup)
		{
			var key = input ?? Configuration[ConfigKey.BotKey];

			if (startup)
			{
				if (!String.IsNullOrWhiteSpace(key))
				{
					try
					{
						await ClientActions.LoginAsync(client, key).CAF();
						return true;
					}
					catch (Exception)
					{
						ConsoleActions.WriteLine("The given key is no longer valid. Please enter a new valid key:");
						return false;
					}
				}

				ConsoleActions.WriteLine("Please enter the bot's key:");
				return false;
			}

			try
			{
				await ClientActions.LoginAsync(client, key).CAF();

				ConsoleActions.WriteLine("Succesfully logged in via the given bot key.");
				Configuration[ConfigKey.BotKey] = key;
				Save();
				return true;
			}
			catch (Exception)
			{
				ConsoleActions.WriteLine("The given key is invalid. Please enter a valid key:");
				return false;
			}
		}

		/// <summary>
		/// Creates a path similar to C:/Users/User/Appdata/Local/Advobot/Advobot1.config.
		/// </summary>
		/// <returns></returns>
		private static string CeateSavePath()
		{
			//Start by grabbing the entry assembly location then cutting out everything but the file name
			//Use entry so console and ui applications can have diff configs
			var currentName = Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location);
			//Count how many exist with that name so they can be saved as Advobot1, Advobot2, etc.
			var count = Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Length;
			//Add the config file into the local application data folder under Advobot
			var configFileName = currentName + count.ToString() + ".config";
			return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Advobot", configFileName);
		}
		/// <summary>
		/// Attempts to load the configuration from <see cref="_SavePath"/> otherwise uses the default initialization for <see cref="ConfigDict"/>.
		/// </summary>
		/// <returns></returns>
		private static ConfigDict LoadConfigDictionary()
		{
			ConfigDict tempDict = null;
			if (File.Exists(_SavePath))
			{
				try
				{
					using (var reader = new StreamReader(_SavePath))
					{
						tempDict = SavingAndLoadingActions.Deserialize<ConfigDict>(reader.ReadToEnd(), typeof(ConfigDict));
					}
				}
				catch (Exception e)
				{
					ConsoleActions.ExceptionToConsole(e);
				}
			}
			return tempDict ?? new ConfigDict();
		}
		/// <summary>
		/// Writes the current <see cref="ConfigDict"/> to file.
		/// </summary>
		public static void Save()
		{
			SavingAndLoadingActions.OverWriteFile(new FileInfo(_SavePath), SavingAndLoadingActions.Serialize(Configuration));
		}

		/// <summary>
		/// Creates a dictionary which only holds the values for <see cref="ConfigKeys"/> to be modified.
		/// </summary>
		public class ConfigDict
		{
			[JsonProperty("Config")]
			private Dictionary<ConfigKey, string> _ConfigDict = new Dictionary<ConfigKey, string>
			{
				{ ConfigKey.SavePath, null },
				{ ConfigKey.BotKey, null },
				{ ConfigKey.BotId, "0" },
			};

			[JsonIgnore]
			public string this[ConfigKey key]
			{
				get => _ConfigDict[key];
				set => _ConfigDict[key] = value;
			}
		}
	}
}
