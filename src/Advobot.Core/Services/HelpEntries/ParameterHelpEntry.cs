using Discord.Commands;

using System.Collections.Immutable;
using System.Diagnostics;
using System.Reflection;

namespace Advobot.Services.HelpEntries;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
internal sealed class ParameterHelpEntry(Discord.Commands.ParameterInfo parameter) : IParameterHelpEntry
{
	public bool IsOptional { get; } = parameter.IsOptional;
	public string Name { get; } = parameter.Name;
	public IReadOnlyList<string> NamedArguments { get; } = GetNamedArgumentNames(parameter.Type);
	public IReadOnlyList<IParameterPrecondition> Preconditions { get; } = parameter.Preconditions.OfType<IParameterPrecondition>().ToImmutableArray();
	public string Summary { get; } = parameter.Summary;
	public Type Type { get; } = parameter.Type;
	private string DebuggerDisplay => $"{Name} ({Type.Name})";

	private static IReadOnlyList<string> GetNamedArgumentNames(Type type)
	{
		var info = type.GetTypeInfo();
		if (info.GetCustomAttribute<NamedArgumentTypeAttribute>() == null)
		{
			return Array.Empty<string>();
		}

		return info.DeclaredProperties
			.Where(x => x.SetMethod?.IsPublic == true && !x.SetMethod.IsStatic)
			.Select(x => x.Name)
			.ToArray();
	}
}