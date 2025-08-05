namespace Advobot.Services.LogCounters;

/// <summary>
/// Provides information about what log counter to increment.
/// </summary>
/// <remarks>
/// Creates an instance of <see cref="LogCounterIncrementEventArgs"/>.
/// </remarks>
/// <param name="name"></param>
/// <param name="count"></param>
internal sealed class LogCounterIncrementEventArgs(string name, int count) : EventArgs
{
	/// <summary>
	/// The amount to increment. Can be negative, in which case this would be a decrement.
	/// </summary>
	public int Count { get; } = count;

	/// <summary>
	/// The name of the log counter to increment.
	/// </summary>
	public string Name { get; } = name;
}