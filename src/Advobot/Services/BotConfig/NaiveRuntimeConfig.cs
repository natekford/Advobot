using Discord;

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Advobot.Services.BotConfig;

/// <summary>
/// Holds settings for the bot.
/// </summary>
[Replacable]
internal sealed class NaiveRuntimeConfig : IRuntimeConfig
{
	public bool AlwaysDownloadUsers { get; set; }
	[JsonIgnore]
	public DirectoryInfo BaseBotDirectory { get; private set; } = new(Directory.GetCurrentDirectory());
	public string? Game { get; set; }
	public LogSeverity LogLevel { get; set; } = LogSeverity.Debug;
	public int MaxBannedNames { get; set; } = 25;
	public int MaxBannedPunishments { get; set; } = 10;
	public int MaxBannedRegex { get; set; } = 25;
	public int MaxBannedStrings { get; set; } = 50;
	public int MaxMessageGatherSize { get; set; } = 500000;
	public int MaxQuotes { get; set; } = 500;
	public int MaxRuleCategories { get; set; } = 20;
	public int MaxRulesPerCategory { get; set; } = 20;
	public int MaxSelfAssignableRoleGroups { get; set; } = 10;
	public int MaxUserGatherCount { get; set; } = 100;
	public int MessageCacheSize { get; set; } = 1000;
	public bool Pause { get; set; }
	public string Prefix { get; set; } = "&&";
	public string? Stream { get; set; }
	public IList<ulong> UsersIgnoredFromCommands { get; set; } = [];
	public IList<ulong> UsersUnableToDmOwner { get; set; } = [];

	/// <summary>
	/// Creates an instance of <see cref="NaiveRuntimeConfig"/> from file.
	/// </summary>
	/// <param name="config"></param>
	/// <returns></returns>
	public static NaiveRuntimeConfig CreateOrLoad(IConfig config)
	{
		NaiveRuntimeConfig runtimeConfig;
		var path = GetPath(config);
		if (path.Exists)
		{
			using var stream = path.OpenRead();
			runtimeConfig = JsonSerializer.Deserialize<NaiveRuntimeConfig>(stream, StartupConfig.JsonOptions)!;
		}
		else
		{
			runtimeConfig = new NaiveRuntimeConfig();
		}

		runtimeConfig.BaseBotDirectory = config.BaseBotDirectory;

		// File doesn't exist, save the original config so the user can edit it
		if (!path.Exists)
		{
			runtimeConfig.Save();
		}
		return runtimeConfig;
	}

	/// <inheritdoc />
	public void Save()
	{
		using var stream = GetPath(this).OpenWrite();
		JsonSerializer.Serialize(stream, this, StartupConfig.JsonOptions);
	}

	private static FileInfo GetPath(IConfig config)
		=> config.GetFile("BotSettings.json");
}