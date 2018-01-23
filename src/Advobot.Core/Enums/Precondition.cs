using System;

namespace Advobot.Core.Enums
{
	/// <summary>
	/// Used in <see cref="Classes.Attributes.OtherRequirementAttribute"/> to perform various checks.
	/// </summary>
	[Flags]
	public enum Precondition : uint
	{
		/// <summary>
		/// If a user has any permissions that generally indicate they can use semi spammy commands without abusing them.
		/// </summary>
		GenericPerms = (1U << 0),
		/// <summary>
		/// If a user is the owner of the guild.
		/// </summary>
		GuildOwner = (1U << 1),
		/// <summary>
		/// If a user is a trusted user of the bot.
		/// </summary>
		TrustedUser = (1U << 2),
		/// <summary>
		/// If a user is the owner of the bot.
		/// </summary>
		BotOwner = (1U << 3)
	}
}
