using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Advobot.CommandAssemblies;

using Discord.Commands;

namespace Advobot.Services.Commands
{
	/// <summary>
	/// Interface for a class to handle commands.
	/// </summary>
	public interface ICommandHandlerService
	{
		/// <summary>
		/// Tells whether a command was executed correctly, failed, or ignored.
		/// </summary>
		event Func<ICommandContext, IResult, Task> CommandInvoked;

		/// <summary>
		/// Adds the commands contained within each assembly.
		/// </summary>
		/// <param name="commands"></param>
		/// <returns></returns>
		Task AddCommandsAsync(IEnumerable<CommandAssembly> commands);
	}
}