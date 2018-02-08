using Advobot.Core.Classes;
using Advobot.Core.Classes.Rules;
using Advobot.Core.Classes.Settings;
using Advobot.Core.Enums;
using Discord.WebSocket;
using System.Collections.Generic;

namespace Advobot.Core.Interfaces
{
	/// <summary>
	/// Holds guild settings.
	/// </summary>
	public interface IGuildSettings : ISettingsBase
	{
		//Saved settings
		GuildNotification WelcomeMessage { get; set; }
		GuildNotification GoodbyeMessage { get; set; }
		ListedInvite ListedInvite { get; set; }
		Slowmode Slowmode { get; set; }
		string Prefix { get; set; }
		ulong ServerLogId { get; set; }
		ulong ModLogId { get; set; }
		ulong ImageLogId { get; set; }
		ulong MuteRoleId { get; set; }
		bool NonVerboseErrors { get; set; }
		Dictionary<SpamType, SpamPreventionInfo> SpamPreventionDictionary { get; }
		Dictionary<RaidType, RaidPreventionInfo> RaidPreventionDictionary { get; }
		List<PersistentRole> PersistentRoles { get; }
		List<BotImplementedPermissions> BotUsers { get; }
		List<SelfAssignableRoles> SelfAssignableGroups { get; }
		List<Quote> Quotes { get; }
		List<LogAction> LogActions { get; }
		List<ulong> IgnoredCommandChannels { get; }
		List<ulong> IgnoredLogChannels { get; }
		List<ulong> ImageOnlyChannels { get; }
		List<BannedPhrase> BannedPhraseStrings { get; }
		List<BannedPhrase> BannedPhraseRegex { get; }
		List<BannedPhrase> BannedPhraseNames { get; }
		List<BannedPhrasePunishment> BannedPhrasePunishments { get; }
		RuleHolder Rules { get; }
		CommandSettings CommandSettings { get; }

		//Non-saved settings
		List<CachedInvite> Invites { get; }
		List<string> EvaluatedRegex { get; }
		MessageDeletion MessageDeletion { get; }
		bool Loaded { get; }
	}
}
