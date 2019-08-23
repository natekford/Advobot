using System.Collections.Generic;
using System.Linq;
using Advobot.Attributes;
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
			Aliases = command.Aliases;
			Preconditions = command.Preconditions.OfType<IPrecondition>().ToArray();
			Parameters = command.Parameters
				.Where(x => !x.Attributes.Any(a => a is HiddenAttribute))
				.Select(x => new ParameterHelpEntry(x))
				.ToArray();
		}
	}
}