namespace Advobot.Quotes.Formatting
{
	/// <summary>
	/// The main rule format options for rule formatter.
	/// </summary>
	[Flags]
	public enum RuleFormat : uint
	{
		/// <summary>
		/// No formatting.
		/// </summary>
		Nothing = 0,

		/// <summary>
		/// Keep the categories formatted like a numbered list.
		/// </summary>
		Numbers = 1U << 0,

		/// <summary>
		/// Use dashes to list out each category.
		/// </summary>
		Dashes = 1U << 1,

		/// <summary>
		/// Use bullets to list out each category.
		/// </summary>
		Bullets = 1U << 2,

		/// <summary>
		/// Bold each category's name.
		/// </summary>
		Bold = 1U << 3,
	}
}