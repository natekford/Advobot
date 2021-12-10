namespace Advobot.Services.LogCounters;

/// <summary>
/// Abstraction for a log module. Handles counts of actions, and which commands have been ran.
/// </summary>
public interface ILogCounterService
{
	/// <summary>
	/// How many videos/gifs have been sent.
	/// </summary>
	ILogCounter Animated { get; }

	/// <summary>
	/// How many commands have been used in total.
	/// </summary>
	ILogCounter AttemptedCommands { get; }

	/// <summary>
	/// How many commands failed.
	/// </summary>
	ILogCounter FailedCommands { get; }

	/// <summary>
	/// How many files have been sent.
	/// </summary>
	ILogCounter Files { get; }

	/// <summary>
	/// How many images have been sent.
	/// </summary>
	ILogCounter Images { get; }

	/// <summary>
	/// How many messages have been deleted.
	/// </summary>
	ILogCounter MessageDeletes { get; }

	/// <summary>
	/// How many messages have been edited.
	/// </summary>
	ILogCounter MessageEdits { get; }

	/// <summary>
	/// How many messages have been sent.
	/// </summary>
	ILogCounter Messages { get; }

	/// <summary>
	/// How many commands were successful.
	/// </summary>
	ILogCounter SuccessfulCommands { get; }

	/// <summary>
	/// How many guilds does the bot encompass.
	/// </summary>
	ILogCounter TotalGuilds { get; }

	/// <summary>
	/// How many users does the bot encompass.
	/// </summary>
	ILogCounter TotalUsers { get; }

	/// <summary>
	/// How many users have modified themselves.
	/// </summary>
	ILogCounter UserChanges { get; }

	/// <summary>
	/// How many users have joined.
	/// </summary>
	ILogCounter UserJoins { get; }

	/// <summary>
	/// How many users have left.
	/// </summary>
	ILogCounter UserLeaves { get; }
}