using System;
using System.Threading.Tasks;
using Advobot.Services.GuildSettings.Settings;

namespace Advobot.Services.Logging.Loggers
{
	internal sealed class LoggingContextArgs<T> where T : ILoggingContext
	{
		public LogAction Action { get; set; }
		public string LogCounterName { get; set; } = "";
		public Func<T, Task>[] WhenCanLog { get; set; } = Array.Empty<Func<T, Task>>();
		public Func<T, Task>[] AnyTime { get; set; } = Array.Empty<Func<T, Task>>();
	}
}
