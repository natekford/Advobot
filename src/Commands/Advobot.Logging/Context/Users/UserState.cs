
using Advobot.Logging.Database;

using Discord;

namespace Advobot.Logging.Context.Users
{
	public class UserState : ILogState
	{
		public bool IsValid => !(User is null);
		public IGuildUser User { get; }
		public IGuild Guild => User.Guild;

		public UserState(IGuildUser user)
		{
			User = user;
		}

		// Only log if it wasn't this bot that was affected
		public virtual Task<bool> CanLog(ILoggingDatabase db, ILogContext context)
			=> Task.FromResult(User.Id != context.Bot.Id);
	}
}