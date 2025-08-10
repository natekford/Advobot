using Advobot.Utilities;

using Discord;
using Discord.Net;
using Discord.Rest;
using Discord.WebSocket;

using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Advobot;

/// <summary>
/// Low level configuration that is necessary for the bot to run. Holds the bot key, bot id, and save path.
/// </summary>
public sealed class StartupConfig : IConfig
{
	[JsonIgnore]
	private readonly DiscordRestClient _TestClient = new();
	[JsonInclude]
	[JsonPropertyName("BotKey")]
	private string? _BotKey;
	[JsonIgnore]
	private bool _IsKeyValidated;
	[JsonIgnore]
	private bool _IsPathValidated;

	/// <inheritdoc />
	[JsonIgnore]
	public DirectoryInfo BaseBotDirectory
		=> Directory.CreateDirectory(Path.Combine(SavePath!, "Advobot", $"{BotId}"));
	/// <summary>
	/// The id of the bot.
	/// </summary>
	[JsonIgnore]
	public ulong BotId { get; private set; }
	/// <summary>
	/// The instance number of the bot at launch. This is used to find the correct config.
	/// </summary>
	[JsonIgnore]
	public int Instance { get; private set; } = -1;
	/// <summary>
	/// The previous process id of the application.
	/// </summary>
	[JsonIgnore]
	public int PreviousProcessId { get; private set; } = -1;
	/// <inheritdoc />
	[JsonIgnore]
	public string RestartArguments =>
		$"-{nameof(PreviousProcessId)} {Environment.ProcessId} " +
		$"-{nameof(Instance)} {Instance} ";
	/// <summary>
	/// The path leading to the bot's directory.
	/// </summary>
	[JsonInclude]
	[JsonPropertyName(nameof(SavePath))]
	public string? SavePath { get; private set; }

	internal static JsonSerializerOptions JsonOptions { get; } = new()
	{
		Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
		IncludeFields = true,
		WriteIndented = true,
	};

	static StartupConfig()
	{
		JsonOptions.Converters.Add(new JsonStringEnumConverter());
	}

	/// <summary>
	/// Attempts to load the configuration with the supplied instance number otherwise uses the default initialization for config.
	/// </summary>
	/// <returns></returns>
	public static StartupConfig Load(string[] args)
	{
		// There are better ways to do this, maybe if more arguments come I'll bother
		var instance = -1;
		var previousProcessId = -1;
		for (var i = 0; i < args.Length - 1; i += 2)
		{
			var key = args[i];
			var val = args[i + 1];
			if (key.CaseInsEquals($"-{nameof(Instance)}"))
			{
				instance = int.Parse(val);
			}
			else if (key.CaseInsEquals($"-{nameof(PreviousProcessId)}"))
			{
				previousProcessId = int.Parse(val);
			}
		}
		// Instance is for the config so they can be named Advobot1, Advobot2, etc.
		instance = instance < 1 ? 1 : instance;

		StartupConfig advobotConfig;
		var path = GetPath(instance);
		if (path.Exists)
		{
			using var stream = path.OpenRead();
			advobotConfig = JsonSerializer.Deserialize<StartupConfig>(stream, JsonOptions)!;
		}
		else
		{
			advobotConfig = new StartupConfig();
		}

		advobotConfig.Instance = instance;
		advobotConfig.PreviousProcessId = previousProcessId;

		// File doesn't exist, save the original config so the user can edit it
		if (!path.Exists)
		{
			advobotConfig.Save();
		}
		return advobotConfig;
	}

	/// <summary>
	/// Logs in and starts the client.
	/// </summary>
	/// <param name="client"></param>
	/// <returns></returns>
	public async Task StartAsync(BaseSocketClient client)
	{
		if (!_IsPathValidated)
		{
			throw new InvalidOperationException("Path not validated.");
		}
		if (!_IsKeyValidated)
		{
			throw new InvalidOperationException("Key not validated.");
		}

		// Remove the bot key from being easily accessible via reflection
		client.LoggedIn += () =>
		{
			_BotKey = null;
			return Task.CompletedTask;
		};

		await client.LoginAsync(TokenType.Bot, _BotKey).ConfigureAwait(false);
		Console.WriteLine("Connecting to Discord...");
		await client.StartAsync().ConfigureAwait(false);
		Console.WriteLine("Successfully connected to Discord.");
	}

	/// <summary>
	/// Prompts the user to enter a valid bot key.
	/// </summary>
	public async Task ValidateBotKey()
	{
		if (_IsKeyValidated)
		{
			return;
		}
		else if (!string.IsNullOrWhiteSpace(_BotKey))
		{
			try
			{
				await _TestClient.LoginAsync(TokenType.Bot, _BotKey).ConfigureAwait(false);
				BotId = _TestClient.CurrentUser.Id;
				_IsKeyValidated = true;
				return;
			}
			catch (HttpException)
			{
				Console.WriteLine("The saved key is no longer valid. Enter a new valid key:");
			}
		}
		else
		{
			Console.WriteLine("Enter a bot's key:");
		}

		do
		{
			var key = Console.ReadLine();
			try
			{
				await _TestClient.LoginAsync(TokenType.Bot, key).ConfigureAwait(false);
				_BotKey = key;
				Save();
				BotId = _TestClient.CurrentUser.Id;
				_IsKeyValidated = true;

				// Clear so bot key isn't stuck in the console
				Console.Clear();
				Console.WriteLine("Succesfully logged in via the given bot key.");
				return;
			}
			catch (HttpException)
			{
				Console.WriteLine("The given key is invalid. Enter a valid key:");
			}
		} while (!_IsKeyValidated);
	}

	/// <summary>
	/// Prompts the user to enter a valid directory to save to.
	/// </summary>
	/// <returns></returns>
	public void ValidatePath()
	{
		if (_IsPathValidated)
		{
			return;
		}
		else if (!string.IsNullOrWhiteSpace(SavePath) && Directory.Exists(SavePath))
		{
			_IsPathValidated = true;
			return;
		}

		Console.WriteLine("Enter an existing directory to save files or say 'AppData':");
		do
		{
			var path = Console.ReadLine();
			if (path.CaseInsEquals("appdata"))
			{
				var appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
				path = Path.Combine(appdata, "Advobot");
				Directory.CreateDirectory(path);
			}
			if (Directory.Exists(path))
			{
				SavePath = path;
				Save();
				_IsPathValidated = true;

				Console.WriteLine($"Successfully set the save path as {path}.");
				return;
			}

			Console.WriteLine("Invalid directory. Enter a valid directory:");
		} while (!_IsPathValidated);
	}

	private static FileInfo GetPath(int instance)
	{
		// Add the config file into the local application data folder under Advobot
		var appdata = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
		return new(Path.Combine(appdata, "Advobot", $"Advobot{instance}.config"));
	}

	private void Save()
	{
		using var stream = GetPath(Instance).OpenWrite();
		JsonSerializer.Serialize(stream, this, JsonOptions);
	}
}