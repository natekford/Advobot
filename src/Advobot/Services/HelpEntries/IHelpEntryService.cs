namespace Advobot.Services.HelpEntries;

/// <summary>
/// Abstraction for a service providing information about commands.
/// </summary>
public interface IHelpEntryService
{
	/// <summary>
	/// Adds a help entry to this service.
	/// </summary>
	/// <param name="helpEntry"></param>
	void Add(IModuleHelpEntry helpEntry);

	/// <summary>
	/// Returns an array of help entries with similar names.
	/// </summary>
	/// <param name="input"></param>
	/// <returns></returns>
	IReadOnlyList<IModuleHelpEntry> FindCloseHelpEntries(string input);

	/// <summary>
	/// Returns an array of every command category.
	/// </summary>
	/// <returns></returns>
	IReadOnlyCollection<string> GetCategories();

	/// <summary>
	/// Returns an array of every <see cref="IModuleHelpEntry"/> unless a category is specified.
	/// </summary>
	/// <param name="category"></param>
	/// <returns></returns>
	IEnumerable<IModuleHelpEntry> GetHelpEntries(string? category = null);
}