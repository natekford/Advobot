using Advobot.Classes;
using Advobot.Classes.Settings;
using Advobot.Classes.UserInformation;
using Advobot.Enums;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Advobot.Interfaces
{
	/// <summary>
	/// Holds guild settings.
	/// </summary>
	public interface IGuildSettings : ISettingsBase, ISettingsProvider<IGuildSettings>
	{
		/// <summary>
		/// Message to display when a user joins the guild.
		/// </summary>
		GuildNotification WelcomeMessage { get; set; }
		/// <summary>
		/// Message to display when a user leaves the guild.
		/// </summary>
		GuildNotification GoodbyeMessage { get; set; }
		/// <summary>
		/// The slowmode on the guild.
		/// </summary>
		Slowmode Slowmode { get; set; }
		/// <summary>
		/// The prefix to use for the guild. If this is null, the bot prefix will be used.
		/// </summary>
		string Prefix { get; set; }
		/// <summary>
		/// The id for the server log.
		/// </summary>
		ulong ServerLogId { get; set; }
		/// <summary>
		/// The id for the mod log.
		/// </summary>
		ulong ModLogId { get; set; }
		/// <summary>
		/// The id for the image log.
		/// </summary>
		ulong ImageLogId { get; set; }
		/// <summary>
		/// The id for the mute role.
		/// </summary>
		ulong MuteRoleId { get; set; }
		/// <summary>
		/// Whether or not errors in commands should be printed to the server.
		/// </summary>
		bool NonVerboseErrors { get; set; }
		/// <summary>
		/// To limit spam.
		/// </summary>
		Dictionary<SpamType, SpamPreventionInfo> SpamPreventionDictionary { get; }
		/// <summary>
		/// To limit raids.
		/// </summary>
		Dictionary<RaidType, RaidPreventionInfo> RaidPreventionDictionary { get; }
		/// <summary>
		/// Roles that persist across a user leaving and rejoining.
		/// </summary>
		List<PersistentRole> PersistentRoles { get; }
		/// <summary>
		/// Permissions given through the bot and not Discord itself.
		/// </summary>
		List<BotImplementedPermissions> BotUsers { get; }
		/// <summary>
		/// Roles users can assign themselves.
		/// </summary>
		List<SelfAssignableRoles> SelfAssignableGroups { get; }
		/// <summary>
		/// Quotes which can be called up through their name.
		/// </summary>
		List<Quote> Quotes { get; }
		/// <summary>
		/// Actions which get logged. Users joining, leaving, deleting messages, etc.
		/// </summary>
		List<LogAction> LogActions { get; }
		/// <summary>
		/// Channels ignored from commands.
		/// </summary>
		List<ulong> IgnoredCommandChannels { get; }
		/// <summary>
		/// Channels ignored from the log.
		/// </summary>
		List<ulong> IgnoredLogChannels { get; }
		/// <summary>
		/// Channels ignored from gaining xp in.
		/// </summary>
		List<ulong> IgnoredXpChannels { get; }
		/// <summary>
		/// Channels which have messages deleted in them unless they have an image attached.
		/// </summary>
		List<ulong> ImageOnlyChannels { get; }
		/// <summary>
		/// Deletes messages and punishes users if the strings are found in their messages.
		/// </summary>
		List<BannedPhrase> BannedPhraseStrings { get; }
		/// <summary>
		/// Deletes messages and punishes users if the patterns are found in their messages.
		/// </summary>
		List<BannedPhrase> BannedPhraseRegex { get; }
		/// <summary>
		/// Banned names for joining users.
		/// </summary>
		List<BannedPhrase> BannedPhraseNames { get; }
		/// <summary>
		/// Punishments to give when thresholds are reached with banned strings/regex.
		/// </summary>
		List<BannedPhrasePunishment> BannedPhrasePunishments { get; }
		/// <summary>
		/// List of rules for easy formatting.
		/// </summary>
		RuleHolder Rules { get; }
		/// <summary>
		/// Settings for commands. Which ones are enabled, disabled, for specific roles/users/channel/guild.
		/// </summary>
		CommandSettings CommandSettings { get; }
		/// <summary>
		/// Users which have been affected by slowmode. This is not saved.
		/// </summary>
		List<SlowmodeUserInfo> SlowmodeUsers { get; }
		/// <summary>
		/// Users which have been affected by spam prevention. This is not saved.
		/// </summary>
		List<SpamPreventionUserInfo> SpamPreventionUsers { get; }
		/// <summary>
		/// Users which have been affected by banned phrases. This is not saved.
		/// </summary>
		List<BannedPhraseUserInfo> BannedPhraseUsers { get; }
		/// <summary>
		/// Cached invites holding uses. This is not saved.
		/// </summary>
		List<CachedInvite> Invites { get; }
		/// <summary>
		/// Regex which has been evaluted to be mostly safe. This is not saved.
		/// </summary>
		List<string> EvaluatedRegex { get; }
		/// <summary>
		/// Holds messages which have been deleted and waits to print them out. This is not saved.
		/// </summary>
		MessageDeletion MessageDeletion { get; }
		/// <summary>
		/// Whether or not this guild is loaded yet. This is not saved.
		/// </summary>
		bool Loaded { get; }

		/// <summary>
		/// What to do after deserialization.
		/// </summary>
		/// <param name="guild"></param>
		/// <returns></returns>
		Task PostDeserializeAsync(SocketGuild guild);
	}
}
