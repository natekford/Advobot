using System;

namespace Advobot.Core.Enums
{
	/// <summary>
	/// Markdown format options for rule formatter.
	/// </summary>
	[Flags]
	public enum MarkDownFormat : uint
	{
		/// <summary>
		/// Add bold in markdown.
		/// </summary>
		Bold = (1U << 0),
		/// <summary>
		/// Add italics in markdown.
		/// </summary>
		Italics = (1U << 1),
		/// <summary>
		/// Add backticks in markdown.
		/// </summary>
		Code = (1U << 2),
	}
}
