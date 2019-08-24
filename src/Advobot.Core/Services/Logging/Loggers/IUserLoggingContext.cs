using Discord;

namespace Advobot.Services.Logging.Loggers
{
	/// <summary>
	/// Helps with logging a user.
	/// </summary>
	public interface IUserLoggingContext : ILoggingContext
	{
		/// <summary>
		/// The user this context is on.
		/// </summary>
		IGuildUser User { get; }
	}
}
