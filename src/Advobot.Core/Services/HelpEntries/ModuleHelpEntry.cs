using System.Collections.Immutable;
using System.Diagnostics;

using Advobot.Attributes;

using Discord.Commands;

namespace Advobot.Services.HelpEntries
{
	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	internal sealed class ModuleHelpEntry : IModuleHelpEntry
	{
		public bool AbleToBeToggled { get; }
		public IReadOnlyList<string> Aliases { get; }
		public string Category { get; }
		public IReadOnlyList<ICommandHelpEntry> Commands { get; }
		public bool EnabledByDefault { get; }
		public string Id { get; }
		public string Name { get; }
		public IReadOnlyList<IPrecondition> Preconditions { get; }
		public IReadOnlyList<IModuleHelpEntry> Submodules { get; }
		public string Summary { get; }
		private string DebuggerDisplay => $"{Name} ({Id})";

		public ModuleHelpEntry(ModuleInfo module, MetaAttribute meta, CategoryAttribute category)
		{
			AbleToBeToggled = meta.CanToggle;
			EnabledByDefault = meta.IsEnabled;
			Id = meta.Guid.ToString();

			Category = category?.Category?.ToLower() ?? throw new ArgumentNullException(nameof(Category));
			Name = module.Name?.ToLower() ?? throw new ArgumentNullException(nameof(Name));
			Summary = module.Summary ?? throw new ArgumentNullException(nameof(Summary));

			Aliases = module.Aliases.Select(x => x.ToLower()).ToImmutableArray();
			Preconditions = module.Preconditions.OfType<IPrecondition>().ToImmutableArray();
			Commands = module
				.Commands
				.Where(x => !x.Attributes.Any(a => a is HiddenAttribute))
				.OrderBy(x => x.Parameters.Count)
				.Select(x => new CommandHelpEntry(x))
				.ToImmutableArray();
			Submodules = module
				.Submodules
				.Where(x => !x.Attributes.Any(a => a is HiddenAttribute))
				.OrderBy(x => x.Name)
				.Select(x => new ModuleHelpEntry(x, meta, category))
				.ToImmutableArray();
		}
	}
}