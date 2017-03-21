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
		[Usage("[@User] <Time>")]
		[Summary("If the user is not text muted, this will mute them. If they are text muted, this will unmute them. Time is in minutes, and if no time is given then the mute will not expire.")]
		[PermissionRequirement((1U << (int)GuildPermission.ManageRoles) | (1U << (int)GuildPermission.ManageMessages))]
		[DefaultEnabled(true)]
		public async Task FullMute([Remainder] string input)
		{
			//Check if role already exists, if not, create it
			var muteRole = await Actions.CreateMuteRoleIfNotFound(Context.Guild, Constants.MUTE_ROLE_NAME);
			if (muteRole == null)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Unable to get the mute role."));
				return;
			}

			//See if both the bot and the user can edit/use this role
			if (await Actions.GetRoleEditAbility(Context, role: muteRole) == null)
				return;

			//Split the input
			var inputArray = input.Split(new char[] { ' ' }, 2);
			//Test if valid user mention
			var user = await Actions.GetUser(Context.Guild, inputArray[0]);
			if (user == null)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.USER_ERROR));
				return;
			}

			if (user.RoleIds.Contains(muteRole.Id))
			{
				//Remove the role
				await Actions.TakeRole(user, Actions.GetRole(Context.Guild, Constants.MUTE_ROLE_NAME));
				await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully unmuted `{0}#{1}`.", user.Username, user.Discriminator));
			}
			else
			{
				//Check if time is given
				var timeString = "";
				if (inputArray.Length == 2)
				{
					if (int.TryParse(inputArray[1], out int time))
					{
						Variables.PunishedUsers.Add(new RemovablePunishment(Context.Guild, user, muteRole, DateTime.UtcNow.AddMinutes(time)));
						timeString = String.Format(" for {0} seconds", time);
					}
					else
					{
						await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid time supplied."));
						return;
					}
				}

				//Give them the mute role
				await Actions.GiveRole(user, muteRole);
				await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully text muted `{0}`{1}.", Actions.FormatUser(user), timeString));
			}
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
			var inputArray = input.Split(new char[] { ' ' }, 2);
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
						Variables.PunishedUsers.Add(new RemovablePunishment(Context.Guild, user, PunishmentType.Mute, DateTime.UtcNow.AddMinutes(time)));
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
				await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully muted `{0}`{1}.", Actions.FormatUser(user), timeString));
			}
			else
			{
				//Unmute the user
				await user.ModifyAsync(x => x.Mute = false);
				await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully unmuted `{0}`.", Actions.FormatUser(user)));
			}
		}

		[Command("deafen")]
		[Alias("dfn", "d")]
		[Usage("[@User] <Time>")]
		[Summary("If the user is not voice muted, this will mute them. If they are voice muted, this will unmute them. Time is in minutes, and if no time is given then the mute will not expire.")]
		[PermissionRequirement(1U << (int)GuildPermission.DeafenMembers)]
		[DefaultEnabled(true)]
		public async Task Deafen([Remainder] string input)
		{
			//Split the input
			var inputArray = input.Split(new char[] { ' ' }, 2);
			//Test if valid user mention
			var user = await Actions.GetUser(Context.Guild, inputArray[0]);
			if (user == null)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.USER_ERROR));
				return;
			}

			//See if it should deafen or undeafen
			if (!user.IsDeafened)
			{
				//Check if time was supplied
				var timeString = "";
				if (inputArray.Length == 2)
				{
					if (int.TryParse(inputArray[1], out int time))
					{
						Variables.PunishedUsers.Add(new RemovablePunishment(Context.Guild, user, PunishmentType.Deafen, DateTime.UtcNow.AddMinutes(time)));
						timeString = String.Format(" for {0} minutes", time);
					}
					else
					{
						await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid time supplied."));
						return;
					}
				}

				//Deafen them
				await user.ModifyAsync(x => x.Deaf = true);
				await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully deafened `{0}`{1}.", Actions.FormatUser(user), timeString));
			}
			else
			{
				//Undeafen them
				await user.ModifyAsync(x => x.Deaf = false);
				await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully undeafened `{0}`.", Actions.FormatUser(user)));
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
			var inputArray = input.Split(new char[] { ' ' }, 2);
			if (inputArray.Length != 2)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR));
				return;
			}
			var userStr = inputArray[0];
			var chanStr = inputArray[1];

			//Check if valid user
			var user = await Actions.GetUser(Context.Guild, userStr);
			if (user == null)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.USER_ERROR));
				return;
			}

			//Check if user is in a voice channel
			if (user.VoiceChannel == null)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("User is not in a voice channel."));
				return;
			}
			//See if the bot can move people from this channel
			else if (Actions.GetChannelMovability(user.VoiceChannel, await Actions.GetUser(Context.Guild, Variables.Bot_ID)) == null)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Unable to move this user due to permissions or due to the user being in a voice channel before the bot started up."));
				return;
			}
			//See if the user can move people from this channel
			else if (Actions.GetChannelMovability(user.VoiceChannel, Context.User) == null)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("You are unable to move people from this channel."));
				return;
			}

			//Check if valid channel that the user can edit
			var channel = await Actions.GetChannelEditAbility(Context, chanStr);
			if (channel == null)
				return;
			var vc = channel as IVoiceChannel;
			if (vc == null)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("This command will not work on a text channel."));
				return;
			}

			//See if trying to put user in the exact same channel
			if (user.VoiceChannel == channel)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("User is already in that channel"));
				return;
			}

			await user.ModifyAsync(x => x.Channel = Optional.Create(vc));
			await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully moved `{0}#{1}` to `{2}`.", user.Username, user.Discriminator, channel.Name));
		}

		[Command("nickname")]
		[Alias("nn")]
		[Usage("[@User] [New Nickname|Remove]")]
		[Summary("Gives the user a nickname.")]
		[PermissionRequirement(1U << (int)GuildPermission.ManageNicknames)]
		[DefaultEnabled(true)]
		public async Task Nickname([Remainder] string input)
		{
			//Input and splitting
			var inputArray = input.Split(new char[] { ' ' }, 2);

			//Get the nickname
			var nickname = "";
			if (inputArray.Length == 2)
			{
				if (Actions.CaseInsEquals(inputArray[1], "remove"))
				{
					nickname = null;
				}
				else
				{
					nickname = inputArray[1];
				}
			}
			else
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR));
				return;
			}
			//Check if valid length
			if (nickname != null && nickname.Length > Constants.NICKNAME_MAX_LENGTH)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(String.Format("Nicknames cannot be longer than `{0}` characters.", Constants.NICKNAME_MAX_LENGTH)));
				return;
			}
			else if (nickname != null && nickname.Length < Constants.NICKNAME_MIN_LENGTH)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(String.Format("Nicknames cannot be less than `{0}` characters.", Constants.NICKNAME_MIN_LENGTH)));
				return;
			}

			//Check if valid user
			var user = await Actions.GetUser(Context.Guild, inputArray[0]);
			if (user == null)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.USER_ERROR));
				return;
			}

			var bot = await Actions.GetUser(Context.Guild, Variables.Bot_ID);

			//Checks for positions
			if (!Actions.UserCanBeModifiedByUser(Context, user))
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("User cannot be nicknamed by you."));
				return;
			}
			else if (!Actions.UserCanBeModifiedByBot(Context.Guild, user, bot))
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("User cannot be nicknamed by the bot."));
				return;
			}

			//Give the user the nickname
			await user.ModifyAsync(x => x.Nickname = nickname);
			if (nickname != null)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully gave the nickname `{0}` to `{1}`.", nickname, Actions.FormatUser(user)));
			}
			else
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Sucessfully removed the nickname from `{0}`.", Actions.FormatUser(user)));
			}
		}

		[Command("replacewordsinnames")]
		[Alias("rwin")]
		[Usage("\"[String to Find]\" \"[String to Replace]\" <" + Constants.BYPASS_STRING + ">")]
		[Summary("Gives any users who have a username/nickname with the given string a new nickname that replaces it. Max is 100 users per use unless the bypass string is said.")]
		[PermissionRequirement(1U << (int)GuildPermission.ManageNicknames)]
		[DefaultEnabled(true)]
		public async Task NicknameAllWithName([Remainder] string input)
		{
			//Split and get variables
			var inputArray = Actions.SplitByCharExceptInQuotes(input, ' ');
			if (inputArray.Length < 2 || inputArray.Length > 3)
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
			if (find.Length > Constants.NICKNAME_MAX_LENGTH)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(String.Format("The string to replace can only be up to `{0}` characters long.")));
				return;
			}
			if (with.Length > Constants.NICKNAME_MAX_LENGTH)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(String.Format("The string to replace with can only be up to `{0}` characters long.")));
				return;
			}

			//Get the users 
			var maxLength = (inputArray.Length == 3 && Actions.CaseInsEquals(inputArray[2], Constants.BYPASS_STRING)) ? int.MaxValue : 100;
			var users = (await Context.Guild.GetUsersAsync()).Where(x => Actions.CaseInsIndexOf(x.Username, find) || (x.Nickname != null && Actions.CaseInsIndexOf(x.Nickname, find))).ToList();
			var userCount = users.Count;
			users.RemoveRange(Math.Min(maxLength, userCount), Math.Max(userCount - maxLength, 0));

			//User count checking and stuff
			userCount = users.Count;
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
			var bot = await Actions.GetUser(Context.Guild, Variables.Bot_ID);
			await users.ForEachAsync(async x =>
			{
				++count;
				if (count % 10 == 0)
				{
					await msg.ModifyAsync(y => y.Content = String.Format("Attempting to rename `{0}` people.", userCount - count));
				}

				if (!Actions.UserCanBeModifiedByBot(Context.Guild, x, bot))
					return;

				if (x.Nickname != null)
				{
					await x.ModifyAsync(y => y.Nickname = Actions.CaseInsReplace(x.Nickname, find, with));
				}
				else
				{
					await x.ModifyAsync(y => y.Nickname = Actions.CaseInsReplace(x.Username, find, with));
				}
			});

			//Get rid of stuff and send a success message
			typing.Dispose();
			await Actions.DeleteMessage(msg);
			await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully renamed `{0}` people.", count));
		}

		[Command("removeallnicknames")]
		[Alias("rann")]
		[Usage("<" + Constants.BYPASS_STRING + ">")]
		[Summary("Remove all nicknames of users on the guild. Max is 100 users per use unless the bypass string is said.")]
		[PermissionRequirement(1U << (int)GuildPermission.ManageNicknames)]
		[DefaultEnabled(true)]
		public async Task RemoveAllNickNames([Optional, Remainder] string input)
		{
			var maxLength = (input != null && Actions.CaseInsEquals(input, Constants.BYPASS_STRING)) ? int.MaxValue : 100;
			var users = (await Context.Guild.GetUsersAsync()).Where(x => x.Nickname != null).ToList();
			var userCount = users.Count;
			users.RemoveRange(Math.Min(maxLength, userCount), Math.Max(userCount - maxLength, 0));

			//User count checking and stuff
			userCount = users.Count;
			if (userCount == 0)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Unable to find any users with a nickname."));
				return;
			}

			//Have the bot stay in the typing state and have a message that can be updated 
			var msg = await Actions.SendChannelMessage(Context, String.Format("Attempting to nickname from `{0}` people.", userCount)) as IUserMessage;
			var typing = Context.Channel.EnterTypingState();

			//Actually rename them all
			var count = 0;
			var bot = await Actions.GetUser(Context.Guild, Variables.Bot_ID);
			await users.ForEachAsync(async x =>
			{
				++count;
				if (count % 10 == 0)
				{
					await msg.ModifyAsync(y => y.Content = String.Format("Attempting to remove the nickname from `{0}` people.", userCount - count));
				}

				if (!Actions.UserCanBeModifiedByBot(Context.Guild, x, bot))
					return;

				if (x.Nickname != null)
				{
					await x.ModifyAsync(y => y.Nickname = null);
				}
				else
				{
					await x.ModifyAsync(y => y.Nickname = null);
				}
			});

			//Get rid of stuff and send a success message
			typing.Dispose();
			await Actions.DeleteMessage(msg);
			await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully removed the nicknames of `{0}` people.", count));
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
			var inputArray = input.Split(new char[] { ' ' }, 2);
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
			//Test if valid user mention
			var inputUser = await Actions.GetUser(Context.Guild, input);
			if (inputUser == null)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.USER_ERROR));
				return;
			}

			var bot = await Actions.GetUser(Context.Guild, Variables.Bot_ID);

			//Determine if the user is allowed to softban this person
			if (!Actions.UserCanBeModifiedByUser(Context, inputUser))
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("User is unable to be soft-banned by you."));
				return;
			}
			//Determine if the bot can softban this person
			else if (!Actions.UserCanBeModifiedByBot(Context.Guild, inputUser, bot))
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Bot is unable to soft-ban user."));
				return;
			}

			//Softban the targetted user
			await Context.Guild.AddBanAsync(inputUser, 3);
			await Context.Guild.RemoveBanAsync(inputUser);
			await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully banned and unbanned `{0}#{1}`.", inputUser.Username, inputUser.Discriminator));
		}

		[Command("ban")]
		[Alias("b")]
		[Usage("[@User] <Days:int> <Time:int>")]
		[Summary("Bans the user from the guild. Days specifies how many days worth of messages to delete. Time specifies how long and is in minutes.")]
		[PermissionRequirement(1U << (int)GuildPermission.BanMembers)]
		[DefaultEnabled(true)]
		public async Task Ban([Remainder] string input)
		{
			var inputArray = input.Split(' ');
			var daysStr = Actions.GetVariable(inputArray, "days");
			var timeStr = Actions.GetVariable(inputArray, "time");

			//Check if any prune days
			var pruneDays = 0;
			if (daysStr != null)
			{
				//Checking for valid days requested
				if (!Int32.TryParse(daysStr, out pruneDays))
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("The input for days was not a number."));
					return;
				}
			}

			//Test if valid user mention
			var inputUser = await Actions.GetUser(Context.Guild, inputArray[0]);
			if (inputUser != null)
			{
				var bot = await Actions.GetUser(Context.Guild, Variables.Bot_ID);

				//Determine if the user is allowed to ban this person
				if (!Actions.UserCanBeModifiedByUser(Context, inputUser))
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("User is unable to be banned by you."));
					return;
				}
				//Determine if the bot can ban this person
				else if (!Actions.UserCanBeModifiedByBot(Context.Guild, inputUser, bot))
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Bot is unable to ban user."));
					return;
				}

				//Ban the user
				await Context.Guild.AddBanAsync(inputUser, pruneDays);
			}
			else
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.USER_ERROR));
				return;
			}

			//Forming the second half of the string that prints out when a user is successfully banned
			var plurality = pruneDays == 1 ? "day" : "days";
			var latterHalfOfString = pruneDays > 0 ? String.Format(", and deleted `{0}` {1} worth of messages.", pruneDays, plurality ): ".";

			if (timeStr != null)
			{
				//Checking for valid time requested
				if (!Int32.TryParse(timeStr, out int timeForBan))
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("The input for time was not a number."));
					return;
				}
				else
				{
					Variables.PunishedUsers.Add(new RemovablePunishment(Context.Guild, inputUser, PunishmentType.Ban, DateTime.UtcNow.AddMinutes(timeForBan)));
					timeStr = String.Format("They will be unbanned in `{0}` minutes.", timeForBan);
				}
			}

			//Send a success message
			await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully banned `{0}`{1}", Actions.FormatUser(inputUser), latterHalfOfString), 10000);
		}

		[Command("unban")]
		[Alias("ub")]
		[Usage("[User|User#Discriminator|User ID]")]
		[Summary("Unbans the user from the guild.")]
		[PermissionRequirement(1U << (int)GuildPermission.BanMembers)]
		[DefaultEnabled(true)]
		public async Task Unban([Remainder] string input)
		{
			//Cut the user mention into the username and the discriminator
			var inputArray = input.Split('#');

			//Get a list of the bans
			var bans = await Context.Guild.GetBansAsync();

			//Get their name and discriminator or ulong
			var secondHalfOfTheSecondaryMessage = "";

			if (inputArray.Length == 2)
			{
				//Unban given a username and discriminator
				if (!ushort.TryParse(inputArray[1], out ushort discriminator))
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid discriminator provided."));
				}
				var username = inputArray[0].Replace("@", "");

				//Get a list of users with the name username and discriminator
				var bannedUserWithNameAndDiscriminator = bans.ToList().Where(x => x.User.Username.Equals(username) && x.User.Discriminator.Equals(discriminator)).ToList();

				//Unban the user
				var bannedUser = bannedUserWithNameAndDiscriminator[0].User;
				await Context.Guild.RemoveBanAsync(bannedUser);
				secondHalfOfTheSecondaryMessage = String.Format("unbanned the user `{0}#{1}` with the ID `{2}`.", bannedUser.Username, bannedUser.Discriminator, bannedUser.Id);
			}
			else if (!ulong.TryParse(input, out ulong inputUserID))
			{
				//Unban given just a username
				var bannedUsersWithSameName = bans.ToList().Where(x => x.User.Username.Equals(input)).ToList();
				if (bannedUsersWithSameName.Count > 1)
				{
					//Return a message saying if there are multiple users
					var msg = String.Join("`, `", bannedUsersWithSameName.Select(x => Actions.FormatUser(x.User)));
					await Actions.SendChannelMessage(Context, String.Format("The following users have that name: `{0}`.", msg));
					return;
				}
				else if (bannedUsersWithSameName.Count == 1)
				{
					//Unban the user
					var bannedUser = bannedUsersWithSameName[0].User;
					await Context.Guild.RemoveBanAsync(bannedUser);
					secondHalfOfTheSecondaryMessage = String.Format("unbanned the user `{0}#{1}` with the ID `{2}`.", bannedUser.Username, bannedUser.Discriminator, bannedUser.Id);
				}
				else
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("No user on the ban list has that username."));
					return;
				}
			}
			else
			{
				//Unban given a user ID
				if (Actions.GetUlong(input).Equals(0))
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid user ID."));
					return;
				}

				var bannedUser = bans.FirstOrDefault(x => x.User.Id.Equals(inputUserID)).User;
				await Context.Guild.RemoveBanAsync(bannedUser);
				secondHalfOfTheSecondaryMessage = String.Format("unbanned the user with the ID `{0}`.", inputUserID);
			}
			await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully {0}", secondHalfOfTheSecondaryMessage), 10000);
		}

		[Command("kick")]
		[Alias("k")]
		[Usage("[@User]")]
		[Summary("Kicks the user from the guild.")]
		[PermissionRequirement(1U << (int)GuildPermission.KickMembers)]
		[DefaultEnabled(true)]
		public async Task Kick([Remainder] string input)
		{
			//Test if valid user mention
			var inputUser = await Actions.GetUser(Context.Guild, input);
			if (inputUser == null)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.USER_ERROR));
				return;
			}

			//Get the bot
			var bot = await Actions.GetUser(Context.Guild, Variables.Bot_ID);

			//Determine if the user is allowed to kick this person
			if (!Actions.UserCanBeModifiedByUser(Context, inputUser))
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("User is unable to be kicked by you."));
				return;
			}
			//Determine if the bot can kick this person
			else if (!Actions.UserCanBeModifiedByBot(Context.Guild, inputUser, bot))
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Bot is unable to kick user."));
				return;
			}

			//Kick the targetted user
			await inputUser.KickAsync();
			await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully kicked `{0}#{1}` with the ID `{2}`.",
				inputUser.Username, inputUser.Discriminator, inputUser.Id));
		}

		[Command("currentbanlist")]
		[Alias("cbl")]
		[Usage("")]
		[Summary("Displays all the bans on the guild.")]
		[PermissionRequirement(1U << (int)GuildPermission.BanMembers)]
		[DefaultEnabled(true)]
		public async Task CurrentBanList()
		{
			//Get the bans
			var bans = (await Context.Guild.GetBansAsync()).ToList();

			//Format the ban string
			var description = "";
			var count = 1;
			var lengthForCount = bans.Count.ToString().Length;
			bans.ForEach(x =>
			{
				description += String.Format("`{0}.` `{1}`\n", count++.ToString().PadLeft(lengthForCount, '0'), Actions.FormatUser(x.User));
			});

			//Check the length of the message
			if (String.IsNullOrWhiteSpace(description))
			{
				await Actions.SendChannelMessage(Context, "This guild has no bans.");
			}
			else
			{
				//Make and send the embed
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
			IGuildUser inputUser = null;
			var mentionedUsers = Context.Message.MentionedUserIds;
			if (mentionedUsers.Count == 1)
			{
				inputUser = await Actions.GetUser(Context.Guild, mentionedUsers.FirstOrDefault());
			}
			else if (mentionedUsers.Count > 1)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("More than one user was input."));
				return;
			}

			//Get the channel mention
			var inputChannel = Context.Channel as ITextChannel;
			var mentionedChannels = Context.Message.MentionedChannelIds;
			if (mentionedChannels.Count == 1)
			{
				inputChannel = await Context.Guild.GetTextChannelAsync(mentionedChannels.FirstOrDefault());
			}
			else if (mentionedChannels.Count > 1)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("More than one channel was input."));
				return;
			}

			//Check if the channel that's having messages attempted to be removed on is a log channel
			var serverlogChannel = await Actions.GetLogChannel(Context.Guild, Constants.SERVER_LOG_CHECK_STRING);
			var modlogChannel = await Actions.GetLogChannel(Context.Guild, Constants.MOD_LOG_CHECK_STRING);
			if (Context.User.Id != Context.Guild.OwnerId && (inputChannel == serverlogChannel || inputChannel == modlogChannel))
			{
				//Send a message in the channel
				await Actions.SendChannelMessage(serverlogChannel ?? modlogChannel, String.Format("Hey, @here, {0} is trying to delete stuff.", Context.User.Mention));

				//DM the owner of the server
				await Actions.SendDMMessage(await (await Context.Guild.GetOwnerAsync()).CreateDMChannelAsync(), String.Format("`{0}` is trying to delete stuff from the server/mod log.", Actions.FormatUser(Context.User)));
				return;
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
			if (Context.Channel == inputChannel)
			{
				++requestCount;
			}

			await Actions.RemoveMessages(inputChannel, inputUser, requestCount);
			await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully deleted `{0}` {1}{2}{3}.",
				requestCount,
				requestCount != 1 ? "messages" : "message",
				inputUser == null ? "" : String.Format(" from `{0}`", Actions.FormatUser(inputUser)),
				inputChannel == null ? "" : String.Format(" on `{0}`", Actions.FormatChannel(inputChannel))));
		}

		[Command("slowmode")]
		[Alias("sm")]
		[Usage("<Roles:.../.../> <Messages:1 to 5> <Time:1 to 30> <Guild:Yes> | Off [Guild|Channel|All]")]
		[Summary("The first argument is the roles that get ignored by slowmode, the second is the amount of messages, and the third is the time period. Default is: none, 1, 5." +
			"Bots are unaffected by slowmode. Any users who are immune due to roles stay immune even if they lose said role until a new slowmode is started.")]
		[PermissionRequirement]
		[DefaultEnabled(true)]
		public async Task SlowMode([Optional, Remainder] string input)
		{
			//Split everything
			string[] inputArray = null;
			if (input != null)
			{
				inputArray = input.Split(' ');
			}

			//Check if the input starts with 'off'
			if (inputArray != null && Actions.CaseInsEquals(inputArray[0], "off"))
			{
				if (inputArray.Length != 2)
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR));
				}
				else if (Actions.CaseInsEquals(inputArray[1], "guild"))
				{
					//Remove the guild
					Variables.SlowmodeGuilds.Remove(Context.Guild.Id);

					//Send a success message
					await Actions.MakeAndDeleteSecondaryMessage(Context, "Successfully removed the slowmode on the guild.");
				}
				else if (Actions.CaseInsEquals(inputArray[1], "channel"))
				{
					//Remove the channel
					Variables.SlowmodeChannels.Remove(Context.Channel.Id);

					//Send a success message
					await Actions.MakeAndDeleteSecondaryMessage(Context, "Successfully removed the slowmode on the channel.");
				}
				else if (Actions.CaseInsEquals(inputArray[1], "all"))
				{
					//Remove the guild and every single channel on the guild
					Variables.SlowmodeGuilds.Remove(Context.Guild.Id);
					(await Context.Guild.GetTextChannelsAsync()).ToList().ForEach(channel => Variables.SlowmodeChannels.Remove(channel.Id));

					//Send a success message
					await Actions.MakeAndDeleteSecondaryMessage(Context, "Successfully removed all slowmodes on the guild and its channels.");
				}
				else
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("With off, the second argument must be either Guild, Channel, or All."));
				}
				return;
			}

			//Check if too many args
			if (inputArray != null && inputArray.Length > 4)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Too many arguments. There should not be any spaces between the colons and the variables."));
				return;
			}

			//Initialize the variables
			var roleString = Actions.GetVariable(inputArray, "roles");
			var messageString = Actions.GetVariable(inputArray, "messages");
			var timeString = Actions.GetVariable(inputArray, "time");
			var targetString = Actions.GetVariable(inputArray, "guild");

			//Check if the target is already in either dictionary
			if (targetString == null)
			{
				//Check the channel dictionary
				if (Variables.SlowmodeChannels.ContainsKey(Context.Channel.Id))
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, "Channel already is in slowmode.");
					return;
				}
			}
			else
			{
				//Check the guild dicionary
				if (Variables.SlowmodeGuilds.ContainsKey(Context.Guild.Id))
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, "Guild already is in slowmode.");
					return;
				}
			}

			//Get the roles
			var rolesIDs = new List<ulong>();
			if (roleString != null)
			{
				//Split the string into the role names
				var roleArray = roleString.Split('/').ToList();

				//Get each role name and check if it's a valid role
				roleArray.ForEach(x =>
				{
					var role = Actions.GetRole(Context.Guild, x);
					if (role != null)
					{
						//Add them to the list of roles
						rolesIDs.Add(role.Id);
					}
				});
			}

			//Make a list of the role names
			var roleNames = new List<string>();
			rolesIDs.Distinct().ToList().ForEach(x => roleNames.Add(Context.Guild.GetRole(x).Name));

			//Get the messages limit
			if (int.TryParse(messageString, out int msgsLimit))
			{
				//Check if is a valid number
				if (msgsLimit > 5 || msgsLimit < 1)
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Message limit must be between 1 and 5 inclusive."));
					return;
				}
			}
			else if (messageString != null)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("The input for messages was not a number. Remember: no space after the colon."));
				return;
			}
			else
			{
				msgsLimit = 1;
			}


			//Get the time limit
			if (int.TryParse(timeString, out int timeLimit))
			{
				//Check if is a valid number
				if (timeLimit > 30 || timeLimit < 1)
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Time must be between 1 and 10 inclusive."));
					return;
				}
			}
			else if (timeString != null)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("The input for time was not a number. Remember: no space after the colon."));
				return;
			}
			else
			{
				timeLimit = 5;
			}

			//Add the users into the list with their given messages and if they're affected
			var slowmodeUsers = (await Context.Guild.GetUsersAsync()).Where(x => !x.RoleIds.Intersect(rolesIDs).Any()).Select(x =>
								{ return new SlowmodeUser(x, msgsLimit, msgsLimit, timeLimit); }).ToList();

			//If targetString is null then take that as only the channel and not the guild
			if (targetString == null)
			{
				//Add the channel and list to a dictionary
				Variables.SlowmodeChannels.Add(Context.Channel.Id, slowmodeUsers);
			}
			else
			{
				//Add the guild and list to a dictionary
				Variables.SlowmodeGuilds.Add(Context.Guild.Id, slowmodeUsers);
			}

			//Send a success message
			await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully enabled slowmode on `{0}` with a message limit of `{1}` and time interval of `{2}` seconds.{3}",
				targetString == null ? Context.Channel.Name : Context.Guild.Name, msgsLimit, timeLimit, roleNames.Count == 0 ? "" : String.Format("\nImmune roles: `{0}`.", String.Join("`, `", roleNames))));
		}

		[Command("forallwithrole")]
		[Alias("fawr")]
		[Usage("[Give|Take|Nickname] [Role]/[Role|Nickname] <" + Constants.BYPASS_STRING + ">")]
		[Summary("Only self hosted bots are allowed to go past 100 users per use. When used on a self bot, \"" + Constants.BYPASS_STRING + "\" removes the 100 users limit.")]
		[PermissionRequirement]
		[DefaultEnabled(true)]
		public async Task ForAllWithRole([Remainder] string input)
		{
			//Separating input into the action and role/role or nickname + bypass
			var inputArray = input.Split(new char[] { ' ' }, 2);
			if (inputArray.Length < 2)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR));
				return;
			}

			//Get the action
			var action = inputArray[0];
			if (!Enum.TryParse(action, true, out FAWRType actionEnum))
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ACTION_ERROR));
				return;
			}

			//Separate role/role or nickname + bypass into role and role or nickname + bypass
			var values = inputArray[1].Split('/');
			if (values.Length != 2)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR));
				return;
			}

			//Get the input role
			var inputString = values[0];
			var roleToGather = Actions.GetRole(Context.Guild, inputString);
			if (roleToGather == null)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid role to gather."));
				return;
			}

			//Check if bypass, up the max limit, and remove the bypass string from the values array
			var outputString = values[1];
			var maxLength = 100;
			if (outputString.EndsWith(Constants.BYPASS_STRING))
			{
				maxLength = int.MaxValue;
				outputString = outputString.Substring(0, outputString.Length - Constants.BYPASS_STRING.Length).Trim();
			}

			//Check if the role trying to be given if the role gathered
			if (actionEnum == FAWRType.Give && Actions.CaseInsEquals(inputString, outputString))
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Cannot give the same role that is being gathered."));
				return;
			}
			else if (actionEnum == FAWRType.Nickname)
			{
				if (outputString.Length > Constants.NICKNAME_MAX_LENGTH)
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(String.Format("Nicknames cannot be longer than `{0}` charaters.", Constants.NICKNAME_MAX_LENGTH)));
					return;
				}
				else if (outputString.Length < Constants.NICKNAME_MIN_LENGTH)
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(String.Format("Nicknames cannot be less than `{0}` characters.", Constants.NICKNAME_MIN_LENGTH)));
					return;
				}
			}

			//Get all of the valid users
			var bot = await Actions.GetUser(Context.Guild, Variables.Bot_ID) as IGuildUser;
			var listUsersWithRole = (await Context.Guild.GetUsersAsync()).Where(x =>
			{
				return x.RoleIds.Contains(roleToGather.Id) && Actions.UserCanBeModifiedByUser(Context, x) && Actions.UserCanBeModifiedByBot(Context.Guild, x, bot);
			}).ToList();

			var userCount = listUsersWithRole.Count;
			listUsersWithRole.RemoveRange(Math.Min(maxLength, userCount), Math.Max(userCount - maxLength, 0));
			userCount = listUsersWithRole.Count;
			if (userCount == 0)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Unable to find any users with the input role that could be modified."));
				return;
			}

			//Send a message detailing how many users are being changed and how long it will likely take
			var plurality = userCount != 1 ? "s" : "";
			var time = (int)(userCount * 1.2);
			var msg = await Actions.SendChannelMessage(Context, String.Format("Grabbed `{0}` user{1}. ETA on completion: `{2}` seconds.", userCount, plurality, time)) as IUserMessage;
			var typing = Context.Channel.EnterTypingState();

			var count = 0;
			switch (actionEnum)
			{
				case FAWRType.Give:
				{
					var outputRole = await Actions.GetRoleEditAbility(Context, outputString);
					if (outputRole == null)
						return;
					else if (Context.Guild.EveryoneRole.Id.Equals(outputRole.Id))
					{
						await Actions.MakeAndDeleteSecondaryMessage(Context, "You can't give the `@everyone` role.");
						return;
					}
					Variables.Guilds[Context.Guild.Id].FAWRRoles.Add(outputRole);

					foreach (var user in listUsersWithRole)
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
					await Actions.SendChannelMessage(Context, String.Format("Successfully gave the role `{0}` to `{1}` users.", outputRole.Name, count));
					break;
				}
				case FAWRType.Take:
				{
					var outputRole = await Actions.GetRoleEditAbility(Context, outputString);
					if (outputRole == null)
						return;
					else if (Context.Guild.EveryoneRole.Id.Equals(outputRole.Id))
					{
						await Actions.MakeAndDeleteSecondaryMessage(Context, "You can't take the `@everyone` role.");
						return;
					}
					Variables.Guilds[Context.Guild.Id].FAWRRoles.Add(outputRole);

					foreach (var user in listUsersWithRole)
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
					await Actions.SendChannelMessage(Context, String.Format("Successfully took the role `{0}` from `{1}` users.", outputRole.Name, count));
					break;
				}
				case FAWRType.Nickname:
				{
					Variables.Guilds[Context.Guild.Id].FAWRNicknames.Add(outputString);

					foreach (var user in listUsersWithRole)
					{
						++count;
						if (count % 10 == 0)
						{
							await msg.ModifyAsync(x => x.Content = String.Format("ETA on completion: `{0}` seconds.", (int)((userCount - count) * 1.2)));
						}

						await user.ModifyAsync(x => x.Nickname = outputString);
					}
					await Actions.SendChannelMessage(Context, String.Format("Successfully gave the nickname `{0}` to `{1}` users.", outputString, count));
					break;
				}
			}

			typing.Dispose();
			await msg.DeleteAsync();
		}
	}
}
