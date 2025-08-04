using Advobot.Attributes;

using AdvorangesUtils;

using Discord.Commands;

using System.Collections.Immutable;
using System.Diagnostics;

namespace Advobot.Services.HelpEntries;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
internal sealed class CommandHelpEntry(CommandInfo command) : ICommandHelpEntry
{
	public IReadOnlyList<string> Aliases { get; } = command.Aliases;
	public string Name { get; } = command.Aliases.Any(a => a.Split(' ')[^1].CaseInsEquals(command.Name))
			? command.Name : "";
	public IReadOnlyList<IParameterHelpEntry> Parameters { get; } = command.Parameters
			.Where(x => !x.Attributes.Any(a => a is HiddenAttribute))
			.Select(x => new ParameterHelpEntry(x))
			.ToImmutableArray();
	public IReadOnlyList<IPrecondition> Preconditions { get; } = [.. command.Preconditions
			.Concat(command.Module.Preconditions)
			.OfType<IPrecondition>()];
	public string Summary { get; } = command.Summary;
	private string DebuggerDisplay => $"{Name} ({Parameters.Count})";
}