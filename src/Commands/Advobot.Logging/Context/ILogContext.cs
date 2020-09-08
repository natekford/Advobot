using System.Collections.Generic;

using Discord;

namespace Advobot.Logging.Context
{
	/// <summary>
	/// Helps with logging.
	/// </summary>
	public interface ILogContext
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
	}
}