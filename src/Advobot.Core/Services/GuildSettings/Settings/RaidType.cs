using System;

namespace Advobot.Services.GuildSettings.Settings
{
	/// <summary>
	/// Specifies what raid prevention to modify/set up.
	/// </summary>
	[Flags]
	public enum RaidType : uint
	{
		/// <summary>
		/// No raid type.
		/// </summary>
		Nothing = 0,

		/// <summary>
		/// ???
		/// </summary>
		Regular = (1U << 0),

		/// <summary>
		/// When users join too quickly, this can indicate a raid.
		/// </summary>
		RapidJoins = (1U << 1),
	}
}