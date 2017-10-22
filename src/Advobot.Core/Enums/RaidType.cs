using System;

namespace Advobot.Core.Enums
{
	/// <summary>
	/// Specifies what raid prevention to modify/set up.
	/// </summary>
	[Flags]
	public enum RaidType : uint
	{
		Regular = (1U << 0),
		RapidJoins = (1U << 1),
	}
}
