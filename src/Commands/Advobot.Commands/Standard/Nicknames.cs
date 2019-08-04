using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Advobot.Attributes;
using Advobot.Attributes.ParameterPreconditions.DiscordObjectValidation.Users;
using Advobot.Attributes.ParameterPreconditions.Numbers;
using Advobot.Attributes.ParameterPreconditions.Strings;
using Advobot.Attributes.Preconditions.Permissions;
using Advobot.Commands.Localization;
using Advobot.Commands.Resources;
using Advobot.Modules;
using Advobot.TypeReaders;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Discord.Commands;

namespace Advobot.Commands.Standard
{
	public sealed class Nicknames : ModuleBase
	{
		[Group(nameof(ModifyNickName)), ModuleInitialismAlias(typeof(ModifyNickName))]
		[LocalizedSummary(nameof(Summaries.ModifyNickName))]
		[GuildPermissionRequirement(GuildPermission.ManageNicknames)]
		[EnabledByDefault(true)]
		public sealed class ModifyNickName : AdvobotModuleBase
		{
			[Command]
			public async Task<RuntimeResult> Command(
				[User] IGuildUser user)
			{
				await user.ModifyAsync(x => x.Nickname = user.Username, GenerateRequestOptions()).CAF();
				return Responses.Nicknames.RemovedNickname(user);
			}
			[Command]
			public async Task<RuntimeResult> Command(
				[User] IGuildUser user,
				[Nickname] string nickname)
			{
				await user.ModifyAsync(x => x.Nickname = nickname, GenerateRequestOptions()).CAF();
				return Responses.Nicknames.ModifiedNickname(user, nickname);
			}
		}

		[Group(nameof(ReplaceWordsInNames)), ModuleInitialismAlias(typeof(ReplaceWordsInNames))]
		[LocalizedSummary(nameof(Summaries.ReplaceWordsInNames))]
		[GuildPermissionRequirement(GuildPermission.ManageNicknames)]
		[EnabledByDefault(true)]
		public sealed class ReplaceWordsInNames : MultiUserActionModule
		{
			[Command(RunMode = RunMode.Async)]
			public async Task<RuntimeResult> Command(
				[Nickname] string search,
				[Nickname] string replace,
				[Optional, OverrideTypeReader(typeof(BypassUserLimitTypeReader))] bool bypass)
			{
				ProgressLogger = new MultiUserActionProgressLogger(Context.Channel, i => Responses.Nicknames.MultiUserAction(i.AmountLeft).Reason, GenerateRequestOptions());
				var amountChanged = await ProcessAsync(bypass,
					u => (u.Nickname != null && u.Nickname.CaseInsContains(search)) || (u.Nickname == null && u.Username.CaseInsContains(search)),
					u => u.ModifyAsync(x => x.Nickname = replace, GenerateRequestOptions())).CAF();
				return Responses.Nicknames.MultiUserActionSuccess(amountChanged);
			}
		}

		[Group(nameof(ReplaceByUtf16)), ModuleInitialismAlias(typeof(ReplaceByUtf16))]
		[LocalizedSummary(nameof(Summaries.ReplaceByUtf16))]
		[GuildPermissionRequirement(GuildPermission.ManageNicknames)]
		[EnabledByDefault(true)]
		public sealed class ReplaceByUtf16 : MultiUserActionModule
		{
			[Command(RunMode = RunMode.Async)]
			public async Task<RuntimeResult> Command(
				[Positive] int upperLimit,
				[Nickname] string replace,
				[Optional, OverrideTypeReader(typeof(BypassUserLimitTypeReader))] bool bypass)
			{
				ProgressLogger = new MultiUserActionProgressLogger(Context.Channel, i => Responses.Nicknames.MultiUserAction(i.AmountLeft).Reason, GenerateRequestOptions());
				var amountChanged = await ProcessAsync(bypass,
					u => (u.Nickname != null && !u.Nickname.AllCharsWithinLimit(upperLimit)) || (u.Nickname == null && !u.Username.AllCharsWithinLimit(upperLimit)),
					u => u.ModifyAsync(x => x.Nickname = replace, GenerateRequestOptions())).CAF();
				return Responses.Nicknames.MultiUserActionSuccess(amountChanged);
			}
		}

		[Group(nameof(RemoveAllNickNames)), ModuleInitialismAlias(typeof(RemoveAllNickNames))]
		[LocalizedSummary(nameof(Summaries.RemoveAllNickNames))]
		[GuildPermissionRequirement(GuildPermission.ManageNicknames)]
		[EnabledByDefault(true)]
		public sealed class RemoveAllNickNames : MultiUserActionModule
		{
			[Command(RunMode = RunMode.Async)]
			public async Task<RuntimeResult> Command(
				[Optional, OverrideTypeReader(typeof(BypassUserLimitTypeReader))] bool bypass)
			{
				ProgressLogger = new MultiUserActionProgressLogger(Context.Channel, i => Responses.Nicknames.MultiUserAction(i.AmountLeft).Reason, GenerateRequestOptions());
				var amountChanged = await ProcessAsync(bypass,
					u => u.Nickname != null,
					u => u.ModifyAsync(x => x.Nickname = u.Username, GenerateRequestOptions())).CAF();
				return Responses.Nicknames.MultiUserActionSuccess(amountChanged);
			}
		}
	}
}
