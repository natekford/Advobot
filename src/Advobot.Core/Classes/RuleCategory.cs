namespace Advobot.Core.Classes
{
	/// <summary>
	/// A category for a rule.
	/// </summary>
	public struct RuleCategory
	{
		/// <summary>
		/// The name of the category.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// Sets the name as the passed in name.
		/// </summary>
		/// <param name="name"></param>
		public RuleCategory(string name)
		{
			Name = name;
		}
	}
}
