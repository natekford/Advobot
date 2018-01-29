using Discord;
using System.Collections.Generic;

namespace Advobot.Core.Interfaces
{
	/// <summary>
	/// Holds bot settings.
	/// </summary>
	public interface IBotSettings : ISettingsBase
	{
		//Saved settings
		LogSeverity LogLevel { get; set; }
		List<ulong> TrustedUsers { get; }
		List<ulong> UsersUnableToDmOwner { get; }
		List<ulong> UsersIgnoredFromCommands { get; }
		string Prefix { get; set; }
		string Game { get; set; }
		string Stream { get; set; }
		bool AlwaysDownloadUsers { get; set; }
		int ShardCount { get; set; }
		int MessageCacheCount { get; set; }
		int MaxUserGatherCount { get; set; }
		int MaxMessageGatherSize { get; set; }
		int MaxRuleCategories { get; set; }
		int MaxRulesPerCategory { get; set; }
		int MaxSelfAssignableRoleGroups { get; set; }
		int MaxQuotes { get; set; }
		int MaxBannedStrings { get; set; }
		int MaxBannedRegex { get; set; }
		int MaxBannedNames { get; set; }
		int MaxBannedPunishments { get; set; }

		//Non-saved settings
		bool Pause { get; set; }
	}
}
