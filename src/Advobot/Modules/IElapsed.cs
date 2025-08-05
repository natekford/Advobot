namespace Advobot.Modules;

/// <summary>
/// Holds time since the command was started.
/// </summary>
public interface IElapsed
{
	/// <summary>
	/// Time elapsed between receiving the message starting this command and ending it.
	/// </summary>
	TimeSpan Elapsed { get; }
}