using Advobot.Classes;
using Advobot.Enums;
using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;

namespace Advobot.Interfaces
{
	public interface IBotSettings
	{
		IReadOnlyList<ulong> TrustedUsers { get; set; }
		IReadOnlyList<ulong> UsersUnableToDMOwner { get; set; }
		IReadOnlyList<ulong> UsersIgnoredFromCommands { get; set; }
		uint ShardCount { get; set; }
		uint MessageCacheCount { get; set; }
		uint MaxUserGatherCount { get; set; }
		uint MaxMessageGatherSize { get; set; }
		string Prefix { get; set; }
		string Game { get; set; }
		string Stream { get; set; }
		bool AlwaysDownloadUsers { get; set; }
		LogSeverity LogLevel { get; set; }

		bool Windows { get; }
		bool Console { get; }
		bool FirstInstanceOfBotStartingUpWithCurrentKey { get; }
		bool GotPath { get; }
		bool GotKey { get; }
		bool Loaded { get; }
		bool Pause { get; }
		DateTime StartupTime { get; }

		void TogglePause();
		void SetLoaded();
		void SetGotKey();
		void SetGotPath();
	}

	public interface IGuildSettings
	{
		List<BotImplementedPermissions> BotUsers { get; set; }
		List<SelfAssignableGroup> SelfAssignableGroups { get; set; }
		List<Quote> Quotes { get; set; }
		List<LogAction> LogActions { get; set; }
		List<ulong> IgnoredCommandChannels { get; set; }
		List<ulong> IgnoredLogChannels { get; set; }
		List<ulong> ImageOnlyChannels { get; set; }
		List<BannedPhrase> BannedPhraseStrings { get; set; }
		List<BannedPhrase> BannedPhraseRegex { get; set; }
		List<BannedPhrase> BannedNamesForJoiningUsers { get; set; }
		List<BannedPhrasePunishment> BannedPhrasePunishments { get; set; }
		List<CommandSwitch> CommandSwitches { get; set; }
		List<CommandOverride> CommandsDisabledOnUser { get; set; }
		List<CommandOverride> CommandsDisabledOnRole { get; set; }
		List<CommandOverride> CommandsDisabledOnChannel { get; set; }
		List<PersistentRole> PersistentRoles { get; set; }
		ITextChannel ServerLog { get; set; }
		ITextChannel ModLog { get; set; }
		ITextChannel ImageLog { get; set; }
		IRole MuteRole { get; set; }
		Dictionary<SpamType, SpamPreventionInfo> SpamPreventionDictionary { get; set; }
		Dictionary<RaidType, RaidPreventionInfo> RaidPreventionDictionary { get; set; }
		GuildNotification WelcomeMessage { get; set; }
		GuildNotification GoodbyeMessage { get; set; }
		ListedInvite ListedInvite { get; set; }
		Slowmode Slowmode { get; set; }
		string Prefix { get; set; }
		bool VerboseErrors { get; set; }

		List<BannedPhraseUser> BannedPhraseUsers { get; }
		List<SpamPreventionUser> SpamPreventionUsers { get; }
		List<BotInvite> Invites { get; }
		List<string> EvaluatedRegex { get; }
		MessageDeletion MessageDeletion { get; }
		SocketGuild Guild { get; }
		bool Loaded { get; }

		void SaveSettings();
	}

	public interface ISetting
	{
		string ToString();
		string ToString(SocketGuild guild);
	}
}
