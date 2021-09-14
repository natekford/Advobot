
using Advobot.Interfaces;

using Discord.Commands;

namespace Advobot.Services.HelpEntries
{
	/// <summary>
	/// Contains information about a parameter.
	/// </summary>
	public interface IParameterHelpEntry : INameable, ISummarizable
	{
		/// <summary>
		/// Whether or not the parameter is optional.
		/// </summary>
		bool IsOptional { get; }
		/// <summary>
		/// The names of the available properties if the parameter's type has <see cref="NamedArgumentTypeAttribute"/>.
		/// </summary>
		IReadOnlyList<string> NamedArguments { get; }
		/// <summary>
		/// The base permissions to have this parameter be valid.
		/// </summary>
		IReadOnlyList<IParameterPrecondition> Preconditions { get; }
		/// <summary>
		/// The type of the parameter.
		/// </summary>
		Type Type { get; }
	}
}