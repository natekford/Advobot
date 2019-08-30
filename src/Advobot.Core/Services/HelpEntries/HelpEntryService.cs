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
		private readonly Localized<List<IModuleHelpEntry>> _HelpEntries = Localized.Create<List<IModuleHelpEntry>>();

		/// <inheritdoc />
		public void Add(IModuleHelpEntry item)
			=> _HelpEntries.Get().Add(item);

		/// <inheritdoc />
		public IReadOnlyList<IModuleHelpEntry> FindCloseHelpEntries(string input)
		{
			var matches = new CloseHelpEntries(GetHelpEntries()).FindMatches(input);
			return matches.Select(x => x.Value).ToArray();
		}

		/// <inheritdoc />
		public IReadOnlyList<string> GetCategories()
			=> GetHelpEntries().Select(x => x.Category).Distinct().ToArray();

		/// <inheritdoc />
		public IReadOnlyList<IModuleHelpEntry> GetHelpEntries(string? category = null)
		{
			var helpEntries = _HelpEntries.Get();
			if (category == null)
			{
				return helpEntries;
			}
			return helpEntries.Where(x => x.Category.CaseInsEquals(category)).ToArray();
		}
	}
}