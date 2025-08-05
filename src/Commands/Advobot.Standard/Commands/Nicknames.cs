using Advobot.Attributes;
using Advobot.Localization;
using Advobot.Modules;
using Advobot.ParameterPreconditions.DiscordObjectValidation.Users;
using Advobot.ParameterPreconditions.Numbers;
using Advobot.ParameterPreconditions.Strings;
using Advobot.Preconditions.Permissions;
using Advobot.Resources;
using Advobot.TypeReaders;

using AdvorangesUtils;

using Discord;
using Discord.Commands;

namespace Advobot.Standard.Commands;

[Category(nameof(Nicknames))]
public sealed class Nicknames : ModuleBase
{
	[LocalizedGroup(nameof(Groups.ModifyNickName))]
	[LocalizedAlias(nameof(Aliases.ModifyNickName))]
	[LocalizedSummary(nameof(Summaries.ModifyNickName))]
	[Meta("3e6e2221-3929-4bc3-a019-cfa5b04b5621", IsEnabled = true)]
	[RequireGuildPermissions(GuildPermission.ManageNicknames)]
	public sealed class ModifyNickName : AdvobotModuleBase
	{
		[Command]
		public async Task<RuntimeResult> Command(
			[CanModifyUser] IGuildUser user)
		{
			await user.ModifyAsync(x => x.Nickname = user.Username, GetOptions()).CAF();
			return Responses.Nicknames.RemovedNickname(user);
		}

		[Command]
		public async Task<RuntimeResult> Command(
			[CanModifyUser] IGuildUser user,
			[Nickname] string nickname)
		{
			await user.ModifyAsync(x => x.Nickname = nickname, GetOptions()).CAF();
			return Responses.Nicknames.ModifiedNickname(user, nickname);
		}
	}

	[LocalizedGroup(nameof(Groups.RemoveAllNickNames))]
	[LocalizedAlias(nameof(Aliases.RemoveAllNickNames))]
	[LocalizedSummary(nameof(Summaries.RemoveAllNickNames))]
	[Meta("d31a48de-ad5d-4f15-b216-299b8b8c66dd", IsEnabled = true)]
	[RequireGuildPermissions(GuildPermission.ManageNicknames)]
	public sealed class RemoveAllNickNames : MultiUserActionModule
	{
		[Command(RunMode = RunMode.Async)]
		public async Task<RuntimeResult> Command(
			[OverrideTypeReader(typeof(BypassUserLimitTypeReader))]
				bool bypass = false
		)
		{
			var amountChanged = await ProcessAsync(
				bypass,
				u => u.Nickname != null,
				(u, o) => u.ModifyAsync(x => x.Nickname = u.Username, o),
				i => Responses.Users.MultiUserActionProgress(i.AmountLeft).Reason,
				GetOptions()
			).CAF();
			return Responses.Users.MultiUserActionSuccess(amountChanged);
		}
	}

	[LocalizedGroup(nameof(Groups.ReplaceByUtf16))]
	[LocalizedAlias(nameof(Aliases.ReplaceByUtf16))]
	[LocalizedSummary(nameof(Summaries.ReplaceByUtf16))]
	[Meta("8d4e53fd-c728-4e55-9262-3078468738e5", IsEnabled = true)]
	[RequireGuildPermissions(GuildPermission.ManageNicknames)]
	public sealed class ReplaceByUtf16 : MultiUserActionModule
	{
		[Command(RunMode = RunMode.Async)]
		public async Task<RuntimeResult> Command(
			[Positive]
			int upperLimit,
			[Nickname]
			string replace,
			[OverrideTypeReader(typeof(BypassUserLimitTypeReader))]
			bool bypass = false
		)
		{
			var amountChanged = await ProcessAsync(
				bypass,
				u => (u.Nickname?.AllCharsWithinLimit(upperLimit) == false) || (u.Nickname == null && !u.Username.AllCharsWithinLimit(upperLimit)),
				(u, o) => u.ModifyAsync(x => x.Nickname = replace, o),
				i => Responses.Users.MultiUserActionProgress(i.AmountLeft).Reason,
				GetOptions()
			).CAF();
			return Responses.Users.MultiUserActionSuccess(amountChanged);
		}
	}

	[LocalizedGroup(nameof(Groups.ReplaceWordsInNames))]
	[LocalizedAlias(nameof(Aliases.ReplaceWordsInNames))]
	[LocalizedSummary(nameof(Summaries.ReplaceWordsInNames))]
	[Meta("f637abf3-f944-413a-95d3-d06aa07921fd", IsEnabled = true)]
	[RequireGuildPermissions(GuildPermission.ManageNicknames)]
	public sealed class ReplaceWordsInNames : MultiUserActionModule
	{
		[Command(RunMode = RunMode.Async)]
		public async Task<RuntimeResult> Command(
			[Nickname]
			string search,
			[Nickname]
			string replace,
			[OverrideTypeReader(typeof(BypassUserLimitTypeReader))]
			bool bypass = false
		)
		{
			var amountChanged = await ProcessAsync(
				bypass,
				u => (u.Nickname?.CaseInsContains(search) == true) || (u.Nickname == null && u.Username.CaseInsContains(search)),
				(u, o) => u.ModifyAsync(x => x.Nickname = replace, o),
				i => Responses.Users.MultiUserActionProgress(i.AmountLeft).Reason,
				GetOptions()
			).CAF();
			return Responses.Users.MultiUserActionSuccess(amountChanged);
		}
	}
}