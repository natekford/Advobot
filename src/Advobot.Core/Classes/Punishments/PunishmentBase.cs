using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Advobot.Core.Enums;
using Advobot.Core.Interfaces;

namespace Advobot.Core.Classes.Punishments
{
	/// <summary>
	/// Base for classes like <see cref="PunishmentGiver"/> and <see cref="PunishmentRemover"/>. Holds the past tense words for removing
	/// and giving <see cref="Punishment"/> values.
	/// </summary>
	public abstract class PunishmentBase
	{
		/// <summary>
		/// Strings for saying the type of punishment given.
		/// </summary>
		protected static ImmutableDictionary<Punishment, string> Given = new Dictionary<Punishment, string>
		{
			{ Punishment.Kick, "kicked" },
			{ Punishment.Ban, "banned" },
			{ Punishment.Deafen, "deafened" },
			{ Punishment.VoiceMute, "voice-muted" },
			{ Punishment.RoleMute, "role-muted" },
			{ Punishment.Softban, "softbanned" }
		}.ToImmutableDictionary();
		/// <summary>
		/// Strings for saying the type of punishment removed.
		/// </summary>
		protected static ImmutableDictionary<Punishment, string> Removed = new Dictionary<Punishment, string>
		{
			{ Punishment.Kick, "unkicked" }, //Doesn't make sense
			{ Punishment.Ban, "unbanned" },
			{ Punishment.Deafen, "undeafened" },
			{ Punishment.VoiceMute, "unvoice-muted" },
			{ Punishment.RoleMute, "unrole-muted" },
			{ Punishment.Softban, "unsoftbanned" } //Doesn't make sense either
		}.ToImmutableDictionary();

		/// <summary>
		/// The timer service to add punishments to or remove punishments from.
		/// </summary>
		protected ITimersService Timers;
		/// <summary>
		/// The actions which were done on users.
		/// </summary>
		protected List<string> Actions = new List<string>();

		/// <summary>
		/// Creates an instance of punishment base.
		/// </summary>
		/// <param name="timers"></param>
		public PunishmentBase(ITimersService timers)
		{
			Timers = timers;
		}

		/// <summary>
		/// Returns all the actions joined together.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return String.Join("\n", Actions);
		}
	}
}
