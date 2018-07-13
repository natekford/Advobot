using Discord;
using System.Collections.Generic;

namespace Advobot.Interfaces
{
	/// <summary>
	/// Holds bot settings.
	/// </summary>
	public interface IBotSettings : ISettingsBase
	{
		/// <summary>
		/// The log level to use for the discord wrapper.
		/// </summary>
		LogSeverity LogLevel { get; set; }
		/// <summary>
		/// The prefix for commands.
		/// </summary>
		string Prefix { get; set; }
		/// <summary>
		/// The game the bot should be listed as playing.
		/// </summary>
		string Game { get; set; }
		/// <summary>
		/// The Twitch stream the bot should link to.
		/// </summary>
		string Stream { get; set; }
		/// <summary>
		/// Whether or not to always download users when joining the guild.
		/// </summary>
		bool AlwaysDownloadUsers { get; set; }
		/// <summary>
		/// How many shards to use with the bot.
		/// </summary>
		int ShardCount { get; set; }
		/// <summary>
		/// How many messages to cache at a time.
		/// </summary>
		int MessageCacheCount { get; set; }
		/// <summary>
		/// How many users to gather for commands on users unless the bypass string is said.
		/// </summary>
		int MaxUserGatherCount { get; set; }
		/// <summary>
		/// How big of a file size to use for gathering messages.
		/// </summary>
		int MaxMessageGatherSize { get; set; }
		/// <summary>
		/// How many rule categories should be the maximum.
		/// </summary>
		int MaxRuleCategories { get; set; }
		/// <summary>
		/// How many rules should go into each rule category.
		/// </summary>
		int MaxRulesPerCategory { get; set; }
		/// <summary>
		/// How many self assignable role groups should be the maximum.
		/// </summary>
		int MaxSelfAssignableRoleGroups { get; set; }
		/// <summary>
		/// How many quotes should be the maximum.
		/// </summary>
		int MaxQuotes { get; set; }
		/// <summary>
		/// How many banned strings should be the maximum.
		/// </summary>
		int MaxBannedStrings { get; set; }
		/// <summary>
		/// How many banned regex should be the maximum.
		/// </summary>
		int MaxBannedRegex { get; set; }
		/// <summary>
		/// How many banned names should be the maximum.
		/// </summary>
		int MaxBannedNames { get; set; }
		/// <summary>
		/// How many banned punishments should be the maximum.
		/// </summary>
		int MaxBannedPunishments { get; set; }
		/// <summary>
		/// Users who have permissions only slightly lower than the bot owner.
		/// </summary>
		List<ulong> TrustedUsers { get; }
		/// <summary>
		/// Users who are not allowed to dm the owner through the bot.
		/// </summary>
		List<ulong> UsersUnableToDmOwner { get; }
		/// <summary>
		/// Users who are ignored from being able to use commands.
		/// </summary>
		List<ulong> UsersIgnoredFromCommands { get; }
		/// <summary>
		/// Indicates whether or not the bot is currently paused. This setting is not saved.
		/// </summary>
		bool Pause { get; set; }
	}
}
