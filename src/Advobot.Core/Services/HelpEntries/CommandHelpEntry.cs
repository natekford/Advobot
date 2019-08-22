using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Discord.Commands;

namespace Advobot.Services.HelpEntries
{
	internal sealed class CommandHelpEntry : ICommandHelpEntry
	{
		public string Name { get; }
		public string Summary { get; }
		public IReadOnlyList<string> Aliases { get; }
		public IReadOnlyList<IPrecondition> Preconditions { get; }
		public IReadOnlyList<IParameterHelpEntry> Parameters { get; }

		public CommandHelpEntry(CommandInfo command)
		{
			Name = command.Name;
			Summary = command.Summary;
			Aliases = command.Aliases.Any() ? command.Aliases : ImmutableArray.Create("N/A");
			Preconditions = command.Preconditions.OfType<IPrecondition>().ToArray();
			Parameters = command.Parameters.Select(x => new ParameterHelpEntry(x)).ToArray();
		}
	}
}