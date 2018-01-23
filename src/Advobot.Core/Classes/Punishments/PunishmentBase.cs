using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Advobot.Core.Enums;
using Advobot.Core.Interfaces;

namespace Advobot.Core.Classes.Punishments
{
	/// <summary>
	/// Base for classes like <see cref="PunishmentGiver"/> and <see cref="PunishmentRemover"/>. Holds the past tense words for removing
	/// and giving <see cref="PunishmentType"/> values.
	/// </summary>
	public abstract class PunishmentBase
	{
		protected static ImmutableDictionary<PunishmentType, string> _Given = new Dictionary<PunishmentType, string>
		{
			{ PunishmentType.Kick, "kicked" },
			{ PunishmentType.Ban, "banned" },
			{ PunishmentType.Deafen, "deafened" },
			{ PunishmentType.VoiceMute, "voice-muted" },
			{ PunishmentType.RoleMute, "role-muted" },
			{ PunishmentType.Softban, "softbanned" }
		}.ToImmutableDictionary();
		protected static ImmutableDictionary<PunishmentType, string> _Removal = new Dictionary<PunishmentType, string>
		{
			{ PunishmentType.Kick, "unkicked" }, //Doesn't make sense
			{ PunishmentType.Ban, "unbanned" },
			{ PunishmentType.Deafen, "undeafened" },
			{ PunishmentType.VoiceMute, "unvoice-muted" },
			{ PunishmentType.RoleMute, "unrole-muted" },
			{ PunishmentType.Softban, "unsoftbanned" } //Doesn't make sense either
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
