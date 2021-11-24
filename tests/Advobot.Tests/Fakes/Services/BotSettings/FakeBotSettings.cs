using Advobot.Services.BotSettings;

using Discord;

namespace Advobot.Tests.Fakes.Services.BotSettings
{
	public sealed class FakeBotSettings : IBotSettings
	{
		public bool AlwaysDownloadUsers { get; set; } = true;
		public DirectoryInfo BaseBotDirectory => throw new NotImplementedException();
		public string? Game { get; set; }
		public LogSeverity LogLevel { get; set; } = LogSeverity.Warning;
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
		public string RestartArguments => throw new NotImplementedException();
		public string? Stream { get; set; }
		public IList<ulong> UsersIgnoredFromCommands { get; set; } = new List<ulong>();
		public IList<ulong> UsersUnableToDmOwner { get; set; } = new List<ulong>();

		public IReadOnlyCollection<string> GetSettingNames()
			=> throw new NotImplementedException();

		public void ResetSetting(string name)
			=> throw new NotImplementedException();

		public void Save()
			=> throw new NotImplementedException();
	}
}