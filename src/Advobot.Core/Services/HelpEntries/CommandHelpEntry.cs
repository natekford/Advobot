using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;

using Advobot.Attributes;

using AdvorangesUtils;

using Discord.Commands;

namespace Advobot.Services.HelpEntries
{
	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	internal sealed class CommandHelpEntry : ICommandHelpEntry
	{
		public IReadOnlyList<string> Aliases { get; }
		public string Name { get; }
		public IReadOnlyList<IParameterHelpEntry> Parameters { get; }
		public IReadOnlyList<IPrecondition> Preconditions { get; }
		public string Summary { get; }
		private string DebuggerDisplay => $"{Name} ({Parameters.Count})";

		public CommandHelpEntry(CommandInfo command)
		{
			Name = command.Aliases.Any(a => a.Split(' ')[^1].CaseInsEquals(command.Name))
				? command.Name : "";
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