using Advobot.Attributes;
using Advobot.Utilities;

using Discord.Commands;

using System.Collections.Immutable;
using System.Diagnostics;

namespace Advobot.Services.Help;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
internal sealed class HelpCommand(CommandInfo command) : IHelpCommand
{
	public IReadOnlyList<string> Aliases { get; } = command.Aliases;
	public string Name { get; } = command.Aliases.Any(a => a.Split(' ')[^1].CaseInsEquals(command.Name))
			? command.Name : "";
	public IReadOnlyList<IHelpParameter> Parameters { get; } = command.Parameters
			.Where(x => !x.Attributes.Any(a => a is HiddenAttribute))
			.Select(x => new HelpParameter(x))
			.ToImmutableArray();
	public IReadOnlyList<IHelpPrecondition> Preconditions { get; } = [.. command.Preconditions
			.Concat(command.Module.Preconditions)
			.OfType<IHelpPrecondition>()];
	public string Summary { get; } = command.Summary;
	private string DebuggerDisplay => $"{Name} ({Parameters.Count})";
}