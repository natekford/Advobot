using System.Collections.Generic;
using System.Threading.Tasks;
using Advobot.Attributes;
using Advobot.Services.GuildSettings.Settings;
using Advobot.Services.GuildSettings.UserInformation;
using Advobot.Settings;
using Discord;
using Discord.Commands;

namespace Advobot.Services.GuildSettings
{
	/// <summary>
	/// Auto mod guild settings.
	/// </summary>
	public interface IAutoModGuildSettings
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
		/// Channels which have messages deleted in them unless they have an image attached.
		/// </summary>
		IList<ulong> ImageOnlyChannels { get; }

		/// <summary>
		/// Roles that persist across a user leaving and rejoining.
		/// </summary>
		IList<PersistentRole> PersistentRoles { get; }

		/// <summary>
		/// To limit raids.
		/// </summary>
		IList<RaidPrev> RaidPrevention { get; }

		/// <summary>
		/// To limit spam.
		/// </summary>
		IList<SpamPrev> SpamPrevention { get; }

		/// <summary>
		/// Users which have been affected by banned phrases.
		/// </summary>
		IList<BannedPhraseUserInfo> GetBannedPhraseUsers();
	}

	/// <summary>
	/// Checks whether a command can be invoked.
	/// </summary>
	public interface ICommandChecker
	{
		/// <summary>
		/// Checks whether <paramref name="command"/> can be invoked in <paramref name="context"/>.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="command"></param>
		/// <returns></returns>
		Task<PreconditionResult> CanInvokeAsync(ICommandContext context, CommandInfo command);
	}

	/// <summary>
	/// Command guild settings.
	/// </summary>
	public interface ICommandGuildSettings
	{
		/// <summary>
		/// Permissions given through the bot and not Discord itself.
		/// </summary>
		IList<BotUser> BotUsers { get; }

		/// <summary>
		/// Settings for commands. Which ones are enabled, disabled, for specific roles/users/channel/guild.
		/// </summary>
		CommandSettings CommandSettings { get; }

		/// <summary>
		/// Channels ignored from commands.
		/// </summary>
		IList<ulong> IgnoredCommandChannels { get; }
	}

	/// <summary>
	/// Core guild settings.
	/// </summary>
	public interface ICoreGuildSettings
	{
		/// <summary>
		/// The culture to use for the bot's responses.
		/// </summary>
		string Culture { get; set; }

		/// <summary>
		/// Whether or not to delete messages invoking a command after the command has been invoked.
		/// </summary>
		bool DeleteInvokingMessages { get; set; }

		/// <summary>
		/// The guild's id.
		/// </summary>
		ulong GuildId { get; }

		/// <summary>
		/// The id for the mute role.
		/// </summary>
		ulong MuteRoleId { get; set; }

		/// <summary>
		/// Whether or not errors in commands should be printed to the server.
		/// </summary>
		bool NonVerboseErrors { get; set; }

		/// <summary>
		/// The prefix to use for the guild. If this is null, the bot prefix will be used.
		/// </summary>
		string? Prefix { get; set; }
	}

	/// <summary>
	/// Holds guild settings.
	/// </summary>
	public interface IGuildSettings :
		IAutoModGuildSettings,
		ICommandGuildSettings,
		ICoreGuildSettings,
		INotificationGuildSettings,
		IQuoteGuildSettings,
		IRuleGuildSettings,
		ISelfAssignableRoleGuildSettings,
		ISettingsBase
	{
	}

	/// <summary>
	/// Notification guild settings.
	/// </summary>
	public interface INotificationGuildSettings
	{
		/// <summary>
		/// Message to display when a user leaves the guild.
		/// </summary>
		GuildNotification? GoodbyeMessage { get; set; }

		/// <summary>
		/// Message to display when a user joins the guild.
		/// </summary>
		GuildNotification? WelcomeMessage { get; set; }
	}

	/// <summary>
	/// Holds quote guild settings.
	/// </summary>
	public interface IQuoteGuildSettings
	{
		/// <summary>
		/// Quotes which can be called up through their name.
		/// </summary>
		IList<Quote> Quotes { get; }
	}

	/// <summary>
	/// Rule guild settings.
	/// </summary>
	public interface IRuleGuildSettings
	{
		/// <summary>
		/// List of rules for easy formatting.
		/// </summary>
		RuleHolder Rules { get; }
	}

	/// <summary>
	/// Self assingable role guild settings.
	/// </summary>
	public interface ISelfAssignableRoleGuildSettings
	{
		/// <summary>
		/// Roles users can assign themselves.
		/// </summary>
		IList<SelfAssignableRoles> SelfAssignableGroups { get; }
	}
}