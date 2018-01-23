using System;

namespace Advobot.Core.Enums
{
	/// <summary>
	/// Specifies which type of notification a guild notification should be.
	/// </summary>
	[Flags]
	public enum GuildNotificationType : uint
	{
		Welcome = (1U << 0),
		Goodbye = (1U << 1)
	}
}
