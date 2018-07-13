using Advobot.Classes;
using Advobot.Classes.CloseWords;
using Advobot.Classes.Punishments;
using Advobot.Enums;
using Discord;
using System.Threading.Tasks;

namespace Advobot.Interfaces
{
	/// <summary>
	/// Abstraction for a timers module. Handles timed punishments, close words, and timed message deletion/sending.
	/// </summary>
	public interface ITimersService
	{
		/// <summary>
		/// Starts the second, minute, and hour timers.
		/// </summary>
		void Start();

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
		/// Updates or inserts the supplied value in the database. If inserting, use AddAsync instead to do the other actions.
		/// </summary>
		/// <param name="punishment"></param>
		/// <returns></returns>
		bool Update(RemovablePunishment punishment);
		/// <summary>
		/// Updates or inserts the supplied value in the database. If inserting, use AddAsync instead to do the other actions.
		/// </summary>
		/// <param name="help"></param>
		/// <returns></returns>
		bool Update(CloseHelpEntries help);
		/// <summary>
		/// Updates or inserts the supplied value in the database. If inserting, use AddAsync instead to do the other actions.
		/// </summary>
		/// <param name="quote"></param>
		/// <returns></returns>
		bool Update(CloseQuotes quote);
		/// <summary>
		/// Updates or inserts the supplied value in the database. If inserting, use AddAsync instead to do the other actions.
		/// </summary>
		/// <param name="message"></param>
		/// <returns></returns>
		bool Update(RemovableMessage message);
		/// <summary>
		/// Updates or inserts the supplied value in the database. If inserting, use AddAsync instead to do the other actions.
		/// </summary>
		/// <param name="message"></param>
		/// <returns></returns>
		bool Update(TimedMessage message);

		/// <summary>
		/// Removes the punishment from the database and returns it.
		/// </summary>
		/// <param name="guild"></param>
		/// <param name="userId"></param>
		/// <param name="punishment"></param>
		/// <returns></returns>
		Task<RemovablePunishment> RemovePunishmentAsync(IGuild guild, ulong userId, Punishment punishment);
		/// <summary>
		/// Removes the close help from the database and returns it.
		/// </summary>
		/// <param name="user"></param>
		/// <returns></returns>
		Task<CloseHelpEntries> RemoveActiveCloseHelpAsync(IUser user);
		/// <summary>
		/// Removes the close quotes from the database and returns it.
		/// </summary>
		/// <param name="user"></param>
		/// <returns></returns>
		Task<CloseQuotes> RemoveActiveCloseQuoteAsync(IUser user);
	}
}
