using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Advobot.Attributes;

using Discord.Commands;

namespace Advobot.Services.HelpEntries
{
	internal sealed class CommandHelpEntry : ICommandHelpEntry
	{
		public IReadOnlyList<string> Aliases { get; }
		public string Name { get; }
		public IReadOnlyList<IParameterHelpEntry> Parameters { get; }
		public IReadOnlyList<IPrecondition> Preconditions { get; }
		public string Summary { get; }

		public CommandHelpEntry(CommandInfo command)
		{
			Name = command.Name;
			Summary = command.Summary;
			Aliases = command.Aliases;
			Preconditions = command.Preconditions
				.Concat(command.Module.Preconditions)
				.OfType<IPrecondition>()
				.ToImmutableArray();
			Parameters = command.Parameters
				.Where(x => !x.Attributes.Any(a => a is HiddenAttribute))
				.Select(x => new ParameterHelpEntry(x))
				.ToImmutableArray();
		}
	}
}