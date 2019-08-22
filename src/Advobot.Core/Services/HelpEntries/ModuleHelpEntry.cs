using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Advobot.Attributes;
using AdvorangesUtils;
using Discord.Commands;

namespace Advobot.Services.HelpEntries
{
	internal sealed class ModuleHelpEntry : IModuleHelpEntry
	{
		public string Name { get; }
		public string Summary { get; }
		public bool AbleToBeToggled { get; }
		public bool EnabledByDefault { get; }
		public string Id { get; }
		public string Category { get; }
		public IReadOnlyList<string> Aliases { get; }
		public IReadOnlyList<IPrecondition> Preconditions { get; }
		public IReadOnlyList<ICommandHelpEntry> Commands { get; }

		public ModuleHelpEntry(ModuleInfo module)
		{
			var meta = module.Attributes.GetAttribute<MetaAttribute>();
			var category = module.Attributes.GetAttribute<CategoryAttribute>();

			AbleToBeToggled = meta.CanToggle;
			EnabledByDefault = meta.IsEnabled;
			Id = meta.Guid.ToString();

			Category = category?.Category?.ToLower() ?? throw new ArgumentNullException(nameof(Category));
			Name = module?.Name?.ToLower() ?? throw new ArgumentNullException(nameof(Name));
			Summary = module.Summary ?? throw new ArgumentNullException(nameof(Summary));

			Aliases = module.Aliases.Any() ? module.Aliases : ImmutableArray.Create("N/A");
			Preconditions = module.Preconditions.OfType<IPrecondition>().ToArray();
			Commands = module.Commands.Select(x => new CommandHelpEntry(x)).ToArray();
		}
	}
}