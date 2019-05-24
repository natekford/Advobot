using System.Collections.Generic;
using Advobot.Classes.Settings;

namespace Advobot.Interfaces
{
	/// <summary>
	/// Abstraction for information about a module.
	/// </summary>
	public interface IHelpEntry : INameable
	{
		/// <summary>
		/// Whether or not the command can be toggled.
		/// </summary>
		bool AbleToBeToggled { get; }
		/// <summary>
		/// Other names to invoke the command.
		/// </summary>
		IReadOnlyCollection<string> Aliases { get; }
		/// <summary>
		/// The base permissions to use the command.
		/// </summary>
		string BasePerms { get; }
		/// <summary>
		/// The category the command is in.
		/// </summary>
		string Category { get; }
		/// <summary>
		/// Whether or not the command is on by default.
		/// </summary>
		bool DefaultEnabled { get; }
		/// <summary>
		/// Describes what the command does.
		/// </summary>
		string Description { get; }
		/// <summary>
		/// How to use the command. This is automatically generated.
		/// </summary>
		string Usage { get; }

		/// <summary>
		/// Returns a string with all the information about the command.
		/// </summary>
		/// <returns></returns>
		string ToString();
		/// <summary>
		/// Returns a string with all the information about the command and whether it's currently enabled.
		/// </summary>
		/// <param name="settings"></param>
		/// <returns></returns>
		string ToString(CommandSettings? settings);
	}
}