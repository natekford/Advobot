using System;

namespace Advobot.Core.Enums
{
	/// <summary>
	/// Markdown format options for <see cref="Classes.Rules.RuleFormatter"/>.
	/// </summary>
	[Flags]
	public enum MarkDownFormat : uint
	{
		Bold = (1U << 0),
		Italics = (1U << 1),
		Code = (1U << 2)
	}
}
