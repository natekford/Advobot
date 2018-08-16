using System;

namespace Advobot.Enums
{
	/// <summary>
	/// Specifies what to target with a command. Discord entities, names, prefix, etc.
	/// </summary>
	[Flags]
	public enum Target : uint
	{
		/// <summary>
		/// Target a guild.
		/// </summary>
		Guild = (1U << 0),
		/// <summary>
		/// Target a channel.
		/// </summary>
		Channel = (1U << 1),
		/// <summary>
		/// Target a rule.
		/// </summary>
		Role = (1U << 2),
		/// <summary>
		/// Target a user.
		/// </summary>
		User = (1U << 3),
		/// <summary>
		/// Target an emote.
		/// </summary>
		Emote = (1U << 4),
		/// <summary>
		/// Target an invite.
		/// </summary>
		Invite = (1U << 5),
		/// <summary>
		/// Target the bot.
		/// </summary>
		Bot = (1U << 6),
		/// <summary>
		/// Target a username.
		/// </summary>
		Name = (1U << 7),
		/// <summary>
		/// Target a nickname.
		/// </summary>
		Nickname = (1U << 8),
		/// <summary>
		/// Target a game for playing status.
		/// </summary>
		Game = (1U << 9),
		/// <summary>
		/// Target a user stream link.
		/// </summary>
		Stream = (1U << 10),
		/// <summary>
		/// Target a channel topic.
		/// </summary>
		Topic = (1U << 11),
		/// <summary>
		/// Target a prefix.
		/// </summary>
		Prefix = (1U << 12),
		/// <summary>
		/// Target a regex.
		/// </summary>
		Regex = (1U << 13),
		/// <summary>
		/// Target a rule category.
		/// </summary>
		RuleCategory = (1U << 14),
		/// <summary>
		/// Target a rule.
		/// </summary>
		Rule = (1U << 15),
	}
}
