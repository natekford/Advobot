using Advobot.Classes;

namespace Advobot.Interfaces
{
	/// <summary>
	/// Base interface of the specific loggers.
	/// </summary>
	public interface ILogger
	{
		/// <summary>
		/// Notifies what log count to increment.
		/// </summary>
		event LogCounterIncrementEventHandler LogCounterIncrement;
	}
}