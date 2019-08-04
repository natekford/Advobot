using System.Collections.Generic;
using System.Linq;
using Advobot.Localization;
using AdvorangesUtils;

namespace Advobot.Services.HelpEntries
{
	/// <summary>
	/// Creates a help entry for every command and then allows those to be accessed.
	/// </summary>
	internal sealed class HelpEntryService : IHelpEntryService
	{
		private readonly Localized<List<IHelpEntry>> _HelpEntries = Localized.Create<List<IHelpEntry>>();

		/// <inheritdoc />
		public void Add(IHelpEntry item)
			=> _HelpEntries.Get().Add(item);
		/// <inheritdoc />
		public IReadOnlyList<string> GetCategories()
			=> GetHelpEntries().Select(x => x.Category).Distinct().ToArray();
		/// <inheritdoc />
		public IReadOnlyList<IHelpEntry> FindCloseHelpEntries(string input)
		{
			var matches = new CloseHelpEntries(GetHelpEntries()).FindMatches(input);
			return matches.Select(x => x.Value).ToArray();
		}
		/// <inheritdoc />
		public IReadOnlyList<IHelpEntry> GetHelpEntries(string? category = null)
		{
			var helpEntries = _HelpEntries.Get();
			if (category == null)
			{
				return helpEntries;
			}
			return helpEntries.Where(x => x.Category.CaseInsEquals(category)).ToArray();
		}
		/// <inheritdoc />
		public IReadOnlyList<IHelpEntry> GetUnsetCommands(IEnumerable<string> setCommands)
			=> GetHelpEntries().Where(x => !setCommands.CaseInsContains(x.Name)).ToArray();
	}
}