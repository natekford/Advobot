using Advobot.Enums;
using System.Collections.Generic;

namespace Advobot.Classes.Punishments
{
	public class PunishmentHandlerBase
	{
		protected static Dictionary<PunishmentType, string> _Given = new Dictionary<PunishmentType, string>
		{
			{ PunishmentType.Kick,		"kicked"		},
			{ PunishmentType.Ban,		"banned"		},
			{ PunishmentType.Deafen,	"deafened"		},
			{ PunishmentType.VoiceMute,	"voice-muted"	},
			{ PunishmentType.RoleMute,	"role-muted"	},
			{ PunishmentType.Softban,	"softbanned"	},
		};
		protected static Dictionary<PunishmentType, string> _Removal = new Dictionary<PunishmentType, string>
		{
			{ PunishmentType.Kick,      "unkicked"		}, //Doesn't make sense
			{ PunishmentType.Ban,       "unbanned"		},
			{ PunishmentType.Deafen,    "undeafened"	},
			{ PunishmentType.VoiceMute, "unvoice-muted" },
			{ PunishmentType.RoleMute,  "unrole-muted"	},
			{ PunishmentType.Softban,   "unsoftbanned"	}, //Doesn't make sense either
		};
	}
}
