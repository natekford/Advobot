using System.Threading.Tasks;
using Advobot.Classes;
using Advobot.Classes.CloseWords;
using Advobot.Enums;

namespace Advobot.Interfaces
{
	/// <summary>
	/// Abstraction for a timers module. Handles timed punishments, close words, and timed message deletion/sending.
	/// </summary>
	public interface ITimerService : IUsesDatabase
	{
		/// <summary>
		/// Removes all older instances, undoes their current punishment, and stores <paramref name="value"/>.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		Task AddAsync(RemovablePunishment value);
		/// <summary>
		/// Stores <paramref name="value"/>.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		Task AddAsync(RemovableMessage value);
		/// <summary>
		/// Stores <paramref name="value"/>.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		Task AddAsync(TimedMessage value);
		/// <summary>
		/// Removes the punishment from the database and returns it.
		/// </summary>
		/// <param name="guildId"></param>
		/// <param name="userId"></param>
		/// <param name="punishment"></param>
		/// <returns></returns>
		Task<RemovablePunishment> RemovePunishmentAsync(ulong guildId, ulong userId, Punishment punishment);
	}
}
