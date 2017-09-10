using Advobot.Actions;
using Discord;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Advobot
{
	/// <summary>
	/// Low level configuration that is necessary for the bot to run. Holds the bot key, bot id, and save path.
	/// </summary>
	public static class Config
	{
		[JsonIgnore]
		private static string _SavePath;
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
					Configuration[ConfigKeys.Save_Path] = path;
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
						await ClientActions.Login(client, key);
						return true;
					}
					catch (Exception)
					{
						ConsoleActions.WriteLine("The given key is no longer valid. Please enter a new valid key:");
					}
				}
				else
				{
					ConsoleActions.WriteLine("Please enter the bot's key:");
				}
				return false;
			}

			try
			{
				await ClientActions.Login(client, key);

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

		private static ConfigDict LoadConfigDictionary()
		{
			//Get the current name and count of processes running with that name and find out how many are running to allow multiple different configs if there are multiple bots running
			var currentName = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
			var count = System.Diagnostics.Process.GetProcessesByName(currentName).Length;
			_SavePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Advobot", currentName + count.ToString() + ".config");
			/* Should look like this:
			 * C:/Users/User/Appdata/Local/Advobot/Advobot1.config
			 * C:/Users/User/Appdata/Local/Advobot/Advobot2.config //If a second process gets started
			 */

			ConfigDict tempDict = null;
			if (File.Exists(_SavePath))
			{
				try
				{
					using (var reader = new StreamReader(_SavePath))
					{
						tempDict = JsonConvert.DeserializeObject<ConfigDict>(reader.ReadToEnd());
					}
				}
				catch (Exception e)
				{
					ConsoleActions.ExceptionToConsole(e);
				}
			}
			return tempDict ?? new ConfigDict();
		}
		public static void Save()
		{
			SavingAndLoadingActions.OverWriteFile(new FileInfo(_SavePath), SavingAndLoadingActions.Serialize(Configuration));
		}
	}

	/// <summary>
	/// 
	/// </summary>
	public class ConfigDict
	{
		[JsonProperty("Config")]
		private Dictionary<ConfigKeys, string> _ConfigDict = new Dictionary<ConfigKeys, string>
		{
			{ ConfigKeys.Save_Path, null },
			{ ConfigKeys.Bot_Key, null },
			{ ConfigKeys.Bot_Id, "0" },
		};

		[JsonIgnore]
		public string this[ConfigKeys key]
		{
			get => _ConfigDict[key];
			set => _ConfigDict[key] = value;
		}
	}

	/// <summary>
	/// 
	/// </summary>
	public enum ConfigKeys : uint
	{
		Save_Path			= (1U << 0),
		Bot_Key				= (1U << 1),
		Bot_Id				= (1U << 2),
	}
}
