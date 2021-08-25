
using Advobot.Logging.Database;

using Discord;

namespace Advobot.Logging.Context
{
	public interface ILogState
	{
		IGuild? Guild { get; }
		bool IsValid { get; }

		Task<bool> CanLog(ILoggingDatabase db, ILogContext context);
	}
}