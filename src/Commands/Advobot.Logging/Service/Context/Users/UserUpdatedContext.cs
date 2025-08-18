using Advobot.Logging.Database;

using Discord;

namespace Advobot.Logging.Service.Context.Users;

public class UserUpdatedContext(IGuild guild, IUser before, IUser after)
	: UserContext(guild, after)
{
	public IUser Before { get; } = before;

	public override Task<bool> IsValidAsync(LoggingDatabase db)
	{
		if (User.IsBot || User.IsWebhook)
		{
			return Task.FromResult(false);
		}

		return base.IsValidAsync(db);
	}
}