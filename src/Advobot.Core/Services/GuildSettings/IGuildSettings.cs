using System.Collections.Generic;

using Advobot.Services.GuildSettings.Settings;
using Advobot.Services.GuildSettings.UserInformation;
using Advobot.Settings;

namespace Advobot.Services.GuildSettings
{
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
		ICommandGuildSettings,
		ICoreGuildSettings,
		IQuoteGuildSettings,
		IRuleGuildSettings,
		ISelfAssignableRoleGuildSettings,
		ISettingsBase
	{
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