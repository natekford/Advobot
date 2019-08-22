using System.Collections.Generic;
using Advobot.Interfaces;

namespace Advobot.Services.HelpEntries
{
	/// <summary>
	/// Contains information about a module.
	/// </summary>
	public interface IModuleHelpEntry : INameable, ISummarizable
	{
		/// <summary>
		/// Whether or not the command can be toggled.
		/// </summary>
		bool AbleToBeToggled { get; }
		/// <summary>
		/// Whether or not the command is on by default.
		/// </summary>
		bool EnabledByDefault { get; }
		/// <summary>
		/// The constant Id for this help entry.
		/// </summary>
		string Id { get; }
		/// <summary>
		/// The category the command is in.
		/// </summary>
		string Category { get; }
		/// <summary>
		/// Other names to invoke the command.
		/// </summary>
		IReadOnlyList<string> Aliases { get; }
		/// <summary>
		/// The base permissions to use the command.
		/// </summary>
		IReadOnlyList<IPrecondition> Preconditions { get; }
		/// <summary>
		/// The overloads/actual commands in this module.
		/// </summary>
		IReadOnlyList<ICommandHelpEntry> Commands { get; }
	}
}