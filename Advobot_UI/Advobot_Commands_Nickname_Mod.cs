using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Advobot
{
	//Commands that affect nicknames
	[Name("Nickname_Moderation")]
	class Advobot_Commands_Nickname_Mod : ModuleBase
	{
		[Command("nickname")]
		[Alias("nn")]
		[Usage("[User] [New Nickname|Remove]")]
		[Summary("Gives the user a nickname.")]
		[PermissionRequirement(1U << (int)GuildPermission.ManageNicknames)]
		[DefaultEnabled(true)]
		public async Task Nickname([Remainder] string input)
		{
			var returnedArgs = Actions.GetArgs(Context, input, new ArgNumbers(2, 2));
			if (returnedArgs.Reason != ArgFailureReason.Not_Failure)
			{
				await Actions.HandleArgsGettingErrors(Context, returnedArgs);
				return;
			}
			var userStr = returnedArgs.Arguments[0];
			var nickStr = returnedArgs.Arguments[1];

			if (String.IsNullOrWhiteSpace(nickStr))
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("No input for nickname was given."));
				return;
			}
			else if (Actions.CaseInsEquals(nickStr, "remove"))
			{
				nickStr = null;
			}
			else if (nickStr.Length > Constants.MAX_NICKNAME_LENGTH)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(String.Format("Nicknames cannot be longer than `{0}` characters.", Constants.MAX_NICKNAME_LENGTH)));
				return;
			}
			else if (nickStr.Length < Constants.MIN_NICKNAME_LENGTH)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(String.Format("Nicknames cannot be less than `{0}` characters.", Constants.MIN_NICKNAME_LENGTH)));
				return;
			}

			//Get the user
			var returnedUser = Actions.GetGuildUser(Context, new[] { UserCheck.Can_Be_Edited }, true, userStr);
			if (returnedUser.Reason != FailureReason.Not_Failure)
			{
				await Actions.HandleObjectGettingErrors(Context, returnedUser);
				return;
			}
			var user = returnedUser.Object;

			await Actions.ChangeNickname(user, nickStr);
			if (nickStr != null)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully gave `{0}` the nickname `{1}`.", user.FormatUser(), nickStr));
			}
			else
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully removed the nickname on `{0}`.", user.FormatUser()));
			}
		}

		[Command("replacewordsinnames")]
		[Alias("rwin")]
		[Usage("[\"String to Find\"] [\"String to Replace\"] <" + Constants.BYPASS_STRING + ">")]
		[Summary("Gives any users who have a username/nickname with the given string a new nickname that replaces it. Max is 100 users per use unless the bypass string is said.")]
		[PermissionRequirement(1U << (int)GuildPermission.ManageNicknames)]
		[DefaultEnabled(true)]
		public async Task NicknameAllWithName([Remainder] string input)
		{
			//Split and get variables
			var returnedArgs = Actions.GetArgs(Context, input, new ArgNumbers(2, 3));
			if (returnedArgs.Reason != ArgFailureReason.Not_Failure)
			{
				await Actions.HandleArgsGettingErrors(Context, returnedArgs);
				return;
			}
			var findStr = returnedArgs.Arguments[0];
			var replaceStr = returnedArgs.Arguments[1];

			//Make sure both exist
			if (String.IsNullOrWhiteSpace(findStr))
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("The string to find cannot be empty or null."));
				return;
			}
			else if (String.IsNullOrWhiteSpace(replaceStr))
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("The string to replace with cannot be empty or null."));
				return;
			}

			//Length
			if (findStr.Length > Constants.MAX_NICKNAME_LENGTH)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(String.Format("The string to find can only be up to `{0}` characters long.", Constants.MAX_NICKNAME_LENGTH)));
				return;
			}
			else if (replaceStr.Length < Constants.MIN_NICKNAME_LENGTH)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(String.Format("The string to replace with must be at least `{0}` characters long.", Constants.MIN_NICKNAME_LENGTH)));
				return;
			}
			else if (replaceStr.Length > Constants.MAX_NICKNAME_LENGTH)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(String.Format("The string to replace with can only be up to `{0}` characters long.", Constants.MAX_NICKNAME_LENGTH)));
				return;
			}

			//Get the users 
			var len = Actions.GetMaxNumOfUsersToGather(Context, returnedArgs.Arguments);
			var users = (await Actions.GetUsersTheBotAndUserCanEdit(Context, x => Actions.CaseInsIndexOf(x.Username, findStr) || Actions.CaseInsIndexOf(x?.Nickname, findStr))).GetUpToAndIncludingMinNum(len);

			//User count checking and stuff
			var userCount = users.Count;
			if (userCount == 0)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Unable to find any users with the given string to replace."));
				return;
			}

			//Have the bot stay in the typing state and have a message that can be updated 
			var msg = await Actions.SendChannelMessage(Context, String.Format("Attempting to rename `{0}` people.", userCount)) as IUserMessage;
			var typing = Context.Channel.EnterTypingState();

			//Actually rename them all
			var count = 0;
			await users.ForEachAsync(async x =>
			{
				++count;
				if (count % 10 == 0)
				{
					await msg.ModifyAsync(y => y.Content = String.Format("Attempting to rename `{0}` people.", userCount - count));
				}

				if (x.Nickname != null)
				{
					await Actions.ChangeNickname(x, Actions.CaseInsReplace(x.Nickname, findStr, replaceStr));
				}
				else
				{
					await Actions.ChangeNickname(x, Actions.CaseInsReplace(x.Username, findStr, replaceStr));
				}
			});

			//Get rid of stuff and send a success message
			typing.Dispose();
			await Actions.DeleteMessage(msg);
			await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully renamed `{0}` people.", count));
		}

		[Command("replacenonascii")]
		[Alias("rna")]
		[Usage("[\"String to Replace With\"] <ANSI:True|False> <" + Constants.BYPASS_STRING + ">")]
		[Summary("Any user who has a name and nickname with non regular ascii characters will have their username changed to the given string. No input lists all the users. "
			+ "Max is 100 users per use unless the bypass string is said.")]
		[PermissionRequirement(1U << (int)GuildPermission.ManageNicknames)]
		[DefaultEnabled(true)]
		public async Task ReplaceNonAscii([Optional, Remainder] string input)
		{
			//Splitting input
			var returnedArgs = Actions.GetArgs(Context, input, new ArgNumbers(0, 3), new[] { "ansi" });
			if (returnedArgs.Reason != ArgFailureReason.Not_Failure)
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
				Actions.RenicknameALotOfPeople(Context, validUsers, replaceStr).Forget();
			}
		}

		[Command("removeallnicknames")]
		[Alias("rann")]
		[Usage("<" + Constants.BYPASS_STRING + ">")]
		[Summary("Remove all nicknames of users on the guild. Max is 100 users per use unless the bypass string is said.")]
		[PermissionRequirement(1U << (int)GuildPermission.ManageNicknames)]
		[DefaultEnabled(true)]
		public async Task RemoveAllNickNames([Optional, Remainder] string input)
		{
			var len = Actions.GetMaxNumOfUsersToGather(Context, new[] { input });
			var users = (await Actions.GetUsersTheBotAndUserCanEdit(Context, x => x.Nickname != null)).GetUpToAndIncludingMinNum(len);
			Actions.RenicknameALotOfPeople(Context, users, null).Forget();
		}
	}
}
