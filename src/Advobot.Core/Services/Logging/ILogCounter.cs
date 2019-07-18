namespace Advobot.Services.Logging
{
	/// <summary>
	/// Counts the actions of something.
	/// </summary>
	public interface ILogCounter
	{
		/// <summary>
		/// The name of the counter.
		/// </summary>
		string Name { get; }
		/// <summary>
		/// Its current count.
		/// </summary>
		int Count { get; }
	}
}