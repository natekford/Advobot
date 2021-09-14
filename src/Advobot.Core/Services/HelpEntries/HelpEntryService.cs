
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
			var matches = new CloseHelpEntries(_HelpEntries.Get()).FindMatches(input);
			var array = new IModuleHelpEntry[matches.Count];
			for (var i = 0; i < matches.Count; ++i)
			{
				array[i] = matches[i].Value;
			}
			return array;
		}

		/// <inheritdoc />
		public IReadOnlyCollection<string> GetCategories()
		{
			var set = new HashSet<string>();
			foreach (var entry in _HelpEntries.Get())
			{
				set.Add(entry.Category);
			}
			return set;
		}

		/// <inheritdoc />
		public IEnumerable<IModuleHelpEntry> GetHelpEntries(string? category = null)
		{
			static IEnumerable<IModuleHelpEntry> GetHelpEntries(IModuleHelpEntry entry)
			{
				yield return entry;
				foreach (var submodule in entry.Submodules)
				{
					foreach (var item in GetHelpEntries(submodule))
					{
						yield return item;
					}
				}
			}

			var entries = _HelpEntries.Get().SelectMany(GetHelpEntries);
			if (category == null)
			{
				return entries;
			}
			return entries.Where(x => x.Category.CaseInsEquals(category));
		}
	}
}