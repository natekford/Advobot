using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Discord.Net;
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
	public class LowLevelConfig
	{
		/// <summary>
		/// The path to save the config file to. Does not save any other files here.
		/// </summary>
		[JsonIgnore]
		private static readonly string _SavePath = CeateSavePath();
		/// <summary>
		/// Holds very low level settings: the bot id, key, and save path.
		/// </summary>
		[JsonIgnore]
		public static LowLevelConfig Config = LoadConfigDictionary();

		/// <summary>
		/// The path the bot's files are in.
		/// </summary>
		[JsonProperty("SavePath")]
		public string SavePath { get; set; }
		/// <summary>
		/// The API key for the bot.
		/// </summary>
		[JsonProperty("BotKey")]
		public string BotKey { get; set; }
		/// <summary>
		/// The id of the bot.
		/// </summary>
		[JsonProperty("BotId")]
		public ulong BotId { get; set; }

		/// <summary>
		/// Attempts to set the save path with the given input. Returns a boolean signifying whether the save path is valid or not.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="startup"></param>
		/// <returns></returns>
		public bool ValidatePath(string input, bool startup)
		{
			var path = input ?? Config.SavePath;

			if (startup && !String.IsNullOrWhiteSpace(path) && Directory.Exists(path))
			{
				return true;
			}

			if (startup)
			{
				ConsoleUtils.WriteLine("Please enter a valid directory path in which to save files or say 'AppData':");
				return false;
			}

			if ("appdata".CaseInsEquals(path))
			{
				path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			}

			if (Directory.Exists(path))
			{
				ConsoleUtils.WriteLine("Successfully set the save path as " + path);
				Config.SavePath = path;
				Save();
				return true;
			}

			ConsoleUtils.WriteLine("Invalid directory. Please enter a valid directory:", ConsoleColor.Red);
			return false;
		}
		/// <summary>
		/// Attempts to login with the given input. Returns a boolean signifying whether the login was successful or not.
		/// </summary>
		/// <param name="client">The client to login.</param>
		/// <param name="input">The bot key.</param>
		/// <param name="startup">Whether or not this should be treated as the first attempt at logging in.</param>
		/// <returns>A boolean signifying whether the login was successful or not.</returns>
		public async Task<bool> ValidateBotKey(IDiscordClient client, string input, bool startup)
		{
			var key = input ?? Config.BotKey;

			if (startup && !String.IsNullOrWhiteSpace(key))
			{
				try
				{
					await ClientUtils.LoginAsync(client, key).CAF();
					return true;
				}
				catch (HttpException)
				{
					ConsoleUtils.WriteLine("The given key is no longer valid. Please enter a new valid key:");
					return false;
				}
			}
			if (startup)
			{
				ConsoleUtils.WriteLine("Please enter the bot's key:");
				return false;
			}

			try
			{
				await ClientUtils.LoginAsync(client, key).CAF();

				ConsoleUtils.WriteLine("Succesfully logged in via the given bot key.");
				Config.BotKey = key;
				Save();
				return true;
			}
			catch (HttpException)
			{
				ConsoleUtils.WriteLine("The given key is invalid. Please enter a valid key:", ConsoleColor.Red);
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
			var configFileName = currentName + count + ".config";
			return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Advobot", configFileName);
		}
		/// <summary>
		/// Attempts to load the configuration from <see cref="_SavePath"/> otherwise uses the default initialization for <see cref="ConfigDict"/>.
		/// </summary>
		/// <returns></returns>
		private static LowLevelConfig LoadConfigDictionary()
		{
			return IOUtils.DeserializeFromFile<LowLevelConfig, LowLevelConfig>(new FileInfo(_SavePath)) ?? new LowLevelConfig();
		}
		/// <summary>
		/// Writes the current config to file.
		/// </summary>
		public void Save()
		{
			File.WriteAllText(_SavePath, IOUtils.Serialize(Config));
		}
	}
}
