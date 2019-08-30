using Discord;

namespace Advobot.Services.Logging.Loggers
{
	/// <summary>
	/// Helps with logging a message.
	/// </summary>
	public interface IMessageLoggingContext : ILoggingContext
	{
		/// <summary>
		/// The channel this context is on.
		/// </summary>
		ITextChannel Channel { get; }

		/// <summary>
		/// The message this context is on.
		/// </summary>
		IUserMessage Message { get; }

		/// <summary>
		/// The user this context is on.
		/// </summary>
		IGuildUser User { get; }
	}
}