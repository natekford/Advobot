namespace Advobot.Services.Help;

/// <summary>
/// Abstraction for a service providing information about commands.
/// </summary>
public interface IHelpService
{
	/// <summary>
	/// Adds a help entry to this service.
	/// </summary>
	/// <param name="helpModule"></param>
	void Add(IHelpModule helpModule);

	/// <summary>
	/// Returns an array of help entries with similar names.
	/// </summary>
	/// <param name="input"></param>
	/// <returns></returns>
	IReadOnlyList<IHelpModule> FindCloseHelpModules(string input);

	/// <summary>
	/// Returns an array of every command category.
	/// </summary>
	/// <returns></returns>
	IReadOnlyCollection<string> GetCategories();

	/// <summary>
	/// Returns an array of every <see cref="IHelpModule"/> unless a category is specified.
	/// </summary>
	/// <param name="category"></param>
	/// <returns></returns>
	IEnumerable<IHelpModule> GetHelpModules(string? category = null);
}