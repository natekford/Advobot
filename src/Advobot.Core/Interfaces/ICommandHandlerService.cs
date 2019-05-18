using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Discord.Commands;

namespace Advobot.Interfaces
{
	/// <summary>
	/// Interface for a class to handle commands.
	/// </summary>
	public interface ICommandHandlerService
	{
		/// <summary>
		/// Tells whether a command was executed correctly, failed, or ignored.
		/// </summary>
		event Action<IResult> CommandInvoked;

		/// <summary>
		/// Adds the commands contained within each assembly.
		/// </summary>
		/// <param name="commands"></param>
		/// <returns></returns>
		Task AddCommandsAsync(IEnumerable<Assembly> commands);
	}
}