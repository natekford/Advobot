using Advobot.Localization;
using Advobot.Utilities;

namespace Advobot.Services.Help;

[Replacable]
internal sealed class NaiveHelpService : IHelpService
{
	private readonly Localized<List<IHelpModule>> _Help = Localized.Create<List<IHelpModule>>();

	/// <inheritdoc />
	public void Add(IHelpModule item)
		=> _Help.Get().Add(item);

	/// <inheritdoc />
	public IReadOnlyList<IHelpModule> FindCloseHelpModules(string input)
	{
		var matches = new CloseHelpEntries(_Help.Get()).FindMatches(input);
		var array = new IHelpModule[matches.Count];
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
		foreach (var entry in _Help.Get())
		{
			set.Add(entry.Category);
		}
		return set;
	}

	/// <inheritdoc />
	public IEnumerable<IHelpModule> GetHelpModules(string? category = null)
	{
		static IEnumerable<IHelpModule> GetHelpEntries(IHelpModule entry)
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

		var entries = _Help.Get().SelectMany(GetHelpEntries);
		if (category is null)
		{
			return entries;
		}
		return entries.Where(x => x.Category.CaseInsEquals(category));
	}
}