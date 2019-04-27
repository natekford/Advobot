using System;
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
	}
}