using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

using Advobot.Databases;

using AdvorangesSettingParser.Implementation;
using AdvorangesSettingParser.Implementation.Instance;
using AdvorangesSettingParser.Implementation.Static;
using AdvorangesSettingParser.Utils;

using AdvorangesUtils;

using Discord;
using Discord.Net;
using Discord.Rest;
using Discord.WebSocket;

using Newtonsoft.Json;

namespace Advobot.Settings
{
	/// <summary>
	/// Low level configuration that is necessary for the bot to run. Holds the bot key, bot id, and save path.
	/// </summary>
	public class LowLevelConfig : ILowLevelConfig
	{
		[JsonIgnore]
		private readonly DiscordRestClient _TestClient = new DiscordRestClient();

		/// <summary>
		/// The API key for the bot.
		/// </summary>
		[JsonProperty("BotKey")]
		private string? _BotKey = null;

		/// <inheritdoc />
		[JsonIgnore]
		public DirectoryInfo BaseBotDirectory => Directory.CreateDirectory(Path.Combine(SavePath, $"Discord_Servers_{BotId}"));

		/// <inheritdoc />
		[JsonIgnore]
		public ulong BotId { get; private set; }

		/// <inheritdoc />
		[JsonIgnore]
		public string DatabaseConnectionString { get; set; } = "";

		/// <inheritdoc />
		[JsonIgnore]
		public DatabaseType DatabaseType { get; set; } = DatabaseType.LiteDB;

		/// <inheritdoc />
		[JsonIgnore]
		public int Instance { get; set; } = -1;

		/// <inheritdoc />
		[JsonIgnore]
		public int PreviousProcessId { get; set; } = -1;

		/// <inheritdoc />
		[JsonIgnore]
		public string RestartArguments =>
			$"-{nameof(PreviousProcessId)} {Process.GetCurrentProcess().Id} " +
			$"-{nameof(Instance)} {Instance} " +
			$"-{nameof(DatabaseType)} {DatabaseType} " +
			$"-{nameof(DatabaseConnectionString)} {DatabaseConnectionString} ";

		/// <summary>
		/// The path leading to the bot's directory.
		/// </summary>
		[JsonProperty("SavePath")]
		public string? SavePath { get; private set; }

		/// <inheritdoc />
		[JsonIgnore]
		public bool ValidatedKey { get; private set; }

		/// <inheritdoc />
		[JsonIgnore]
		public bool ValidatedPath { get; private set; }

		static LowLevelConfig()
		{
			StaticSettingParserRegistry.Instance.Register(new StaticSettingParser<LowLevelConfig>
			{
				new StaticSetting<LowLevelConfig, int>(x => x.PreviousProcessId),
				new StaticSetting<LowLevelConfig, int>(x => x.Instance),
				new StaticSetting<LowLevelConfig, DatabaseType>(x => x.DatabaseType),
				new StaticSetting<LowLevelConfig, string>(x => x.DatabaseConnectionString),
			});
		}

		/// <summary>
		/// Attempts to load the configuration with the supplied instance number otherwise uses the default initialization for config.
		/// </summary>
		/// <returns></returns>
		public static LowLevelConfig Load(string[] args)
		{
			var parseArgs = new ParseArgs(args, new[] { '"' }, new[] { '"' });
			var instance = -1;
			new SettingParser { new Setting<int>(() => instance), }.Parse(parseArgs);

			//Instance is for the config so they can be named Advobot1, Advobot2, etc.
			instance = instance < 1 ? 1 : instance;
			var config = IOUtils.DeserializeFromFile<LowLevelConfig>(GetConfigPath(instance)) ?? new LowLevelConfig();
			StaticSettingParserRegistry.Instance.Parse(config, parseArgs);
			return config;
		}

		/// <inheritdoc />
		public async Task StartAsync(BaseSocketClient client)
		{
			if (!(ValidatedPath && ValidatedKey))
			{
				throw new InvalidOperationException($"Either path or key has not been validated yet.");
			}

			//Remove the bot key from being easily accessible via reflection
			client.LoggedIn += () =>
			{
				_BotKey = null;
				return Task.CompletedTask;
			};

			await client.LoginAsync(TokenType.Bot, _BotKey).CAF();
			ConsoleUtils.WriteLine("Connecting the client...");
			await client.StartAsync().CAF();
			ConsoleUtils.WriteLine("Successfully connected the client.");
		}

		/// <inheritdoc />
		public async Task<bool> ValidateBotKey(
			string? input,
			bool startup,
			Func<BaseSocketClient, IRestartArgumentProvider, Task> restartCallback)
		{
			if (ValidatedKey)
			{
				return true;
			}

			var key = input ?? _BotKey;

			if (startup && !string.IsNullOrWhiteSpace(key))
			{
				try
				{
					await _TestClient.LoginAsync(TokenType.Bot, key).CAF();
					BotId = _TestClient.CurrentUser.Id;
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
				_BotKey = key;
				BotId = _TestClient.CurrentUser.Id;
				Save();
				ConsoleUtils.WriteLine("Succesfully logged in via the given bot key.");
				return ValidatedKey = true;
			}
			catch (HttpException)
			{
				ConsoleUtils.WriteLine("The given key is invalid. Please enter a valid key:", ConsoleColor.Red);
				return false;
			}
		}

		/// <inheritdoc />
		public bool ValidatePath(string? input, bool startup)
		{
			if (ValidatedPath)
			{
				return true;
			}

			var path = input ?? SavePath;

			if (startup && !string.IsNullOrWhiteSpace(path) && Directory.Exists(path))
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
				var appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
				path = Path.Combine(appdata, "Advobot");
			}
			if (Directory.Exists(path))
			{
				ConsoleUtils.WriteLine($"Successfully set the save path as {path}");
				SavePath = path;
				Save();
				return ValidatedPath = true;
			}

			ConsoleUtils.WriteLine("Invalid directory. Please enter a valid directory:", ConsoleColor.Red);
			return false;
		}

		/// <summary>
		/// Gets the path leading to the config to use for this bot.
		/// </summary>
		/// <param name="instance"></param>
		/// <returns></returns>
		private static FileInfo GetConfigPath(int instance)
		{
			//Add the config file into the local application data folder under Advobot
			var appdata = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
			return new FileInfo(Path.Combine(appdata, "Advobot", "Advobot" + instance + ".config"));
		}

		/// <inheritdoc />
		private void Save()
			=> IOUtils.SafeWriteAllText(GetConfigPath(Instance), IOUtils.Serialize(this));
	}
}