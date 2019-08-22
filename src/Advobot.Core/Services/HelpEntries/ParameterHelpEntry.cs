using System.Collections.Generic;
using System.Linq;
using Discord.Commands;

namespace Advobot.Services.HelpEntries
{
	internal sealed class ParameterHelpEntry : IParameterHelpEntry
	{
		public string Name { get; }
		public string Summary { get; }
		public string TypeName { get; }
		public IReadOnlyList<IParameterPrecondition> Preconditions { get; }

		public ParameterHelpEntry(ParameterInfo parameter)
		{
			Name = parameter.Name;
			Summary = parameter.Summary;
			TypeName = parameter.Type.Name;
			Preconditions = parameter.Preconditions.OfType<IParameterPrecondition>().ToArray();
		}
	}
}