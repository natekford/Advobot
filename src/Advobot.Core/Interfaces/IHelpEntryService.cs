using System.Collections.Generic;
using System.Reflection;
using Discord.Commands;

namespace Advobot.Interfaces
{
	/// <summary>
	/// Abstraction for a service providing information about commands.
	/// </summary>
	public interface IHelpEntryService : IEnumerable<IHelpEntry>
	{
		/// <summary>
		/// Attempt to get a command with its name.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		IHelpEntry? this[string name] { get; }

		/// <summary>
		/// Creates help entries from assemblies.
		/// </summary>
		/// <param name="assemblies"></param>
		void Add(IEnumerable<Assembly> assemblies);
		/// <summary>
		/// Creates help entires from modules.
		/// </summary>
		/// <param name="modules"></param>
		void Add(IEnumerable<ModuleInfo> modules);
		/// <summary>
		/// Removes a help entry from the service.
		/// </summary>
		/// <param name="helpEntry"></param>
		void Remove(IHelpEntry helpEntry);
		/// <summary>
		/// Returns an array of every command category.
		/// </summary>
		/// <returns></returns>
		string[] GetCategories();
		/// <summary>
		/// Returns an array of every <see cref="IHelpEntry"/> unless a category is specified.
		/// </summary>
		/// <param name="category"></param>
		/// <returns></returns>
		IHelpEntry[] GetHelpEntries(string? category = null);
		/// <summary>
		/// Retrurns an array of <see cref="IHelpEntry"/> which have not had their values set in guild settings.
		/// </summary>
		/// <param name="setCommands"></param>
		/// <returns></returns>
		IHelpEntry[] GetUnsetCommands(IEnumerable<string> setCommands);
	}
}