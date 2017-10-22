using System;

namespace Advobot.Core.Enums
{
	/// <summary>
	/// Indicates whether when searching for a number to look at numbers exactly equal, below, or above.
	/// </summary>
	[Flags]
	public enum CountTarget : uint
	{
		Equal = (1U << 0),
		Below = (1U << 1),
		Above = (1U << 2),
	}
}
