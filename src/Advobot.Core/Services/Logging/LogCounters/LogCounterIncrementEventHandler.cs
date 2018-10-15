namespace Advobot.Services.Logging.LogCounters
{
	/// <summary>
	/// Handler for incrementing a log counter.
	/// </summary>
	/// <param name="source"></param>
	/// <param name="args"></param>
	internal delegate void LogCounterIncrementEventHandler(object source, LogCounterIncrementEventArgs args);
}