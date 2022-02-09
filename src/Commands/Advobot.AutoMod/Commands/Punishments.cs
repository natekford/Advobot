using Advobot.Attributes;
using Advobot.Localization;
using Advobot.Preconditions.Permissions;
using Advobot.Resources;

using Discord.Commands;

namespace Advobot.AutoMod.Commands;

[Category(nameof(Punishments))]
public sealed class Punishments : ModuleBase
{
	[LocalizedGroup(nameof(Groups.ModifyPunishments))]
	[LocalizedAlias(nameof(Aliases.ModifyPunishments))]
	[LocalizedSummary(nameof(Summaries.ModifyPunishments))]
	[Meta("4b4584ae-2b60-4aff-92a1-fb2c929f3daf")]
	[RequireGuildPermissions]
	public sealed class ModifyBannedPhrasePunishments : AutoModModuleBase
	{
	}
}