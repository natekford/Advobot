using Advobot.Attributes;
using Advobot.Modules;
using Advobot.Preconditions.Permissions;
using Advobot.Resources;

using Discord;

using YACCS.Commands.Attributes;
using YACCS.Localization;

namespace Advobot.Standard.Commands;

[LocalizedCategory(nameof(Names.NicknamesCategory))]
public sealed class Nicknames : AdvobotModuleBase
{
	[LocalizedCommand(nameof(Names.RemoveAllNickNames), nameof(Names.RemoveAllNickNamesAlias))]
	[LocalizedSummary(nameof(Summaries.RemoveAllNickNamesSummary))]
	[Meta("d31a48de-ad5d-4f15-b216-299b8b8c66dd", IsEnabled = true)]
	[RequireGuildPermissions(GuildPermission.ManageNicknames)]
	public sealed class RemoveAllNickNames : MultiUserActionModuleBase
	{
		[Command]
		public async Task<AdvobotResult> Remove(bool getUnlimitedUsers = false)
		{
			var amountChanged = await ProcessAsync(
				getUnlimitedUsers,
				u => u.Nickname != null,
				(u, o) => u.ModifyAsync(x => x.Nickname = u.Username, o),
				i => Responses.Users.MultiUserActionProgress(i.AmountLeft).Response,
				GetOptions()
			).ConfigureAwait(false);
			return Responses.Users.MultiUserActionSuccess(amountChanged);
		}
	}
}