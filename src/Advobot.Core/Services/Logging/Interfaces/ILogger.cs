using System;

using Advobot.Services.Logging.LogCounters;

namespace Advobot.Services.Logging.Interfaces
{
	/// <summary>
	/// Base interface of the specific loggers.
	/// </summary>
	internal interface ILogger
	{
		/// <summary>
		/// Notifies what log count to increment.
		/// </summary>
		event EventHandler<LogCounterIncrementEventArgs> LogCounterIncrement;
	}
}