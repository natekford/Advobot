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
public sealed class AdvobotConfig : IConfig
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
		=> Directory.CreateDirectory(Path.Combine(SavePath!, $"Discord_Servers_{BotId}"));
	/// <summary>
	/// The id of the bot.
	/// </summary>
	[JsonIgnore]
	public ulong BotId { get; private set; }
	/// <summary>
	/// The instance number of the bot at launch. This is used to find the correct config.
	/// </summary>
	[JsonIgnore]
	public int Instance { get; set; } = -1;
	/// <summary>
	/// The previous process id of the application.
	/// </summary>
	[JsonIgnore]
	public int PreviousProcessId { get; set; } = -1;
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

	/// <summary>
	/// Attempts to load the configuration with the supplied instance number otherwise uses the default initialization for config.
	/// </summary>
	/// <returns></returns>
	public static AdvobotConfig Load(string[] args)
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

		AdvobotConfig advobotConfig;
		var path = GetPath(instance);
		if (path.Exists)
		{
			using var stream = path.OpenRead();
			advobotConfig = JsonSerializer.Deserialize<AdvobotConfig>(stream, JsonOptions)!;
		}
		else
		{
			advobotConfig = new AdvobotConfig();
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
		Console.WriteLine("Connecting the client...");
		await client.StartAsync().ConfigureAwait(false);
		Console.WriteLine("Successfully connected to the client.");
	}

	/// <summary>
	/// Attempts to login with the given input. Returns a boolean signifying whether the login was successful or not.
	/// </summary>
	/// <param name="key">The bot key.</param>
	/// <param name="isStartup">Whether or not this should be treated as the first attempt at logging in.</param>
	/// <returns>A boolean signifying whether the login was successful or not.</returns>
	public async Task<bool> ValidateBotKey(string? key, bool isStartup)
	{
		if (_IsKeyValidated)
		{
			return true;
		}

		key ??= _BotKey;
		if (isStartup && !string.IsNullOrWhiteSpace(key))
		{
			try
			{
				await _TestClient.LoginAsync(TokenType.Bot, key).ConfigureAwait(false);
				BotId = _TestClient.CurrentUser.Id;
				return _IsKeyValidated = true;
			}
			catch (HttpException)
			{
				Console.WriteLine("The given key is no longer valid. Please enter a new valid key:");
				return false;
			}
		}
		if (isStartup)
		{
			Console.WriteLine("Please enter the bot's key:");
			return false;
		}

		try
		{
			await _TestClient.LoginAsync(TokenType.Bot, key).ConfigureAwait(false);
			_BotKey = key;
			BotId = _TestClient.CurrentUser.Id;
			Save();
			Console.WriteLine("Succesfully logged in via the given bot key.");
			return _IsKeyValidated = true;
		}
		catch (HttpException)
		{
			Console.WriteLine("The given key is invalid. Please enter a valid key:");
			return false;
		}
	}

	/// <summary>
	/// Attempts to set the save path with the given input. Returns a boolean signifying whether the save path is valid or not.
	/// </summary>
	/// <param name="path"></param>
	/// <param name="isStartup"></param>
	/// <returns></returns>
	public bool ValidatePath(string? path, bool isStartup)
	{
		if (_IsPathValidated)
		{
			return true;
		}

		path ??= SavePath;
		if (isStartup && !string.IsNullOrWhiteSpace(path) && Directory.Exists(path))
		{
			return _IsPathValidated = true;
		}
		if (isStartup)
		{
			Console.WriteLine("Please enter a valid directory path in which to save files or say 'AppData':");
			return false;
		}
		if (path.CaseInsEquals("appdata"))
		{
			var appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			path = Path.Combine(appdata, "Advobot");
			Directory.CreateDirectory(path);
		}
		if (Directory.Exists(path))
		{
			Console.WriteLine($"Successfully set the save path as {path}");
			SavePath = path;
			Save();
			return _IsPathValidated = true;
		}

		Console.WriteLine("Invalid directory. Please enter a valid directory:");
		return false;
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