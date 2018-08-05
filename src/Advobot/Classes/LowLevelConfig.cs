using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Discord.Net;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace Advobot.Classes
{
	/// <summary>
	/// Low level configuration that is necessary for the bot to run. Holds the bot key, bot id, and save path.
	/// </summary>
	public sealed class LowLevelConfig
	{
		/// <summary>
		/// The instance number of the bot at launch.
		/// </summary>
		[JsonIgnore]
		public int InstanceNumber { get; private set; }
		/// <summary>
		/// The path the bot's files are in.
		/// </summary>
		[JsonProperty("SavePath")]
		public string SavePath { get; set; }
		/// <summary>
		/// The API key for the bot.
		/// </summary>
		[JsonProperty("BotKey")]
		private string BotKey { get; set; }
		/// <summary>
		/// The id of the bot.
		/// </summary>
		[JsonProperty("BotId")]
		public ulong BotId { get; set; }
		[JsonIgnore]
		private FileInfo ConfigPath;

		/// <summary>
		/// Attempts to set the save path with the given input. Returns a boolean signifying whether the save path is valid or not.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="startup"></param>
		/// <returns></returns>
		public bool ValidatePath(string input, bool startup)
		{
			var path = input ?? SavePath;

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
				SavePath = path;
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
		public async Task<bool> ValidateBotKey(DiscordShardedClient client, string input, bool startup)
		{
			var key = input ?? BotKey;

			if (startup && !String.IsNullOrWhiteSpace(key))
			{
				try
				{
					await client.LoginAsync(TokenType.Bot, key).CAF();
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
				await client.LoginAsync(TokenType.Bot, key).CAF();

				ConsoleUtils.WriteLine("Succesfully logged in via the given bot key.");
				BotKey = key;
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
		/// Sets the bot key to null.
		/// </summary>
		public void ClearBotKey()
		{
			BotKey = null;
		}
		/// <summary>
		/// Writes the current config to file.
		/// </summary>
		public void Save()
		{
			FileUtils.SafeWriteAllText(ConfigPath, IOUtils.Serialize(this));
		}
		/// <summary>
		/// Attempts to load the configuration from with the supplied instance number otherwise uses the default initialization for config.
		/// </summary>
		/// <returns></returns>
		public static LowLevelConfig Load(int instance)
		{
			//Start by grabbing the entry assembly location then cutting out everything but the file name
			//Use entry so console and ui applications can have diff configs
			var currentName = Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location);
			//Count how many exist with that name so they can be saved as Advobot1, Advobot2, etc.
			instance = instance == -1 ? GetDuplicateProccessesCount() : instance;
			//Add the config file into the local application data folder under Advobot
			var appdata = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
			var path = new FileInfo(Path.Combine(appdata, "Advobot", currentName + instance + ".config"));
			var config = IOUtils.DeserializeFromFile<LowLevelConfig, LowLevelConfig>(path) ?? new LowLevelConfig();
			config.InstanceNumber = instance;
			config.ConfigPath = path;
			return config;
		}
		/// <summary>
		/// Returns how many instances of the bot are currently running.
		/// </summary>
		/// <returns></returns>
		public static int GetDuplicateProccessesCount()
		{
			return Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Length;
		}
	}
}
