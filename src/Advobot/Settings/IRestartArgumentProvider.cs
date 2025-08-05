namespace Advobot.Settings;

/// <summary>
/// Abstraction for something that supplies arguments for restarting.
/// </summary>
public interface IRestartArgumentProvider
{
	/// <summary>
	/// Arguments to use when the bot is restart.
	/// </summary>
	string RestartArguments { get; }
}