using Discord;

namespace Advobot.Logging.Context
{
	/// <summary>
	/// Helps with logging.
	/// </summary>
	public interface ILoggingContext
	{
		/// <summary>
		/// The bot.
		/// </summary>
		IGuildUser Bot { get; }

		/// <summary>
		/// The guild this context is on.
		/// </summary>
		IGuild Guild { get; }

		/// <summary>
		/// Where images get logged.
		/// </summary>
		ITextChannel? ImageLog { get; }

		/// <summary>
		/// Where successful commands get logged.
		/// </summary>
		ITextChannel? ModLog { get; }

		/// <summary>
		/// Where message/user actions get logged.
		/// </summary>
		ITextChannel? ServerLog { get; }

		/// <summary>
		/// Whether the current context can be logged.
		/// </summary>
		bool CanLog(LogAction action);

		/// <summary>
		/// Whether the current channel can be logged.
		/// </summary>
		/// <returns></returns>
		bool ChannelCanBeLogged();
	}
}