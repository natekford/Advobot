using Advobot.Actions;
using Discord;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace Advobot
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
			var path = input ?? Configuration[ConfigKeys.Save_Path];

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
				Configuration[ConfigKeys.Save_Path] = path;
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
			var key = input ?? Configuration[ConfigKeys.Bot_Key];

			if (startup)
			{
				if (!String.IsNullOrWhiteSpace(key))
				{
					try
					{
						await ClientActions.LoginAsync(client, key);
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
				await ClientActions.LoginAsync(client, key);

				ConsoleActions.WriteLine("Succesfully logged in via the given bot key.");
				Configuration[ConfigKeys.Bot_Key] = key;
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
			//Start by grabbing the executing assembly location then cutting out everything but the file name
			var currentName = Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().Location);
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
			private Dictionary<ConfigKeys, string> _ConfigDict = new Dictionary<ConfigKeys, string>
			{
				{ ConfigKeys.Save_Path, null },
				{ ConfigKeys.Bot_Key,	null },
				{ ConfigKeys.Bot_Id,	"0" },
			};

			[JsonIgnore]
			public string this[ConfigKeys key]
			{
				get => _ConfigDict[key];
				set => _ConfigDict[key] = value;
			}
		}

		/// <summary>
		/// Keys to be used in <see cref="ConfigDict"/>.
		/// </summary>
		public enum ConfigKeys : uint
		{
			Save_Path			= (1U << 0),
			Bot_Key				= (1U << 1),
			Bot_Id				= (1U << 2),
		}
	}
}
