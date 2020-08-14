using System;
using System.Threading.Tasks;

namespace Advobot.Punishments
{
	/// <summary>
	/// Add and remove punishments for guild users.
	/// </summary>
	public interface IPunisher
	{
		/// <summary>
		/// When a punishment is given.
		/// </summary>
		public event Func<IPunishmentContext, Task> PunishmentGiven;

		/// <summary>
		/// When a punishment is removed.
		/// </summary>
		public event Func<IPunishmentContext, Task> PunishmentRemoved;

		/// <summary>
		/// Handles a punishment context.
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
		public Task HandleAsync(IPunishmentContext context);
	}
}