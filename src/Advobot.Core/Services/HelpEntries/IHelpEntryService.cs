using System.Collections.Generic;

namespace Advobot.Services.HelpEntries
{
	/// <summary>
	/// Abstraction for a service providing information about commands.
	/// </summary>
	public interface IHelpEntryService : ICollection<IHelpEntry>
	{
		/// <summary>
		/// Returns an array of every command category.
		/// </summary>
		/// <returns></returns>
		IReadOnlyList<string> GetCategories();
		/// <summary>
		/// Returns an array of help entries with similar names.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		IReadOnlyList<IHelpEntry> FindCloseHelpEntries(string input);
		/// <summary>
		/// Returns an array of every <see cref="IHelpEntry"/> unless a category is specified.
		/// </summary>
		/// <param name="category"></param>
		/// <returns></returns>
		IReadOnlyList<IHelpEntry> GetHelpEntries(string? category = null);
		/// <summary>
		/// Retrurns an array of <see cref="IHelpEntry"/> which have not had their values set in guild settings.
		/// </summary>
		/// <param name="setCommands"></param>
		/// <returns></returns>
		IReadOnlyList<IHelpEntry> GetUnsetCommands(IEnumerable<string> setCommands);
	}
}