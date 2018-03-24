using System;

namespace Advobot.Core.Enums
{
	/// <summary>
	/// Specifies which type of notification a guild notification should be.
	/// </summary>
	[Flags]
	public enum GuildNotificationType : uint
	{
		/// <summary>
		/// This notification is for welcoming a user joining the guild.
		/// </summary>
		Welcome = (1U << 0),
		/// <summary>
		/// This notification is for saying goodbye to a user leaving the guild.
		/// </summary>
		Goodbye = (1U << 1),
	}
}
