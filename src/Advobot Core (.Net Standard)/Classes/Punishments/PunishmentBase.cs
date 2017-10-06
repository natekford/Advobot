using Advobot.Enums;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;

namespace Advobot.Classes.Punishments
{
	public abstract class PunishmentBase
	{
		protected static ImmutableDictionary<PunishmentType, string> _Given = new Dictionary<PunishmentType, string>
		{
			{ PunishmentType.Kick, "kicked" },
			{ PunishmentType.Ban, "banned" },
			{ PunishmentType.Deafen, "deafened" },
			{ PunishmentType.VoiceMute, "voice-muted" },
			{ PunishmentType.RoleMute, "role-muted" },
			{ PunishmentType.Softban, "softbanned" },
		}.ToImmutableDictionary();
		protected static ImmutableDictionary<PunishmentType, string> _Removal = new Dictionary<PunishmentType, string>
		{
			{ PunishmentType.Kick, "unkicked" }, //Doesn't make sense
			{ PunishmentType.Ban, "unbanned" },
			{ PunishmentType.Deafen, "undeafened" },
			{ PunishmentType.VoiceMute, "unvoice-muted" },
			{ PunishmentType.RoleMute, "unrole-muted" },
			{ PunishmentType.Softban, "unsoftbanned" }, //Doesn't make sense either
		}.ToImmutableDictionary();
	}
}
