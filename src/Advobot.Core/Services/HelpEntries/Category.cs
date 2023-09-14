namespace Advobot.Services.HelpEntries;

/// <summary>
/// A category in the help entry service.
/// </summary>
/// <remarks>
/// Creates an instance of <see cref="Category"/>.
/// </remarks>
/// <param name="name"></param>
public readonly struct Category(string name)
{
	/// <summary>
	/// The name of the category.
	/// </summary>
	public string Name { get; } = name;
}