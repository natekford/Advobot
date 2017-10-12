using Advobot.Actions;
using Advobot.Actions.Formatting;
using Advobot.Classes;
using Advobot.Classes.Attributes;
using Advobot.Classes.TypeReaders;
using Advobot.Enums;
using Discord;
using Discord.Commands;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Advobot.Commands.NicknameModeration
{
	[Group(nameof(ModifyNickName)), TopLevelShortAlias(typeof(ModifyNickName))]
	[Summary("Gives the user a nickname. Inputting no nickname resets their nickname.")]
	[PermissionRequirement(new[] { GuildPermission.ManageNicknames }, null)]
	[DefaultEnabled(true)]
	public sealed class ModifyNickName : AdvobotModuleBase
	{
		[Command]
		public async Task Command([VerifyObject(false, ObjectVerification.CanBeEdited)] IGuildUser user, [Optional, VerifyStringLength(Target.Nickname)] string nickname)
		{
			await UserActions.ChangeNicknameAsync(user, nickname, new ModerationReason(Context.User, null));
			var response = nickname == null
				? $"Successfully removed the nickname from `{user.FormatUser()}`."
				: $"Successfully gave `{user.FormatUser()}` the nickname `{nickname}`.";
			await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, response);
		}
	}

	[Group(nameof(ReplaceWordsInNames)), TopLevelShortAlias(typeof(ReplaceWordsInNames))]
	[Summary("Gives users a new nickname if their nickname or username contains the search phrase. Max is 100 users per use unless the bypass string is said.")]
	[PermissionRequirement(new[] { GuildPermission.ManageNicknames }, null)]
	[DefaultEnabled(true)]
	public sealed class ReplaceWordsInNames : AdvobotModuleBase
	{
		[Command(RunMode = RunMode.Async)]
		public async Task Command([VerifyStringLength(Target.Nickname)] string search,
			[VerifyStringLength(Target.Nickname)] string replace,
			[Optional, OverrideTypeReader(typeof(BypassUserLimitTypeReader))] bool bypass)
		{
			var users = (await UserActions.GetUsersTheBotAndUserCanEditAsync(Context)).Where(x => false
				|| (x.Nickname != null && x.Nickname.CaseInsContains(search))
				|| (x.Nickname == null && x.Username.CaseInsContains(search)));
			await new MultiUserAction(Context, users, bypass).NicknameManyUsersAsync(replace, new ModerationReason(Context.User, null));
		}
	}

	[Group(nameof(ReplaceByUTF16)), TopLevelShortAlias(typeof(ReplaceByUTF16))]
	[Summary("Replaces nickname/usernames that contain any characters above the supplied character value in UTF-16. Max is 100 users per use unless the bypass string is said.")]
	[PermissionRequirement(new[] { GuildPermission.ManageNicknames }, null)]
	[DefaultEnabled(true)]
	public sealed class ReplaceByUTF16 : AdvobotModuleBase
	{
		[Command(RunMode = RunMode.Async)]
		public async Task Command(uint upperLimit,
			[VerifyStringLength(Target.Nickname)] string replace,
			[Optional, OverrideTypeReader(typeof(BypassUserLimitTypeReader))] bool bypass)
		{
			var users = (await UserActions.GetUsersTheBotAndUserCanEditAsync(Context)).Where(x => false
				|| (x.Nickname != null && !x.Nickname.AllCharactersAreWithinUpperLimit((int)upperLimit))
				|| (x.Nickname == null && !x.Username.AllCharactersAreWithinUpperLimit((int)upperLimit)));
			await new MultiUserAction(Context, users, bypass).NicknameManyUsersAsync(replace, new ModerationReason(Context.User, null));
		}
	}

	[Group(nameof(RemoveAllNickNames)), TopLevelShortAlias(typeof(RemoveAllNickNames))]
	[Summary("Remove all nicknames of users on the guild. Max is 100 users per use unless the bypass string is said.")]
	[PermissionRequirement(new[] { GuildPermission.ManageNicknames }, null)]
	[DefaultEnabled(true)]
	public sealed class RemoveAllNickNames : AdvobotModuleBase
	{
		[Command(RunMode = RunMode.Async)]
		public async Task Command([Optional, OverrideTypeReader(typeof(BypassUserLimitTypeReader))] bool bypass)
		{
			var users = (await UserActions.GetUsersTheBotAndUserCanEditAsync(Context)).Where(x => x.Nickname != null);
			await new MultiUserAction(Context, users, bypass).NicknameManyUsersAsync(null, new ModerationReason(Context.User, null));
		}
	}
}
