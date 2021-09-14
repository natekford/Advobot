using System.Collections.Immutable;
using System.Diagnostics;
using System.Reflection;

using Discord.Commands;

namespace Advobot.Services.HelpEntries
{
	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	internal sealed class ParameterHelpEntry : IParameterHelpEntry
	{
		public bool IsOptional { get; }
		public string Name { get; }
		public IReadOnlyList<string> NamedArguments { get; }
		public IReadOnlyList<IParameterPrecondition> Preconditions { get; }
		public string Summary { get; }
		public Type Type { get; }
		private string DebuggerDisplay => $"{Name} ({Type.Name})";

		public ParameterHelpEntry(Discord.Commands.ParameterInfo parameter)
		{
			Name = parameter.Name;
			Summary = parameter.Summary;
			Type = parameter.Type;
			IsOptional = parameter.IsOptional;
			NamedArguments = GetNamedArgumentNames(parameter.Type);
			Preconditions = parameter.Preconditions.OfType<IParameterPrecondition>().ToImmutableArray();
		}

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
}