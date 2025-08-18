using Advobot.Logging.Database;

using Discord;

namespace Advobot.Logging.Service.Context.Users;

public class UserContext(IGuild guild, IUser user) : ILogContext
{
	public IGuild Guild { get; } = guild;
	public IUser User { get; } = user;

	public virtual async Task<bool> IsValidAsync(LoggingDatabase db)
	{
		if (Guild is null || User is null)
		{
			return false;
		}

		// Only log if it wasn't the bot that was affected
		var bot = await Guild.GetCurrentUserAsync().ConfigureAwait(false);
		return User.Id != bot.Id;
	}
}