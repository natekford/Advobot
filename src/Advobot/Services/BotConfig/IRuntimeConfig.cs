﻿using Discord;

namespace Advobot.Services.BotConfig;

/// <summary>
/// Holds bot settings.
/// </summary>
public interface IRuntimeConfig : IConfig
{
	/// <summary>
	/// Whether or not to always download users when joining a guild.
	/// </summary>
	bool AlwaysDownloadUsers { get; set; }
	/// <summary>
	/// The game the bot should be listed as playing.
	/// </summary>
	string? Game { get; set; }
	/// <summary>
	/// What level to log messages at in the console.
	/// </summary>
	LogSeverity LogLevel { get; set; }
	/// <summary>
	/// How many banned names should be the maximum.
	/// </summary>
	int MaxBannedNames { get; set; }
	/// <summary>
	/// How many banned punishments should be the maximum.
	/// </summary>
	int MaxBannedPunishments { get; set; }
	/// <summary>
	/// How many banned regex should be the maximum.
	/// </summary>
	int MaxBannedRegex { get; set; }
	/// <summary>
	/// How many banned strings should be the maximum.
	/// </summary>
	int MaxBannedStrings { get; set; }
	/// <summary>
	/// How big of a file size to use for gathering messages.
	/// </summary>
	int MaxMessageGatherSize { get; set; }
	/// <summary>
	/// How many quotes should be the maximum.
	/// </summary>
	int MaxQuotes { get; set; }
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
	/// How many users to gather for commands on users unless the bypass string is said.
	/// </summary>
	int MaxUserGatherCount { get; set; }
	/// <summary>
	/// How many messages to cache.
	/// </summary>
	int MessageCacheSize { get; set; }
	/// <summary>
	/// Indicates whether or not the bot is currently paused. This setting is not saved.
	/// </summary>
	bool Pause { get; set; }
	/// <summary>
	/// The prefix for commands.
	/// </summary>
	string Prefix { get; set; }
	/// <summary>
	/// The Twitch stream the bot should link to.
	/// </summary>
	string? Stream { get; set; }
	/// <summary>
	/// Users who are ignored from being able to use commands.
	/// </summary>
	IList<ulong> UsersIgnoredFromCommands { get; set; }
	/// <summary>
	/// Users who are not allowed to dm the owner through the bot.
	/// </summary>
	IList<ulong> UsersUnableToDmOwner { get; set; }
}