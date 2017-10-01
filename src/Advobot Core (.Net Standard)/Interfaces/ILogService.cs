using Advobot.Classes;
using System.Collections.Generic;

namespace Advobot.Interfaces
{
	/// <summary>
	/// Abstraction for a log module. Handles counts of actions, and which commands have been ran. 
	/// </summary>
	public interface ILogService
	{
		List<LoggedCommand> RanCommands	{ get; }
		LogCounter TotalUsers			{ get; }
		LogCounter TotalGuilds			{ get; }
		LogCounter AttemptedCommands	{ get; }
		LogCounter SuccessfulCommands	{ get; }
		LogCounter FailedCommands		{ get; }
		LogCounter UserJoins			{ get; }
		LogCounter UserLeaves			{ get; }
		LogCounter UserChanges			{ get; }
		LogCounter MessageEdits			{ get; }
		LogCounter MessageDeletes		{ get; }
		LogCounter Messages				{ get; }
		LogCounter Images				{ get; }
		LogCounter Gifs					{ get; }
		LogCounter Files				{ get; }

		IBotLogger BotLogger			{ get; }
		IGuildLogger GuildLogger		{ get; }
		IUserLogger UserLogger			{ get; }
		IMessageLogger MessageLogger	{ get; }

		string FormatLoggedCommands();
		string FormatLoggedActions();
	}

	public interface IBotLogger
	{
	}

	public interface IGuildLogger
	{
	}

	public interface IUserLogger
	{
	}

	public interface IMessageLogger
	{
	}
}
