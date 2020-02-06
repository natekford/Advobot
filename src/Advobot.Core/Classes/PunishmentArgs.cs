using System;

using Discord;

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
		/// The amount of days worth of messages to delete. This will only be used if the punishment involves banning.
		/// </summary>
		public int? Days { get; set; }

		/// <summary>
		/// The Discord request options.
		/// </summary>
		public RequestOptions? Options { get; set; }

		/// <summary>
		/// Whether a punishment was removed from the timers.
		/// </summary>
		public bool PunishmentRemoved { get; private set; }

		/// <summary>
		/// The role to give or remove. This will only be used if the punishment involves roles.
		/// </summary>
		public IRole? Role { get; set; }

		/// <summary>
		/// The amount of time the punishment should last for.
		/// </summary>
		public TimeSpan? Time { get; set; }

		/// <summary>
		/// Creates an instance of <see cref="PunishmentArgs"/> with no time or timers.
		/// </summary>
		public PunishmentArgs() { }

		void IPunishmentRemoved.SetPunishmentRemoved()
			=> PunishmentRemoved = true;

		internal interface IPunishmentRemoved
		{
			void SetPunishmentRemoved();
		}
	}
}