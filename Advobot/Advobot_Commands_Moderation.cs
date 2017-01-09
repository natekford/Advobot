using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using Discord;
using Discord.Commands;
using Discord.Modules;
using Discord.WebSocket;
using System.Net;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Advobot
{
	[Name("Moderation")]
	public class Moderation_Commands : ModuleBase
	{
		[Command("fullmute")]
		[Alias("fm")]
		[Usage(Constants.BOT_PREFIX + "fullmute [@User]")]
		[Summary("Removes the user's ability to speak and type via the 'Muted' role.")]
		[PermissionRequirements((1U << (int)GuildPermission.ManageRoles) | (1U << (int)GuildPermission.ManageMessages))]
		public async Task FullMute([Remainder] string input)
		{
			//Check if role already exists, if not, create it
			IRole muteRole = await Actions.createRoleIfNotFound(Context.Guild, Constants.MUTE_ROLE_NAME);
			if (muteRole == null)
				return;

			//See if both the bot and the user can edit/use this role
			if (await Actions.getRoleEditAbility(Context, Constants.MUTE_ROLE_NAME) == null)
				return;

			//Test if valid user mention
			IGuildUser user = await Actions.getUser(Context.Guild, input);
			if (user == null)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.USER_ERROR));
				return;
			}

			//Give the targetted user the role
			await Actions.giveRole(user, muteRole);
			await Actions.makeAndDeleteSecondaryMessage(Context, String.Format("Successfully muted `{0}#{1}`.", user.Username, user.Discriminator));
		}

		[Command("fullunmute")]
		[Alias("fum", "chum")]
		[Usage(Constants.BOT_PREFIX + "fullunmute [@User]")]
		[Summary("Gives the user back the ability to speak and type via removing the 'Muted' role.")]
		[PermissionRequirements((1U << (int)GuildPermission.ManageRoles) | (1U << (int)GuildPermission.ManageMessages))]
		public async Task FullUnmute([Remainder] string input)
		{
			//Test if valid user mention
			IGuildUser user = await Actions.getUser(Context.Guild, input);
			if (user == null)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.USER_ERROR));
				return;
			}

			//Remove the role
			await Actions.takeRole(user, Actions.getRole(Context.Guild, Constants.MUTE_ROLE_NAME));
			await Actions.makeAndDeleteSecondaryMessage(Context, String.Format("Successfully unmuted `{0}#{1}`.", user.Username, user.Discriminator));
		}

		[Command("kick")]
		[Alias("k")]
		[Usage(Constants.BOT_PREFIX + "kick [@User]")]
		[Summary("Kicks the user from the guild.")]
		[PermissionRequirements(1U << (int)GuildPermission.KickMembers)]
		public async Task Kick([Remainder] string input)
		{
			//Test if valid user mention
			IGuildUser inputUser = await Actions.getUser(Context.Guild, input);
			if (inputUser == null)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.USER_ERROR));
				return;
			}

			//Determine if the user is allowed to kick this person
			int kickerPosition = Actions.getPosition(Context.Guild, Context.User as IGuildUser);
			int kickeePosition = Actions.getPosition(Context.Guild, inputUser);
			if (kickerPosition <= kickeePosition)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("User is unable to be kicked by you."));
				return;
			}

			//Determine if the bot can kick this person
			if (Actions.getPosition(Context.Guild, await Context.Guild.GetUserAsync(Variables.Bot_ID)) <= kickeePosition)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Bot is unable to kick user."));
				return;
			}

			//Kick the targetted user
			await inputUser.KickAsync();
			await Actions.makeAndDeleteSecondaryMessage(Context, String.Format("Successfully kicked `{0}#{1}` with the ID `{2}`.",
				inputUser.Username, inputUser.Discriminator, inputUser.Id));
		}

		[Command("prunemembers")]
		[Alias("pmems")]
		[Usage(Constants.BOT_PREFIX + "prunemembers [1|7|30] <Real>")]
		[Summary("Removes users who have no roles and have not been seen in the past given amount of days. These users can rejoin via instant invites.\n" +
			"Real means an actual prune, otherwise this returns the number of users that would have been pruned.")]
		[PermissionRequirements]
		public async Task PruneMembers([Remainder] string input)
		{
			//Split into the ints and 'bool'
			string[] inputArray = input.Split(new char[] { ' ' }, 2);
			int[] validDays = { 1, 7, 30 };

			//Get the int
			int amountOfDays = 0;
			if (!int.TryParse(inputArray[0], out amountOfDays))
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Not a number."));
				return;
			}
			else if (!validDays.Contains(amountOfDays))
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Not a valid number of days."));
				return;
			}

			//If only one argument and it's an int then it's a simulation
			if (inputArray.Length != 2)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, String.Format("`{0}` members would be pruned with a prune period of `{1}` days.",
					await Context.Guild.PruneUsersAsync(amountOfDays, true), amountOfDays));
			}
			//Otherwise test if it's the real deal
			else if(inputArray[1].ToLower().Equals("real"))
			{
				await Context.Guild.PruneUsersAsync(amountOfDays);
			}
			else
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("You got a valid number, but you put some gibberish at the end."));
			}
		}

		[Command("softban")]
		[Alias("sb")]
		[Usage(Constants.BOT_PREFIX + "softban [@User]")]
		[Summary("Bans then unbans a user from the guild.")]
		[PermissionRequirements(1U << (int)GuildPermission.BanMembers)]
		public async Task SoftBan([Remainder] string input)
		{
			//Test if valid user mention
			IGuildUser inputUser = await Actions.getUser(Context.Guild, input);
			if (inputUser == null)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.USER_ERROR));
				return;
			}

			//Determine if the user is allowed to softban this person
			int sberPosition = Actions.getPosition(Context.Guild, Context.User as IGuildUser);
			int sbeePosition = Actions.getPosition(Context.Guild, inputUser);
			if (sberPosition <= sbeePosition)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("User is unable to be soft-banned by you."));
				return;
			}

			//Determine if the bot can softban this person
			if (Actions.getPosition(Context.Guild, await Context.Guild.GetUserAsync(Variables.Bot_ID)) <= sbeePosition)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Bot is unable to soft-ban user."));
				return;
			}

			//Softban the targetted use
			await Context.Guild.AddBanAsync(inputUser);
			await Context.Guild.RemoveBanAsync(inputUser);
			await Actions.makeAndDeleteSecondaryMessage(Context, String.Format("Successfully banned and unbanned `{0}#{1}`.", inputUser.Username, inputUser.Discriminator));
		}

		[Command("ban")]
		[Alias("b")]
		[Usage(Constants.BOT_PREFIX + "ban [@User]")]
		[Summary("Bans the user from the guild.")]
		[PermissionRequirements(1U << (int)GuildPermission.BanMembers)]
		public async Task Ban([Remainder] string input)
		{

			//Test number of arguments
			string[] values = input.Split(' ');
			if (values.Length != 2)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR));
				return;
			}

			//Test if valid user mention
			IGuildUser inputUser = null;
			if (values[0].StartsWith("<@"))
			{
				inputUser = await Actions.getUser(Context.Guild, values[0]);
			}
			else if (Actions.getUlong(values[0]) != 0)
			{
				inputUser = await Context.Guild.GetUserAsync(Actions.getUlong(values[0]));
			}

			if (inputUser == null)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.USER_ERROR));
				return;
			}

			//Determine if the user is allowed to ban this person
			int bannerPosition = Actions.getPosition(Context.Guild, Context.User as IGuildUser);
			int banneePosition = Actions.getPosition(Context.Guild, inputUser);
			if (bannerPosition <= banneePosition)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("User is unable to be banned by you."));
				return;
			}

			//Determine if the bot can ban this person
			if (Actions.getPosition(Context.Guild, await Context.Guild.GetUserAsync(Variables.Bot_ID)) <= banneePosition)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Bot is unable to ban user."));
				return;
			}

			//Checking for valid days requested
			int pruneDays = 0;
			if (values.Length == 2 && !Int32.TryParse(values[1], out pruneDays))
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Incorrect input for days of messages to be deleted."));
				return;
			}

			//Forming the second half of the string that prints out when a user is successfully banned
			string plurality = "days";
			if (pruneDays == 1)
			{
				plurality = "day";
			}
			string latterHalfOfString = "";
			if (pruneDays > 0)
			{
				latterHalfOfString = String.Format(", and deleted {0} {1} worth of messages.", pruneDays, plurality);
			}
			else if (pruneDays == 0)
			{
				latterHalfOfString = ".";
			}

			//Ban the user
			await Context.Guild.AddBanAsync(inputUser, pruneDays);
			await Actions.makeAndDeleteSecondaryMessage(Context, String.Format("Successfully banned `{0}#{1}` with the ID `{2}`{3}",
				inputUser.Username, inputUser.Discriminator, inputUser.Id, latterHalfOfString), 10000);
		}

		[Command("unban")]
		[Alias("ub")]
		[Usage(Constants.BOT_PREFIX + "unban [User|User#Discriminator|User ID]")]
		[Summary("Unbans the user from the guild.")]
		[PermissionRequirements(1U << (int)GuildPermission.BanMembers)]
		public async Task Unban([Remainder] string input)
		{
			//Cut the user mention into the username and the discriminator
			string[] values = input.Split('#');
			if (values.Length != 2)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR));
				return;
			}

			//Get a list of the bans
			var bans = await Context.Guild.GetBansAsync();

			//Get their name and discriminator or ulong
			ulong inputUserID;
			string secondHalfOfTheSecondaryMessage = "";
			
			if (values.Length == 2)
			{
				//Unban given a username and discriminator
				ushort discriminator;
				if (!ushort.TryParse(values[1], out discriminator))
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid discriminator provided."));
				}
				string username = values[0].Replace("@", "");

				//Get a list of users with the name username and discriminator
				List<IBan> bannedUserWithNameAndDiscriminator = bans.ToList().Where(x => x.User.Username.Equals(username) && x.User.Discriminator.Equals(discriminator)).ToList();

				//Unban the user
				IUser bannedUser = bannedUserWithNameAndDiscriminator[0].User;
				if (!Variables.UnbannedUsers.ContainsKey(bannedUser.Id))
				{
					//Add the user to the unban dictionary
					Variables.UnbannedUsers.Add(bannedUser.Id, bannedUser);
				}

				await Context.Guild.RemoveBanAsync(bannedUser);
				secondHalfOfTheSecondaryMessage = String.Format("unbanned the user `{0}#{1}` with the ID `{2}`.", bannedUser.Username, bannedUser.Discriminator, bannedUser.Id);
			}
			else if (!ulong.TryParse(input, out inputUserID))
			{
				//Unban given just a username
				List<IBan> bannedUsersWithSameName = bans.ToList().Where(x => x.User.Username.Equals(input)).ToList();
				if (bannedUsersWithSameName.Count > 1)
				{
					//Return a message saying if there are multiple users
					await Actions.sendChannelMessage(Context.Channel, String.Format("The following users have that name: `{0}`.", String.Join("`, `", bannedUsersWithSameName)));
					return;
				}
				else if (bannedUsersWithSameName.Count == 1)
				{
					//Unban the user
					IUser bannedUser = bannedUsersWithSameName[0].User;
					if (!Variables.UnbannedUsers.ContainsKey(bannedUser.Id))
					{
						//Add the user to the unban dictionary
						Variables.UnbannedUsers.Add(bannedUser.Id, bannedUser);
					}

					await Context.Guild.RemoveBanAsync(bannedUser);
					secondHalfOfTheSecondaryMessage = String.Format("unbanned the user `{0}#{1}` with the ID `{2}`.", bannedUser.Username, bannedUser.Discriminator, bannedUser.Id);
				}
				else
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("No user on the ban list has that username."));
					return;
				}
			}
			else
			{
				//Unban given a user ID
				if (Actions.getUlong(input).Equals(0))
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid user ID."));
					return;
				}

				IUser bannedUser = bans.FirstOrDefault(x => x.User.Id.Equals(inputUserID)).User;
				if (!Variables.UnbannedUsers.ContainsKey(bannedUser.Id))
				{
					//Add the user to the unban dictionary
					Variables.UnbannedUsers.Add(bannedUser.Id, bannedUser);
				}

				await Context.Guild.RemoveBanAsync(bannedUser);
				secondHalfOfTheSecondaryMessage = String.Format("unbanned the user with the ID `{0}`.", inputUserID);
			}
			await Actions.makeAndDeleteSecondaryMessage(Context, String.Format("Successfully {0}", secondHalfOfTheSecondaryMessage), 10000);
		}

		[Command("currentbanlist")]
		[Alias("cbl")]
		[Usage(Constants.BOT_PREFIX + "currentbanlist")]
		[Summary("Displays all the bans on the guild.")]
		[PermissionRequirements(1U << (int)GuildPermission.BanMembers)]
		public async Task CurrentBanList()
		{
			//Get and add the ban information
			var bans = await Context.Guild.GetBansAsync();

			//Initialize the ban string
			string description = "";
			
			//Initialize the count
			int count = 0;

			//Add everything to the string
			bans.ToList().ForEach(x =>
			{
				count++;
				description += String.Format("`{0}.` `{1}#{2}` ID: `{3}`\n", count.ToString("00"), x.User.Username, x.User.Discriminator, x.User.Id);
			});

			//Check the length of the message
			if (description.Length > Constants.SHORT_LENGTH_CHECK)
			{
				if (!Constants.TEXT_FILE)
				{
					string hastebin = Actions.uploadToHastebin(Actions.replaceMessageCharacters(description));
					await Actions.sendEmbedMessage(Context.Channel, Actions.makeNewEmbed(title: "Current Bans", description: hastebin));
				}
				else
				{
					await Actions.uploadTextFile(Context.Guild, Context.Channel, Actions.replaceMessageCharacters(description), "Current_Ban_List_", "Current Bans");
				}
				return;
			}
			else if (bans.Count == 0)
			{
				await Actions.sendChannelMessage(Context.Channel, "This guild has no bans.");
				return;
			}

			//Make and send the embed
			await Actions.sendEmbedMessage(Context.Channel, Actions.makeNewEmbed(null, "Current Bans", description));
		}

		[Command("removemessages")]
		[Alias("rm")]
		[Usage(Constants.BOT_PREFIX + "removemessages <@User> <#Channel> [Number of Messages]")]
		[Summary("Removes the selected number of messages from either the user, the channel, both, or, if neither is input, the current channel." +
			"People without administrator can only delete up to 100 messages at a time.")]
		[PermissionRequirements(1U << (int)GuildPermission.ManageMessages)]
		public async Task RemoveMessages([Remainder] string input)
		{
			string[] values = input.Split(' ');
			if (values.Length < 1 || values.Length > 3)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR));
				return;
			}

			int argIndex = 0;
			int argCount = values.Length;

			//Testing if starts with user mention
			IGuildUser inputUser = null;
			if (argIndex < argCount && values[argIndex].StartsWith("<@"))
			{
				inputUser = await Actions.getUser(Context.Guild, values[argIndex]);
				if (inputUser == null)
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.USER_ERROR));
					return;
				}
				++argIndex;
			}

			//Testing if starts with channel mention
			ITextChannel inputChannel = Context.Channel as ITextChannel;
			if (argIndex < argCount && values[argIndex].StartsWith("<#"))
			{
				inputChannel = (await Actions.getChannelID(Context.Guild, values[argIndex]) as ITextChannel);
				if (inputChannel == null)
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.CHANNEL_ERROR));
					return;
				}
				++argIndex;
			}

			//Check if the channel that's having messages attempted to be removed on is a log channel
			ITextChannel serverlogChannel = await Actions.logChannelCheck(Context.Guild, Constants.SERVER_LOG_CHECK_STRING);
			ITextChannel modlogChannel = await Actions.logChannelCheck(Context.Guild, Constants.MOD_LOG_CHECK_STRING);
			if (Context.User.Id != Context.Guild.OwnerId && (inputChannel == serverlogChannel || inputChannel == modlogChannel))
			{
				//Send a message in the channel
				await Actions.sendChannelMessage(serverlogChannel == null ? modlogChannel : serverlogChannel,
					String.Format("Hey, @here, {0} is trying to delete stuff.", Context.User.Mention));

				//DM the owner of the server
				await (await (await Context.Guild.GetOwnerAsync()).CreateDMChannelAsync()).SendMessageAsync(
					String.Format("`{0}#{1}` ID: `{2}` is trying to delete stuff from the server/mod log.", Context.User.Username, Context.User.Discriminator, Context.User.Id));
				return;
			}

			//Checking for valid request count
			int requestCount = (argIndex == argCount - 1) ? Actions.getInteger(values[argIndex]) : -1;
			if (requestCount < 1)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Incorrect input for number of messages to be removed."));
				return;
			}

			//See if the user is trying to delete more than 100 messages at a time
			if (requestCount > 100 && !(await Context.Guild.GetUserAsync(Context.User.Id)).GuildPermissions.Administrator)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("You need administrator to remove more than 100 messages at a time."));
				return;
			}

			//Removing the command message itself
			if (Context.Channel != inputChannel)
			{
				await Actions.removeMessages(Context.Channel, 0);
			}
			else if ((Context.User == inputUser) && (Context.Channel == inputChannel))
			{
				++requestCount;
			}

			await Actions.removeMessages(inputChannel, requestCount, inputUser);
			await Actions.makeAndDeleteSecondaryMessage(Context, String.Format("Successfully deleted `{0}` {1}{2}{3}.",
				requestCount,
				requestCount > 1 ? "messages" : "message",
				inputUser == null ? "" : " from `" + inputUser.Username + "#" + inputUser.Discriminator + "`",
				inputChannel == null ? "" : " on `#" + inputChannel.Name + "`"));
		}

		[Command("slowmode")]
		[Alias("sm")]
		[Usage(Constants.BOT_PREFIX + "slowmode <Roles:.../.../> <Messages:1 to 5> <Time:1 to 30> <Guild:Yes> | Off [Guild|Channel|All]")]
		[Summary("The first argument is the roles that get ignored by slowmode, the second is the amount of messages, and the third is the time period. Default is: none, 1, 5." +
			"Say `" + Constants.BOT_PREFIX + "slowmode Off` to turn off all slowmodes on the guild. Bots are unaffected by slowmode.")]
		[PermissionRequirements]
		public async Task SlowMode([Optional, Remainder] string input)
		{
			//Split everything
			string[] inputArray = null;
			if (input != null)
			{
				inputArray = input.Split(' ');
			}

			//Check if the input starts with 'off'
			if (inputArray != null && inputArray[0].ToLower().Equals("off"))
			{
				if (inputArray.Length != 2)
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR));
				}
				else if (inputArray[1].ToLower().Equals("guild"))
				{
					//Remove the guild
					Variables.SlowmodeGuilds.Remove(Context.Guild.Id);

					//Send a success message
					await Actions.makeAndDeleteSecondaryMessage(Context, "Successfully removed the slowmode on the guild.");
				}
				else if (inputArray[1].ToLower().Equals("channel"))
				{
					//Remove the channel
					Variables.SlowmodeChannels.Remove(Context.Channel as IGuildChannel);

					//Send a success message
					await Actions.makeAndDeleteSecondaryMessage(Context, "Successfully removed the slowmode on the channel.");
				}
				else if (inputArray[1].ToLower().Equals("all"))
				{
					//Remove the guild and every single channel on the guild
					Variables.SlowmodeGuilds.Remove(Context.Guild.Id);
					(await Context.Guild.GetTextChannelsAsync()).ToList().ForEach(channel => Variables.SlowmodeChannels.Remove(channel as IGuildChannel));

					//Send a success message
					await Actions.makeAndDeleteSecondaryMessage(Context, "Successfully removed all slowmodes on the guild and its channels.");
				}
				else
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("With off, the second argument must be either Guild, Channel, or All."));
				}
				return;
			}

			//Check if too many args
			if (inputArray != null && inputArray.Length > 4)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Too many arguments. There are no spaces between the colons and the variables."));
				return;
			}

			//Initialize the variables
			string roleString = Actions.getVariable(inputArray, "roles");
			string messageString = Actions.getVariable(inputArray, "messages");
			string timeString = Actions.getVariable(inputArray, "time");
			string targetString = Actions.getVariable(inputArray, "guild");

			//Check if the target is already in either dictionary
			if (targetString == null)
			{
				//Check the channel dictionary
				if (Variables.SlowmodeChannels.ContainsKey(Context.Channel as IGuildChannel))
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, "Channel already is in slowmode.");
					return;
				}
			}
			else
			{
				//Check the guild dicionary
				if (Variables.SlowmodeGuilds.ContainsKey(Context.Guild.Id))
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, "Guild already is in slowmode.");
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
					IRole role = Actions.getRole(Context.Guild, x);
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
			int msgsLimit = 1;
			//Check if is a number
			if (messageString != null && !int.TryParse(messageString, out msgsLimit))
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("The input for messages was not a number. Remember: no space after the colon."));
				return;
			}
			//Check if is a valid number
			if (msgsLimit > 5 || msgsLimit < 1)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Message limit must be between 1 and 5 inclusive."));
				return;
			}

			//Get the time limit
			int timeLimit = 5;
			//Check if is a number
			if (timeString != null && !int.TryParse(timeString, out timeLimit))
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("The input for time was not a number. Remember: no space after the colon."));
				return;
			}
			//Check if is a valid number
			if (timeLimit > 30 || timeLimit < 1)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Time must be between 1 and 10 inclusive."));
				return;
			}

			//Add the users into the list with their given messages and if they're affected
			var slowmodeUsers = new List<SlowmodeUser>();
			(await Context.Guild.GetUsersAsync()).ToList().ForEach(x =>
			{
				//If they have any role ids that are on the role list then they're immune
				if (!x.RoleIds.ToList().Intersect(rolesIDs).Any())
				{
					slowmodeUsers.Add(new SlowmodeUser(x, msgsLimit, msgsLimit, timeLimit));
				}
			});

			//If targetString is null then take that as only the channel and not the guild
			if (targetString == null)
			{
				//Add the channel and list to a dictionary
				Variables.SlowmodeChannels.Add(Context.Channel as IGuildChannel, slowmodeUsers);
			}
			else
			{
				//Add the guild and list to a dictionary
				Variables.SlowmodeGuilds.Add(Context.Guild.Id, slowmodeUsers);
			}

			//Send a success message
			await Actions.makeAndDeleteSecondaryMessage(Context, String.Format("Successfully enabled slowmode on `{0}` with a message limit of `{1}` and time interval of `{2}`.{3}",
				targetString == null ? Context.Channel.Name : Context.Guild.Name,
				msgsLimit,
				timeLimit,
				roleNames.Count == 0 ? "" : String.Format("\nImmune roles: `{0}`.", String.Join("`, `", roleNames))));
		}

		[Command("giverole")]
		[Alias("gr")]
		[Usage(Constants.BOT_PREFIX + "giverole [@User] [Role]/<Role>/...")]
		[Summary("Gives the user the role (assuming the person using the command and bot both have the ability to give that role).")]
		[PermissionRequirements(1U << (int)GuildPermission.ManageRoles)]
		public async Task GiveRole([Remainder] string input)
		{
			//Test number of arguments
			string[] values = input.Split(new char[] { ' ' }, 2);
			if (values.Length != 2)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR));
				return;
			}

			//Test if valid user mention
			IGuildUser inputUser = await Actions.getUser(Context.Guild, values[0]);
			if (inputUser == null)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.USER_ERROR));
				return;
			}

			//Determine if the role exists and if it is able to be edited by both the bot and the user
			List<string> inputRoles = values[1].Split('/').ToList();
			if (inputRoles.Count == 1)
			{
				//Check if it actually exists
				IRole role = await Actions.getRoleEditAbility(Context, inputRoles[0]);
				if (role == null)
					return;

				//See if the role is unable to be given due to management
				if (role.IsManaged)
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Role is managed and unable to be given."));
					return;
				}

				//Check if trying to delete the everyone role
				if (role == Context.Guild.EveryoneRole)
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Unable to give the everyone role."));
					return;
				}

				//Give the role and make a message
				await Actions.giveRole(inputUser, role);
				await Actions.makeAndDeleteSecondaryMessage(Context,
					String.Format("Successfully gave `{0}#{1}` the `{2}` role.", inputUser.Username, inputUser.Discriminator, role));
			}
			else
			{
				List<string> failedRoles = new List<string>();
				List<string> succeededRoles = new List<string>();
				List<IRole> roles = new List<IRole>();
				foreach (string roleName in inputRoles)
				{
					IRole role = await Actions.getRoleEditAbility(Context, roleName, true);
					if (role == null || role.IsManaged || role == Context.Guild.EveryoneRole)
					{
						failedRoles.Add(roleName);
					}
					else
					{
						roles.Add(role);
						succeededRoles.Add(roleName);
					}
				}

				//Format the success message
				string succeed = "";
				if (succeededRoles.Count > 0)
				{
					succeed = String.Format("Successfully gave `{0}#{1}` the `{2}` role{3}",
						inputUser.Username,
						inputUser.Discriminator,
						String.Join(", ", succeededRoles),
						succeededRoles.Count > 1 ? "s" : "");
				}
				//Check if an and is needed
				string and = ".";
				if (succeededRoles.Count > 0 && failedRoles.Count > 0)
				{
					and = " and ";
				}
				//Format the fail message
				string failed = "";
				if (failedRoles.Count > 0)
				{
					failed = String.Format("{0}ailed to give{1} the `{2}` role{3}.",
						String.IsNullOrEmpty(succeed) ? "F" : "f",
						String.IsNullOrEmpty(succeed) ? String.Format(" `{0}#{1}`", inputUser.Username, inputUser.Discriminator) : "",
						String.Join(", ", failedRoles),
						failedRoles.Count > 1 ? "s" : "");
				}

				await Actions.giveRole(inputUser, roles.ToArray());
				await Actions.makeAndDeleteSecondaryMessage(Context, succeed + and + failed);
			}
		}

		[Command("takerole")]
		[Alias("tr")]
		[Usage(Constants.BOT_PREFIX + "takerole [@User] [Role]")]
		[Summary("Take the role from the user (assuming the person using the command and bot both have the ability to take that role).")]
		[PermissionRequirements(1U << (int)GuildPermission.ManageRoles)]
		public async Task TakeRole([Remainder] string input)
		{
			//Test number of arguments
			string[] values = input.Split(new char[] { ' ' }, 2);
			if (values.Length != 2)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR));
				return;
			}

			//Test if valid user mention
			IGuildUser inputUser = await Actions.getUser(Context.Guild, values[0]);
			if (inputUser == null)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.USER_ERROR));
				return;
			}

			//Determine if the role exists and if it is able to be edited by both the bot and the user
			List<string> inputRoles = values[1].Split('/').ToList();
			if (inputRoles.Count == 1)
			{
				//Check if it actually exists
				IRole role = await Actions.getRoleEditAbility(Context, inputRoles[0]);
				if (role == null)
					return;

				//See if the role is unable to be taken due to management
				if (role.IsManaged)
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Role is managed and unable to be taken."));
					return;
				}

				//Check if trying to delete the everyone role
				if (role == Context.Guild.EveryoneRole)
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Unable to take the everyone role."));
					return;
				}

				//Take the role and make a message
				await Actions.takeRole(inputUser, role);
				await Actions.makeAndDeleteSecondaryMessage(Context,
					String.Format("Successfully took `{0}` from `{1}#{2}`.", role, inputUser.Username, inputUser.Discriminator));
			}
			else
			{
				List<string> failedRoles = new List<string>();
				List<string> succeededRoles = new List<string>();
				List<IRole> roles = new List<IRole>();
				foreach (string roleName in inputRoles)
				{
					IRole role = await Actions.getRoleEditAbility(Context, roleName, true);
					if (role == null || role.IsManaged || role == Context.Guild.EveryoneRole)
					{
						failedRoles.Add(roleName);
					}
					else
					{
						roles.Add(role);
						succeededRoles.Add(roleName);
					}
				}

				//Format the success message
				string succeed = "";
				if (succeededRoles.Count > 0)
				{
					succeed = String.Format("Successfully took the `{0}` role{1} from `{2}#{3}`",
						String.Join(", ", succeededRoles), succeededRoles.Count > 1 ? "s" : "", inputUser.Username, inputUser.Discriminator);
				}
				//Check if an and is needed
				string and = ".";
				if (succeededRoles.Count > 0 && failedRoles.Count > 0)
				{
					and = " and ";
				}
				//Format the fail message
				string failed = "";
				if (failedRoles.Count > 0)
				{
					failed = String.Format("{0}ailed to take the `{1}` role{2}{3}.",
						String.IsNullOrEmpty(succeed) ? "F" : "f", String.Join(", ", failedRoles), failedRoles.Count > 1 ? "s" : "",
						String.IsNullOrEmpty(succeed) ? String.Format(" from `{0}#{1}`", inputUser.Username, inputUser.Discriminator) : "");
				}

				await Actions.takeRole(inputUser, roles.ToArray());
				await Actions.makeAndDeleteSecondaryMessage(Context, succeed + and + failed);
			}
		}

		[Command("createrole")]
		[Alias("cr")]
		[Usage(Constants.BOT_PREFIX + "createrole [Role]")]
		[Summary("Adds a role to the guild with the chosen name.")]
		[PermissionRequirements(1U << (int)GuildPermission.ManageRoles)]
		public async Task CreateRole([Remainder] string input)
		{
			//Check length
			if (input.Length > Constants.ROLE_NAME_LENGTH)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Roles can only have a name length of up to 32 characters."));
				return;
			}

			//Create role
			await Context.Guild.CreateRoleAsync(input, new GuildPermissions(0));
			await Actions.makeAndDeleteSecondaryMessage(Context, String.Format("Successfully created the `{0}` role.", input));
		}

		[Command("softdeleterole")]
		[Alias("sdrole", "sdr")]
		[Usage(Constants.BOT_PREFIX + "softdeleterole [Role]")]
		[Summary("Removes all permissions from a role (and all channels the role had permissions on) and removes the role from everyone. Leaves the name and color behind.")]
		[PermissionRequirements(1U << (int)GuildPermission.ManageRoles)]
		public async Task SoftDeleteRole([Remainder] string input)
		{
			//Determine if the role exists and if it is able to be edited by both the bot and the user
			IRole inputRole = await Actions.getRoleEditAbility(Context, input);
			if (inputRole == null)
				return;

			//Check if even removable
			if (inputRole.IsManaged)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Role is managed and unable to be softdeleted."));
				return;
			}

			//Check if trying to delete the everyone role
			if (inputRole == Context.Guild.EveryoneRole)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Unable to softdelete the everyone role."));
				return;
			}

			//TODO: Fix this
			//Create a new role with the same attributes (including space) and no perms
			IRole newRole = await Context.Guild.CreateRoleAsync(inputRole.Name, new GuildPermissions(0), inputRole.Color);

			//Make a new list of IRole
			var roles = new List<IRole>();
			//Grab all roles that aren't the targeted one
			Context.Guild.Roles.Where(x => x != newRole).ToList().ForEach(x => roles.Add(x));
			//Sort the list by position
			roles = roles.OrderBy(x => x.Position).ToList();
			//Add in the targetted role with the given position
			roles.Insert(Math.Min(roles.Count(), inputRole.Position), newRole);

			//Make a new list of BulkRoleProperties
			var listOfBulk = new List<BulkRoleProperties>();
			//Add the role's IDs and positions into it
			roles.ForEach(x => listOfBulk.Add(new BulkRoleProperties(x.Id)));
			//Readd the positions to it
			listOfBulk.ForEach(x => x.Position = listOfBulk.IndexOf(x));
			//Mass modify the roles with the list having the correct positions
			await Context.Guild.ModifyRolesAsync(listOfBulk);

			//Delete the old role
			await inputRole.DeleteAsync();
			await Actions.makeAndDeleteSecondaryMessage(Context,
				String.Format("Successfully removed all permissions from `{0}` and removed the role from all users on the guild.", inputRole.Name));
		}

		[Command("deleterole")]
		[Alias("drole", "dr")]
		[Usage(Constants.BOT_PREFIX + "deleterole [Role]")]
		[Summary("Deletes the role. 'Drole' is a pretty funny alias.")]
		[PermissionRequirements(1U << (int)GuildPermission.ManageRoles)]
		public async Task DeleteRole([Remainder] string input)
		{
			//Determine if the role exists and if it is able to be edited by both the bot and the user
			IRole role = await Actions.getRoleEditAbility(Context, input);
			if (role == null)
				return;

			//Check if even removable
			if (role.IsManaged)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Role is managed and unable to be deleted."));
				return;
			}

			//Check if trying to delete the everyone role
			if (role == Context.Guild.EveryoneRole)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Unable to delete the everyone role."));
				return;
			}

			await role.DeleteAsync();
			await Actions.makeAndDeleteSecondaryMessage(Context, String.Format("Successfully deleted the `{0}` role.", input));
		}

		[Command("roleposition")]
		[Alias("rpos")]
		[Usage(Constants.BOT_PREFIX + "roleposition [Role] [New Position]")]
		[Summary("Moves the role to the given position. @ev" + Constants.ZERO_LENGTH_CHAR + "eryone is the first position and starts at zero.")]
		[PermissionRequirements(1U << (int)GuildPermission.ManageRoles)]
		public async Task RolePosition([Remainder] string input)
		{
			//Get the role
			IRole role = await Actions.getRoleEditAbility(Context, input.Substring(0, input.LastIndexOf(' ')), true);
			if (role == null)
				return;

			//Check if it's the everyone role
			if (role == Context.Guild.EveryoneRole)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Unable to move the everyone role."));
				return;
			}

			//Get the position as an int
			int position = 0;
			if (!int.TryParse(input.Substring(input.LastIndexOf(' ')), out position))
			{
				await Actions.sendChannelMessage(Context.Channel, String.Format("The `{0}` role has a position of `{1}`.", role.Name, role.Position));
				return;
			}

			//Checking if valid positions
			int maxPos = 0;
			Context.Guild.Roles.ToList().ForEach(x => maxPos = Math.Max(maxPos, x.Position));
			if (position <= 0)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Cannot set a role to a position lower than or equal to one."));
				return;
			}
			else if (position > maxPos)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Cannot set a role to a position higher than the highest role."));
				return;
			}

			//See if the user can access that position
			if (position > Actions.getPosition(Context.Guild, Context.User as IGuildUser))
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Position is higher than you can access."));
				return;
			}
			//See if the bot can access that position
			if (position > Actions.getPosition(Context.Guild, await Context.Guild.GetUserAsync(Variables.Bot_ID)))
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Position is higher than the bot can access."));
				return;
			}

			//Make a new list of IRole
			var roles = new List<IRole>();
			//Grab all roles that aren't the targeted one
			Context.Guild.Roles.Where(x => x != role).ToList().ForEach(x => roles.Add(x));
			//Sort the list by position
			roles = roles.OrderBy(x => x.Position).ToList();
			//Add in the targetted role with the given position
			roles.Insert(Math.Min(roles.Count(), position), role);

			//Make a new list of BulkRoleProperties
			var listOfBulk = new List<BulkRoleProperties>();
			//Add the role's IDs and positions into it
			roles.ForEach(x => listOfBulk.Add(new BulkRoleProperties(x.Id)));
			//Readd the positions to it
			listOfBulk.ForEach(x => x.Position = listOfBulk.IndexOf(x));
			//Mass modify the roles with the list having the correct positions
			await Context.Guild.ModifyRolesAsync(listOfBulk);

			//Send a message stating what position the channel was sent to
			await Actions.sendChannelMessage(Context.Channel, String.Format("Successfully gave the `{0}` role the position `{1}`.", role.Name, roles.IndexOf(role)));
		}

		[Command("listrolepositions")]
		[Alias("lrp")]
		[Usage(Constants.BOT_PREFIX + "listrolepositions")]
		[Summary("Lists the positions of each role on the guild.")]
		[PermissionRequirements(1U << (int)GuildPermission.ManageRoles)]
		public async Task ListRolePositions()
		{
			//List of the roles
			var roles = Context.Guild.Roles.OrderBy(x => x.Position).Reverse().ToList();

			//Put them into strings now
			string description = "";
			foreach (var role in roles)
			{
				if (role == Context.Guild.EveryoneRole)
				{
					description += "`" + role.Position.ToString("00") + ".` @ev" + Constants.ZERO_LENGTH_CHAR + "eryone";
					continue;
				}
				description += "`" + role.Position.ToString("00") + ".` " + role.Name + "\n";
			}

			//Check the length to see if the message can be sent
			if (description.Length > Constants.SHORT_LENGTH_CHECK)
			{
				description = Actions.uploadToHastebin(Actions.replaceMessageCharacters(description));
			}

			//Send the embed
			await Actions.sendEmbedMessage(Context.Channel, Actions.makeNewEmbed(null, "Role Positions", description));
		}

		[Command("rolepermissions")]
		[Alias("erp", "rp")]
		[Usage(Constants.BOT_PREFIX + "rolepermissions [Show|Add|Remove] [Role] [Permission/...]")]
		[Summary("Add/remove the selected permissions to/from the role. Permissions must be separated by a `/`! " +
			"Type `" + Constants.BOT_PREFIX + "rolepermissions [Show]` to see the available permissions. " +
			"Type `" + Constants.BOT_PREFIX + "rolepermissions [Show] [Role]` to see the permissions of that role.")]
		[PermissionRequirements(1U << (int)GuildPermission.ManageRoles)]
		public async Task RolePermissions([Remainder] string input)
		{
			//Set the permission types into a list to later check against
			List<string> permissionTypeStrings = Variables.PermissionNames.Values.ToList();

			string[] actionRolePerms = input.ToLower().Split(new char[] { ' ' }, 2); //Separate the role and whether to add or remove from the permissions
			string permsString = null; //Set placeholder perms variable
			string roleString = null; //Set placeholder role variable
			bool show = false; //Set show bool

			//If the user wants to see the permission types, print them out
			if (input.Equals("show", StringComparison.OrdinalIgnoreCase))
			{
				await Actions.sendEmbedMessage(Context.Channel, Actions.makeNewEmbed(null, "Role Permissions", String.Join("\n", permissionTypeStrings)));
				return;
			}
			//If something is said after show, take that as a role.
			else if (input.StartsWith("show", StringComparison.OrdinalIgnoreCase))
			{
				roleString = input.Substring("show".Length).Trim();
				show = true;
			}
			//If show is not input, take the stuff being said as a role and perms
			else
			{
				if (actionRolePerms.Length == 1)
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR));
					return;
				}
				int lastSpace = actionRolePerms[1].LastIndexOf(' ');
				if (lastSpace <= 0)
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR));
					return;
				}
				//Separate out the permissions
				permsString = actionRolePerms[1].Substring(lastSpace).Trim();
				//Separate out the role
				roleString = actionRolePerms[1].Substring(0, lastSpace).Trim();
			}

			//Determine if the role exists and if it is able to be edited by both the bot and the user
			IRole role = await Actions.getRoleEditAbility(Context, roleString);
			if (role == null)
				return;

			//Send a message of the permissions the targetted role has
			if (show)
			{
				GuildPermissions rolePerms = new GuildPermissions(Context.Guild.GetRole(role.Id).Permissions.RawValue);
				List<string> currentRolePerms = new List<string>();
				foreach (var permissionValue in Variables.PermissionValues.Values)
				{
					int bit = permissionValue;
					if (((int)rolePerms.RawValue & (1 << bit)) != 0)
					{
						currentRolePerms.Add(Variables.PermissionNames[bit]);
					}
				}
				string permissionsString = String.IsNullOrEmpty(String.Join("\n", currentRolePerms)) ? "No permissions" : String.Join("\n", currentRolePerms);
				await Actions.sendEmbedMessage(Context.Channel, Actions.makeNewEmbed(null, role.Name, permissionsString));
				return;
			}

			//See if it's add or remove
			string addOrRemove = actionRolePerms[0];
			bool add;
			if (addOrRemove.Equals("add"))
			{
				add = true;
			}
			else if (addOrRemove.Equals("remove"))
			{
				add = false;
			}
			else
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Add or remove not specified."));
				return;
			}

			//Get the permissions
			List<string> permissions = permsString.Split('/').ToList();
			//Check if valid permissions
			List<string> validPerms = permissions.Intersect(permissionTypeStrings, StringComparer.OrdinalIgnoreCase).ToList();
			if (validPerms.Count != permissions.Count)
			{
				List<string> invalidPermissions = new List<string>();
				foreach (string permission in permissions)
				{
					if (!validPerms.Contains(permission, StringComparer.OrdinalIgnoreCase))
					{
						invalidPermissions.Add(permission);
					}
				}
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(String.Format("Invalid {0} supplied: `{1}`.",
					permissions.Count - permissions.Intersect(permissionTypeStrings).Count() == 1 ? "permission" : "permissions",
					String.Join("`, `", invalidPermissions))), 7500);
				return;
			}

			//Determine the permissions being added
			uint rolePermissions = 0;
			foreach (string permission in permissions)
			{
				List<string> perms = Variables.PermissionValues.Keys.ToList();
				try
				{
					int bit = Variables.PermissionValues[permission];
					rolePermissions |= (1U << bit);
				}
				catch (Exception)
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(String.Format("Couldn't parse permission '{0}'", permission)));
					return;
				}
			}

			//Determine if the user can give these perms
			if (!await Actions.userHasOwner(Context.Guild, Context.User as IGuildUser))
			{
				if (!(Context.User as IGuildUser).GuildPermissions.Administrator)
				{
					rolePermissions &= (uint)(Context.User as IGuildUser).GuildPermissions.RawValue;
				}
				//If the role has something, but the user is not allowed to edit a permissions
				if (rolePermissions == 0)
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(String.Format("You do not have the ability to modify {0}.",
						permissions.Count == 1 ? "that permission" : "those permissions")));
					return;
				}
			}

			//Get a list of the permissions that were given
			List<string> givenPermissions = Actions.getPermissionNames(rolePermissions).ToList();
			//Get a list of the permissions that were not given
			List<string> skippedPermissions = permissions.Except(givenPermissions, StringComparer.OrdinalIgnoreCase).ToList();

			//New perms
			uint currentBits = (uint)Context.Guild.GetRole(role.Id).Permissions.RawValue;
			if (add)
			{
				currentBits |= rolePermissions;
			}
			else
			{
				currentBits &= ~rolePermissions;
			}

			await Context.Guild.GetRole(role.Id).ModifyAsync(x => x.Permissions = new GuildPermissions(currentBits));
			await Actions.makeAndDeleteSecondaryMessage(Context, String.Format("Successfully {0} `{1}` {2} {3} `{4}`.",
				(add ? "added" : "removed"),
				String.Join("`, `", givenPermissions),
				(skippedPermissions.Count > 0 ? " and failed to " + (add ? "add `" : "remove `") + String.Join("`, `", skippedPermissions) + "`" : ""),
				(add ? "to" : "from"), role.Name),
				7500);
		}

		[Command("copyrolepermissions")]
		[Alias("crp")]
		[Usage(Constants.BOT_PREFIX + "copyrolepermissions [Role]/[Role]")]
		[Summary("Copies the permissions from the first role to the second role. Will not copy roles that the user does not have access to." +
			"Will not overwrite roles that are above the user's top role.")]
		[PermissionRequirements(1U << (int)GuildPermission.ManageRoles)]
		public async Task CopyRolePermissions([Remainder] string input)
		{
			//Put the input into a string
			input = input.ToLower();
			string[] roles = input.Split(new char[] { '/' }, 2);

			//Test if two roles were input
			if (roles.Length != 2)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR));
				return;
			}

			//Determine if the input role exists
			IRole inputRole = await Actions.getRole(Context, roles[0]);
			if (inputRole == null)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ROLE_ERROR));
				return;
			}

			//Determine if the role exists and if it is able to be edited by both the bot and the user
			IRole outputRole = await Actions.getRoleEditAbility(Context, roles[1]);
			if (outputRole == null)
				return;

			//Get the permissions
			uint rolePermissions = (uint)inputRole.Permissions.RawValue;
			List<string> permissions = Actions.getPermissionNames(rolePermissions).ToList();
			if (rolePermissions != 0)
			{
				//Determine if the user can give these permissions
				if (!await Actions.userHasOwner(Context.Guild, Context.User as IGuildUser))
				{
					if (!(Context.User as IGuildUser).GuildPermissions.Administrator)
					{
						rolePermissions &= (uint)(Context.User as IGuildUser).GuildPermissions.RawValue;
					}
					//If the role has something, but the user is not allowed to edit a permissions
					if (rolePermissions == 0)
					{
						await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(String.Format("You do not have the ability to modify {0}.",
							permissions.Count == 1 ? "that permission" : "those permissions")));
						return;
					}
				}
			}

			//Get a list of the permissions that were given
			List<string> givenPermissions = Actions.getPermissionNames(rolePermissions).ToList();
			//Get a list of the permissions that were not given
			List<string> skippedPermissions = permissions.Except(givenPermissions).ToList();

			//Actually change the permissions
			await Context.Guild.GetRole(outputRole.Id).ModifyAsync(x => x.Permissions = new GuildPermissions(rolePermissions));
			//Send the long ass message detailing what happened with the command
			await Actions.makeAndDeleteSecondaryMessage(Context, String.Format("Successfully copied `{0}` {1} from `{2}` to `{3}`.",
				(givenPermissions.Count == 0 ? "NOTHING" : givenPermissions.Count == permissions.Count ? "ALL" : String.Join("`, `", givenPermissions)),
				(skippedPermissions.Count > 0 ? "and failed to copy `" + String.Join("`, `", skippedPermissions) + "`" : ""),
				inputRole, outputRole),
				7500);
		}

		[Command("clearrolepermissions")]
		[Alias("clrrole")]
		[Usage(Constants.BOT_PREFIX + "clearrolepermissions [Role]")]
		[Summary("Removes all permissions from a role.")]
		[PermissionRequirements(1U << (int)GuildPermission.ManageRoles)]
		public async Task ClearRolePermissions([Remainder] string input)
		{
			//Determine if the role exists and if it is able to be edited by both the bot and the user
			IRole role = await Actions.getRoleEditAbility(Context, input);
			if (role == null)
				return;

			//Clear the role's perms
			await role.ModifyAsync(x => x.Permissions = new GuildPermissions(0));
			await Actions.makeAndDeleteSecondaryMessage(Context, String.Format("Successfully removed all permissions from `{0}`.", input));
		}

		[Command("changerolename")]
		[Alias("crn")]
		[Usage(Constants.BOT_PREFIX + "changerolename [Role|Position{x}]/[New Name]")]
		[Summary("Changes the name of the role. This is *extremely* useful for when multiple roles have the same name but you want to edit things.")]
		[PermissionRequirements(1U << (int)GuildPermission.ManageRoles)]
		public async Task ChangeRoleName([Remainder] string input)
		{
			//Split at the current role name and the new role name
			string[] values = input.Split(new char[] { '/' }, 2);

			//Check if correct number of arguments
			if (values.Length != 2)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR));
				return;
			}

			//See if the new name is a valid length
			if (values[1].Length > Constants.ROLE_NAME_LENGTH)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Roles can only have a name length of up to 32 characters."));
				return;
			}

			//Initialize the role
			IRole role = null;

			//See if it's a position trying to be gotten instead
			int position;
			if (values[0].ToLower().Contains("position{") && int.TryParse(values[0].Substring(9, 1), out position))
			{
				//Grab the roles with the position
				var roles = Context.Guild.Roles.Where(x => x.Position == position).ToList();
				if (roles.Count == 0)
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(String.Format("No role has a position of `{0}`", position)));
					return;
				}
				else if (roles.Count == 1)
				{
					//Get the role
					role = await Actions.getRoleEditAbility(Context, role: roles.First());
				}
				else
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, String.Format("`{0}` roles have the position `{1}`.", roles.Count, position));
					return;
				}
			}

			//Determine if the role exists and if it is able to be edited by both the bot and the user
			role = role ?? await Actions.getRoleEditAbility(Context, values[0]);
			if (role == null)
				return;

			//Get a before name
			string beforeName = role.Name;

			//Check if it's the everyone role
			if (role == Context.Guild.EveryoneRole)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Unable to rename the everyone role."));
				return;
			}

			//Change the name
			await Context.Guild.GetRole(role.Id).ModifyAsync(x => x.Name = values[1]);
			await Actions.makeAndDeleteSecondaryMessage(Context, String.Format("Successfully changed the name of the role `{0}` to `{1}`.", beforeName, values[1]));
		}

		[Command("changerolecolor")]
		[Alias("crc")]
		[Usage(Constants.BOT_PREFIX + "changerolecolor Role/[Hexadecimal|Color Name]")]
		[Summary("Changes the role's color. A color of '0' sets the role back to the default color. " +
			"Colors must either be in hexadecimal format or be a color listed [here](https://msdn.microsoft.com/en-us/library/system.drawing.color).")]
		[PermissionRequirements(1U << (int)GuildPermission.ManageRoles)]
		public async Task ChangeRoleColor([Remainder] string input)
		{
			string[] values = input.Split(new char[] { '/' }, 2);

			//Determine if the role exists and if it is able to be edited by both the bot and the user
			IRole role = await Actions.getRoleEditAbility(Context, values[0]);
			if (role == null)
				return;

			UInt32 colorID = (UInt32)System.Drawing.Color.FromName(values[1]).ToArgb();
			if (colorID == 0)
			{
				//Couldn't get name
				string hexString = values[1];
				//Remove 0x if someone put that in there
				if (hexString.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
				{
					hexString = hexString.Substring(2);
				}
				//If the color ID isn't a hex number
				if (!UInt32.TryParse(hexString, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out colorID))
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Color is unable to be added."));
					return;
				}
			}

			//Change the color
			await Context.Guild.GetRole(role.Id).ModifyAsync(x => x.Color = new Color(colorID & 0xffffff));
			await Actions.makeAndDeleteSecondaryMessage(Context, String.Format("Successfully changed the color of the role `{0}` to `{1}`.",
				values[0], values[1]));
		}

		[Command("createchannel")]
		[Alias("cch")]
		[Usage(Constants.BOT_PREFIX + "createchannel [Name] [Text|Voice]")]
		[Summary("Adds a channel to the guild of the given type with the given name. The name CANNOT contain any spaces: use underscores or dashes instead.")]
		[PermissionRequirements(1U << (int)GuildPermission.ManageChannels)]
		public async Task CreateChannel([Remainder] string input)
		{
			string[] values = input.Split(' ');
			if (values.Length != 2)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR));
				return;
			}
			string type = values[1];

			//Test for args
			if (values.Length != 2)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR));
				return;
			}

			//Test for name validity
			if (values[0].Contains(' '))
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("No spaces allowed in a channel name."));
				return;
			}
			else if (values[0].Length > Constants.CHANNEL_NAME_MAX_LENGTH || values[0].Length < Constants.CHANNEL_NAME_MIN_LENGTH)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Name must be between 2 and 100 characters long."));
				return;
			}
			else if (values[0].Equals(Variables.Bot_Channel, StringComparison.OrdinalIgnoreCase) && await Actions.getDuplicateBotChan(Context.Guild))
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, "Please don't try to make a second bot channel. All that does is confuse the bot.");
				return;
			}

			//Test for text
			if (type.Equals(ChannelType.Text.ToString(), StringComparison.OrdinalIgnoreCase))
			{
				await Context.Guild.CreateTextChannelAsync(values[0]);
			}
			//Test for voice
			else if (type.Equals(ChannelType.Voice.ToString(), StringComparison.OrdinalIgnoreCase))
			{
				await Context.Guild.CreateVoiceChannelAsync(values[0]);
			}
			//Give an error if not text/voice
			else
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid channel type."));
				return;
			}

			await Actions.makeAndDeleteSecondaryMessage(Context,
				String.Format("Successfully created `{0} ({1})`.", values[0], char.ToUpper(type[0]) + type.Substring(1)));
		}

		[Command("softdeletechannel")]
		[Alias("sdch")]
		[Usage(Constants.BOT_PREFIX + "softdeletechannel [#Channel]")]
		[Summary("Makes most roles unable to read the channel and moves it to the bottom of the channel list. Only works for text channels.")]
		[PermissionRequirements(0, (1U << (int)GuildPermission.ManageChannels) | (1U << (int)GuildPermission.ManageRoles))]
		public async Task SoftDeleteChannel([Remainder] string input)
		{
			//See if the input name has spaces
			if (input.Contains(' '))
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.CHANNEL_ERROR));
				return;
			}

			//See if the user can see and thus edit that channel
			IGuildChannel channel = await Actions.getChannelEditAbility(Context, input);
			if (channel == null)
				return;

			//See if not attempted on a text channel
			if (Actions.getChannelType(channel) != Constants.TEXT_TYPE)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Softdelete only works on text channels inside a guild."));
				return;
			}
			//Check if tried on the base channel
			else if (channel.Id == Context.Guild.DefaultChannelId)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Unable to softdelete the base channel."));
				return;
			}

			//Make it so only admins/the owner can read the channel
			foreach (var overwrite in channel.PermissionOverwrites)
			{
				if (overwrite.TargetType == PermissionTarget.Role)
				{
					IRole role = Context.Guild.GetRole(overwrite.TargetId);
					uint allowBits = (uint)channel.GetPermissionOverwrite(role).Value.AllowValue & ~(1U << (int)ChannelPermission.ReadMessages);
					uint denyBits = (uint)channel.GetPermissionOverwrite(role).Value.DenyValue | (1U << (int)ChannelPermission.ReadMessages);
					await channel.AddPermissionOverwriteAsync(role, new OverwritePermissions(allowBits, denyBits));
				}
				else
				{
					IGuildUser user = await Context.Guild.GetUserAsync(overwrite.TargetId);
					uint allowBits = (uint)channel.GetPermissionOverwrite(user).Value.AllowValue & ~(1U << (int)ChannelPermission.ReadMessages);
					uint denyBits = (uint)channel.GetPermissionOverwrite(user).Value.DenyValue | (1U << (int)ChannelPermission.ReadMessages);
					await channel.AddPermissionOverwriteAsync(user, new OverwritePermissions(allowBits, denyBits));
				}
			}

			//Determine the highest position (kind of backwards, the lower the closer to the top, the higher the closer to the bottom)
			int highestPosition = 0;
			foreach (IGuildChannel channelOnGuild in await Context.Guild.GetChannelsAsync())
			{
				if (channelOnGuild.Position > highestPosition)
				{
					highestPosition = channelOnGuild.Position;
				}
			}

			await channel.ModifyAsync(x => x.Position = highestPosition);
			await Actions.sendChannelMessage(channel as IMessageChannel,
				"Successfully softdeleted this channel. Only admins and the owner will be able to read anything on this channel.");
		}

		[Command("deletechannel")]
		[Alias("dch")]
		[Usage(Constants.BOT_PREFIX + "deletechannel " + Constants.CHANNEL_INSTRUCTIONS)]
		[Summary("Deletes the channel.")]
		[PermissionRequirements(1U << (int)GuildPermission.ManageChannels)]
		public async Task DeleteChannel([Remainder] string input)
		{
			//See if the user can see and thus edit that channel
			IGuildChannel channel = await Actions.getChannelEditAbility(Context, input);
			if (channel == null)
				return;

			//Check if tried on the base channel
			if (channel.Id == Context.Guild.DefaultChannelId)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Unable to delete the base channels."));
				return;
			}

			await channel.DeleteAsync();
			await Actions.makeAndDeleteSecondaryMessage(Context, String.Format("Successfully deleted `{0} ({1})`.", channel.Name, Actions.getChannelType(channel)));
		}

		[Command("channelposition")]
		[Alias("chpos")]
		[Usage(Constants.BOT_PREFIX + "channelposition " + Constants.CHANNEL_INSTRUCTIONS + " [New Position]")]
		[Summary("Gives the channel the given position. Position one is the top most position and counting starts at zero. This command is extremely buggy!")]
		[PermissionRequirements(1U << (int)GuildPermission.ManageChannels)]
		public async Task ChannelPosition([Remainder] string input)
		{
			string[] values = input.Split(new char[] { ' ' }, 2);

			//Get the channel
			IGuildChannel channel = await Actions.getChannelEditAbility(Context, values[0]);
			if (channel == null)
				return;

			//Argument count checking
			if (values.Count() != 2)
			{
				await Actions.sendChannelMessage(Context.Channel, String.Format("The `{0} ({1})` channel has a position of `{2}`.",
					channel.Name, Actions.getChannelType(channel), channel.Position));
				return;
			}

			//Get the position as an int
			int position = 0;
			if (!int.TryParse(input.Substring(input.LastIndexOf(' ')), out position))
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid position."));
				return;
			}

			//Check the min against the current position
			if (position < 0)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Cannot set a channel to a position lower than or equal to zero."));
				return;
			}

			//Initialize a new list of IGuildChannel
			var channelAndPositions = new List<IGuildChannel>();
			//Grab either the text or voice channels
			if (Actions.getChannelType(channel) == Constants.TEXT_TYPE)
			{
				//Grab all text channels that aren't the targeted one
				(await Context.Guild.GetTextChannelsAsync()).Where(x => x != channel).ToList().ForEach(x => channelAndPositions.Add(x));
			}
			else
			{
				//Grab all the voice channels that aren't the targeted one
				(await Context.Guild.GetVoiceChannelsAsync()).Where(x => x != channel).ToList().ForEach(x => channelAndPositions.Add(x));
			}
			//Sort the list by position
			channelAndPositions = channelAndPositions.OrderBy(x => x.Position).ToList();
			//Add in the targetted channel with the given position
			channelAndPositions.Insert(Math.Min(channelAndPositions.Count(), position), channel);

			//Initialize a new list of BulkGuildChannelProperties
			var listOfBulk = new List<BulkGuildChannelProperties>();
			//Add the channel's IDs and positions into it
			channelAndPositions.ForEach(cap => listOfBulk.Add(new BulkGuildChannelProperties(cap.Id, channelAndPositions.IndexOf(cap))));
			//Mass modify the channels with the list having the correct positions as an IEnum
			await Context.Guild.ModifyChannelsAsync(listOfBulk as IEnumerable<BulkGuildChannelProperties>);

			//Send a message stating what position the channel was sent to
			await Actions.sendChannelMessage(Context.Channel, String.Format("Successfully moved `{0} ({1})` to position `{2}`.",
				channel.Name, Actions.getChannelType(channel), channelAndPositions.IndexOf(channel)));
		}

		[Command("listchannelpositions")]
		[Alias("lchp")]
		[Usage(Constants.BOT_PREFIX + "listchannelpositions [Text|Voice]")]
		[Summary("Lists the positions of each text or voice channel on the guild.")]
		[PermissionRequirements(1U << (int)GuildPermission.ManageChannels)]
		public async Task ListChannelPositions([Remainder] string input)
		{
			//Check if valid type
			if (!input.Equals(Constants.VOICE_TYPE) && !input.Equals(Constants.TEXT_TYPE))
				return;

			//Initialize the string
			string title;
			string description = "";
			if (input.Equals(Constants.VOICE_TYPE))
			{
				title = "Voice Channels Positions";

				//Put the positions into the string
				var list = (await Context.Guild.GetVoiceChannelsAsync()).OrderBy(x => x.Position).ToList();
				foreach (var channel in list)
				{
					description += "`" + channel.Position.ToString("00") + ".` " + channel.Name + "\n";
				}
			}
			else
			{
				title = "Text Channels Positions";

				//Put the positions into the string
				var list = (await Context.Guild.GetTextChannelsAsync()).OrderBy(x => x.Position).ToList();
				foreach (var channel in list)
				{
					description += "`" + channel.Position.ToString("00") + ".` " + channel.Name + "\n";
				}
			}

			//Check the length to see if the message can be sent
			if (description.Length > Constants.SHORT_LENGTH_CHECK)
			{
				description = Actions.uploadToHastebin(Actions.replaceMessageCharacters(description));
			}

			await Actions.sendEmbedMessage(Context.Channel, Actions.makeNewEmbed(null, title, description));
		}

		[Command("channelpermissions")]
		[Alias("chp")]
		[Usage(Constants.BOT_PREFIX + "channelpermissions [Show|Allow|Inherit|Deny] " + Constants.OPTIONAL_CHANNEL_INSTRUCTIONS + " <Role|User> <Permission/...>")]
		[Summary("Type `" + Constants.BOT_PREFIX + "chp [Show]` to see the available permissions. Permissions must be separated by a `/`! " +
			"Type `" + Constants.BOT_PREFIX + "chp [Show] [Channel]` to see all permissions on a channel. " +
			"Type `" + Constants.BOT_PREFIX + "chp [Show] [Channel] [Role|User]` to see permissions a role/user has on a channel.")]
		[PermissionRequirements(0, (1U << (int)GuildPermission.ManageChannels) | (1U << (int)GuildPermission.ManageRoles))]
		public async Task ChannelPermissions([Remainder] string input)
		{
			//Set the variables
			List<string> permissions = null;
			IGuildChannel channel = null;
			IGuildUser user = null;
			IRole role = null;

			//Split the input
			string[] values = input.ToLower().Trim().Split(new char[] { ' ' }, 2);
			if (values.Length == 0)
				return;
			string actionName = values[0];

			if (actionName.Equals("show"))
			{
				//If only show, take that as a person wanting to see the permission types
				if (values.Length == 1)
				{
					//Embed showing the channel permission types
					await Actions.sendEmbedMessage(Context.Channel, Actions.makeNewEmbed(null, "Channel Permission Types", String.Join("\n", Variables.ChannelPermissionNames.Values)));
					return;
				}

				//Check for valid channel
				values = values[1].Split(new char[] { ' ' }, 2);
				//See if the user can see and thus edit that channel
				channel = await Actions.getChannelEditAbility(Context, values[0]);
				if (channel == null)
					return;

				//Say the overwrites on a channel
				if (values.Length == 1)
				{
					var roleOverwrites = new List<string>();
					var userOverwrites = new List<string>();
					foreach (Overwrite overwrite in channel.PermissionOverwrites)
					{
						if (overwrite.TargetType == PermissionTarget.Role)
						{
							roleOverwrites.Add(Context.Guild.GetRole(overwrite.TargetId).Name);
						}
						else
						{
							userOverwrites.Add((await Context.Guild.GetUserAsync(overwrite.TargetId)).Username);
						}
					}
					//Embed saying the overwrites
					EmbedBuilder embed = Actions.makeNewEmbed(null, channel.Name);
					Actions.addField(embed, "Role", String.Join("\n", roleOverwrites));
					Actions.addField(embed, "User", String.Join("\n", userOverwrites));
					await Actions.sendEmbedMessage(Context.Channel, embed);
					return;
				}

				//Check if valid role or user
				role = await Actions.getRole(Context, values[1]);
				if (role == null)
				{
					user = await Actions.getUser(Context.Guild, values[1]);
					if (user == null)
					{
						await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid role or user supplied."));
						return;
					}
				}

				//Say the permissions of the overwrite
				foreach (Overwrite overwrite in channel.PermissionOverwrites)
				{
					if (role != null && overwrite.TargetId.Equals(role.Id))
					{
						//Embed showing the perm overwrites on a role
						EmbedBuilder embed = Actions.makeNewEmbed(title: String.Format("{0} on {1} ({2})", role.Name, channel.Name, Actions.getChannelType(channel)));
						Actions.addField(embed, "Permission", String.Join("\n", Actions.getPerms(overwrite, channel).Keys));
						Actions.addField(embed, "Value", String.Join("\n", Actions.getPerms(overwrite, channel).Values));
						await Actions.sendEmbedMessage(Context.Channel, embed);
						return;
					}
					else if (user != null && overwrite.TargetId.Equals(user.Id))
					{
						//Embed showing the perm overwrites on a user
						EmbedBuilder embed = Actions.makeNewEmbed(title: String.Format("{0}#{1} on {2} ({3})", user.Username, user.Discriminator, channel.Name, Actions.getChannelType(channel)));
						Actions.addField(embed, "Permission", String.Join("\n", Actions.getPerms(overwrite, channel).Keys));
						Actions.addField(embed, "Value", String.Join("\n", Actions.getPerms(overwrite, channel).Values));
						await Actions.sendEmbedMessage(Context.Channel, embed);
						return;
					}
				}
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(String.Format("Unable to show permissions for `{0}` on `{1} ({2})`.",
					values[1], channel.Name, Actions.getChannelType(channel))));
				return;
			}
			else if (actionName.Equals("allow") || actionName.Equals("deny") || actionName.Equals("inherit"))
			{
				values = values[1].Split(new char[] { ' ' }, 2);

				//Check if valid number of arguments
				if (values.Length == 1)
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR));
					return;
				}

				//See if the user can see and thus edit that channel
				channel = await Actions.getChannelEditAbility(Context, values[0]);
				if (channel == null)
					return;

				//Check if valid perms and potential role/user
				string potentialRoleOrUser;
				if (Actions.getStringAndPermissions(values[1], out potentialRoleOrUser, out permissions))
				{
					//See if valid role
					role = Actions.getRole(Context.Guild, potentialRoleOrUser);
					if (role == null)
					{
						//See if valid user
						user = await Actions.getUser(Context.Guild, potentialRoleOrUser);
						if (user == null)
						{
							//Give error if no user or role that's valid
							await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid role or user supplied."));
							return;
						}
					}
				}
				else
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("No permissions supplied."));
					return;
				}
			}
			else
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid action."));
				return;
			}

			//Get the generic permissions
			List<string> genericPerms = Variables.ChannelPermissionNames.Values.ToList();
			//Check if valid permissions
			List<string> validPerms = permissions.Intersect(genericPerms, StringComparer.OrdinalIgnoreCase).ToList();
			if (validPerms.Count != permissions.Count)
			{
				List<string> invalidPerms = new List<string>();
				foreach (string permission in permissions)
				{
					if (!validPerms.Contains(permission, StringComparer.OrdinalIgnoreCase))
					{
						invalidPerms.Add(permission);
					}
				}
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(String.Format("Invalid {0} supplied: `{1}`.",
					invalidPerms.Count == 1 ? "permission" : "permissions",
					String.Join("`, `", invalidPerms))), 7500);
				return;
			}

			//Remove any attempt to change readmessages on the base channel because nothing can change that
			if (channel.Id == Context.Guild.DefaultChannelId && permissions.Contains("readmessages"))
			{
				permissions.RemoveAll(x => x.StartsWith("readmessages"));
			}

			//Get the permissions
			uint changeValue = 0;
			uint? allowBits = 0;
			uint? denyBits = 0;
			if (role != null)
			{
				if (channel.GetPermissionOverwrite(role).HasValue)
				{
					allowBits = (uint?)channel.GetPermissionOverwrite(role).Value.AllowValue;
					denyBits = (uint?)channel.GetPermissionOverwrite(role).Value.DenyValue;
				}
			}
			else
			{
				if (channel.GetPermissionOverwrite(user).HasValue)
				{
					allowBits = (uint?)channel.GetPermissionOverwrite(user).Value.AllowValue;
					denyBits = (uint?)channel.GetPermissionOverwrite(user).Value.DenyValue;
				}
			}

			//Changing the bit values
			foreach (string permission in permissions)
			{
				changeValue = await Actions.getBit(Context, permission, changeValue);
			}
			if (actionName.Equals("allow"))
			{
				allowBits |= changeValue;
				denyBits &= ~changeValue;
				actionName = "allowed";
			}
			else if (actionName.Equals("inherit"))
			{
				allowBits &= ~changeValue;
				denyBits &= ~changeValue;
				actionName = "inherited";
			}
			else
			{
				allowBits &= ~changeValue;
				denyBits |= changeValue;
				actionName = "denied";
			}

			//Change the permissions
			string roleNameOrUsername;
			if (role != null)
			{
				await channel.AddPermissionOverwriteAsync(role, new OverwritePermissions((uint)allowBits, (uint)denyBits));
				roleNameOrUsername = role.Name;
			}
			else
			{
				await channel.AddPermissionOverwriteAsync(user, new OverwritePermissions((uint)allowBits, (uint)denyBits));
				roleNameOrUsername = user.Username + "#" + user.Discriminator;
			}

			await Actions.makeAndDeleteSecondaryMessage(Context, String.Format("Successfully {0} `{1}` for `{2}` on `{3} ({4})`",
				actionName, String.Join("`, `", permissions), roleNameOrUsername, channel.Name, Actions.getChannelType(channel)), 7500);
		}

		[Command("copychannelpermissions")]
		[Alias("cchp")]
		[Usage(Constants.BOT_PREFIX + "copychannelpermissions " + Constants.CHANNEL_INSTRUCTIONS + " " + Constants.CHANNEL_INSTRUCTIONS + " [Role|User|All]")]
		[Summary("Copy permissions from one channel to another. Works for a role, a user, or everything.")]
		[PermissionRequirements(0, (1U << (int)GuildPermission.ManageChannels) | (1U << (int)GuildPermission.ManageRoles))]
		public async Task CopyChannelPermissions([Remainder] string input)
		{
			//Get arguments
			string[] inputArray = input.ToLower().Split(new char[] { ' ' }, 3);

			//Separating the channels
			IGuildChannel inputChannel = await Actions.getChannel(Context, inputArray[0]);
			if (inputChannel == null)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.CHANNEL_ERROR));
				return;
			}

			//See if the user can see and thus edit that channel
			IGuildChannel outputChannel = await Actions.getChannelEditAbility(Context, inputArray[1]);
			if (outputChannel == null)
				return;

			//Trim the third arg
			inputArray[2] = inputArray[2].Trim();

			//Copy the selected target
			string target;
			if (inputArray[2].Equals("all"))
			{
				target = "ALL";
				foreach (Overwrite permissionOverwrite in inputChannel.PermissionOverwrites)
				{
					if (permissionOverwrite.TargetType == PermissionTarget.Role)
					{
						IRole role = Context.Guild.GetRole(permissionOverwrite.TargetId);
						await outputChannel.AddPermissionOverwriteAsync(role, new OverwritePermissions(inputChannel.GetPermissionOverwrite(role).Value.AllowValue,
							inputChannel.GetPermissionOverwrite(role).Value.DenyValue));
					}
					else
					{
						IGuildUser user = await Context.Guild.GetUserAsync(permissionOverwrite.TargetId);
						await outputChannel.AddPermissionOverwriteAsync(user, new OverwritePermissions(inputChannel.GetPermissionOverwrite(user).Value.AllowValue,
							inputChannel.GetPermissionOverwrite(user).Value.DenyValue));
					}
				}
			}
			else
			{
				IRole role = await Actions.getRole(Context, inputArray[2]);
				if (role != null)
				{
					target = role.Name;
					await outputChannel.AddPermissionOverwriteAsync(role, new OverwritePermissions(inputChannel.GetPermissionOverwrite(role).Value.AllowValue,
						inputChannel.GetPermissionOverwrite(role).Value.DenyValue));
				}
				else
				{
					IGuildUser user = await Actions.getUser(Context.Guild, inputArray[2]);
					if (user != null)
					{
						target = user.Username;
						await outputChannel.AddPermissionOverwriteAsync(user, new OverwritePermissions(inputChannel.GetPermissionOverwrite(user).Value.AllowValue,
							inputChannel.GetPermissionOverwrite(user).Value.DenyValue));
					}
					else
					{
						await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("No valid role/user or all input."));
						return;
					}
				}
			}

			await Actions.makeAndDeleteSecondaryMessage(Context, String.Format("Successfully copied `{0}` from `{1} ({2})` to `{3} ({4})`",
				target, inputChannel.Name, Actions.getChannelType(inputChannel), outputChannel.Name, Actions.getChannelType(outputChannel)), 7500);
		}

		[Command("clearchannelpermissions")]
		[Alias("clchp")]
		[Usage(Constants.BOT_PREFIX + "clearchannelpermissions " + Constants.CHANNEL_INSTRUCTIONS)]
		[Summary("Removes all permissions set on a channel.")]
		[PermissionRequirements(0, (1U << (int)GuildPermission.ManageChannels) | (1U << (int)GuildPermission.ManageRoles))]
		public async Task ClearChannelPermissions([Remainder] string input)
		{
			//See if the user can see and thus edit that channel
			IGuildChannel channel = await Actions.getChannelEditAbility(Context, input);
			if (channel == null)
				return;

			//Check if channel has permissions to clear
			if (channel.PermissionOverwrites.Count < 1)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Channel has no permissions to clear."));
				return;
			}

			//Remove all the permission overwrites
			channel.PermissionOverwrites.ToList().ForEach(async x =>
			{
				if (x.TargetType == PermissionTarget.Role)
				{
					await channel.RemovePermissionOverwriteAsync(Context.Guild.GetRole(x.TargetId));
				}
				else
				{
					await channel.RemovePermissionOverwriteAsync(await Context.Guild.GetUserAsync(x.TargetId));
				}
			});
			await Actions.makeAndDeleteSecondaryMessage(Context, String.Format("Successfully removed all channel permissions from `{0} ({1})`.",
				channel.Name, Actions.getChannelType(channel)), 7500);
		}

		[Command("channelname")]
		[Alias("chn")]
		[Usage(Constants.BOT_PREFIX + "channelname [#Channel|[Channel|Position{x}/Text|Voice]] [New Name]")]
		[Summary("Changes the name of the channel. This is *extremely* useful for when multiple channels have the same name but you want to edit things.")]
		[PermissionRequirements(1U << (int)GuildPermission.ManageChannels)]
		public async Task ChangeChannelName([Remainder] string input)
		{
			string[] inputArray = input.Split(new char[] { ' ' }, 2);
			if (inputArray.Length != 2)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR));
				return;
			}

			//Checking if valid name
			if (inputArray[1].Contains(' '))
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("No spaces allowed in a channel name."));
				return;
			}
			else if (inputArray[1].Length > Constants.CHANNEL_NAME_MAX_LENGTH || inputArray[1].Length < Constants.CHANNEL_NAME_MIN_LENGTH)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Name must be between 2 and 100 characters long."));
				return;
			}
			else if (inputArray[1].Equals(Variables.Bot_Channel, StringComparison.OrdinalIgnoreCase) && await Actions.getDuplicateBotChan(Context.Guild))
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, "Please don't try to rename a channel to the bot channel if one alreay exists. All that does is confuse the bot.");
				return;
			}

			//Initialize the channel
			IGuildChannel channel = null;

			//See if it's a position trying to be gotten instead
			int position;
			if (inputArray[0].ToLower().Contains("position{") && int.TryParse(inputArray[0].Substring(9, 1), out position))
			{
				//Split the input
				string[] splitInputArray = inputArray[0].Split(new char[] { '/' }, 2);
				//Give the channeltype
				string channelType = splitInputArray[1].ToLower();

				//Initialize the channels list
				var textChannels = new List<ITextChannel>();
				var voiceChannels = new List<IVoiceChannel>();

				if (channelType == Constants.TEXT_TYPE)
				{
					textChannels = (await Context.Guild.GetTextChannelsAsync()).Where(x => x.Position == position).ToList();
				}
				else if (channelType == Constants.VOICE_TYPE)
				{
					voiceChannels = (await Context.Guild.GetVoiceChannelsAsync()).Where(x => x.Position == position).ToList();
				}
				else
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("No valid channel type."));
					return;
				}

				//Check the count now
				if (textChannels.Count == 0 && voiceChannels.Count == 0)
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(String.Format("No {0} channel has a position of `{1}`.", channelType, position)));
					return;
				}
				else if (textChannels.Count == 1 || voiceChannels.Count == 1)
				{
					//Get the channel
					var chan = textChannels.Count == 1 ? textChannels.First() as IGuildChannel : voiceChannels.First() as IGuildChannel;
					channel = await Actions.getChannelEditAbility(chan, Context.User as IGuildUser);
				}
				else
				{
					//Get the count
					int count = textChannels.Count > 0 ? textChannels.Count : voiceChannels.Count;
					await Actions.makeAndDeleteSecondaryMessage(Context, String.Format("`{0}` {1} channels have the position `{2}`.", count, channelType, position));
					return;
				}
			}

			//Check if valid channel
			channel = channel ?? await Actions.getChannelEditAbility(Context, inputArray[0]);
			if (channel == null)
				return;

			string previousName = channel.Name;
			await channel.ModifyAsync(x => x.Name = inputArray[1]);
			await Actions.makeAndDeleteSecondaryMessage(Context, String.Format("Successfully changed channel `{0}` to `{1}`.", previousName, inputArray[1]), 5000);
		}

		[Command("channeltopic")]
		[Alias("cht")]
		[Usage(Constants.BOT_PREFIX + "channeltopic [#Channel] [New Topic]")]
		[Summary("Changes the subtext of a channel to whatever is input.")]
		[PermissionRequirements(1U << (int)GuildPermission.ManageChannels)]
		public async Task ChangeChannelTopic([Remainder] string input)
		{
			string[] inputArray = input.Split(new char[] { ' ' }, 2);
			if (inputArray.Length != 2)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR));
				return;
			}
			string newTopic = inputArray[1];

			//See if valid length
			if (newTopic.Length > Constants.TOPIC_LENGTH)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Topics cannot be longer than 1024 characters in length."));
				return;
			}

			//Test if valid channel
			IGuildChannel channel = await Actions.getChannelEditAbility(Context, inputArray[0]);
			if (channel == null)
				return;

			//See if not a text channel
			if (Actions.getChannelType(channel) != Constants.TEXT_TYPE)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Only text channels can have their topic set."));
				return;
			}

			//See what current topic is
			string currentTopic = (channel as ITextChannel).Topic;
			if (String.IsNullOrWhiteSpace(currentTopic))
			{
				currentTopic = "NOTHING";
			}

			await (channel as ITextChannel).ModifyAsync(x => x.Topic = newTopic);
			await Actions.makeAndDeleteSecondaryMessage(Context, String.Format("Successfully changed the topic in `{0}` from `{1}` to `{2}`.",
				channel.Name, currentTopic, newTopic == "" ? "NOTHING" : newTopic));
		}

		[Command("channellimit")]
		[Alias("chl")]
		[Usage(Constants.BOT_PREFIX + "channellimit [Channel Name] [New Limit]")]
		[Summary("Changes the limit to how many users can be in a voice channel. The limit ranges from 0 (no limit) to 99.")]
		[PermissionRequirements(1U << (int)GuildPermission.ManageChannels)]
		public async Task ChangeChannelLimit([Remainder] string input)
		{
			//Separate the input
			string[] inputArray = input.Split(new char[] { ' ' }, 2);
			string channelName = inputArray[0];
			string newLimit = inputArray[1];

			//Check if valid channel that the user can edit
			IGuildChannel channel = await Actions.getChannelEditAbility(Context, channelName + "/voice");
			if (channel == null)
				return;
			else if (Actions.getChannelType(channel) != Constants.VOICE_TYPE)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Only a voice channel can have a limit set on it."));
				return;
			}

			//Check if valid number
			int limit = 0;
			if (!int.TryParse(newLimit, out limit))
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("The second argument is not a valid number."));
				return;
			}

			//Check if number between 0 and 99
			if (limit > 99 || limit < 0)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Number must be between 0 and 99 inclusive."));
				return;
			}

			//Change it and send a success message
			await (channel as IVoiceChannel).ModifyAsync(x => x.UserLimit = limit);
			await Actions.makeAndDeleteSecondaryMessage(Context, String.Format("Successfully set the user limit for `{0}` to `{1}`.", channel.Name, limit));
		}

		[Command("channelbitrate")]
		[Alias("chbr")]
		[Usage(Constants.BOT_PREFIX + "channelbitrate [Channel Name] [8 to 96]")]
		[Summary("Changes the bit rate (in kbps) on the selected channel to the given value. The default value is 64.")]
		[PermissionRequirements(1U << (int)GuildPermission.ManageChannels)]
		public async Task ChangeChannelBitRate([Remainder] string input)
		{
			//Separate the input
			string[] inputArray = input.Split(new char[] { ' ' }, 2);
			string channelName = inputArray[0];
			string newBitRate = inputArray[1];

			//Check if valid channel that the user can edit
			IGuildChannel channel = await Actions.getChannelEditAbility(Context, channelName + "/voice");
			if (channel == null)
				return;
			else if (Actions.getChannelType(channel) != Constants.VOICE_TYPE)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Only a voice channel can have its bit rate changed."));
				return;
			}

			//Check if valid number
			int bitRate = 0;
			if (!int.TryParse(newBitRate, out bitRate))
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("The second argument is not a valid number."));
				return;
			}

			//Check if number between 8 and 96 
			if (bitRate > 96 || bitRate < 8)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Number must be between 8 and 96 inclusive."));
				return;
			}

			//Change it and send a success message
			await (channel as IVoiceChannel).ModifyAsync(x => x.Bitrate = bitRate * 1000);
			await Actions.makeAndDeleteSecondaryMessage(Context, String.Format("Successfully set the user limit for `{0}` to `{1}kbps`.", channel.Name, bitRate));
		}

		[Command("moveuser")]
		[Alias("mu")]
		[Usage(Constants.BOT_PREFIX + "moveuser [@User] [Channel]")]
		[Summary("Moves the user to the given voice channel.")]
		[PermissionRequirements(1U << (int)GuildPermission.MoveMembers)]
		public async Task MoveUser([Remainder] string input)
		{
			//Input and splitting
			string[] inputArray = input.Split(new char[] { ' ' }, 2);

			//Check if valid user
			IGuildUser user = await Actions.getUser(Context.Guild, inputArray[0]);
			if (user == null)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.USER_ERROR));
				return;
			}

			//Check if user is in a voice channel
			if (user.VoiceChannel == null)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("User is not in a voice channel."));
				return;
			}

			//See if the bot can move people from this channel
			if (await Actions.getChannelEditAbility(user.VoiceChannel, await Context.Guild.GetUserAsync(Variables.Bot_ID)) == null)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Unable to move this user due to permissions " +
					"or due to the user being in a voice channel before the bot started up."), 5000);
				return;
			}
			//See if the user can move people from this channel
			if (await Actions.getChannelEditAbility(user.VoiceChannel, Context.User as IGuildUser) == null)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("You are unable to move people from this channel."));
				return;
			}

			//Check if valid channel
			IGuildChannel channel = await Actions.getChannelEditAbility(Context, inputArray[1] + "/voice");
			if (channel == null)
				return;
			else if (Actions.getChannelType(channel) != Constants.VOICE_TYPE)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Users can only be moved to a voice channel."));
				return;
			}

			//See if trying to put user in the exact same channel
			if (user.VoiceChannel == channel)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("User is already in that channel"));
				return;
			}

			await user.ModifyAsync(x => x.Channel = Optional.Create(channel as IVoiceChannel));
			await Actions.makeAndDeleteSecondaryMessage(Context, String.Format("Successfully moved `{0}#{1}` to `{2}`.", user.Username, user.Discriminator, channel.Name), 5000);
		}

		[Command("mute")]
		[Alias("m")]
		[Usage(Constants.BOT_PREFIX + "mute [@User]")]
		[Summary("If the user is not guild muted, this will mute them. If they are guild muted, this will unmute them.")]
		[PermissionRequirements(1U << (int)GuildPermission.MuteMembers)]
		public async Task Mute([Remainder] string input)
		{
			//Test if valid user mention
			IGuildUser user = await Actions.getUser(Context.Guild, input);
			if (user == null)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.USER_ERROR));
				return;
			}

			//See if it should mute or unmute
			if (!user.IsMuted)
			{
				await user.ModifyAsync(x => x.Mute = true);
				await Actions.makeAndDeleteSecondaryMessage(Context, String.Format("Successfully guild muted `{0}#{1}`.", user.Username, user.Discriminator));
				return;
			}
			await user.ModifyAsync(x => x.Mute = false);
			await Actions.makeAndDeleteSecondaryMessage(Context, String.Format("Successfully removed the guild mute on `{0}#{1}`.", user.Username, user.Discriminator));
		}

		[Command("deafen")]
		[Alias("dfn", "d")]
		[Usage(Constants.BOT_PREFIX + "deafen [@User]")]
		[Summary("Bans then unbans a user from the guild.")]
		[PermissionRequirements(1U << (int)GuildPermission.DeafenMembers)]
		public async Task Deafen([Remainder] string input)
		{
			//Test if valid user mention
			IGuildUser user = await Actions.getUser(Context.Guild, input);
			if (user == null)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.USER_ERROR));
				return;
			}

			//See if it should deafen or undeafen
			if (!user.IsDeafened)
			{
				await user.ModifyAsync(x => x.Deaf = true);
				await Actions.makeAndDeleteSecondaryMessage(Context, String.Format("Successfully server deafened `{0}#{1}`.", user.Username, user.Discriminator));
				return;
			}
			await user.ModifyAsync(x => x.Deaf = false);
			await Actions.makeAndDeleteSecondaryMessage(Context, String.Format("Successfully removed the server deafen on `{0}#{1}`.", user.Username, user.Discriminator));
		}

		[Command("nickname")]
		[Alias("nn")]
		[Usage(Constants.BOT_PREFIX + "nickname [@User] [New Nickname|Remove]")]
		[Summary("Gives the user a nickname.")]
		[PermissionRequirements(1U << (int)GuildPermission.ManageNicknames)]
		public async Task Nickname([Remainder] string input)
		{
			//Input and splitting
			string[] inputArray = input.Split(new char[] { ' ' }, 2);
			string nickname;
			if (inputArray.Length == 2)
			{
				if (inputArray[1].ToLower().Equals("remove"))
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
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR));
				return;
			}

			//Check if valid length
			if (nickname != null && nickname.Length > Constants.NICKNAME_LENGTH)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Nicknames cannot be longer than 32 characters."));
				return;
			}

			//Check if valid user
			IGuildUser user = await Actions.getUser(Context.Guild, inputArray[0]);
			if (user == null)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.USER_ERROR));
				return;
			}

			//Checks for positions
			int nicknamePosition = Actions.getPosition(Context.Guild, user);
			if (nicknamePosition > Actions.getPosition(Context.Guild, Context.User as IGuildUser))
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("User cannot be nicknamed by you."));
				return;
			}
			if (nicknamePosition > Actions.getPosition(Context.Guild, await Context.Guild.GetUserAsync(Variables.Bot_ID)))
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("User cannot be nicknamed by the bot."));
				return;
			}

			await user.ModifyAsync(x => x.Nickname = nickname);
			if (nickname != null)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, String.Format("Successfully gave the nickname `{0}` to `{1}#{2}`.", nickname, user.Username, user.Discriminator));
				return;
			}
			await Actions.makeAndDeleteSecondaryMessage(Context, String.Format("Sucessfully removed the nickname from `{0}#{1}`.", user.Username, user.Discriminator));
		}

		[Command("allwithrole")]
		[Alias("awr")]
		[Usage(Constants.BOT_PREFIX + "allwithrole <File|Upload> [Role]")]
		[Summary("Prints out a list of all users with the given role. File specifies a text document which can show more symbols. Upload specifies to use a text uploader.")]
		[PermissionRequirements(1U << (int)GuildPermission.ManageRoles)]
		public async Task AllWithRole([Remainder] string input)
		{
			//Split into the bools and role
			List<string> values = input.Split(new char[] { ' ' }, 2).ToList();

			//Initializing input and variables
			IRole role = await Actions.getRole(Context, values.Last());
			if (role == null)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ROLE_ERROR));
				return;
			}

			//If two args, check what action to take
			bool overwriteBool = false;
			bool textFileBool = false;
			if (values.Count == 2)
			{
				if (values[0].ToLower().Equals("file"))
				{
					textFileBool = true;
					overwriteBool = true;
				}
				else if (values[0].ToLower().Equals("upload"))
				{
					overwriteBool = true;
				}
			}

			//Initialize the lists
			List<string> usersMentions = new List<string>();
			List<string> usersText = new List<string>();
			int characters = 0;
			int count = 1;

			//Grab each user
			IReadOnlyCollection<IGuildUser> guildUsers = await Context.Guild.GetUsersAsync();
			guildUsers.Where(x => x.JoinedAt != null).ToList().OrderBy(x => x.JoinedAt.Value.Ticks).ToList().ForEach(x =>
			{
				if (x.RoleIds.ToList().Contains(role.Id))
				{
					string text = "`" + x.Username + "#" + x.Discriminator + "`";
					usersMentions.Add(text);
					usersText.Add("`" + count++.ToString("00") + ".` " + x.Username + "#" + x.Discriminator + " ID: " + x.Id);
					characters += text.Length + 3;
				}
			});

			//Checking if the message can fit in a single message
			string roleName = role.Name.Substring(0, 3) + Constants.ZERO_LENGTH_CHAR + role.Name.Substring(3);
			if (characters > 1000 || overwriteBool)
			{
				string info = Actions.replaceMessageCharacters(String.Join("\n", usersText));
				if (!textFileBool)
				{
					await Actions.sendEmbedMessage(Context.Channel, Actions.makeNewEmbed(null, roleName, Actions.uploadToHastebin(info)));
					return;
				}
				//Upload the file
				await Actions.uploadTextFile(Context.Guild, Context.Channel, info, roleName + "_" + role.Name.ToUpper() + "_", roleName);
			}
			else
			{
				await Actions.sendEmbedMessage(Context.Channel, Actions.makeNewEmbed(null, roleName, String.Join(", ", usersMentions)));
			}
		}

		[Command("forallwithrole")]
		[Alias("fawr")]
		[Usage(Constants.BOT_PREFIX + "forallwithrole [Give|Take|Nickname] [Role]/[Role|Nickname] <" + Constants.BYPASS_STRING + ">")]
		[Summary("Only self hosted bots are allowed to go past ten members per use. When used on a self bot, \"" + Constants.BYPASS_STRING + "\" removes the 10 user limit.")]
		[PermissionRequirements]
		public async Task ForAllWithRole([Remainder] string input)
		{
			//Separating input into the action and role/role or nickname + bypass
			string[] inputArray = input.Split(new char[] { ' ' }, 2);
			string action = inputArray[0];
			if (inputArray.Length < 2)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR));
				return;
			}

			//Separate role/role or nickname + bypass into role and role or nickname + bypass
			string[] values = inputArray[1].Split('/');
			if (values.Length != 2)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR));
				return;
			}

			//Check if bypass, up the max limit, and remove the bypass string from the values array
			int maxLength = 10;
			if (values[1].EndsWith(Constants.BYPASS_STRING) && Context.User.Id.Equals(Constants.OWNER_ID))
			{
				maxLength = int.MaxValue;
				values[1] = values[1].Substring(0, values[1].Length - Constants.BYPASS_STRING.Length);
			}

			if (action.Equals("give"))
			{
				if (values[0].Equals(values[1]))
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Cannot give the same role that is being gathered."));
					return;
				}

				//Check if valid roles
				IRole roleToGather = Actions.getRole(Context.Guild, values[0]);
				if (roleToGather == null)
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid role to gather."));
					return;
				}

				//Get the roles and their edit ability
				IRole roleToGive = await Actions.getRoleEditAbility(Context, values[1]);
				if (roleToGive == null)
				{
					return;
				}

				//Check if trying to give @everyone
				if (Context.Guild.EveryoneRole.Id.Equals(roleToGive.Id))
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, "You can't give the `@everyone` role.");
					return;
				}

				//Grab each user and give them the role
				List<IGuildUser> listUsersWithRole = new List<IGuildUser>();
				foreach (IGuildUser user in (await Context.Guild.GetUsersAsync()).ToList())
				{
					if (user.RoleIds.Contains(roleToGather.Id))
					{
						listUsersWithRole.Add(user);
					}
				}

				//Checking if too many users listed
				if (listUsersWithRole.Count() > maxLength)
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Too many users; max is 10."));
					return;
				}
				foreach (IGuildUser user in listUsersWithRole)
				{
					await Actions.giveRole(user, roleToGive);
				}

				await Actions.sendChannelMessage(Context.Channel, String.Format("Successfully gave `{0}` to all users{1} ({2} users).",
					roleToGive.Name, Context.Guild.EveryoneRole.Id.Equals(roleToGather.Id) ? "" : " with `" + roleToGather.Name + "`", listUsersWithRole.Count()));
			}
			else if (action.Equals("take"))
			{
				//Check if valid roles
				IRole roleToGather = Actions.getRole(Context.Guild, values[0]);
				if (roleToGather == null)
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid role to gather."));
					return;
				}
				IRole roleToTake = await Actions.getRoleEditAbility(Context, values[1]);
				if (roleToTake == null)
				{
					return;
				}

				//Check if trying to take @everyone
				if (Context.Guild.EveryoneRole.Id.Equals(roleToTake.Id))
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, "You can't take the `@everyone` role.");
					return;
				}

				//Grab each user and give them the role
				List<IGuildUser> listUsersWithRole = new List<IGuildUser>();
				foreach (IGuildUser user in (await Context.Guild.GetUsersAsync()).ToList())
				{
					if (user.RoleIds.Contains(roleToGather.Id))
					{
						listUsersWithRole.Add(user);
					}
				}

				//Checking if too many users listed
				if (listUsersWithRole.Count() > maxLength)
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Too many users; max is 10."));
					return;
				}
				foreach (IGuildUser user in listUsersWithRole)
				{
					await Actions.takeRole(user, roleToTake);
				}

				await Actions.sendChannelMessage(Context.Channel, String.Format("Successfully took `{0}` from all users{1} ({2} users).",
					roleToTake.Name, Context.Guild.EveryoneRole.Id.Equals(roleToGather.Id) ? "" : " with `" + roleToGather.Name + "`", listUsersWithRole.Count()));
			}
			else if (action.Equals("nickname"))
			{
				//Check if valid role
				IRole roleToGather = Actions.getRole(Context.Guild, values[0]);
				if (roleToGather == null)
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid role to gather."));
					return;
				}

				//Check if valid nickname length
				string inputNickname = values[1];
				if (inputNickname.Length > Constants.NICKNAME_LENGTH)
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Nicknames cannot be longer than 32 charaters."));
					return;
				}

				//Rename each user who has the role
				int botPosition = Actions.getPosition(Context.Guild, await Context.Guild.GetUserAsync(Variables.Bot_ID));
				int commandUserPosition = Actions.getPosition(Context.Guild, Context.User as IGuildUser);
				List<IGuildUser> listUsersWithRole = new List<IGuildUser>();
				foreach (IGuildUser user in (await Context.Guild.GetUsersAsync()).ToList())
				{
					if (user.RoleIds.Contains(roleToGather.Id))
					{
						int userPosition = Actions.getPosition(Context.Guild, Context.User as IGuildUser);
						if (userPosition < commandUserPosition && userPosition < botPosition && Context.Guild.OwnerId != user.Id)
						{
							listUsersWithRole.Add(user);
						}
					}
				}

				//Checking if too many users listed
				if (listUsersWithRole.Count() > maxLength)
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Too many users; max is 250."));
					return;
				}
				foreach (IGuildUser user in listUsersWithRole)
				{
					await user.ModifyAsync(x => x.Nickname = inputNickname);
				}

				await Actions.sendChannelMessage(Context.Channel, String.Format("Successfully gave the nickname `{0}` to all users{1} ({2} users).",
					inputNickname, Context.Guild.EveryoneRole.Id.Equals(roleToGather.Id) ? "" : " with `" + roleToGather.Name + "`", listUsersWithRole.Count()));
			}
			else
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid action."));
				return;
			}
		}

		[Command("listuserswithname")]
		[Alias("luwn")]
		[Usage(Constants.BOT_PREFIX + "listuserswithname [Name]")]
		[Summary("Lists all users where their username contains the given string.")]
		[UserHasAPermission]
		public async Task ListUsersWithName([Remainder] string input)
		{
			//Find the users
			var users = (await Context.Guild.GetUsersAsync()).Where(x => x.Username.IndexOf(input, StringComparison.OrdinalIgnoreCase) >= 0).OrderBy(x => x.JoinedAt).ToList();

			//Initialize the string
			string description = "";

			//Add them to the string
			int count = 1;
			users.ForEach(x =>
			{
				description += String.Format("`{0}.` `{1}#{2}` ID: `{3}`\n", count++.ToString("00"), x.Username, x.Discriminator, x.Id);
			});

			//Set the title
			string title = String.Format("Users With Names Containing '{0}'", input);

			//See if the string length is over the check amount
			if (description.Length > Constants.LENGTH_CHECK)
			{
				if (!Constants.TEXT_FILE)
				{
					//Upload the embed with the hastebin links
					EmbedBuilder uploadEmbed = Actions.makeNewEmbed(null, title, Actions.uploadToHastebin(Actions.replaceMessageCharacters(description)));
					await Actions.sendEmbedMessage(Context.Channel, uploadEmbed);
					return;
				}
				else
				{
					//Upload a file that is deleted after upload
					await Actions.uploadTextFile(Context.Guild, Context.Channel, Actions.replaceMessageCharacters(description), "List_Users_With_Name_", title);
					return;
				}
			}

			//Make and send the embed
			await Actions.sendEmbedMessage(Context.Channel, Actions.makeNewEmbed(null, title, description));
		}

		[Command("guildname")]
		[Alias("gn")]
		[Usage(Constants.BOT_PREFIX + "guildname [New Name]")]
		[Summary("Change the name of the guild to the given name.")]
		[PermissionRequirements(1U << (int)GuildPermission.ManageGuild)]
		public async Task ChangeGuildName([Remainder] string input)
		{
			//Guild names have the same length requirements as channel names, so I'm not changing the variable names

			//Check if valid length
			if (input.Length > Constants.CHANNEL_NAME_MAX_LENGTH)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Guild names cannot be longer than 100 characters."));
				return;
			}
			else if (input.Length < Constants.CHANNEL_NAME_MIN_LENGTH)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Guild names cannot be shorter than 2 characters."));
				return;
			}

			//Change the name and say what it was changed to
			await Context.Guild.ModifyAsync(x => x.Name = input);
			await Actions.makeAndDeleteSecondaryMessage(Context, String.Format("Successfully changed the guild name to `{0}`.", input));
		}

		[Command("guildregion")]
		[Alias("greg")]
		[Usage(Constants.BOT_PREFIX + "guildregion [Regions|Current|Region ID]")]
		[Summary("Shows or changes the guild's server region. `Regions` lists all valid region IDs.")]
		[PermissionRequirements(1U << (int)GuildPermission.ManageGuild)]
		public async Task ChangeGuildLocation([Remainder] string input)
		{
			//Check if a valid region or asking to see the region types
			if (input.ToLower().Equals("regions"))
			{
				await Actions.sendEmbedMessage(Context.Channel, Actions.makeNewEmbed(null, "Region IDs", String.Join("\n", Constants.VALIDREGIONIDS)));
			}
			else if (input.ToLower().Equals("current"))
			{
				await Actions.sendChannelMessage(Context.Channel, String.Format("The guild's current server region is `{0}`.", Context.Guild.VoiceRegionId));
			}
			else if (Constants.VALIDREGIONIDS.Contains(input.ToLower()))
			{
				//Capture the previous region
				string bRegion = Context.Guild.VoiceRegionId;

				//Change the region
				await Context.Guild.ModifyAsync(x => x.RegionId = input);
				await Actions.makeAndDeleteSecondaryMessage(Context, String.Format("Successfully changed the server region of the guild from `{0}` to `{1}`.", bRegion, input));
			}
			else
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("No valid region ID was input."));
			}
		}

		[Command("guildafk")]
		[Alias("gafk")]
		[Usage(Constants.BOT_PREFIX + "guildafk [Channel|Time] [Voice Channel Name|Time in Seconds]")]
		[Summary("The first argument tells if the channel or timer is going to be changed. The second is what it will be changed to.")]
		[PermissionRequirements(1U << (int)GuildPermission.ManageGuild)]
		public async Task ChangeGuildAFK([Remainder] string input)
		{
			//Split at space into two args
			string[] inputArray = input.Split(new char[] { ' ' }, 2);

			//Check if valid number of args
			if (inputArray.Length != 2)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR));
				return;
			}

			//Check if valid action
			if (inputArray[0].ToLower().Equals("channel"))
			{
				//Check if valid channel
				IGuildChannel channel = await Actions.getChannelEditAbility(Context, inputArray[1] + "/voice");
				if (channel == null)
					return;
				else if (Actions.getChannelType(channel) != Constants.VOICE_TYPE)
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("The guild's afk channel has to be a voice channel."));
					return;
				}

				//Capture the before channel
				string bChan = Context.Guild.AFKChannelId.HasValue ? (await Context.Guild.GetChannelAsync(Context.Guild.AFKChannelId.Value)).Name : "Nothing";

				//Change the afk channel to the input
				await Context.Guild.ModifyAsync(x => x.AfkChannelId = channel.Id);
				await Actions.makeAndDeleteSecondaryMessage(Context, String.Format("Successfully changed the guild's AFK channel from `{0}` to `{1}`", bChan, inputArray[1]));
			}
			else if (inputArray[0].ToLower().Equals("time"))
			{
				//Check if valid time
				int time = 0;
				if (!int.TryParse(inputArray[1], out time))
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid input for time."));
					return;
				}

				//Check if valid amount
				int[] validAmount = { 60, 300, 900, 1800, 3600 };
				if (!validAmount.Contains(time))
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid time input, must be one of the following: `" + String.Join("`, `", validAmount) + "`."));
					return;
				}

				//Capture the before timer
				string bTime = Context.Guild.AFKTimeout.ToString();

				//Change the afk timer
				await Context.Guild.ModifyAsync(x => x.AfkTimeout = time);
				await Actions.makeAndDeleteSecondaryMessage(Context, String.Format("Successfully changed the guild's AFK timer from `{0}` to `{1}`.", bTime, inputArray[1]));
			}
			else
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("No valid action was input."));
			}
		}

		[Command("guildmsgnotifications")]
		[Alias("gmn")]
		[Usage(Constants.BOT_PREFIX + "guildmsgnotifications [All Messages|Mentions Only]")]
		[Summary("Changes the message notifications to either all messages or mentions only.")]
		[PermissionRequirements(1U << (int)GuildPermission.ManageGuild)]
		public async Task ChangeGuildMsgNotifications([Remainder] string input)
		{
			if (input.ToLower().Equals("all messages"))
			{
				await Context.Guild.ModifyAsync(x => x.DefaultMessageNotifications = DefaultMessageNotifications.AllMessages);
				await Actions.makeAndDeleteSecondaryMessage(Context, "Successfully changed the default message notification setting to all messages.");
			}
			else if (input.ToLower().Equals("mentions only"))
			{
				await Context.Guild.ModifyAsync(x => x.DefaultMessageNotifications = DefaultMessageNotifications.MentionsOnly);
				await Actions.makeAndDeleteSecondaryMessage(Context, "Successfully changed the default message notification setting to mentions only.");
			}
			else
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid message notification setting."));
			}
		}

		[Command("guildverification")]
		[Alias("gv")]
		[Usage(Constants.BOT_PREFIX + "guildverification [0|1|2|3]")]
		[Summary("Changes the verification level. 0 is the most lenient (no requirements to type), 3 is the harshest (10 minutes in the guild before new members can type).")]
		[PermissionRequirements(1U << (int)GuildPermission.ManageGuild)]
		public async Task ChangeGuildVerification([Remainder] string input)
		{
			//Check if valid int
			int vLevel = -1;
			if (!int.TryParse(input, out vLevel))
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid verification level."));
				return;
			}

			//Check if valid verification level position
			if (vLevel > 3 || vLevel < 0)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid verification level. Verification levels range from 0 to 3."));
				return;
			}
			else
			{
				//Change the verification level
				await Context.Guild.ModifyAsync(x => x.VerificationLevel = (VerificationLevel)vLevel);
				
				//Get the verification level's name as a string
				string vString = Enum.GetName(typeof(VerificationLevel), vLevel);
				//Send a success message
				await Actions.makeAndDeleteSecondaryMessage(Context, String.Format("Successfully set the guild verification level as `{0}`.", vString));
			}
		}

		[Command("guildicon")]
		[Alias("gi")]
		[Usage(Constants.BOT_PREFIX + "guildicon [Attached Image|Embedded Image|Remove]")]
		[Summary("Changes the guild icon to the given image. Must be less than 2.5MB simply because the bot would use more data and be slower otherwise.")]
		[PermissionRequirements(1U << (int)GuildPermission.ManageGuild)]
		public async Task ChangeGuildIcon([Optional] string input)
		{
			//See if the user wants to remove the icon
			if (input.ToLower().Equals("remove"))
			{
				await Context.Guild.ModifyAsync(x => x.Icon = new Image());
				await Actions.sendChannelMessage(Context.Channel, "Successfully removed the guild's icon.");
				return;
			}

			//Check if there are even any attachments or embeds
			if (Context.Message.Attachments.Count + Context.Message.Embeds.Count == 0)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("No attached or embedded image."));
				return;
			}
			//Check if there are too many
			else if (Context.Message.Attachments.Count + Context.Message.Embeds.Count > 1)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Too many attached or embedded images."));
				return;
			}

			//Get the URL of the image
			string imageURL = null;
			if (Context.Message.Embeds.Count == 1)
			{
				imageURL = Context.Message.Embeds.First().Thumbnail.ToString();
			}
			else
			{
				imageURL = Context.Message.Attachments.First().Url;
			}

			//Run separate due to the time it takes
			var downloadUploadAndDelete = Task.Run(async () =>
			{
				//Check the image's file size first
				WebRequest req = HttpWebRequest.Create(imageURL);
				req.Method = "HEAD";
				using (WebResponse resp = req.GetResponse())
				{
					int ContentLength = 0;
					if (int.TryParse(resp.Headers.Get("Content-Length"), out ContentLength))
					{
						if (ContentLength > 2625000)
						{
							//Check if bigger than 2.5MB
							await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Image is bigger than 2.5MB. Please manually upload instead."));
							return;
						}
						else if (ContentLength == 0)
						{
							//Check if nothing was gotten
							await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Unable to get the image's file size."));
							return;
						}
						else
						{
							//Check if valid content type
							if (!Constants.VALIDIMAGEEXTENSIONS.Contains("." + resp.Headers.Get("Content-Type").Split('/').Last()))
							{
								await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Image must be a png or jpg."));
								return;
							}
						}
					}
					else
					{
						await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Unable to get the image's file size."));
						return;
					}
				}

				//Set the name of the file to prevent typos between the three places that use it
				string path = Actions.getServerFilePath(Context.Guild.Id, "guildicon" + Path.GetExtension(imageURL).ToLower());

				//Download the image
				using (var webclient = new WebClient())
				{
					webclient.DownloadFile(imageURL, path);
				}

				//Create a filestream to check the image's size
				using (FileStream imgStream = new FileStream(path, FileMode.Open, FileAccess.Read))
				{
					var img = System.Drawing.Image.FromStream(imgStream);
					if (img.Width < 128 || img.Height < 128)
					{
						await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Images must be at least 128x128 pixels."));
						return;
					}
				}

				//Create a second filestream to upload the image
				using (FileStream imgStream = new FileStream(path, FileMode.Open, FileAccess.Read))
				{
					//Change the guild's icon to the downloaded image
					await Context.Guild.ModifyAsync(x => x.Icon = new Image(imgStream));
				}

				//Delete the file and send a success message
				File.Delete(path);
				await Actions.sendChannelMessage(Context.Channel, "Successfully changed the guild icon.");
			});
		}

		[Command("listemojis")]
		[Alias("lemojis")]
		[Usage(Constants.BOT_PREFIX + "listemojis [Global|Guild]")]
		[Summary("Lists the emoji in the guild. As of right now, with the current API wrapper version this bot uses, there's no way to upload or remove emojis yet; sorry.")]
		[PermissionRequirements(1U << (int)GuildPermission.ManageEmojis)]
		public async Task ListEmojis([Remainder] string input)
		{
			//Make the string
			string description = null;

			//Add the emojis to the string
			int count = 1;
			if (input.ToLower().Equals("guild"))
			{
				//Get all of the guild emojis
				Context.Guild.Emojis.Where(x => !x.IsManaged).ToList().ForEach(x =>
				{
						description += String.Format("`{0}.` <:{1}:{2}> `{3}`\n", count++.ToString("00"), x.Name, x.Id, x.Name);
				});
			}
			else if (input.ToLower().Equals("global"))
			{
				//Get all of the global emojis
				Context.Guild.Emojis.Where(x => x.IsManaged).ToList().ForEach(x =>
				{
					description += String.Format("`{0}.` <:{1}:{2}> `{3}`\n", count++.ToString("00"), x.Name, x.Id, x.Name);
				});
			}
			else
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid option."));
				return;
			}

			//Check if the description is still null
			description = description ?? String.Format("This guild has no {0} emojis.", input.ToLower());

			//Send the embed
			await Actions.sendEmbedMessage(Context.Channel, Actions.makeNewEmbed(null, "Emojis", description));
		}
	}
}