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
		public class ChangeNickname : MyModuleBase
		{
			[Command]
			public async Task Command([VerifyObject(ObjectVerification.CanBeEdited)] IGuildUser user, [Optional, VerifyStringLength(Target.Nickname)] string nickname)
			{
				await CommandRunner(user, nickname);
			}

			private async Task CommandRunner(IGuildUser user, string nickname)
			{
				await Actions.ChangeNickname(user, nickname);
				var response = nickname == null
					? String.Format("Successfully removed the nickname from `{0}`.", user.FormatUser())
					: String.Format("Successfully gave `{0}` the nickname `{1}`.", user.FormatUser(), nickname);
				await Actions.MakeAndDeleteSecondaryMessage(Context, response);
			}
		}

		[Group("replacewordsinnames"), Alias("rwin")]
		[Usage("[\"Find\"] [\"Replace\"] <" + Constants.BYPASS_STRING + ">")]
		[Summary("Gives users a new nickname based off of words in their nickname or username. Max is 100 users per use unless the bypass string is said.")]
		[PermissionRequirement(new[] { GuildPermission.ManageNicknames }, null)]
		[DefaultEnabled(true)]
		public class ReplaceWordsInNames : MyModuleBase
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
				var users = Actions.GetUsersTheBotAndUserCanEdit(Context).Where(x => false
					|| (x.Nickname != null && x.Nickname.CaseInsContains(search)) //If nickname is there, check based off of nickname
					|| (x.Nickname == null && x.Username.CaseInsContains(search)) //If nickname isn't there, check based off of username
					).ToList().GetUpToAndIncludingMinNum(Actions.GetMaxAmountOfUsersToGather(bypass));

				var msg = await Actions.SendChannelMessage(Context, String.Format("Attempting to rename `{0}` people.", users.Count)) as IUserMessage;
				for (int i = 0; i < users.Count; i++)
				{
					if (i % 10 == 0)
					{
						await msg.ModifyAsync(y => y.Content = String.Format("Attempting to rename `{0}` people.", users.Count - i));
					}

					await Actions.ChangeNickname(users[i], replace);
				}

				await Actions.DeleteMessage(msg);
				await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully renamed `{0}` people.", users.Count));
			}
		}

		[Group("replacenonascii"), Alias("rna")]
		[Usage("[Number] [\"Replace\"] <" + Constants.BYPASS_STRING + ">")]
		[Summary("Replaces nickname/usernames that contain any characters above the supplied number. Max is 100 users per use unless the bypass string is said.")]
		[PermissionRequirement(new[] { GuildPermission.ManageNicknames }, null)]
		[DefaultEnabled(true)]
		public class ReplaceNonAscii : MyModuleBase
		{
			[Command(RunMode = RunMode.Async)]
			public async Task Command(uint upperLimit, [VerifyStringLength(Target.Nickname)] string replace, [Optional, OverrideTypeReader(typeof(BypassUserLimitTypeReader))] bool bypass)
			{
				await CommandRunner(upperLimit, replace, bypass);
			}

			private async Task CommandRunner(uint upperLimit, string replace, bool bypass)
			{
				var users = Actions.GetUsersTheBotAndUserCanEdit(Context).Where(x => false
					|| (x.Nickname != null && !x.Nickname.AllCharactersAreWithinUpperLimit((int)upperLimit)) //If nickname is there, check based off of nickname
					|| (x.Nickname == null && !x.Username.AllCharactersAreWithinUpperLimit((int)upperLimit)) //If nickname isn't there, check based off of username
					).ToList().GetUpToAndIncludingMinNum(Actions.GetMaxAmountOfUsersToGather(bypass));
			}
		}
	}
	/*
	//Commands that affect nicknames
	[Name("NicknameModeration")]
	public class Advobot_Commands_Nickname_Mod : ModuleBase
	{


		public async Task ReplaceNonAscii([Optional, Remainder] string input)
		{
			//Splitting input
			var returnedArgs = Actions.GetArgs(Context, input, new ArgNumbers(0, 3), new[] { "ansi" });
			if (returnedArgs.Reason != FailureReason.NotFailure)
			{
				await Actions.HandleArgsGettingErrors(Context, returnedArgs);
				return;
			}
			var replaceStr = returnedArgs.Arguments[0];
			var ansiStr = returnedArgs.GetSpecifiedArg("ansi");

			//Getting the upper limit for the Unicode characters
			var upperLimit = 127;
			if (!String.IsNullOrWhiteSpace(ansiStr))
			{
				if (!bool.TryParse(ansiStr, out bool ANSI))
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid input for ANSI."));
					return;
				}
				else if (ANSI)
				{
					upperLimit = 255;
				}
			}

			//Find users who have invalid usernames and no valid nicknames
			var users = await Actions.GetUsersTheBotAndUserCanEdit(Context, x => !Actions.GetIfValidUnicode(x.Username, upperLimit) && !Actions.GetIfValidUnicode(x?.Nickname, upperLimit));
			if (String.IsNullOrWhiteSpace(replaceStr))
			{
				if (!users.Any())
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("No users have an irregular Unicode name."));
					return;
				}

				//Format the description and send it
				var count = 1;
				var length = users.Count.ToString().Length;
				var desc = String.Join("\n", users.Select(x => String.Format("`{0}.` `{1}`", count++.ToString().PadLeft(length, '0'), x.FormatUser())));
				var embed = Actions.MakeNewEmbed("Non ASCII Names", desc);
				await Actions.SendEmbedMessage(Context.Channel, embed);
				return;
			}
			else
			{
				var len = Actions.GetMaxNumOfUsersToGather(Context, returnedArgs.Arguments);
				var validUsers = users.GetUpToAndIncludingMinNum(len);
				Actions.DontWaitForResultOfBigUnimportantFunction(Context.Channel, async () =>
				{
					await Actions.RenicknameALotOfPeople(Context, validUsers, replaceStr);
				});
			}
		}

		[Command("removeallnicknames")]
		[Alias("rann")]
		[Usage("<" + Constants.BYPASS_STRING + ">")]
		[Summary("Remove all nicknames of users on the guild. Max is 100 users per use unless the bypass string is said.")]
		[PermissionRequirement(new[] { GuildPermission.ManageNicknames }, null)]
		[DefaultEnabled(true)]
		public async Task RemoveAllNickNames([Optional, Remainder] string input)
		{
			var len = Actions.GetMaxNumOfUsersToGather(Context, new[] { input });
			var users = (await Actions.GetUsersTheBotAndUserCanEdit(Context, x => x.Nickname != null)).GetUpToAndIncludingMinNum(len);
			Actions.DontWaitForResultOfBigUnimportantFunction(Context.Channel, async () =>
			{
				await Actions.RenicknameALotOfPeople(Context, users, null);
			});
		}
	}
	*/
}
