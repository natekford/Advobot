using System;

namespace Advobot.Formatting.Rules
{
	/// <summary>
	/// Additional format options for rule formatter.
	/// </summary>
	[Flags]
	public enum RuleFormatOption : uint
	{
		/// <summary>
		/// No formatting.
		/// </summary>
		Nothing = 0,

		/// <summary>
		/// Keep all the numbers the same length so everything looks uniform.
		/// </summary>
		NumbersSameLength = 1U << 0,

		/// <summary>
		/// Put in extra lines between categories.
		/// </summary>
		ExtraLines = 1U << 1,
	}
}