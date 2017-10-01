using Advobot.Enums;
using Discord;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Advobot.Classes.Punishments
{
	public class PunishmentHandlerBase
	{
		protected static ReadOnlyDictionary<PunishmentType, string> _Given = new ReadOnlyDictionary<PunishmentType, string>(new Dictionary<PunishmentType, string>
		{
			{ PunishmentType.Kick,		"kicked"		},
			{ PunishmentType.Ban,		"banned"		},
			{ PunishmentType.Deafen,	"deafened"		},
			{ PunishmentType.VoiceMute,	"voice-muted"	},
			{ PunishmentType.RoleMute,	"role-muted"	},
			{ PunishmentType.Softban,	"softbanned"	},
		});
		protected static ReadOnlyDictionary<PunishmentType, string> _Removal = new ReadOnlyDictionary<PunishmentType, string>(new Dictionary<PunishmentType, string>
		{
			{ PunishmentType.Kick,      "unkicked"		}, //Doesn't make sense
			{ PunishmentType.Ban,       "unbanned"		},
			{ PunishmentType.Deafen,    "undeafened"	},
			{ PunishmentType.VoiceMute, "unvoice-muted" },
			{ PunishmentType.RoleMute,  "unrole-muted"	},
			{ PunishmentType.Softban,   "unsoftbanned"	}, //Doesn't make sense either
		});
	}
}
