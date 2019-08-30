using System;
using System.Collections.Generic;
using System.Linq;

using Advobot.Attributes;

using AdvorangesUtils;

using Discord.Commands;

namespace Advobot.Services.HelpEntries
{
	internal sealed class ModuleHelpEntry : IModuleHelpEntry
	{
		public ModuleHelpEntry(ModuleInfo module)
		{
			var meta = module.Attributes.GetAttribute<MetaAttribute>();
			var category = module.Attributes.GetAttribute<CategoryAttribute>();

			AbleToBeToggled = meta.CanToggle;
			EnabledByDefault = meta.IsEnabled;
			Id = meta.Guid.ToString();

			Category = category?.Category?.ToLower() ?? throw new ArgumentNullException(nameof(Category));
			Name = module.Name?.ToLower() ?? throw new ArgumentNullException(nameof(Name));
			Summary = module.Summary ?? throw new ArgumentNullException(nameof(Summary));

			Aliases = module.Aliases;
			Preconditions = module.Preconditions.OfType<IPrecondition>().ToArray();
			Commands = module.Commands
				.Where(x => !x.Attributes.Any(a => a is HiddenAttribute))
				.OrderBy(x => x.Parameters.Count)
				.Select(x => new CommandHelpEntry(x))
				.ToArray();
		}

		public bool AbleToBeToggled { get; }
		public IReadOnlyList<string> Aliases { get; }
		public string Category { get; }
		public IReadOnlyList<ICommandHelpEntry> Commands { get; }
		public bool EnabledByDefault { get; }
		public string Id { get; }
		public string Name { get; }
		public IReadOnlyList<IPrecondition> Preconditions { get; }
		public string Summary { get; }
	}
}