using System;

namespace Advobot.Enums
{
	/// <summary>
	/// Indicates whether when searching for a number to look at numbers exactly equal, below, or above.
	/// </summary>
	[Flags]
	public enum CountTarget : uint
	{
		/// <summary>
		/// Valid results are results that are the same.
		/// </summary>
		Equal = (1U << 0),
		/// <summary>
		/// Valid results are results that are below.
		/// </summary>
		Below = (1U << 1),
		/// <summary>
		/// Valid results are results that are above.
		/// </summary>
		Above = (1U << 2),
	}
}
