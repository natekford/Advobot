using System.Collections.Generic;
using Advobot.Interfaces;

namespace Advobot.Services.HelpEntries
{
	/// <summary>
	/// Contains information about a parameter.
	/// </summary>
	public interface IParameterHelpEntry : INameable, ISummarizable
	{
		/// <summary>
		/// The name of the parameter type.
		/// </summary>
		string TypeName { get; }
		/// <summary>
		/// Whether or not the parameter is optional.
		/// </summary>
		bool IsOptional { get; }
		/// <summary>
		/// The base permissions to have this parameter be valid.
		/// </summary>
		IReadOnlyList<IParameterPrecondition> Preconditions { get; }
	}
}