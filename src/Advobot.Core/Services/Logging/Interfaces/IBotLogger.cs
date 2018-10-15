using System.Threading.Tasks;
using Discord;

namespace Advobot.Services.Logging.Interfaces
{
	/// <summary>
	/// Logs actions related to the bot.
	/// </summary>
	internal interface IBotLogger : ILogger
	{
		/// <summary>
		/// When the api wrapper sends a log message.
		/// </summary>
		/// <param name="message"></param>
		/// <returns></returns>
		Task OnLogMessageSent(LogMessage message);
	}
}