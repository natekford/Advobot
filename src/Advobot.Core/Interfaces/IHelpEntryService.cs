using System.Collections.Generic;

namespace Advobot.Interfaces
{
	/// <summary>
	/// Abstraction for a service providing information about commands.
	/// </summary>
	public interface IHelpEntryService : IReadOnlyDictionary<string, IHelpEntry>, ICollection<IHelpEntry>
	{
		/// <summary>
		/// Returns an array of every command category.
		/// </summary>
		/// <returns></returns>
		IReadOnlyCollection<string> GetCategories();
		/// <summary>
		/// Returns an array of every <see cref="IHelpEntry"/> unless a category is specified.
		/// </summary>
		/// <param name="category"></param>
		/// <returns></returns>
		IReadOnlyCollection<IHelpEntry> GetHelpEntries(string? category = null);
		/// <summary>
		/// Retrurns an array of <see cref="IHelpEntry"/> which have not had their values set in guild settings.
		/// </summary>
		/// <param name="setCommands"></param>
		/// <returns></returns>
		IReadOnlyCollection<IHelpEntry> GetUnsetCommands(IEnumerable<string> setCommands);
	}
}