using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Advobot.Core.Classes;
using Advobot.Core.Interfaces;
using Advobot.Core.Utilities;
using Discord;
using Newtonsoft.Json;

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
		/// <summary>
		/// Holds very low level settings: the bot id, key, and save path.
		/// </summary>
		[JsonProperty("Config")]
		public static ConfigDict Configuration = LoadConfigDictionary();
		[JsonIgnore]
		private static Type _GuildSettingsType = typeof(AdvobotGuildSettings);
		/// <summary>
		/// The explicit guild settings type to create or deserialize.
		/// </summary>
		[JsonIgnore]
		public static Type GuildSettingsType
		{
			get => _GuildSettingsType;
			set
			{
				if (!typeof(IGuildSettings).IsAssignableFrom(value))
				{
					throw new ArgumentException($"Must inherit {nameof(IGuildSettings)}.", nameof(GuildSettingsType));
				}
				_GuildSettingsType = value;
			}
		}
		[JsonIgnore]
		private static Type _BotSettingsType = typeof(AdvobotBotSettings);
		/// <summary>
		/// The explicit bot settings type to create or deserialize.
		/// </summary>
		[JsonIgnore]
		public static Type BotSettingsType
		{
			get => _BotSettingsType;
			set
			{
				if (!typeof(IBotSettings).IsAssignableFrom(value))
				{
					throw new ArgumentException($"Must inherit {nameof(IBotSettings)}.", nameof(BotSettingsType));
				}
				_BotSettingsType = value;
			}
		}

		/// <summary>
		/// Attempts to set the save path with the given input. Returns a boolean signifying whether the save path is valid or not.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="startup"></param>
		/// <returns></returns>
		public static bool ValidatePath(string input, bool startup)
		{
			var path = input ?? Configuration[ConfigDict.ConfigKey.SavePath];

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
				Configuration[ConfigDict.ConfigKey.SavePath] = path;
				Save();
				return true;
			}

			ConsoleUtils.WriteLine("Invalid directory. Please enter a valid directory:");
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
			var key = input ?? Configuration[ConfigDict.ConfigKey.BotKey];

			if (startup && !String.IsNullOrWhiteSpace(key))
			{
				try
				{
					await ClientUtils.LoginAsync(client, key).CAF();
					return true;
				}
				catch
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
				Configuration[ConfigDict.ConfigKey.BotKey] = key;
				Save();
				return true;
			}
			catch
			{
				ConsoleUtils.WriteLine("The given key is invalid. Please enter a valid key:");
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
		private static ConfigDict LoadConfigDictionary()
		{
			return IOUtils.DeserializeFromFile<ConfigDict>(new FileInfo(_SavePath), typeof(ConfigDict), true);
		}
		/// <summary>
		/// Writes the current <see cref="ConfigDict"/> to file.
		/// </summary>
		public static void Save()
		{
			IOUtils.OverwriteFile(new FileInfo(_SavePath), IOUtils.Serialize(Configuration));
		}

		/// <summary>
		/// Creates a dictionary which only holds the values for <see cref="ConfigKey"/> to be modified.
		/// </summary>
		public class ConfigDict
		{
			[JsonProperty("Config")]
			private Dictionary<ConfigKey, string> _ConfigDict = new Dictionary<ConfigKey, string>
			{
				{ ConfigKey.SavePath, null },
				{ ConfigKey.BotKey, null },
				{ ConfigKey.BotId, "0" }
			};

			[JsonIgnore]
			public string this[ConfigKey key]
			{
				get => _ConfigDict[key];
				set => _ConfigDict[key] = value;
			}

			/// <summary>
			/// Keys to be used in <see cref="Config.ConfigDict"/>.
			/// </summary>
			[Flags]
			public enum ConfigKey : uint
			{
				SavePath = (1U << 0),
				BotKey = (1U << 1),
				BotId = (1U << 2)
			}
		}
	}
}
