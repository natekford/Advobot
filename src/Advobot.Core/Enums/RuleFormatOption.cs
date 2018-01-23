using System;

namespace Advobot.Core.Enums
{
	/// <summary>
	/// Additional format options for <see cref="Classes.Rules.RuleFormatter"/>.
	/// </summary>
	[Flags]
	public enum RuleFormatOption : uint
	{
		NumbersSameLength = (1U << 0),
		ExtraLines = (1U << 1)
	}
}
