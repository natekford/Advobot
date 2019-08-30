using System.Collections.Generic;
using System.Linq;

using Discord.Commands;

namespace Advobot.Services.HelpEntries
{
	internal sealed class ParameterHelpEntry : IParameterHelpEntry
	{
		public bool IsOptional { get; }

		public string Name { get; }

		public IReadOnlyList<IParameterPrecondition> Preconditions { get; }

		public string Summary { get; }

		public string TypeName { get; }

		public ParameterHelpEntry(ParameterInfo parameter)
		{
			Name = parameter.Name;
			Summary = parameter.Summary;
			TypeName = parameter.Type.Name;
			IsOptional = parameter.IsOptional;
			Preconditions = parameter.Preconditions.OfType<IParameterPrecondition>().ToArray();
		}
	}
}