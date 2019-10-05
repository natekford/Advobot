namespace Advobot.Services.LogCounters
{
	/// <summary>
	/// Counts the actions of something.
	/// </summary>
	public interface ILogCounter
	{
		/// <summary>
		/// Its current count.
		/// </summary>
		int Count { get; }

		/// <summary>
		/// The name of the counter.
		/// </summary>
		string Name { get; }
	}
}