namespace Advobot.Interactivity
{
	/// <summary>
	/// A result for interactivity.
	/// </summary>
	public interface IInteractiveResult
	{
		/// <summary>
		/// Whether or not the interactivity has been canceled.
		/// </summary>
		bool HasBeenCanceled { get; }

		/// <summary>
		/// Whether or not the timeout has been passed before receiving a valid result.
		/// </summary>
		bool HasTimedOut { get; }

		/// <summary>
		/// Whether or not a valid result was parsed.
		/// </summary>
		bool HasValue { get; }

		/// <summary>
		/// The parsed value.
		/// </summary>
		object Value { get; }
	}
}