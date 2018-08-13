using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Advobot.Classes.Attributes;
using Advobot.Interfaces;
using Advobot.Utilities;
using AdvorangesSettingParser;
using AdvorangesUtils;
using Discord;
using Discord.Net;
using Discord.Rest;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace Advobot.Classes
{
	/// <summary>
	/// Low level configuration that is necessary for the bot to run. Holds the bot key, bot id, and save path.
	/// </summary>
	public class LowLevelConfig : SettingsBase, ILowLevelConfig
	{
		/// <summary>
		/// The previous process id of the application. This is used with the .Net Framework version to make sure the old one is killed first when restarting.
		/// </summary>
		[JsonIgnore]
		public int PreviousProcessId { get; private set; } = -1;
		/// <summary>
		/// The instance number of the bot at launch. This is used to find the correct config.
		/// </summary>
		[JsonIgnore]
		public int CurrentInstance { get; private set; } = -1;
		/// <inheritdoc />
		[JsonProperty("SavePath"), Setting(null)]
		public string SavePath { get; set; }
		/// <inheritdoc />
		[JsonProperty("BotId"), Setting(0)]
		public ulong BotId { get; set; }
		/// <inheritdoc />
		[JsonProperty("MessageCacheSize"), Setting(10000)]
		public int MessageCacheSize { get; set; } = 10000;
		/// <inheritdoc />
		[JsonProperty("LogLevel"), Setting(LogSeverity.Warning)]
		public LogSeverity LogLevel { get; set; } = LogSeverity.Warning;
		/// <inheritdoc />
		[JsonProperty("AlwaysDownloadUsers"), Setting(true)]
		public bool AlwaysDownloadUsers { get; set; } = true;
		/// <summary>
		/// The API key for the bot.
		/// </summary>
		[JsonProperty("BotKey")]
		private string BotKey { get; set; }
		/// <inheritdoc />
		[JsonIgnore]
		public bool ValidatedPath { get; private set; }
		/// <inheritdoc />
		[JsonIgnore]
		public bool ValidatedKey { get; private set; }

		[JsonIgnore]
		private readonly DiscordRestClient _TestClient = new DiscordRestClient();

		/// <inheritdoc />
		public bool ValidatePath(string input, bool startup)
		{
			var path = input ?? SavePath;

			if (startup && !String.IsNullOrWhiteSpace(path) && Directory.Exists(path))
			{
				return ValidatedPath = true;
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
				ConsoleUtils.WriteLine($"Successfully set the save path as {path}");
				SavePath = path;
				SaveSettings();
				return ValidatedPath = true;
			}

			ConsoleUtils.WriteLine("Invalid directory. Please enter a valid directory:", ConsoleColor.Red);
			return false;
		}
		/// <inheritdoc />
		public async Task<bool> ValidateBotKey(string input, bool startup, Func<ILowLevelConfig, BaseSocketClient, Task> restartCallback)
		{
			var key = input ?? BotKey;

			if (startup && !String.IsNullOrWhiteSpace(key))
			{
				try
				{
					await _TestClient.LoginAsync(TokenType.Bot, key).CAF();
					await ValidateBotId(restartCallback).CAF();
					return ValidatedKey = true;
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
				await _TestClient.LoginAsync(TokenType.Bot, key).CAF();
				ConsoleUtils.WriteLine("Succesfully logged in via the given bot key.");
				BotKey = key;
				SaveSettings();
				await ValidateBotId(restartCallback).CAF();
				return ValidatedKey = true;
			}
			catch (HttpException)
			{
				ConsoleUtils.WriteLine("The given key is invalid. Please enter a valid key:", ConsoleColor.Red);
				return false;
			}
		}
		private async Task ValidateBotId(Func<ILowLevelConfig, BaseSocketClient, Task> restartCallback)
		{
			if (BotId != _TestClient.CurrentUser.Id)
			{
				BotId = _TestClient.CurrentUser.Id;
				SaveSettings();
				ConsoleUtils.WriteLine("The bot needs to be restarted in order for the config to be loaded correctly.");
				await restartCallback.Invoke(this, null).CAF();
			}
		}
		/// <inheritdoc />
		public async Task StartAsync(BaseSocketClient client)
		{
			if (!(ValidatedPath && ValidatedKey))
			{
				throw new InvalidOperationException($"Either path of key has not been validated yet.");
			}
			await ClientUtils.StartAsync(client, BotKey);
		}
		/// <inheritdoc />
		public void ResetBotKey()
		{
			BotKey = null;
		}
		/// <inheritdoc />
		public void SaveSettings()
		{
			SaveSettings(this);
		}
		/// <inheritdoc />
		public DirectoryInfo GetBaseBotDirectory()
		{
			return Directory.CreateDirectory(Path.Combine(SavePath, $"Discord_Servers_{BotId}"));
		}
		/// <inheritdoc />
		public FileInfo GetBaseBotDirectoryFile(string fileName)
		{
			return new FileInfo(Path.Combine(GetBaseBotDirectory().FullName, fileName));
		}
		/// <inheritdoc />
		protected override FileInfo GetPath(ILowLevelConfig config)
		{
			return GetConfigPath(CurrentInstance);
		}
		/// <summary>
		/// Attempts to load the configuration with the supplied instance number otherwise uses the default initialization for config.
		/// </summary>
		/// <returns></returns>
		public static LowLevelConfig Load(string[] args)
		{
			var processId = -1;
			var instance = -1;
			//No help command because this is not intended to be used more than semi internally
			new SettingParser(false, "-", "--", "/")
			{
				//Don't bother adding descriptions because of the aforementioned removal
				new Setting<int>(new[] { nameof(PreviousProcessId), "procid" }, x => processId = x),
				new Setting<int>(new[] { nameof(CurrentInstance), "instance" }, x => instance = x)
			}.Parse(args);

			//Count how many exist with that name so they can be saved as Advobot1, Advobot2, etc.
			instance = instance == -1 ? GetDuplicateProccessesCount() : instance;
			var config = IOUtils.DeserializeFromFile<LowLevelConfig>(GetConfigPath(instance)) ?? new LowLevelConfig();
			config.PreviousProcessId = processId;
			config.CurrentInstance = instance;
			return config;
		}
		/// <summary>
		/// Gets the path leading to the config to use for this bot.
		/// </summary>
		/// <param name="instance"></param>
		/// <returns></returns>
		private static FileInfo GetConfigPath(int instance)
		{
			//Start by grabbing the entry assembly location then cutting out everything but the file name
			//Use entry so console and ui applications can have diff configs
			var currentName = Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location);
			//Add the config file into the local application data folder under Advobot
			var appdata = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
			return new FileInfo(Path.Combine(appdata, "Advobot", currentName + instance + ".config"));
		}
		/// <summary>
		/// Returns how many instances of the bot are currently running.
		/// </summary>
		/// <returns></returns>
		private static int GetDuplicateProccessesCount()
		{
			return Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Length;
		}
		/// <summary>
		/// Returns arguments used for initializing the bot.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return $"-{nameof(PreviousProcessId)} {Process.GetCurrentProcess().Id} -{nameof(CurrentInstance)} {CurrentInstance}";
		}
	}
}
