using Advobot.Actions;
using Advobot.Attributes;
using Advobot.Enums;
using Advobot.NonSavedClasses;
using Advobot.TypeReaders;
using Discord;
using Discord.Commands;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Advobot
{
	namespace NicknameModeration
	{
		[Group("changenickname"), Alias("cnn")]
		[Usage("[User] <Nickname>")]
		[Summary("Gives the user a nickname. Inputting no nickname resets their nickname.")]
		[PermissionRequirement(new[] { GuildPermission.ManageNicknames }, null)]
		[DefaultEnabled(true)]
		public sealed class ChangeNickname : MyModuleBase
		{
			[Command]
			public async Task Command([VerifyObject(false, ObjectVerification.CanBeEdited)] IGuildUser user, [Optional, VerifyStringLength(Target.Nickname)] string nickname)
			{
				await CommandRunner(user, nickname);
			}

			private async Task CommandRunner(IGuildUser user, string nickname)
			{
				await UserActions.ChangeNickname(user, nickname, FormattingActions.FormatUserReason(Context.User));
				var response = nickname == null
					? String.Format("Successfully removed the nickname from `{0}`.", user.FormatUser())
					: String.Format("Successfully gave `{0}` the nickname `{1}`.", user.FormatUser(), nickname);
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, response);
			}
		}

		[Group("replacewordsinnames"), Alias("rwin")]
		[Usage("[\"Search\"] [\"Replace\"] <" + Constants.BYPASS_STRING + ">")]
		[Summary("Gives users a new nickname if their nickname or username contains the search phrase. Max is 100 users per use unless the bypass string is said.")]
		[PermissionRequirement(new[] { GuildPermission.ManageNicknames }, null)]
		[DefaultEnabled(true)]
		public sealed class ReplaceWordsInNames : MyModuleBase
		{
			[Command(RunMode = RunMode.Async)]
			public async Task Command([VerifyStringLength(Target.Nickname)] string search,
									  [VerifyStringLength(Target.Nickname)] string replace,
									  [Optional, OverrideTypeReader(typeof(BypassUserLimitTypeReader))] bool bypass)
			{
				await CommandRunner(search, replace, bypass);
			}

			private async Task CommandRunner(string search, string replace, bool bypass)
			{
				var userAmt = GetActions.GetMaxAmountOfUsersToGather(Context.BotSettings, bypass);
				var users = (await UserActions.GetUsersTheBotAndUserCanEdit(Context)).Where(x => false
					|| (x.Nickname != null && x.Nickname.CaseInsContains(search)) //If nickname is there, check based off of nickname
					|| (x.Nickname == null && x.Username.CaseInsContains(search)) //If nickname isn't there, check based off of username
					).ToList().GetUpToAndIncludingMinNum(userAmt);

				await UserActions.NicknameManyUsers(Context, users, replace, FormattingActions.FormatUserReason(Context.User));
			}
		}

		[Group("replacebyutf16"), Alias("rbu")]
		[Usage("[Number] [\"Replace\"] <" + Constants.BYPASS_STRING + ">")]
		[Summary("Replaces nickname/usernames that contain any characters above the supplied character value in UTF-16. Max is 100 users per use unless the bypass string is said.")]
		[PermissionRequirement(new[] { GuildPermission.ManageNicknames }, null)]
		[DefaultEnabled(true)]
		public sealed class ReplaceByUTF16 : MyModuleBase
		{
			[Command(RunMode = RunMode.Async)]
			public async Task Command(uint upperLimit, [VerifyStringLength(Target.Nickname)] string replace, [Optional, OverrideTypeReader(typeof(BypassUserLimitTypeReader))] bool bypass)
			{
				await CommandRunner(upperLimit, replace, bypass);
			}

			private async Task CommandRunner(uint upperLimit, string replace, bool bypass)
			{
				var userAmt = GetActions.GetMaxAmountOfUsersToGather(Context.BotSettings, bypass);
				var users = (await UserActions.GetUsersTheBotAndUserCanEdit(Context)).Where(x => false
					|| (x.Nickname != null && !x.Nickname.AllCharactersAreWithinUpperLimit((int)upperLimit)) //If nickname is there, check based off of nickname
					|| (x.Nickname == null && !x.Username.AllCharactersAreWithinUpperLimit((int)upperLimit)) //If nickname isn't there, check based off of username
					).ToList().GetUpToAndIncludingMinNum(userAmt);

				await UserActions.NicknameManyUsers(Context, users, replace, FormattingActions.FormatUserReason(Context.User));
			}
		}

		[Group("removeallnicknames"), Alias("rann")]
		[Usage("<" + Constants.BYPASS_STRING + ">")]
		[Summary("Remove all nicknames of users on the guild. Max is 100 users per use unless the bypass string is said.")]
		[PermissionRequirement(new[] { GuildPermission.ManageNicknames }, null)]
		[DefaultEnabled(true)]
		public sealed class RemoveAllNicknames : MyModuleBase
		{
			[Command(RunMode = RunMode.Async)]
			public async Task Command([Optional, OverrideTypeReader(typeof(BypassUserLimitTypeReader))] bool bypass)
			{
				await CommandRunner(bypass);
			}

			private async Task CommandRunner(bool bypass)
			{
				var userAmt = GetActions.GetMaxAmountOfUsersToGather(Context.BotSettings, bypass);
				var users = (await UserActions.GetUsersTheBotAndUserCanEdit(Context)).Where(x => x.Nickname != null).ToList().GetUpToAndIncludingMinNum(userAmt);

				await UserActions.NicknameManyUsers(Context, users, null, FormattingActions.FormatUserReason(Context.User));
			}
		}
	}
}
