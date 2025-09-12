using Advobot.Localization;
using Advobot.Utilities;

namespace Advobot.Services.Help;

[Replacable]
internal sealed class NaiveHelpService : IHelpService
{
	private readonly Localized<Dictionary<string, IHelpModule>> _Help
		= Localized.Create<Dictionary<string, IHelpModule>>();

	/// <inheritdoc />
	public void Add(IHelpModule item)
		=> _Help.Get().Add(item.Id, item);

	/// <inheritdoc />
	public IReadOnlyList<IHelpModule> FindCloseHelpModules(string input)
	{
		return [.. new CloseHelpEntries(GetHelpModules(includeSubmodules: true))
			.FindMatches(input)
			.Select(x => x.Value)
		];
	}

	/// <inheritdoc />
	public IReadOnlyCollection<string> GetCategories()
		=> _Help.Get().Values.Select(x => x.Category).ToHashSet();

	/// <inheritdoc />
	public IEnumerable<IHelpModule> GetHelpModules(bool includeSubmodules)
	{
		static IEnumerable<IHelpModule> IncludeSubmodules(IHelpModule module)
		{
			yield return module;
			foreach (var submodule in module.Submodules)
			{
				foreach (var item in IncludeSubmodules(submodule))
				{
					yield return item;
				}
			}
		}

		IEnumerable<IHelpModule> entries = _Help.Get().Values;
		if (includeSubmodules)
		{
			entries = entries.SelectMany(IncludeSubmodules);
		}
		return entries;
	}
}