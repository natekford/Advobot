using System;

namespace Advobot.Core.Enums
{
	/// <summary>
	/// Used in <see cref="Classes.Attributes.OtherRequirementAttribute"/> to perform various checks.
	/// </summary>
	[Flags]
	public enum Precondition : uint
	{
		UserHasAPerm = (1U << 0),
		GuildOwner = (1U << 1),
		TrustedUser = (1U << 2),
		BotOwner = (1U << 3),
	}
}
