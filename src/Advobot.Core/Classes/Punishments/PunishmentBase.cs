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
		protected static ImmutableDictionary<Punishment, string> _Given = new Dictionary<Punishment, string>
		{
			{ Punishment.Kick, "kicked" },
			{ Punishment.Ban, "banned" },
			{ Punishment.Deafen, "deafened" },
			{ Punishment.VoiceMute, "voice-muted" },
			{ Punishment.RoleMute, "role-muted" },
			{ Punishment.Softban, "softbanned" }
		}.ToImmutableDictionary();
		protected static ImmutableDictionary<Punishment, string> _Removal = new Dictionary<Punishment, string>
		{
			{ Punishment.Kick, "unkicked" }, //Doesn't make sense
			{ Punishment.Ban, "unbanned" },
			{ Punishment.Deafen, "undeafened" },
			{ Punishment.VoiceMute, "unvoice-muted" },
			{ Punishment.RoleMute, "unrole-muted" },
			{ Punishment.Softban, "unsoftbanned" } //Doesn't make sense either
		}.ToImmutableDictionary();

		protected ITimersService _Timers;
		protected List<string> _Actions = new List<string>();
		public ImmutableList<string> Actions => _Actions.ToImmutableList();

		public PunishmentBase(ITimersService timers)
		{
			_Timers = timers;
		}

		public override string ToString()
		{
			return String.Join("\n", _Actions);
		}
	}
}
