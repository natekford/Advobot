using Discord;

namespace Advobot.Logging.Context
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