using System;
using System.Linq;
using System.Threading.Tasks;
using Advobot.Enums;
using Advobot.Interfaces;
using AdvorangesUtils;
using Discord;
using Discord.WebSocket;

namespace Advobot.Classes
{
	/// <summary>
	/// Extra arguments used when punishing a user or removing a punishment from a user.
	/// </summary>
	public sealed class PunishmentArgs : PunishmentArgs.IPunishmentRemoved
	{
		/// <summary>
		/// An empty non-null default value of this class.
		/// </summary>
		public static readonly PunishmentArgs Default = new PunishmentArgs();

		/// <summary>
		/// The amount of time the punishment should last for.
		/// </summary>
		public TimeSpan? Time { get; }
		/// <summary>
		/// The timer service that timed objects should be added to.
		/// </summary>
		public ITimerService? Timers { get; }
		/// <summary>
		/// The Discord request options.
		/// </summary>
		public RequestOptions? Options { get; set; }
		/// <summary>
		/// Whether a punishment was removed from the timers.
		/// </summary>
		public bool PunishmentRemoved { get; private set; }

		/// <summary>
		/// Creates an instance of <see cref="PunishmentArgs"/> with no time or timers.
		/// </summary>
		public PunishmentArgs() { }
		/// <summary>
		/// Creates an instance of <see cref="PunishmentArgs"/> with timer and timers.
		/// </summary>
		/// <param name="timers"></param>
		/// <param name="time"></param>
		public PunishmentArgs(ITimerService timers, TimeSpan time)
		{
			Timers = timers;
			Time = time;
		}

		void IPunishmentRemoved.SetPunishmentRemoved()
			=> PunishmentRemoved = true;

		internal interface IPunishmentRemoved
		{
			void SetPunishmentRemoved();
		}
	}
}
