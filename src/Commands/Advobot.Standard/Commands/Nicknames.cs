using Advobot.Attributes;
using Advobot.Localization;
using Advobot.Modules;
using Advobot.Preconditions.Permissions;
using Advobot.Resources;

using Discord;
using Discord.Commands;

namespace Advobot.Standard.Commands;

[Category(nameof(Nicknames))]
public sealed class Nicknames : ModuleBase
{
	[LocalizedGroup(nameof(Groups.RemoveAllNickNames))]
	[LocalizedAlias(nameof(Aliases.RemoveAllNickNames))]
	[LocalizedSummary(nameof(Summaries.RemoveAllNickNames))]
	[Meta("d31a48de-ad5d-4f15-b216-299b8b8c66dd", IsEnabled = true)]
	[RequireGuildPermissions(GuildPermission.ManageNicknames)]
	public sealed class RemoveAllNickNames : MultiUserActionModuleBase
	{
		[Command(RunMode = RunMode.Async)]
		public async Task<RuntimeResult> Command(bool getUnlimitedUsers = false)
		{
			var amountChanged = await ProcessAsync(
				getUnlimitedUsers,
				u => u.Nickname != null,
				(u, o) => u.ModifyAsync(x => x.Nickname = u.Username, o),
				i => Responses.Users.MultiUserActionProgress(i.AmountLeft).Reason,
				GetOptions()
			).ConfigureAwait(false);
			return Responses.Users.MultiUserActionSuccess(amountChanged);
		}
	}
}