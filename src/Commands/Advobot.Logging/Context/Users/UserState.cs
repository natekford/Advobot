using Advobot.Logging.Database;

using Discord;

namespace Advobot.Logging.Context.Users;

public class UserState(IGuild guild, IUser user) : ILogState
{
	public IGuild Guild { get; } = guild;
	public bool IsValid => User is not null;
	public IUser User { get; } = user;

	public UserState(IGuildUser user) : this(user.Guild, user)
	{
	}

	// Only log if it wasn't the bot that was affected
	public virtual Task<bool> CanLog(ILoggingDatabase db, ILogContext context)
		=> Task.FromResult(User.Id != context.Bot.Id);
}