using Advobot.Services.BotConfig;

using Discord;

namespace Advobot.Tests.Fakes.Services.BotConfig;

public sealed record FakeBotConfig : IRuntimeConfig
{
	public bool AlwaysDownloadUsers { get; set; } = true;
	public DirectoryInfo BaseBotDirectory { get; set; } = new(Environment.CurrentDirectory);
	public string? Game { get; set; }
	public LogSeverity LogLevel { get; set; } = LogSeverity.Debug;
	public int MaxBannedNames { get; set; }
	public int MaxBannedPunishments { get; set; }
	public int MaxBannedRegex { get; set; }
	public int MaxBannedStrings { get; set; }
	public int MaxMessageGatherSize { get; set; }
	public int MaxQuotes { get; set; }
	public int MaxRuleCategories { get; set; }
	public int MaxRulesPerCategory { get; set; }
	public int MaxSelfAssignableRoleGroups { get; set; }
	public int MaxUserGatherCount { get; set; }
	public int MessageCacheSize { get; set; }
	public bool Pause { get; set; }
	public string Prefix { get; set; } = "&&";
	public string RestartArguments { get; set; } = "";
	public string? Stream { get; set; }
	public IList<ulong> UsersIgnoredFromCommands { get; set; } = [];
	public IList<ulong> UsersUnableToDmOwner { get; set; } = [];
}