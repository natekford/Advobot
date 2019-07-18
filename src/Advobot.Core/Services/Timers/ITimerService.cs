using Advobot.Classes;
using Advobot.Enums;

namespace Advobot.Services.Timers
{
	/// <summary>
	/// Abstraction for a timers module. Handles timed punishments, close words, and timed message deletion/sending.
	/// </summary>
	public interface ITimerService
	{
		/// <summary>
		/// Removes all older instances, and stores <paramref name="value"/>.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		void Add(RemovablePunishment value);
		/// <summary>
		/// Stores <paramref name="value"/>.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		void Add(RemovableMessage value);
		/// <summary>
		/// Stores <paramref name="value"/>.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		void Add(TimedMessage value);
		/// <summary>
		/// Removes the punishment from the database and returns it.
		/// </summary>
		/// <param name="guildId"></param>
		/// <param name="userId"></param>
		/// <param name="punishment"></param>
		/// <returns></returns>
		bool RemovePunishmentAsync(ulong guildId, ulong userId, Punishment punishment);
	}
}
