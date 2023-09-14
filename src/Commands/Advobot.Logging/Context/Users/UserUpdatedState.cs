using Advobot.Logging.Database;

using Discord;

namespace Advobot.Logging.Context.Users;

public class UserUpdatedState(IUser before, IGuildUser user) : UserState(user)
{
	public IUser Before { get; } = before;

	public override Task<bool> CanLog(ILoggingDatabase db, ILogContext context)
		=> Task.FromResult(!(User.IsBot || User.IsWebhook));
}