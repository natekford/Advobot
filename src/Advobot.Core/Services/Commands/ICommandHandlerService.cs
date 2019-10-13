using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Advobot.CommandAssemblies;
using Discord;
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
		event Func<CommandInfo, ICommandContext, IResult, Task> CommandInvoked;

		/// <summary>
		/// Notifies about log messages.
		/// </summary>
		event Func<LogMessage, Task> Log;

		/// <summary>
		/// The command handler is finished starting up.
		/// </summary>
		event Func<Task> Ready;

		/// <summary>
		/// Adds the commands contained within each assembly.
		/// </summary>
		/// <param name="commands"></param>
		/// <returns></returns>
		Task AddCommandsAsync(IEnumerable<CommandAssembly> commands);
	}
}