using Advobot.Modules;
using Advobot.Preconditions.Permissions;
using Advobot.Resources;

using YACCS.Commands.Attributes;
using YACCS.Localization;

namespace Advobot.AutoMod.Commands;

[LocalizedCategory(nameof(Names.PunishmentsCategory))]
public sealed class Punishments
{
	[Command(nameof(Names.ModifyPunishments), nameof(Names.ModifyPunishmentsAlias))]
	[LocalizedSummary(nameof(Summaries.ModifyPunishmentsSummary))]
	[Id("4b4584ae-2b60-4aff-92a1-fb2c929f3daf")]
	[RequireGuildPermissions]
	public sealed class ModifyBannedPhrasePunishments : AutoModModuleBase;
}