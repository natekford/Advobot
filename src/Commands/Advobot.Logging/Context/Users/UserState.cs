using Advobot.Logging.Database;

using Discord;

namespace Advobot.Logging.Context.Users;

public class UserState : ILogState
{
	public IGuild Guild { get; }
	public bool IsValid => User is not null;
	public IUser User { get; }

	public UserState(IGuildUser user) : this(user.Guild, user)
	{
	}

	public UserState(IGuild guild, IUser user)
	{
		Guild = guild;
		User = user;
	}

	// Only log if it wasn't this bot that was affected
	public virtual Task<bool> CanLog(ILoggingDatabase db, ILogContext context)
		=> Task.FromResult(User.Id != context.Bot.Id);
}