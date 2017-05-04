using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Advobot
{
	//User Moderation commands are commands that affect the users of a guild
	[Name("User_Moderation")]
	public class Advobot_Commands_User_Mod : ModuleBase
	{
		[Command("textmute")]
		[Alias("tm")]
		[Usage("[@User] <Time:Number>")]
		[Summary("If the user is not text muted, this will mute them. Users can be unmuted via the `roletake` command. Time is in minutes, and if no time is given then the mute will not expire.")]
		[PermissionRequirement((1U << (int)GuildPermission.ManageRoles) | (1U << (int)GuildPermission.ManageMessages))]
		[DefaultEnabled(true)]
		public async Task FullMute([Remainder] string input)
		{
			//Check if role already exists, if not, create it
			//TODO: Have this create the role
			var returnedRole = Actions.GetRole(Context, new[] { CheckType.Role_Editability }, Constants.MUTE_ROLE_NAME);
			if (returnedRole.Reason != FailureReason.Not_Failure)
			{
				await Actions.HandleObjectGettingErrors(Context, returnedRole);
				return;
			}
			var role = returnedRole.Object;

			//Split the input
			var inputArray = Actions.SplitByCharExceptInQuotes(input, ' ');
			var timeStr = Actions.GetVariable(inputArray, "time");

			//Get the time
			var time = 0;
			if (!String.IsNullOrWhiteSpace(timeStr))
			{
				if (!int.TryParse(timeStr, out time))
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid time."));
					return;
				}
			}

			//Get out the editable and uneditable users
			var evaluatedUsers = Actions.GetValidEditUsers(Context);
			if (!evaluatedUsers.HasValue)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.USER_ERROR));
				return;
			}
			var success = evaluatedUsers.Value.Success;
			var failure = evaluatedUsers.Value.Failure;

			//Give the mute roles
			await success.ForEachAsync(async x =>
			{
				await Actions.GiveRole(x, role);
				if (time != 0)
				{
					Variables.PunishedUsers.Add(new RemovablePunishment(Context.Guild, x.Id, role, DateTime.UtcNow.AddMinutes(time)));
				}
			});

			var response = Actions.FormatResponseMessagesForCmdsOnLotsOfObjects(success, failure, "user", "muted", "mute");
			if (time != 0)
			{
				response += String.Format("These mutes will last for `{0}` minutes.");
			}
			await Actions.MakeAndDeleteSecondaryMessage(Context, response);
		}

		[Command("voicemute")]
		[Alias("vm")]
		[Usage("[@User] <Time>")]
		[Summary("If the user is not voice muted, this will mute them. If they are voice muted, this will unmute them. Time is in minutes, and if no time is given then the mute will not expire.")]
		[PermissionRequirement(1U << (int)GuildPermission.MuteMembers)]
		[DefaultEnabled(true)]
		public async Task Mute([Remainder] string input)
		{
			//Split the input
			var inputArray = input.Split(new[] { ' ' }, 2);
			//Test if valid user mention
			var user = await Actions.GetUser(Context.Guild, inputArray[0]);
			if (user == null)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.USER_ERROR));
				return;
			}

			//See if it should mute or unmute
			if (!user.IsMuted)
			{
				//Check if time is given
				var timeString = "";
				if (inputArray.Length == 2)
				{
					if (int.TryParse(inputArray[1], out int time))
					{
						Variables.PunishedUsers.Add(new RemovablePunishment(Context.Guild, user.Id, PunishmentType.Mute, DateTime.UtcNow.AddMinutes(time)));
						timeString = String.Format(" for {0} minutes", time);
					}
					else
					{
						await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid time supplied."));
						return;
					}
				}
				
				//Mute the user
				await user.ModifyAsync(x => x.Mute = true);
				await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully muted `{0}`{1}.", Actions.FormatUser(user, user?.Id), timeString));
			}
			else
			{
				//Unmute the user
				await user.ModifyAsync(x => x.Mute = false);
				await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully unmuted `{0}`.", Actions.FormatUser(user, user?.Id)));
			}
		}

		[Command("deafen")]
		[Alias("dfn", "d")]
		[Usage("[@User] <Time:Time in Minutes>")]
		[Summary("If the user is not voice muted, this will mute them. If they are voice muted, this will unmute them. Time is in minutes, and if no time is given then the mute will not expire.")]
		[PermissionRequirement(1U << (int)GuildPermission.DeafenMembers)]
		[DefaultEnabled(true)]
		public async Task Deafen([Remainder] string input)
		{
			//Split the input
			var inputArray = input.Split(' ');
			var userStr = inputArray[0];
			var timeStr = Actions.GetVariable(inputArray, "time");

			//Test if valid user mention
			var returnedUser = Actions.GetGuildUser(Context, new[] { CheckType.None }, userStr);
			if (returnedUser.Reason != FailureReason.Not_Failure)
			{
				await Actions.HandleObjectGettingErrors(Context, returnedUser);
				return;
			}
			var user = returnedUser.Object;

			//See if it should deafen or undeafen
			if (!user.IsDeafened)
			{
				//Check if time was supplied
				if (inputArray.Length == 2)
				{
					if (int.TryParse(timeStr, out int time))
					{
						Variables.PunishedUsers.Add(new RemovablePunishment(Context.Guild, user.Id, PunishmentType.Deafen, DateTime.UtcNow.AddMinutes(time)));
						timeStr = String.Format(" for {0} minutes", time);
					}
					else
					{
						await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid time supplied."));
						return;
					}
				}

				//Deafen them
				await user.ModifyAsync(x => x.Deaf = true);
				await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully deafened `{0}`{1}.", Actions.FormatUser(user, user?.Id), timeStr));
			}
			else
			{
				//Undeafen them
				await user.ModifyAsync(x => x.Deaf = false);
				await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully undeafened `{0}`.", Actions.FormatUser(user, user?.Id)));
			}
		}

		[Command("moveuser")]
		[Alias("mu")]
		[Usage("[@User] [Channel Name]")]
		[Summary("Moves the user to the given voice channel.")]
		[PermissionRequirement(1U << (int)GuildPermission.MoveMembers)]
		[DefaultEnabled(true)]
		public async Task MoveUser([Remainder] string input)
		{
			//Input and splitting
			var inputArray = input.Split(new[] { ' ' }, 2);
			if (inputArray.Length != 2)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR));
				return;
			}
			var userStr = inputArray[0];
			var chanStr = inputArray[1];

			//Check if valid user and that they're in a voice channel
			var returnedUser = Actions.GetGuildUser(Context, new[] { CheckType.User_Channel_Move }, userStr);
			if (returnedUser.Reason != FailureReason.Not_Failure)
			{
				await Actions.HandleObjectGettingErrors(Context, returnedUser);
				return;
			}
			var user = returnedUser.Object;
			if (user.VoiceChannel == null)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("User is not in a voice channel."));
				return;
			}

			//Check if valid channel that the user can edit
			var returnedChannel = Actions.GetChannel(Context, new[] { CheckType.Channel_Move_Users, CheckType.Channel_Voice_Type }, chanStr);
			if (returnedChannel.Reason != FailureReason.Not_Failure)
			{
				await Actions.HandleObjectGettingErrors(Context, returnedChannel);
				return;
			}
			var channel = returnedChannel.Object as IVoiceChannel;

			//See if trying to put user in the exact same channel
			var userChan = user.VoiceChannel;
			if (userChan == channel)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("User is already in that channel"));
				return;
			}

			await user.ModifyAsync(x => x.Channel = Optional.Create(channel));
			await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully moved `{0}#{1}` to `{2}`.", user.Username, user.Discriminator, channel.Name));
		}

		[Command("moveusers")]
		[Alias("mus")]
		[Usage("[\"Channel\"] [\"Channel\"]")]
		[Summary("Moves all users from one channel to another.")]
		[PermissionRequirement(1U << (int)GuildPermission.MoveMembers)]
		[DefaultEnabled(true)]
		public async Task MoveUsers([Remainder] string input)
		{
			var inputArray = Actions.SplitByCharExceptInQuotes(input, ' ');
			if (inputArray.Length != 2)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR));
				return;
			}
			var inputStr = inputArray[0];
			var outputStr = inputArray[1];

			//Check if valid channel that the user can edit
			var returnedInputChannel = Actions.GetChannel(Context, new[] { CheckType.Channel_Move_Channels, CheckType.Channel_Voice_Type }, inputStr);
			if (returnedInputChannel.Reason != FailureReason.Not_Failure)
			{
				await Actions.HandleObjectGettingErrors(Context, returnedInputChannel);
				return;
			}
			var inputChannel = returnedInputChannel.Object as IVoiceChannel;

			//Check if valid channel that the user can edit
			var returnedOutputChannel = Actions.GetChannel(Context, new[] { CheckType.Channel_Move_Channels, CheckType.Channel_Voice_Type }, outputStr);
			if (returnedOutputChannel.Reason != FailureReason.Not_Failure)
			{
				await Actions.HandleObjectGettingErrors(Context, returnedOutputChannel);
				return;
			}
			var outputChannel = returnedOutputChannel.Object as IVoiceChannel;

			//Move the users
			var users = (await inputChannel.GetUsersAsync().ToList()).SelectMany(x => x).ToList();
			await users.ForEachAsync(async x =>
			{
				await x.ModifyAsync(y => y.Channel = Optional.Create(outputChannel));
			});

			//Send a success message
			var text = String.Format("Successfully moved `{0}` from `{1}` to `{2}`.", users.Count, Actions.FormatChannel(inputChannel), Actions.FormatChannel(outputChannel));
			await Actions.MakeAndDeleteSecondaryMessage(Context, text);
		}

		[Command("nickname")]
		[Alias("nn")]
		[Usage("[@User] [\"Nickname:New Nickname|Remove\"]")]
		[Summary("Gives the user a nickname.")]
		[PermissionRequirement(1U << (int)GuildPermission.ManageNicknames)]
		[DefaultEnabled(true)]
		public async Task Nickname([Remainder] string input)
		{
			var inputArray = Actions.SplitByCharExceptInQuotes(input, ' ');
			var nickStr = Actions.GetVariable(inputArray, "nickname");
			var mentionedUsers = Context.Message.MentionedUserIds;

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

			//Get out the editable and uneditable users
			var evaluatedUsers = Actions.GetValidEditUsers(Context);
			if (!evaluatedUsers.HasValue)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.USER_ERROR));
				return;
			}
			var success = evaluatedUsers.Value.Success;
			var failure = evaluatedUsers.Value.Failure;

			//Edit the nicknames of the users who can be edited then send the response message
			await success.ForEachAsync(async x => await Actions.ChangeNickname(x, nickStr));
			if (nickStr != null)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.FormatResponseMessagesForCmdsOnLotsOfObjects(success, failure, "user", "nicknamed", "nickname"));
			}
			else
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.FormatResponseMessagesForCmdsOnLotsOfObjects(success, failure, "user", "removed the nickname from", "remove the nickname from"));
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
			var inputArray = Actions.SplitByCharExceptInQuotes(input, ' ');
			if (inputArray.Length < 2)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR));
				return;
			}
			var find = inputArray[0];
			var with = inputArray[1];
			if (String.IsNullOrWhiteSpace(find) || String.IsNullOrWhiteSpace(with))
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("The strings to find or replace cannot be empty or null."));
			}

			//Test lengths
			if (find.Length > Constants.MAX_NICKNAME_LENGTH)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(String.Format("The string to replace can only be up to `{0}` characters long.", Constants.MAX_NICKNAME_LENGTH)));
				return;
			}
			if (with.Length < Constants.MIN_NICKNAME_LENGTH)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(String.Format("The string to replace with must be at least `{0}` characters long.", Constants.MIN_NICKNAME_LENGTH)));
				return;
			}
			else if (with.Length > Constants.MAX_NICKNAME_LENGTH)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(String.Format("The string to replace with can only be up to `{0}` characters long.", Constants.MAX_NICKNAME_LENGTH)));
				return;
			}

			//Get the users 
			var len = Actions.GetMaxNumOfUsersToGather(Context, inputArray);
			var users = (await Actions.GetUsersTheBotAndUserCanEdit(Context, x => Actions.CaseInsIndexOf(x.Username, find) || Actions.CaseInsIndexOf(x?.Nickname, find))).GetUpToXElement(len);

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
					await Actions.ChangeNickname(x, Actions.CaseInsReplace(x.Nickname, find, with));
				}
				else
				{
					await Actions.ChangeNickname(x, Actions.CaseInsReplace(x.Username, find, with));
				}
			});

			//Get rid of stuff and send a success message
			typing.Dispose();
			await Actions.DeleteMessage(msg);
			await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully renamed `{0}` people.", count));
		}

		[Command("replacenonascii")]
		[Alias("rna")]
		[Usage("<\"Replacement:String to Replace With\"> <ANSI:True|False> <" + Constants.BYPASS_STRING + ">")]
		[Summary("Any user who has a name and nickname with non regular ascii characters will have their username changed to the given string. No replace string just lists the users instead."
			+ "Max is 100 users per use unless the bypass string is said.")]
		[PermissionRequirement(1U << (int)GuildPermission.ManageNicknames)]
		[DefaultEnabled(true)]
		public async Task ReplaceNonAscii([Optional, Remainder] string input)
		{
			//Splitting input
			var inputArray = Actions.SplitByCharExceptInQuotes(input, ' ');
			var replaceStr = Actions.GetVariable(inputArray, "replacement");
			var ansiStr = Actions.GetVariable(inputArray, "ansi");

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
				var desc = String.Join("\n", users.Select(x => String.Format("`{0}.` `{1}`", count++.ToString().PadLeft(length, '0'), Actions.FormatUser(x, x?.Id))));
				var embed = Actions.MakeNewEmbed("Invalid Name Users", desc);
				await Actions.SendEmbedMessage(Context.Channel, embed);
				return;
			}
			else
			{
				var validUsers = users.GetUpToXElement(Actions.GetMaxNumOfUsersToGather(Context, inputArray));
				await Actions.RenicknameALotOfPeople(Context, validUsers, replaceStr);
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
			var users = (await Actions.GetUsersTheBotAndUserCanEdit(Context, x => x.Nickname != null)).GetUpToXElement(Actions.GetMaxNumOfUsersToGather(Context, new[] { input }));
			await Actions.RenicknameALotOfPeople(Context, users, null);
		}

		[Command("prunemembers")]
		[Alias("pmems")]
		[Usage("[1|7|30] <Real>")]
		[Summary("Removes users who have no roles and have not been seen in the past given amount of days. Real means an actual prune, otherwise this returns the number of users that would have been pruned.")]
		[PermissionRequirement]
		[DefaultEnabled(true)]
		public async Task PruneMembers([Remainder] string input)
		{
			//Split into the ints and 'bool'
			var inputArray = input.Split(new[] { ' ' }, 2);
			int[] validDays = { 1, 7, 30 };

			//Get the int
			if (!int.TryParse(inputArray[0], out int amountOfDays))
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Not a number."));
				return;
			}
			else if (!validDays.Contains(amountOfDays))
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Not a valid number of days."));
				return;
			}

			//If only one argument and it's an int then it's a simulation
			if (inputArray.Length != 2)
			{
				var amount = await Context.Guild.PruneUsersAsync(amountOfDays, true);
				await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("`{0}` members would be pruned with a prune period of `{1}` days.", amount, amountOfDays));
			}
			//Otherwise test if it's the real deal
			else if (Actions.CaseInsEquals(inputArray[1], "real"))
			{
				var amount = await Context.Guild.PruneUsersAsync(amountOfDays);
				await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("`{0}` members have been pruned with a prune period of `{1}` days.", amount, amountOfDays));
			}
			else
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("You input a valid number, but put some gibberish at the end."));
			}
		}

		[Command("softban")]
		[Alias("sb")]
		[Usage("[@User]")]
		[Summary("Bans then unbans a user from the guild. Removes all recent messages from them.")]
		[PermissionRequirement(1U << (int)GuildPermission.BanMembers)]
		[DefaultEnabled(true)]
		public async Task SoftBan([Remainder] string input)
		{
			//Get out the editable and uneditable users
			var evaluatedUsers = Actions.GetValidEditUsers(Context);
			if (!evaluatedUsers.HasValue)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.USER_ERROR));
				return;
			}
			var success = evaluatedUsers.Value.Success;
			var failure = evaluatedUsers.Value.Failure;

			//Softban the users and send a response message
			await success.ForEachAsync(async x =>
			{
				await Context.Guild.AddBanAsync(x, 3);
				await Context.Guild.RemoveBanAsync(x);
			});
			await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.FormatResponseMessagesForCmdsOnLotsOfObjects(success, failure, "user", "softbanned", "softban"));
		}

		[Command("ban")]
		[Alias("b")]
		[Usage("[@User] <Days:int> <Time:int>")]
		[Summary("Bans the user from the guild. Days specifies how many days worth of messages to delete. Time specifies how long and is in minutes. Mentions must be used.")]
		[PermissionRequirement(1U << (int)GuildPermission.BanMembers)]
		[DefaultEnabled(true)]
		public async Task Ban([Remainder] string input)
		{
			var inputArray = input.Split(' ');
			var mentionedUsers = Context.Message.MentionedUserIds;
			var daysStr = Actions.GetVariable(inputArray, "days");
			var timeStr = Actions.GetVariable(inputArray, "time");

			var pruneDays = 0;
			if (!String.IsNullOrWhiteSpace(daysStr))
			{
				if (!Int32.TryParse(daysStr, out pruneDays))
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("The input for days was not a number."));
					return;
				}
			}
			var timeForBan = 0;
			if (!String.IsNullOrWhiteSpace(timeStr))
			{
				if (!Int32.TryParse(timeStr, out timeForBan))
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("The input for time was not a number."));
					return;
				}
			}

			//Get out the editable and uneditable users
			var evaluatedUsers = Actions.GetValidEditUsers(Context);
			if (!evaluatedUsers.HasValue)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.USER_ERROR));
				return;
			}
			var success = evaluatedUsers.Value.Success;
			var failure = evaluatedUsers.Value.Failure;

			//Ban the users
			await success.ForEachAsync(async x =>
			{
				await Context.Guild.AddBanAsync(x, pruneDays);
				if (timeForBan != 0)
				{
					Variables.PunishedUsers.Add(new RemovablePunishment(Context.Guild, x.Id, PunishmentType.Ban, DateTime.UtcNow.AddMinutes(timeForBan)));
				}
			});

			var response = Actions.FormatResponseMessagesForCmdsOnLotsOfObjects(success, failure, "user", "banned", "ban");
			if (success.Any() && pruneDays != 0)
			{
				response += String.Format("Also deleted `{0}` day{1} worth of messages for banned users. ", pruneDays, Actions.GetPlural(pruneDays));
			}
			if (success.Any() && timeForBan != 0)
			{
				response += String.Format("Banned users will be unbanned in `{0}` minute{1}. ", timeForBan, Actions.GetPlural(timeForBan));
			}
			await Actions.SendChannelMessage(Context, response);
		}

		[Command("unban")]
		[Alias("ub")]
		[Usage("<\"Username:Name\"> <Discriminator:Number> <ID:User ID>")]
		[Summary("Unbans the user from the guild.")]
		[PermissionRequirement(1U << (int)GuildPermission.BanMembers)]
		[DefaultEnabled(true)]
		public async Task Unban([Remainder] string input)
		{
			//Cut the user mention into the username and the discriminator
			var inputArray = Actions.SplitByCharExceptInQuotes(input, ' ');
			var username = Actions.GetVariable(inputArray, "username");
			var discriminator = Actions.GetVariable(inputArray, "discriminator");
			var userID = Actions.GetVariable(inputArray, "id");

			IUser user = null;
			var bans = (await Context.Guild.GetBansAsync()).ToList();
			if (!String.IsNullOrWhiteSpace(userID))
			{
				if (ulong.TryParse(input, out ulong inputUserID))
				{
					user = bans.FirstOrDefault(x => x.User.Id.Equals(inputUserID)).User;
				}
				else
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid user ID."));
					return;
				}
			}
			else if (!String.IsNullOrWhiteSpace(username))
			{
				//Find users with the given username then the given discriminator if provided
				var users = bans.Where(x => Actions.CaseInsEquals(x.User.Username, input)).ToList();
				if (!String.IsNullOrWhiteSpace(discriminator))
				{
					if (ushort.TryParse(discriminator, out ushort disc))
					{
						users = users.Where(x => x.User.Discriminator.Equals(disc)).ToList();
					}
					else
					{
						await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid discriminator provided."));
						return;
					}
				}

				//Return a message saying if there are multiple users
				if (users.Count > 1)
				{
					var msg = String.Join("`, `", users.Select(x => Actions.FormatUser(x.User, x.User?.Id)));
					await Actions.SendChannelMessage(Context, String.Format("The following users have that name: `{0}`.", msg));
					return;
				}
				else if (users.Count == 1)
				{
					user = users.First().User;
				}
				else
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Could not find a user on the ban list matching the given criteria."));
					return;
				}
			}
			else
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("A username or ID must be provided."));
				return;
			}

			await Context.Guild.RemoveBanAsync(user);
			await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully unbanned the user `{0}`", Actions.FormatUser(user, user?.Id)));
		}

		[Command("kick")]
		[Alias("k")]
		[Usage("[@User]")]
		[Summary("Kicks the user from the guild.")]
		[PermissionRequirement(1U << (int)GuildPermission.KickMembers)]
		[DefaultEnabled(true)]
		public async Task Kick([Remainder] string input)
		{
			//Get out the editable and uneditable users
			var evaluatedUsers = Actions.GetValidEditUsers(Context);
			if (!evaluatedUsers.HasValue)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.USER_ERROR));
				return;
			}
			var success = evaluatedUsers.Value.Success;
			var failure = evaluatedUsers.Value.Failure;

			//Kick the users and send a response message
			await success.ForEachAsync(async x => await x.KickAsync());
			await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.FormatResponseMessagesForCmdsOnLotsOfObjects(success, failure, "user", "kicked", "kick"));
		}

		[Command("currentbanlist")]
		[Alias("cbl")]
		[Usage("")]
		[Summary("Displays all the bans on the guild.")]
		[PermissionRequirement(1U << (int)GuildPermission.BanMembers)]
		[DefaultEnabled(true)]
		public async Task CurrentBanList()
		{
			var bans = (await Context.Guild.GetBansAsync()).ToList();
			var count = 1;
			var lengthForCount = bans.Count.ToString().Length;
			var description = String.Join("\n", bans.Select(x =>
			{
				return String.Format("`{0}.` `{1}`", count++.ToString().PadLeft(lengthForCount, '0'), Actions.FormatUser(x.User, x.User?.Id));
			}));

			if (String.IsNullOrWhiteSpace(description))
			{
				await Actions.SendChannelMessage(Context, "This guild has no bans.");
			}
			else
			{
				await Actions.SendEmbedMessage(Context.Channel, Actions.MakeNewEmbed("Current Bans", description));
			}
		}

		[Command("removemessages")]
		[Alias("rm")]
		[Usage("<@User> <#Channel> [Number of Messages]")]
		[Summary("Removes the selected number of messages from either the user, the channel, both, or, if neither is input, the current channel. Administrators can delete more than 100 messages at a time.")]
		[PermissionRequirement(1U << (int)GuildPermission.ManageMessages)]
		[DefaultEnabled(true)]
		public async Task RemoveMessages([Remainder] string input)
		{
			//Split the input
			var inputArray = input.Split(' ');
			if (inputArray.Length > 3)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR));
				return;
			}

			//Get the user mention
			IGuildUser user = null;
			var mentionedUsers = Context.Message.MentionedUserIds;
			if (mentionedUsers.Count == 1)
			{
				var returnedChannel = Actions.GetChannel(Context, new[] { CheckType.None }, mentionedUsers.First().ToString());
				if (returnedChannel.Reason != FailureReason.Not_Failure)
				{
					await Actions.HandleObjectGettingErrors(Context, returnedChannel);
					return;
				}
				user = returnedChannel.Object as IGuildUser;
			}
			else if (mentionedUsers.Count > 1)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("More than one user was input."));
				return;
			}

			//Get the channel mention
			var channel = Context.Channel as ITextChannel;
			var mentionedChannels = Context.Message.MentionedChannelIds;
			if (mentionedChannels.Count == 1)
			{
				var returnedChannel = Actions.GetChannel(Context, new[] { CheckType.Channel_Delete_Messages }, mentionedChannels.First().ToString());
				if (returnedChannel.Reason != FailureReason.Not_Failure)
				{
					await Actions.HandleObjectGettingErrors(Context, returnedChannel);
					return;
				}
				channel = returnedChannel.Object as ITextChannel;
			}
			else if (mentionedChannels.Count > 1)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("More than one channel was input."));
				return;
			}

			//Check if the channel that's having messages attempted to be removed on is a log channel
			if (Variables.Guilds.TryGetValue(Context.Guild.Id, out BotGuildInfo guildInfo))
			{
				var serverLog = guildInfo.ServerLog;
				var modLog = guildInfo.ModLog;
				var imageLog = guildInfo.ImageLog;
				if (Context.User.Id != Context.Guild.OwnerId && (channel.Id == serverLog.Id || channel.Id == modLog.Id || channel.Id == imageLog.Id))
				{
					//Send a message in the channel
					await Actions.SendChannelMessage(channel, String.Format("Hey, @here, {0} is trying to delete stuff.", Context.User.Mention));

					//DM the owner of the server
					await Actions.SendDMMessage(await (await Context.Guild.GetOwnerAsync()).CreateDMChannelAsync(),
						String.Format("`{0}` is trying to delete stuff from the server/mod log.", Actions.FormatUser(Context.User, Context.User?.Id)));
					return;
				}
			}

			//Checking for valid request count
			var requestCount = -1;
			if (inputArray.FirstOrDefault(x => int.TryParse(x, out requestCount)) == null || requestCount < 1)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Incorrect input for number of messages to be removed."));
				return;
			}

			//See if the user is trying to delete more than 100 messages at a time
			if (requestCount > Constants.MESSAGES_TO_GATHER && !(Context.User as IGuildUser).GuildPermissions.Administrator)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("You need administrator to remove more than 100 messages at a time."));
				return;
			}

			//Check if saying it on the same channel
			if (Context.Channel == channel)
			{
				++requestCount;
			}

			await Actions.RemoveMessages(channel, user, requestCount);
			await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully deleted `{0}` {1}{2}{3}.",
				requestCount,
				requestCount != 1 ? "messages" : "message",
				user == null ? "" : String.Format(" from `{0}`", Actions.FormatUser(user, user?.Id)),
				channel == null ? "" : String.Format(" on `{0}`", Actions.FormatChannel(channel))));
		}

		[Command("slowmode")]
		[Alias("sm")]
		[Usage("<\"Roles:.../.../\"> <Messages:1 to 5> <Time:1 to 30> <Guild:Yes> | Off [Guild|Channel|All]")]
		[Summary("The first argument is the roles that get ignored by slowmode, the second is the amount of messages, and the third is the time period. Default is: none, 1, 5." +
			"Bots are unaffected by slowmode. Any users who are immune due to roles stay immune even if they lose said role until a new slowmode is started.")]
		[PermissionRequirement]
		[DefaultEnabled(true)]
		public async Task SlowMode([Optional, Remainder] string input)
		{
			var inputArray = Actions.SplitByCharExceptInQuotes(input, ' ');
			if (inputArray != null && inputArray.Length > 4)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR));
				return;
			}
			var roleStr = Actions.GetVariable(inputArray, "roles");
			var msgStr = Actions.GetVariable(inputArray, "messages");
			var timeStr = Actions.GetVariable(inputArray, "time");
			var targetStr = Actions.GetVariable(inputArray, "guild");

			//Get the guild info
			var guildInfo = Variables.Guilds[Context.Guild.Id];

			//Check if the input starts with 'off'
			if (inputArray != null && Actions.CaseInsEquals(inputArray[0], "off"))
			{
				if (inputArray.Length != 2)
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR));
				}
				else if (Actions.CaseInsEquals(inputArray[1], "guild"))
				{
					guildInfo.SetSlowmodeGuild(null);
					await Actions.MakeAndDeleteSecondaryMessage(Context, "Successfully removed the slowmode on the guild.");
				}
				else if (Actions.CaseInsEquals(inputArray[1], "channel"))
				{
					//Remove the channel
					guildInfo.SlowmodeChannels.RemoveAll(x => x.ChannelID == Context.Channel.Id);

					//Send a success message
					await Actions.MakeAndDeleteSecondaryMessage(Context, "Successfully removed the slowmode on the channel.");
				}
				else if (Actions.CaseInsEquals(inputArray[1], "all"))
				{
					//Remove the guild and every single channel on the guild
					guildInfo.SetSlowmodeGuild(null);
					guildInfo.ClearSMChannels();

					//Send a success message
					await Actions.MakeAndDeleteSecondaryMessage(Context, "Successfully removed all slowmodes on the guild and its channels.");
				}
				else
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("With off, the second argument must be either Guild, Channel, or All."));
				}
				return;
			}

			//Check if the target is already in either dictionary
			if (!String.IsNullOrWhiteSpace(targetStr))
			{
				//Check the channel dictionary
				if (guildInfo.SlowmodeChannels.Any(x => x.ChannelID == Context.Channel.Id))
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, "Channel already is in slowmode.");
					return;
				}
			}
			else
			{
				//Check the guild dicionary
				if (guildInfo.SlowmodeGuild != null)
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, "Guild already is in slowmode.");
					return;
				}
			}

			//Get the roles
			var rolesIDs = new List<ulong>();
			if (!String.IsNullOrWhiteSpace(roleStr))
			{
				//Split the string into the role names
				var roleArray = roleStr.Split('/').ToList();

				//Get each role name and check if it's a valid role
				roleArray.ForEach(x =>
				{
					var returnedRole = Actions.GetRole(Context, new[] { CheckType.None }, x);
					if (returnedRole.Reason == FailureReason.Not_Failure)
					{
						rolesIDs.Add(returnedRole.Object.Id);
					}
				});
			}

			//Make a list of the role names
			var roleNames = new List<string>();
			rolesIDs.Distinct().ToList().ForEach(x => roleNames.Add(Context.Guild.GetRole(x).Name));

			//Get the messages limit
			var msgsLimit = 1;
			if (!String.IsNullOrWhiteSpace(msgStr))
			{
				if (int.TryParse(msgStr, out msgsLimit))
				{
					if (msgsLimit > 5 || msgsLimit < 1)
					{
						await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Message limit must be between 1 and 5 inclusive."));
						return;
					}
				}
				else
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("The input for messages was not a number. Remember: no space after the colon."));
					return;
				}
			}


			//Get the time limit
			var timeLimit = 5;
			if (!String.IsNullOrWhiteSpace(timeStr))
			{
				if (int.TryParse(timeStr, out timeLimit))
				{
					if (timeLimit > 30 || timeLimit < 1)
					{
						await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Time must be between 1 and 10 inclusive."));
						return;
					}
				}
				else
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("The input for time was not a number. Remember: no space after the colon."));
					return;
				}
			}

			//Add the users into the list with their given messages and if they're affected
			var slowmodeUsers = (await Context.Guild.GetUsersAsync()).Where(x => !x.RoleIds.Intersect(rolesIDs).Any()).Select(x =>
								{ return new SlowmodeUser(x, msgsLimit, msgsLimit, timeLimit); }).ToList();

			//If targetString is null then take that as only the channel and not the guild
			if (!String.IsNullOrWhiteSpace(targetStr))
			{
				//Add the channel and list to a dictionary
				guildInfo.SlowmodeChannels.Add(new SlowmodeChannel(Context.Channel.Id, slowmodeUsers));
			}
			else
			{
				//Add the guild and list to a dictionary
				guildInfo.SetSlowmodeGuild(new SlowmodeGuild(slowmodeUsers));
			}

			//Send a success message
			await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully enabled slowmode on `{0}` with a message limit of `{1}` and time interval of `{2}` seconds.{3}",
				!String.IsNullOrWhiteSpace(targetStr) ? Actions.FormatChannel(Context.Channel) : Actions.FormatGuild(Context.Guild),
				msgsLimit,
				timeLimit,
				roleNames.Count == 0 ? "" : String.Format("\nImmune roles: `{0}`.", String.Join("`, `", roleNames))));
		}

		[Command("forallwithrole")]
		[Alias("fawr")]
		[Usage("[Give_Role|GR|Take_Role|TR|Give_Nickname|GNN|Take_Nickname|TNN] [\"Role\"] <\"Role\"|\"Nickname\"> <" + Constants.BYPASS_STRING + ">")]
		[Summary("Only self hosted bots are allowed to go past 100 users per use. When used on a self bot, `" + Constants.BYPASS_STRING + "` removes the 100 users limit." +
			"All actions but `Take_Nickame` require the output role/nickname.")]
		[PermissionRequirement]
		[DefaultEnabled(true)]
		public async Task ForAllWithRole([Remainder] string input)
		{
			//Argument checking
			var inputArray = Actions.SplitByCharExceptInQuotes(input, ' ');
			var action = inputArray[0];
			if (!Enum.TryParse(action, true, out FAWRType actionEnum))
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ACTION_ERROR));
				return;
			}
			actionEnum = Actions.ClarifyFAWRType(actionEnum);
			if (actionEnum != FAWRType.Take_Nickname && inputArray.Length < 3)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR));
				return;
			}
			else if (actionEnum == FAWRType.Take_Nickname && inputArray.Length < 2)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR));
				return;
			}
			var inputStr = inputArray[1];
			var outputStr = inputArray[2];

			//Verifying the attempted command is valid
			var returnedInputRole = Actions.GetRole(Context, new[] { CheckType.None }, inputStr);
			if (returnedInputRole.Reason != FailureReason.Not_Failure)
			{
				await Actions.HandleObjectGettingErrors(Context, returnedInputRole);
				return;
			}
			var inputRole = returnedInputRole.Object;

			if (actionEnum == FAWRType.Give_Role && Actions.CaseInsEquals(inputStr, outputStr))
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Cannot give the same role that is being gathered."));
				return;
			}
			else if (actionEnum == FAWRType.Give_Nickname)
			{
				if (outputStr.Length > Constants.MAX_NICKNAME_LENGTH)
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(String.Format("Nicknames cannot be longer than `{0}` charaters.", Constants.MAX_NICKNAME_LENGTH)));
					return;
				}
				else if (outputStr.Length < Constants.MIN_NICKNAME_LENGTH)
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(String.Format("Nicknames cannot be less than `{0}` characters.", Constants.MIN_NICKNAME_LENGTH)));
					return;
				}
			}

			//Get the amount of users allowed
			var len = Actions.GetMaxNumOfUsersToGather(Context, inputArray);
			var users = (await Actions.GetUsersTheBotAndUserCanEdit(Context, (x => x.RoleIds.Contains(inputRole.Id)))).GetUpToXElement(len);
			var userCount = users.Count;
			if (userCount == 0)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Unable to find any users with the input role that could be modified."));
				return;
			}

			//Send a message detailing how many users are being changed and how long it will likely take
			var time = (int)(userCount * 1.2);
			var msg = await Actions.SendChannelMessage(Context, String.Format("Grabbed `{0}` user{1}. ETA on completion: `{2}` seconds.", userCount, Actions.GetPlural(userCount), time)) as IUserMessage;
			var typing = Context.Channel.EnterTypingState();

			var guildInfo = Variables.Guilds[Context.Guild.Id];
			var count = 0;
			var t = Task.Run(async () =>
			{
				switch (actionEnum)
				{
					case FAWRType.Give_Role:
					{
						var returnedOutputRole = Actions.GetRole(Context, new[] { CheckType.Role_Editability }, outputStr);
						if (returnedOutputRole.Reason != FailureReason.Not_Failure)
						{
							await Actions.HandleObjectGettingErrors(Context, returnedOutputRole);
							return;
						}
						var outputRole = returnedOutputRole.Object;
						if (Context.Guild.EveryoneRole.Id.Equals(outputRole.Id))
						{
							await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("You can't give the `{0}` role.", Constants.FAKE_EVERYONE));
							return;
						}

						guildInfo.FAWRRoles.Add(outputRole);
						foreach (var user in users)
						{
							++count;
							if (count % 10 == 0)
							{
								await msg.ModifyAsync(x => x.Content = String.Format("ETA on completion: `{0}` seconds.", (int)((userCount - count) * 1.2)));
								if (Context.Guild.GetRole(outputRole.Id) == null)
								{
									await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("The output role has been deleted."));
									return;
								}
							}

							await Actions.GiveRole(user, outputRole);
						}
						guildInfo.FAWRRoles.Remove(outputRole);

						await Actions.SendChannelMessage(Context, String.Format("Successfully gave the role `{0}` to `{1}` users.", outputRole.Name, count));
						break;
					}
					case FAWRType.Take_Role:
					{
						var returnedOutputRole = Actions.GetRole(Context, new[] { CheckType.Role_Editability }, outputStr);
						if (returnedOutputRole.Reason != FailureReason.Not_Failure)
						{
							await Actions.HandleObjectGettingErrors(Context, returnedOutputRole);
							return;
						}
						var outputRole = returnedOutputRole.Object;
						if (Context.Guild.EveryoneRole.Id.Equals(outputRole.Id))
						{
							await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("You can't give the `{0}` role.", Constants.FAKE_EVERYONE));
							return;
						}

						guildInfo.FAWRRoles.Add(outputRole);
						foreach (var user in users)
						{
							++count;
							if (count % 10 == 0)
							{
								await msg.ModifyAsync(x => x.Content = String.Format("ETA on completion: `{0}` seconds.", (int)((userCount - count) * 1.2)));
								if (Context.Guild.GetRole(outputRole.Id) == null)
								{
									await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("The output role has been deleted."));
									return;
								}
							}

							await Actions.TakeRole(user, outputRole);
						}
						guildInfo.FAWRRoles.Remove(outputRole);

						await Actions.SendChannelMessage(Context, String.Format("Successfully took the role `{0}` from `{1}` users.", outputRole.Name, count));
						break;
					}
					case FAWRType.Give_Nickname:
					{
						guildInfo.FAWRNicknames.Add(outputStr);
						foreach (var user in users)
						{
							++count;
							if (count % 10 == 0)
							{
								await msg.ModifyAsync(x => x.Content = String.Format("ETA on completion: `{0}` seconds.", (int)((userCount - count) * 1.2)));
							}

							await Actions.ChangeNickname(user, outputStr);
						}
						guildInfo.FAWRNicknames.Remove(outputStr);

						await Actions.SendChannelMessage(Context, String.Format("Successfully gave the nickname `{0}` to `{1}` users.", outputStr, count));
						break;
					}
					case FAWRType.Take_Nickname:
					{
						guildInfo.FAWRNicknames.Add(Constants.NO_NN);
						foreach (var user in users)
						{
							++count;
							if (count % 10 == 0)
							{
								await msg.ModifyAsync(x => x.Content = String.Format("ETA on completion: `{0}` seconds.", (int)((userCount - count) * 1.2)));
							}

							await Actions.ChangeNickname(user, null);
						}
						guildInfo.FAWRNicknames.Remove(Constants.NO_NN);

						await Actions.SendChannelMessage(Context, String.Format("Successfully removed the nicknames of `{1}` users.", count));
						break;
					}
				}

				typing.Dispose();
				await msg.DeleteAsync();
			});
		}
	}
}
