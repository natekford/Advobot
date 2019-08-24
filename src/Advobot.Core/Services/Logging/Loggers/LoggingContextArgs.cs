using System;
using System.Threading.Tasks;
using Advobot.Services.GuildSettings.Settings;

namespace Advobot.Services.Logging.Loggers
{
	/// <summary>
	/// Holds arguments for handling a <see cref="LoggingContext"/>.
	/// </summary>
	internal sealed class LoggingContextArgs<T> where T : ILoggingContext
	{
		/// <summary>
		/// The action this context is for.
		/// </summary>
		public LogAction Action { get; set; }
		/// <summary>
		/// The log counter to increment.
		/// </summary>
		public string LogCounterName { get; set; } = "";
		/// <summary>
		/// Actions to do when the logging context is valid.
		/// </summary>
		public Func<T, Task>[] WhenCanLog { get; set; } = Array.Empty<Func<T, Task>>();
		/// <summary>
		/// Actions to do no matter the validity of the logging context.
		/// </summary>
		public Func<T, Task>[] AnyTime { get; set; } = Array.Empty<Func<T, Task>>();
	}
}
