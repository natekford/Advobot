using System.Collections.Generic;

using Advobot.Services.GuildSettings.Settings;
using Advobot.Services.GuildSettings.UserInformation;
using Advobot.Settings;

namespace Advobot.Services.GuildSettings
{
	/// <summary>
	/// Holds guild settings.
	/// </summary>
	public interface IGuildSettings : ISettingsBase
	{
		/// <summary>
		/// Banned names for joining users.
		/// </summary>
		IList<BannedPhrase> BannedPhraseNames { get; }

		/// <summary>
		/// Punishments to give when thresholds are reached with banned strings/regex.
		/// </summary>
		IList<BannedPhrasePunishment> BannedPhrasePunishments { get; }

		/// <summary>
		/// Deletes messages and punishes users if the patterns are found in their messages.
		/// </summary>
		IList<BannedPhrase> BannedPhraseRegex { get; }

		/// <summary>
		/// Deletes messages and punishes users if the strings are found in their messages.
		/// </summary>
		IList<BannedPhrase> BannedPhraseStrings { get; }

		/// <summary>
		/// Permissions given through the bot and not Discord itself.
		/// </summary>
		IList<BotUser> BotUsers { get; }

		/// <summary>
		/// Settings for commands. Which ones are enabled, disabled, for specific roles/users/channel/guild.
		/// </summary>
		CommandSettings CommandSettings { get; }

		/// <summary>
		/// The culture to use for the bot's responses.
		/// </summary>
		string Culture { get; set; }

		/// <summary>
		/// Message to display when a user leaves the guild.
		/// </summary>
		GuildNotification? GoodbyeMessage { get; set; }

		/// <summary>
		/// The guild's id.
		/// </summary>
		ulong GuildId { get; }

		/// <summary>
		/// Channels ignored from commands.
		/// </summary>
		IList<ulong> IgnoredCommandChannels { get; }

		/// <summary>
		/// Channels ignored from gaining xp in.
		/// </summary>
		IList<ulong> IgnoredXpChannels { get; }

		/// <summary>
		/// Channels which have messages deleted in them unless they have an image attached.
		/// </summary>
		IList<ulong> ImageOnlyChannels { get; }

		/// <summary>
		/// The id for the mute role.
		/// </summary>
		ulong MuteRoleId { get; set; }

		/// <summary>
		/// Whether or not errors in commands should be printed to the server.
		/// </summary>
		bool NonVerboseErrors { get; set; }

		/// <summary>
		/// Roles that persist across a user leaving and rejoining.
		/// </summary>
		IList<PersistentRole> PersistentRoles { get; }

		/// <summary>
		/// The prefix to use for the guild. If this is null, the bot prefix will be used.
		/// </summary>
		string? Prefix { get; set; }

		/// <summary>
		/// Quotes which can be called up through their name.
		/// </summary>
		IList<Quote> Quotes { get; }

		/// <summary>
		/// To limit raids.
		/// </summary>
		IList<RaidPrev> RaidPrevention { get; }

		/// <summary>
		/// List of rules for easy formatting.
		/// </summary>
		RuleHolder Rules { get; }

		/// <summary>
		/// Roles users can assign themselves.
		/// </summary>
		IList<SelfAssignableRoles> SelfAssignableGroups { get; }

		/// <summary>
		/// To limit spam.
		/// </summary>
		IList<SpamPrev> SpamPrevention { get; }

		/// <summary>
		/// Message to display when a user joins the guild.
		/// </summary>
		GuildNotification? WelcomeMessage { get; set; }

		/// <summary>
		/// Users which have been affected by banned phrases.
		/// </summary>
		IList<BannedPhraseUserInfo> GetBannedPhraseUsers();
	}
}