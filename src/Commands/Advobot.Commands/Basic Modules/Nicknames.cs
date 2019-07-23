using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Advobot.Classes.Attributes;
using Advobot.Classes.Attributes.ParameterPreconditions.DiscordObjectValidation.Users;
using Advobot.Classes.Attributes.ParameterPreconditions.NumberValidation;
using Advobot.Classes.Attributes.ParameterPreconditions.StringLengthValidation;
using Advobot.Classes.Attributes.Preconditions.Permissions;
using Advobot.Classes.Modules;
using Advobot.Classes.TypeReaders;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Advobot.CommandMarking
{
	public sealed class Nicknames : ModuleBase
	{
		[Group(nameof(ModifyNickName)), ModuleInitialismAlias(typeof(ModifyNickName))]
		[Summary("Gives the user a nickname. " +
			"Inputting no nickname resets their nickname.")]
		[UserPermissionRequirement(GuildPermission.ManageNicknames)]
		[EnabledByDefault(true)]
		public sealed class ModifyNickName : AdvobotModuleBase
		{
			[Command]
			public async Task<RuntimeResult> Command([ValidateUser] SocketGuildUser user, [Optional, ValidateNickname] string nickname)
			{
				await user.ModifyAsync(x => x.Nickname = nickname ?? user.Username, GenerateRequestOptions()).CAF();
				return Responses.Nicknames.ModifiedNickname(user.Format(), nickname ?? "Nothing");
			}
		}

		[Group(nameof(ReplaceWordsInNames)), ModuleInitialismAlias(typeof(ReplaceWordsInNames))]
		[Summary("Gives users a new nickname if their nickname or username contains the search phrase. " +
			"Max is 100 users per use unless the bypass string is said.")]
		[UserPermissionRequirement(GuildPermission.ManageNicknames)]
		[EnabledByDefault(true)]
		public sealed class ReplaceWordsInNames : MultiUserActionModule
		{
			[Command(RunMode = RunMode.Async)]
			public async Task<RuntimeResult> Command([ValidateNickname] string search,
				[ValidateNickname] string replace,
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
		[Summary("Replaces nickname/usernames that contain any characters above the supplied character value in UTF-16. " +
			"Max is 100 users per use unless the bypass string is said.")]
		[UserPermissionRequirement(GuildPermission.ManageNicknames)]
		[EnabledByDefault(true)]
		public sealed class ReplaceByUtf16 : MultiUserActionModule
		{
			[Command(RunMode = RunMode.Async)]
			public async Task<RuntimeResult> Command([ValidatePositiveNumber] int upperLimit,
				[ValidateNickname] string replace,
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
		[Summary("Remove all nicknames of users on the guild. " +
			"Max is 100 users per use unless the bypass string is said.")]
		[UserPermissionRequirement(GuildPermission.ManageNicknames)]
		[EnabledByDefault(true)]
		public sealed class RemoveAllNickNames : MultiUserActionModule
		{
			[Command(RunMode = RunMode.Async)]
			public async Task<RuntimeResult> Command([Optional, OverrideTypeReader(typeof(BypassUserLimitTypeReader))] bool bypass)
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
