using System.Collections.Generic;

using Discord;

namespace Advobot.Logging.Context
{

	public interface ILoggingContext<out T> : ILoggingContext
	{
		T State { get; }
	}
}