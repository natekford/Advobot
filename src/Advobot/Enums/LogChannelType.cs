using System;

namespace Advobot.Enums
{
	/// <summary>
	/// Specifies which log channel to modify.
	/// </summary>
	[Flags]
	public enum LogChannelType : uint
	{
		/// <summary>
		/// This channel logs joins, leaves, changes, etc.
		/// </summary>
		Server = (1U << 0),
		/// <summary>
		/// This channel logs command usage.
		/// </summary>
		Mod = (1U << 1),
		/// <summary>
		/// This channel logs images.
		/// </summary>
		Image = (1U << 2),
	}
}
