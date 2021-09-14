
using Advobot.Interfaces;

namespace Advobot.Services.HelpEntries
{
	/// <summary>
	/// Contains information about a command.
	/// </summary>
	public interface ICommandHelpEntry : INameable, ISummarizable
	{
		/// <summary>
		/// Other names to invoke the command.
		/// </summary>
		IReadOnlyList<string> Aliases { get; }

		/// <summary>
		/// The parameters to use this command.
		/// </summary>
		IReadOnlyList<IParameterHelpEntry> Parameters { get; }

		/// <summary>
		/// The base permissions to use the command.
		/// </summary>
		IReadOnlyList<IPrecondition> Preconditions { get; }
	}
}