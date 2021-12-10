using Advobot.Logging.Database;

using Discord;

namespace Advobot.Logging.Context.Users;

public class UserUpdatedState : UserState
{
	public IUser Before { get; }

	public UserUpdatedState(IUser before, IGuildUser user)
		: base(user)
	{
		Before = before;
	}

	public override Task<bool> CanLog(ILoggingDatabase db, ILogContext context)
		=> Task.FromResult(!(User.IsBot || User.IsWebhook));
}