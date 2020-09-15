namespace Advobot.Services.HelpEntries
{
	/// <summary>
	/// A category in the help entry service.
	/// </summary>
	public readonly struct Category
	{
		/// <summary>
		/// The name of the category.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// Creates an instance of <see cref="Category"/>.
		/// </summary>
		/// <param name="name"></param>
		public Category(string name)
		{
			Name = name;
		}
	}
}