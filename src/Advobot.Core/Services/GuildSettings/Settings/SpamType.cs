using System;

namespace Advobot.Services.GuildSettings.Settings
{
	/// <summary>
	/// Specifies what spam prevention to modify/set up.
	/// </summary>
	[Flags]
	public enum SpamType : uint
	{
		/// <summary>
		/// No spam type.
		/// </summary>
		Nothing = 0,

		/// <summary>
		/// A user has sent too many messages.
		/// </summary>
		Message = (1U << 0),

		/// <summary>
		/// A user has sent too many long messages.
		/// </summary>
		LongMessage = (1U << 1),

		/// <summary>
		/// A user has sent too many links.
		/// </summary>
		Link = (1U << 2),

		/// <summary>
		/// A user has sent too many images.
		/// </summary>
		Image = (1U << 3),

		/// <summary>
		/// A user has mentioned too many people.
		/// </summary>
		Mention = (1U << 4),
	}
}