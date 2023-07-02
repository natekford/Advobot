using Advobot.Settings;
using Advobot.Utilities;

using AdvorangesUtils;

using Discord;

namespace Advobot.Services.BotSettings;

/// <summary>
/// Holds settings for the bot.
/// </summary>
[Replacable]
internal sealed class NaiveBotSettings : IBotSettings
{
	public bool AlwaysDownloadUsers { get; set; }
	public DirectoryInfo BaseBotDirectory { get; private set; } = new(Directory.GetCurrentDirectory());
	public string? Game { get; set; }
	public LogSeverity LogLevel { get; set; } = LogSeverity.Warning;
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
	public string RestartArguments { get; private set; } = "";
	public string? Stream { get; set; }
	public IList<ulong> UsersIgnoredFromCommands { get; set; } = new List<ulong>();
	public IList<ulong> UsersUnableToDmOwner { get; set; } = new List<ulong>();

	#region Saving and Loading

	/// <summary>
	/// Creates an instance of <see cref="NaiveBotSettings"/> from file.
	/// </summary>
	/// <param name="config"></param>
	/// <returns></returns>
	public static NaiveBotSettings CreateOrLoad(IConfig config)
	{
		var settings = IOUtils.DeserializeFromFile<NaiveBotSettings>(StaticGetPath(config)) ?? new NaiveBotSettings();
		settings.BaseBotDirectory = config.BaseBotDirectory;
		settings.RestartArguments = config.RestartArguments;
		//asdf
		return settings;
	}

	/// <inheritdoc />
	public void Save()
		=> IOUtils.SafeWriteAllText(StaticGetPath(this), IOUtils.Serialize(this));

	private static FileInfo StaticGetPath(IBotDirectoryAccessor accessor)
		=> accessor.GetBaseBotDirectoryFile("BotSettings.json");

	#endregion Saving and Loading
}