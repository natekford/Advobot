using System;
using System.Threading.Tasks;
using Advobot.Services.GuildSettings.Settings;

namespace Advobot.Services.Logging.Loggers
{
	/// <summary>
	/// Holds arguments for handling a <see cref="LoggingContext"/>.
	/// </summary>
	public sealed class LoggingContextArgs
	{
		/// <summary>
		/// The action this context is for.
		/// </summary>
		public LogAction Action { get; set; }
		/// <summary>
		/// The log counter to increment.
		/// </summary>
		public string LogCounterName { get; set; }
		/// <summary>
		/// Actions to do when the logging context is valid.
		/// </summary>
		public Func<LoggingContext, Task>[] WhenCanLog { get; set; }
		/// <summary>
		/// Actions to do no matter the validity of the logging context.
		/// </summary>
		public Func<LoggingContext, Task>[] AnyTime { get; set; }
	}
}
