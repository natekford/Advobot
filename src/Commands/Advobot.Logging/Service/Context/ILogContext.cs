using Advobot.Logging.Database;

using Discord;

namespace Advobot.Logging.Service.Context;

public interface ILogContext
{
	IGuild Guild { get; }

	Task<bool> IsValidAsync(LoggingDatabase db);
}