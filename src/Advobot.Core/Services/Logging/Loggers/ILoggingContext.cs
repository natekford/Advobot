using Advobot.Services.GuildSettings;
using Advobot.Services.GuildSettings.Settings;

using Discord;

namespace Advobot.Services.Logging.Loggers
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
		/// Where message/user actions get logged.
		/// </summary>
		ITextChannel? ServerLog { get; }

		/// <summary>
		/// The settings this logger is targetting.
		/// </summary>
		IGuildSettings Settings { get; }

		/// <summary>
		/// Whether the current context can be logged.
		/// </summary>
		bool CanLog(LogAction action);
	}
}