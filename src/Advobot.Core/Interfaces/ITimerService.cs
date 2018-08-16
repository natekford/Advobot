using System.Threading.Tasks;
using Advobot.Classes;
using Advobot.Classes.CloseWords;
using Advobot.Enums;

namespace Advobot.Interfaces
{
	/// <summary>
	/// Abstraction for a timers module. Handles timed punishments, close words, and timed message deletion/sending.
	/// </summary>
	public interface ITimerService
	{
		/// <summary>
		/// Removes all older instances, undoes their current punishment, and stores <paramref name="punishment"/>.
		/// </summary>
		/// <param name="punishment"></param>
		/// <returns></returns>
		Task AddAsync(RemovablePunishment punishment);
		/// <summary>
		/// Removes all older instances, deletes the bot's message, and stores <paramref name="helpEntries"/>.
		/// </summary>
		/// <param name="helpEntries"></param>
		/// <returns></returns>
		Task AddAsync(CloseHelpEntries helpEntries);
		/// <summary>
		/// Removes all older instances, deletes the bot's message, and stores <paramref name="quotes"/>.
		/// </summary>
		/// <param name="quotes"></param>
		/// <returns></returns>
		Task AddAsync(CloseQuotes quotes);
		/// <summary>
		/// Stores <paramref name="message"/>.
		/// </summary>
		/// <param name="message"></param>
		/// <returns></returns>
		Task AddAsync(RemovableMessage message);
		/// <summary>
		/// Stores <paramref name="message"/>.
		/// </summary>
		/// <param name="message"></param>
		/// <returns></returns>
		Task AddAsync(TimedMessage message);

		/// <summary>
		/// Removes the punishment from the database and returns it.
		/// </summary>
		/// <param name="guildId"></param>
		/// <param name="userId"></param>
		/// <param name="punishment"></param>
		/// <returns></returns>
		Task<RemovablePunishment> RemovePunishmentAsync(ulong guildId, ulong userId, Punishment punishment);
		/// <summary>
		/// Removes the close help from the database and returns it.
		/// </summary>
		/// <param name="guildId"></param>
		/// <param name="userId"></param>
		/// <returns></returns>
		Task<CloseHelpEntries> RemoveActiveCloseHelpAsync(ulong guildId, ulong userId);
		/// <summary>
		/// Removes the close quotes from the database and returns it.
		/// </summary>
		/// <param name="guildId"></param>
		/// <param name="userId"></param>
		/// <returns></returns>
		Task<CloseQuotes> RemoveActiveCloseQuoteAsync(ulong guildId, ulong userId);
	}
}
