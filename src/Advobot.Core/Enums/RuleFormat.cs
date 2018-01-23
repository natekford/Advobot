using System;

namespace Advobot.Core.Enums
{
	/// <summary>
	/// The main rule format options for <see cref="Classes.Rules.RuleFormatter"/>.
	/// </summary>
	[Flags]
	public enum RuleFormat : uint
	{
		Numbers = (1U << 0),
		Dashes = (1U << 1),
		Bullets = (1U << 2),
		Bold = (1U << 3)
	}
}
