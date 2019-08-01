using System.Collections;
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
		public int Count => _HelpEntries.Get().Count;
		/// <inheritdoc />
		public bool IsReadOnly => false;

		/// <inheritdoc />
		public IReadOnlyList<string> GetCategories()
			=> GetHelpEntries().Select(x => x.Category).ToArray();
		/// <inheritdoc />
		public IReadOnlyList<IHelpEntry> FindCloseHelpEntries(string input)
			=> new CloseHelpEntries(GetHelpEntries()).FindMatches(input).Select(x => x.Value).ToArray();
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
		/// <inheritdoc />
		public void Add(IHelpEntry item)
			=> _HelpEntries.Get().Add(item);
		/// <inheritdoc />
		public void Clear()
			=> _HelpEntries.Get().Clear();
		/// <inheritdoc />
		public bool Contains(IHelpEntry item)
			=> _HelpEntries.Get().Contains(item);
		/// <inheritdoc />
		public void CopyTo(IHelpEntry[] array, int arrayIndex)
			=> _HelpEntries.Get().CopyTo(array, arrayIndex);
		/// <inheritdoc />
		public bool Remove(IHelpEntry item)
			=> _HelpEntries.Get().Remove(item);
		/// <inheritdoc />
		public IEnumerator<IHelpEntry> GetEnumerator()
			=> _HelpEntries.Get().GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator()
			=> _HelpEntries.Get().GetEnumerator();
	}
}