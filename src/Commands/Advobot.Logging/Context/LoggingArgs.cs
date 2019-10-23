using System;
using System.Threading.Tasks;

namespace Advobot.Logging.Context
{
	public sealed class LoggingArgs<T> where T : ILoggingContext
	{
		public LogAction Action { get; set; }
		public Func<T, Task>[] Actions { get; set; } = Array.Empty<Func<T, Task>>();
	}
}