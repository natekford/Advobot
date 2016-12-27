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

namespace Advobot
{
	public class Moderation_Commands : ModuleBase
	{
		[Command("fullmute")]
		[Alias("fm")]
		[Usage(Constants.BOT_PREFIX + "fullmute [@User]")]
		[Summary("Removes the user's ability to speak and type via the 'Muted' role.")]
		[PermissionRequirements(0, (1U << (int)GuildPermission.ManageRoles) | (1U << (int)GuildPermission.ManageMessages))]
		public async Task FullMute([Remainder] String input)
		{
			//Check if role already exists, if not, create it
			IRole muteRole = await Actions.createRoleIfNotFound(Context.Guild, Constants.MUTE_ROLE_NAME);
			if (muteRole == null)
				return;

			//See if both the bot and the user can edit/use this role
			if (await Actions.getRoleEditAbility(Context, Constants.MUTE_ROLE_NAME, false) == null)
				return;

			//Test if valid user mention
			IGuildUser user = await Actions.getUser(Context.Guild, input);
			if (user == null)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.USER_ERROR), Constants.WAIT_TIME);
				return;
			}

			//Give the targetted user the role
			await Actions.giveRole(user, muteRole);
			await Actions.makeAndDeleteSecondaryMessage(Context, String.Format("Successfully muted {0}.", user.Mention), Constants.WAIT_TIME);
		}

		[Command("fullunmute")]
		[Alias("fum", "chum")]
		[Usage(Constants.BOT_PREFIX + "fullunmute [@User]")]
		[Summary("Gives the user back the ability to speak and type via removing the 'Muted' role.")]
		[PermissionRequirements(0, (1U << (int)GuildPermission.ManageRoles) | (1U << (int)GuildPermission.ManageMessages))]
		public async Task FullUnmute([Remainder] String input)
		{
			//Test if valid user mention
			IGuildUser user = await Actions.getUser(Context.Guild, input);
			if (user == null)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.USER_ERROR), Constants.WAIT_TIME);
				return;
			}

			//Remove the role
			await Actions.takeRole(user, Actions.getRole(Context.Guild, Constants.MUTE_ROLE_NAME));
			await Actions.makeAndDeleteSecondaryMessage(Context, String.Format("Successfully unmuted {0}.", user.Mention), Constants.WAIT_TIME);
		}

		[Command("kick")]
		[Alias("k")]
		[Usage(Constants.BOT_PREFIX + "kick [@User]")]
		[Summary("Kicks the user from the guild.")]
		[PermissionRequirements(0, (1U << (int)GuildPermission.KickMembers))]
		public async Task Kick([Remainder] String input)
		{
			//Test if valid user mention
			IGuildUser inputUser = await Actions.getUser(Context.Guild, input);
			if (inputUser == null)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.USER_ERROR), Constants.WAIT_TIME);
				return;
			}

			//Determine if the user is allowed to kick this person
			int kickerPosition = Actions.userHasOwner(Context.Guild, (Context.User as IGuildUser)) ?
				Constants.OWNER_POSITION : Actions.getPosition(Context.Guild, (Context.User as IGuildUser));
			int kickeePosition = Actions.getPosition(Context.Guild, inputUser);
			if (kickerPosition <= kickeePosition)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("User is unable to be kicked by you."), Constants.WAIT_TIME);
				return;
			}

			//Determine if the bot can kick this person
			if (Actions.getPosition(Context.Guild, await Context.Guild.GetUserAsync(Variables.Bot_ID)) <= kickeePosition)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Bot is unable to kick user."), Constants.WAIT_TIME);
				return;
			}

			//Kick the targetted user
			await inputUser.KickAsync();
			await Actions.makeAndDeleteSecondaryMessage(Context, String.Format("Successfully kicked {0}#{1} with the ID `{2}`.",
				inputUser.Username, inputUser.Discriminator, inputUser.Id), Constants.WAIT_TIME);
		}

		[Command("softban")]
		[Alias("sb")]
		[Usage(Constants.BOT_PREFIX + "softban [@User]")]
		[Summary("Bans then unbans a user from the guild.")]
		[PermissionRequirements(0, (1U << (int)GuildPermission.BanMembers))]
		public async Task SoftBan([Remainder] String input)
		{
			//Test if valid user mention
			IGuildUser inputUser = await Actions.getUser(Context.Guild, input);
			if (inputUser == null)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.USER_ERROR), Constants.WAIT_TIME);
				return;
			}

			//Determine if the user is allowed to softban this person
			int sberPosition = Actions.userHasOwner(Context.Guild, (Context.User as IGuildUser)) ?
				Constants.OWNER_POSITION : Actions.getPosition(Context.Guild, (Context.User as IGuildUser));
			int sbeePosition = Actions.getPosition(Context.Guild, inputUser);
			if (sberPosition <= sbeePosition)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("User is unable to be soft-banned by you."), Constants.WAIT_TIME);
				return;
			}

			//Determine if the bot can softban this person
			if (Actions.getPosition(Context.Guild, await Context.Guild.GetUserAsync(Variables.Bot_ID)) <= sbeePosition)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Bot is unable to soft-ban user."), Constants.WAIT_TIME);
				return;
			}

			//Softban the targetted use
			await Context.Guild.AddBanAsync(inputUser);
			await Context.Guild.RemoveBanAsync(inputUser);
			await Actions.makeAndDeleteSecondaryMessage(Context, String.Format("Successfully banned and unbanned {0}.", inputUser.Mention), Constants.WAIT_TIME);
		}

		[Command("ban")]
		[Alias("b")]
		[Usage(Constants.BOT_PREFIX + "ban [@User]")]
		[Summary("Bans the user from the guild.")]
		[PermissionRequirements(0, (1U << (int)GuildPermission.BanMembers))]
		public async Task Ban([Remainder] String input)
		{

			//Test number of arguments
			String[] values = input.Split(' ');
			if ((values.Length < 1) || (values.Length > 2))
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR), Constants.WAIT_TIME);
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
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.USER_ERROR), Constants.WAIT_TIME);
				return;
			}

			//Determine if the user is allowed to ban this person
			int bannerPosition = Actions.userHasOwner(Context.Guild, await Context.Guild.GetUserAsync(Context.User.Id) as IGuildUser) ?
				Constants.OWNER_POSITION : Actions.getPosition(Context.Guild, await Context.Guild.GetUserAsync(Context.User.Id) as IGuildUser);
			int banneePosition = Actions.getPosition(Context.Guild, inputUser);
			if (bannerPosition <= banneePosition)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("User is unable to be banned by you."), Constants.WAIT_TIME);
				return;
			}

			//Determine if the bot can ban this person
			if (Actions.getPosition(Context.Guild, await Context.Guild.GetUserAsync(Variables.Bot_ID)) <= banneePosition)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Bot is unable to ban user."), Constants.WAIT_TIME);
				return;
			}

			//Checking for valid days requested
			int pruneDays = 0;
			if (values.Length == 2 && !Int32.TryParse(values[1], out pruneDays))
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Incorrect input for days of messages to be deleted."), Constants.WAIT_TIME);
				return;
			}

			//Forming the second half of the string that prints out when a user is successfully banned
			String plurality = "days";
			if (pruneDays == 1)
			{
				plurality = "day";
			}
			String latterHalfOfString = "";
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
			await Actions.makeAndDeleteSecondaryMessage(Context, String.Format("Successfully banned {0}#{1} `{2}`{3}",
				inputUser.Username, inputUser.Discriminator, inputUser.Id, latterHalfOfString), 10000);
		}

		[Command("unban")]
		[Alias("ub")]
		[Usage(Constants.BOT_PREFIX + "unban [User|User#Discriminator|User ID]")]
		[Summary("Unbans the user from the guild.")]
		[PermissionRequirements(0, (1U << (int)GuildPermission.BanMembers))]
		public async Task Unban([Remainder] String input)
		{
			//Cut the user mention into the username and the discriminator
			String[] values = input.Split('#');
			if (values.Length < 1 || values.Length > 2)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR), Constants.WAIT_TIME);
				return;
			}

			//Get a list of the bans
			var bans = await Context.Guild.GetBansAsync();

			//Get their name and discriminator or ulong
			ulong inputUserID;
			String secondHalfOfTheSecondaryMessage = "";
			
			if (values.Length == 2)
			{
				//Unban given a username and discriminator
				ushort discriminator;
				if (!ushort.TryParse(values[1], out discriminator))
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid discriminator provided."), Constants.WAIT_TIME);
				}
				String username = values[0].Replace("@", "");

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
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("No user on the ban list has that username."), Constants.WAIT_TIME);
					return;
				}
			}
			else
			{
				//Unban given a user ID
				if (Actions.getUlong(input).Equals(0))
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid user ID."), Constants.WAIT_TIME);
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
		[PermissionRequirements(0, (1U << (int)GuildPermission.BanMembers))]
		public async Task CurrentBanList()
		{
			//New list to hold the ban information
			List<String> banStrings = new List<String>();

			//Get and add the ban information
			var bans = await Context.Guild.GetBansAsync();
			bans.ToList().ForEach(x => banStrings.Add("`" + x.User.Username + "#" + x.User.Discriminator + "` **ID:** `" + x.User.Id + "`"));

			//Check the length of the message
			int lengthCheck = 0;
			banStrings.ForEach(x => lengthCheck += x.Length);
			if (lengthCheck > 1900)
			{
				if (!Constants.TEXT_FILE)
				{
					await Actions.sendChannelMessage(
						Context.Channel, "`[" + DateTime.UtcNow.ToString("HH:mm:ss") + "]` **BANS:**\n" + Actions.uploadToHastebin(Context.Channel, banStrings));
				}
				else
				{
					await Actions.uploadTextFile(Context.Guild, Context.Channel, banStrings, "Current_Ban_List_", "BANS");
				}
				return;
			}
			else if (lengthCheck == 0)
			{
				await Actions.sendChannelMessage(Context.Channel, "This guild has no bans.");
				return;
			}
			await Actions.sendChannelMessage(Context.Channel, String.Join("\n", banStrings));
		}

		[Command("removemessages")]
		[Alias("rm")]
		[Usage(Constants.BOT_PREFIX + "removemessages <@User> <#Channel> [Number of Messages]")]
		[Summary("Removes the selected number of messages from either the user, the channel, both, or, if neither is input, the current channel." +
			"People without administrator can only delete up to 100 messages at a time.")]
		[PermissionRequirements(0, (1U << (int)GuildPermission.ManageMessages))]
		public async Task RemoveMessages([Optional][Remainder] String input)
		{
			String[] values = input.Split(' ');
			if ((values.Length < 1) || (values.Length > 3))
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR), Constants.WAIT_TIME);
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
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.USER_ERROR), Constants.WAIT_TIME);
					return;
				}
				++argIndex;
			}

			//Testing if starts with channel mention
			IMessageChannel inputChannel = Context.Channel;
			if (argIndex < argCount && values[argIndex].StartsWith("<#"))
			{
				inputChannel = await Actions.getChannelID(Context.Guild, values[argIndex]);
				if (inputChannel == null)
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.CHANNEL_ERROR), Constants.WAIT_TIME);
					return;
				}
				++argIndex;
			}

			//Checking for valid request count
			int requestCount = (argIndex == argCount - 1) ? Actions.getInteger(values[argIndex]) : -1;
			if (requestCount < 1)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Incorrect input for number of messages to be removed."), Constants.WAIT_TIME);
				return;
			}

			//See if the user is trying to delete more than 100 messages at a time
			if (requestCount > 100 && !(await Context.Guild.GetUserAsync(Context.User.Id)).GuildPermissions.Administrator)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("You need administrator to remove more than 100 messages at a time."), Constants.WAIT_TIME);
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
			await Actions.makeAndDeleteSecondaryMessage(Context, String.Format("Successfully deleted {0} {1}{2}{3}.",
				requestCount,
				requestCount > 1 ? "messages" : "message",
				inputUser == null ? "" : " from " + inputUser.Mention,
				inputChannel == null ? "" : " on `" + inputChannel.Name + "`"),
				2000);
		}

		[Command("giverole")]
		[Alias("gr")]
		[Usage(Constants.BOT_PREFIX + "giverole [@User] [Role]/<Role>/...")]
		[Summary("Gives the user the role (assuming the person using the command and bot both have the ability to give that role).")]
		[PermissionRequirements(0, (1U << (int)GuildPermission.ManageRoles))]
		public async Task GiveRole([Remainder] String input)
		{
			//Test number of arguments
			String[] values = input.Split(new char[] { ' ' }, 2);
			if (values.Length != 2)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR), Constants.WAIT_TIME);
				return;
			}

			//Test if valid user mention
			IGuildUser inputUser = await Actions.getUser(Context.Guild, values[0]);
			if (inputUser == null)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.USER_ERROR), Constants.WAIT_TIME);
				return;
			}

			//Determine if the role exists and if it is able to be edited by both the bot and the user
			List<String> inputRoles = values[1].Split('/').ToList();
			if (inputRoles.Count == 1)
			{
				//Check if it actually exists
				IRole role = await Actions.getRoleEditAbility(Context, inputRoles[0], false);
				if (role == null)
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ROLE_ERROR), Constants.WAIT_TIME);
					return;
				}
				//See if the role is unable to be given due to management
				else if (role.IsManaged)
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Role is managed and unable to be given."), Constants.WAIT_TIME);
					return;
				}
				//Give the role and make a message
				await Actions.giveRole(inputUser, role);
				await Actions.makeAndDeleteSecondaryMessage(Context,
					String.Format("Successfully gave {0} `{1}`.", inputUser.Mention, role), Constants.WAIT_TIME);
			}
			else
			{
				List<String> failedRoles = new List<String>();
				List<String> succeededRoles = new List<String>();
				List<IRole> roles = new List<IRole>();
				foreach (String roleName in inputRoles)
				{
					IRole role = await Actions.getRoleEditAbility(Context, roleName, true);
					if (role == null || role.IsManaged)
					{
						failedRoles.Add(roleName);
					}
					else
					{
						roles.Add(role);
						succeededRoles.Add(roleName);
					}
				}
				await Actions.giveRole(inputUser, roles.ToArray());
				await Actions.makeAndDeleteSecondaryMessage(Context,
					String.Format("Successfully gave {0} `{1}` and failed to give `{2}`.",
					inputUser.Mention,
					succeededRoles.Count > 0 ? String.Join(", ", succeededRoles) : "NOTHING",
					failedRoles.Count > 0 ? String.Join(", ", failedRoles) : "NOTHING"),
					Constants.WAIT_TIME);
			}
		}

		[Command("takerole")]
		[Alias("tr")]
		[Usage(Constants.BOT_PREFIX + "takerole [@User] [Role]")]
		[Summary("Take the role from the user (assuming the person using the command and bot both have the ability to take that role).")]
		[PermissionRequirements(0, (1U << (int)GuildPermission.ManageRoles))]
		public async Task TakeRole([Remainder] String input)
		{
			//Test number of arguments
			String[] values = input.Split(new char[] { ' ' }, 2);
			if (values.Length != 2)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR), Constants.WAIT_TIME);
				return;
			}

			//Test if valid user mention
			IGuildUser inputUser = await Actions.getUser(Context.Guild, values[0]);
			if (inputUser == null)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.USER_ERROR), Constants.WAIT_TIME);
				return;
			}

			//Determine if the role exists and if it is able to be edited by both the bot and the user
			List<String> inputRoles = values[1].Split('/').ToList();
			if (inputRoles.Count == 1)
			{
				//Check if it actually exists
				IRole role = await Actions.getRoleEditAbility(Context, inputRoles[0], false);
				if (role == null)
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ROLE_ERROR), Constants.WAIT_TIME);
					return;
				}
				//See if the role is unable to be taken due to management
				else if (role.IsManaged)
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Role is managed and unable to be taken."), Constants.WAIT_TIME);
					return;
				}
				//Take the role and make a message
				await Actions.takeRole(inputUser, role);
				await Actions.makeAndDeleteSecondaryMessage(Context,
					String.Format("Successfully took `{0}` from {1}.", role, inputUser.Mention), Constants.WAIT_TIME);
			}
			else
			{
				List<String> failedRoles = new List<String>();
				List<String> succeededRoles = new List<String>();
				List<IRole> roles = new List<IRole>();
				foreach (String roleName in inputRoles)
				{
					IRole role = await Actions.getRoleEditAbility(Context, roleName, true);
					if (role == null || role.IsManaged)
					{
						failedRoles.Add(roleName);
					}
					else
					{
						roles.Add(role);
						succeededRoles.Add(roleName);
					}
				}
				await Actions.takeRole(inputUser, roles.ToArray());
				await Actions.makeAndDeleteSecondaryMessage(Context,
					String.Format("Successfully took `{1}` and failed to take `{2}` from {0}.",
					inputUser.Mention,
					succeededRoles.Count > 0 ? String.Join(", ", succeededRoles) : "NOTHING",
					failedRoles.Count > 0 ? String.Join(", ", failedRoles) : "NOTHING"),
					Constants.WAIT_TIME);
			}
		}

		[Command("createrole")]
		[Alias("cr")]
		[Usage(Constants.BOT_PREFIX + "createrole [Role]")]
		[Summary("Adds a role to the guild with the chosen name.")]
		[PermissionRequirements(0, (1U << (int)GuildPermission.ManageRoles))]
		public async Task CreateRole([Remainder] String input)
		{
			//Check length
			if (input.Length > Constants.ROLE_NAME_LENGTH)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Roles can only have a name length of up to 32 characters."), Constants.WAIT_TIME);
				return;
			}

			//Create role
			await Context.Guild.CreateRoleAsync(input, new GuildPermissions(0));
			await Actions.makeAndDeleteSecondaryMessage(Context, String.Format("Successfully created the `{0}` role.", input), Constants.WAIT_TIME);
		}

		[Command("softdeleterole")]
		[Alias("sdrole", "sdr")]
		[Usage(Constants.BOT_PREFIX + "softdeleterole [Role]")]
		[Summary("Removes all permissions from a role (and all channels the role had permissions on) and removes the role from everyone. Leaves the name and color behind.")]
		[PermissionRequirements(0, (1U << (int)GuildPermission.ManageRoles))]
		public async Task SoftDeleteRole([Remainder] String input)
		{
			//Determine if the role exists and if it is able to be edited by both the bot and the user
			IRole inputRole = await Actions.getRoleEditAbility(Context, input, false);
			if (inputRole == null)
				return;

			//Check if even removable
			if (inputRole.IsManaged)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Role is managed and unable to be softdeleted."), Constants.WAIT_TIME);
				return;
			}

			//Create a new role with the same attributes (including space) and no perms
			IRole newRole = await Context.Guild.CreateRoleAsync(inputRole.Name, new GuildPermissions(0), inputRole.Color);
			//TODO: Get changing position of a role to work
			await newRole.ModifyAsync(x => x.Position = inputRole.Position);

			//Delete the old role
			await inputRole.DeleteAsync();
			await Actions.makeAndDeleteSecondaryMessage(Context,
				String.Format("Successfully removed all permissions from `{0}` and removed the role from all users on the guild.", inputRole.Name), Constants.WAIT_TIME);
		}

		[Command("deleterole")]
		[Alias("drole", "dr")]
		[Usage(Constants.BOT_PREFIX + "deleterole [Role]")]
		[Summary("Deletes the role. 'Drole' is a pretty funny alias.")]
		[PermissionRequirements(0, (1U << (int)GuildPermission.ManageRoles))]
		public async Task DeleteRole([Remainder] String input)
		{
			//Determine if the role exists and if it is able to be edited by both the bot and the user
			IRole role = await Actions.getRoleEditAbility(Context, input, false);
			if (role == null)
				return;

			//Check if even removable
			if (role.IsManaged)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Role is managed and unable to be deleted."), Constants.WAIT_TIME);
				return;
			}

			await role.DeleteAsync();
			await Actions.makeAndDeleteSecondaryMessage(Context, String.Format("Successfully deleted the `{0}` role.", input), Constants.WAIT_TIME);
		}

		[Command("rolepermissions")]
		[Alias("erp", "rp")]
		[Usage(Constants.BOT_PREFIX + "rolepermissions [Show|Add|Remove] [Role] [Permission/...]")]
		[Summary("Add/remove the selected permissions to/from the role. Permissions must be separated by a '/'!" +
			"Type '>rolepermissions [Show]' to see the available permissions. Type '>rolepermissions [Show] [Role]' to see the permissions of that role.")]
		[PermissionRequirements(0, (1U << (int)GuildPermission.ManageRoles))]
		public async Task RolePermissions([Remainder] String input)
		{
			//Set the permission types into a list to later check against
			List<String> permissionTypeStrings = Variables.PermissionNames.Values.ToList();

			String[] actionRolePerms = input.ToLower().Split(new char[] { ' ' }, 2); //Separate the role and whether to add or remove from the permissions
			String permsString = null; //Set placeholder perms variable
			String roleString = null; //Set placeholder role variable
			bool show = false; //Set show bool

			//If the user wants to see the permission types, print them out
			if (input.Equals("show", StringComparison.OrdinalIgnoreCase))
			{
				await Actions.sendChannelMessage(Context.Channel, "**ROLE PERMISSIONS:**```\n" + String.Join("\n", permissionTypeStrings) + "```");
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
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR), Constants.WAIT_TIME);
					return;
				}
				int lastSpace = actionRolePerms[1].LastIndexOf(' ');
				if (lastSpace <= 0)
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR), Constants.WAIT_TIME);
					return;
				}
				//Separate out the permissions
				permsString = actionRolePerms[1].Substring(lastSpace).Trim();
				//Separate out the role
				roleString = actionRolePerms[1].Substring(0, lastSpace).Trim();
			}

			//Determine if the role exists and if it is able to be edited by both the bot and the user
			IRole role = await Actions.getRoleEditAbility(Context, roleString, false);
			if (role == null)
				return;

			//See if the role can be edited
			if (role.IsManaged)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Role is managed and unable to have its permissions changed."), Constants.WAIT_TIME);
				return;
			}

			//Send a message of the permissions the targetted role has
			if (show)
			{
				GuildPermissions rolePerms = new GuildPermissions(Context.Guild.GetRole(role.Id).Permissions.RawValue);
				List<String> currentRolePerms = new List<String>();
				foreach (var permissionValue in Variables.PermissionValues.Values)
				{
					int bit = permissionValue;
					if (((int)rolePerms.RawValue & (1 << bit)) != 0)
					{
						currentRolePerms.Add(Variables.PermissionNames[bit]);
					}
				}
				String pluralityPerms = "permissions";
				if (currentRolePerms.Count == 1)
				{
					pluralityPerms = "permission";
				}
				await Actions.sendChannelMessage(Context.Channel, String.Format("`{0}` has the following {1}: `{2}`.",
					role.Name, pluralityPerms, currentRolePerms.Count == 0 ? "NOTHING" : String.Join("`, `", currentRolePerms).ToLower()));
				return;
			}

			//See if it's add or remove
			String addOrRemove = actionRolePerms[0];
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
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Add or remove not specified."), Constants.WAIT_TIME);
				return;
			}

			//Get the permissions
			List<String> permissions = permsString.Split('/').ToList();
			//Check if valid permissions
			List<String> validPerms = permissions.Intersect(permissionTypeStrings, StringComparer.OrdinalIgnoreCase).ToList();
			if (validPerms.Count != permissions.Count)
			{
				List<String> invalidPermissions = new List<String>();
				foreach (String permission in permissions)
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
			foreach (String permission in permissions)
			{
				List<String> perms = Variables.PermissionValues.Keys.ToList();
				try
				{
					int bit = Variables.PermissionValues[permission];
					rolePermissions |= (1U << bit);
				}
				catch (Exception)
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(String.Format("Couldn't parse permission '{0}'", permission)), Constants.WAIT_TIME);
					return;
				}
			}

			//Determine if the user can give these perms
			if (!Actions.userHasOwner(Context.Guild, Context.User as IGuildUser))
			{
				if (!(Context.User as IGuildUser).GuildPermissions.Administrator)
				{
					rolePermissions &= (uint)(Context.User as IGuildUser).GuildPermissions.RawValue;
				}
				//If the role has something, but the user is not allowed to edit a permissions
				if (rolePermissions == 0)
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(String.Format("You do not have the ability to modify {0}.",
						permissions.Count == 1 ? "that permission" : "those permissions")), Constants.WAIT_TIME);
					return;
				}
			}

			//Get a list of the permissions that were given
			List<String> givenPermissions = Actions.getPermissionNames(rolePermissions).ToList();
			//Get a list of the permissions that were not given
			List<String> skippedPermissions = permissions.Except(givenPermissions, StringComparer.OrdinalIgnoreCase).ToList();

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
		[PermissionRequirements(0, (1U << (int)GuildPermission.ManageRoles))]
		public async Task CopyRolePermissions([Remainder] String input)
		{
			//Put the input into a string
			input = input.ToLower();
			String[] roles = input.Split(new char[] { '/' }, 2);

			//Test if two roles were input
			if (roles.Length != 2)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR), Constants.WAIT_TIME);
				return;
			}

			//Determine if the input role exists
			IRole inputRole = await Actions.getRole(Context, roles[0]);
			if (inputRole == null)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ROLE_ERROR), Constants.WAIT_TIME);
				return;
			}

			//Determine if the role exists and if it is able to be edited by both the bot and the user
			IRole outputRole = await Actions.getRoleEditAbility(Context, roles[1], false);
			if (outputRole == null)
				return;

			if (outputRole.IsManaged)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Role is managed and unable to have its permissions changed."), Constants.WAIT_TIME);
				return;
			}

			//Get the permissions
			uint rolePermissions = (uint)inputRole.Permissions.RawValue;
			List<String> permissions = Actions.getPermissionNames(rolePermissions).ToList();
			if (rolePermissions != 0)
			{
				//Determine if the user can give these permissions
				if (!Actions.userHasOwner(Context.Guild, Context.User as IGuildUser))
				{
					if (!(Context.User as IGuildUser).GuildPermissions.Administrator)
					{
						rolePermissions &= (uint)(Context.User as IGuildUser).GuildPermissions.RawValue;
					}
					//If the role has something, but the user is not allowed to edit a permissions
					if (rolePermissions == 0)
					{
						await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(String.Format("You do not have the ability to modify {0}.",
							permissions.Count == 1 ? "that permission" : "those permissions")), Constants.WAIT_TIME);
						return;
					}
				}
			}

			//Get a list of the permissions that were given
			List<String> givenPermissions = Actions.getPermissionNames(rolePermissions).ToList();
			//Get a list of the permissions that were not given
			List<String> skippedPermissions = permissions.Except(givenPermissions).ToList();

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
		[PermissionRequirements(0, (1U << (int)GuildPermission.ManageRoles))]
		public async Task ClearRolePermissions([Remainder] String input)
		{
			//Determine if the role exists and if it is able to be edited by both the bot and the user
			IRole role = await Actions.getRoleEditAbility(Context, input, false);
			if (role == null)
			{
				return;
			}

			//See if the role can have its perms changed
			if (role.IsManaged)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Role is managed and unable to have its permissions changed."), Constants.WAIT_TIME);
				return;
			}

			//Clear the role's perms
			await role.ModifyAsync(x => x.Permissions = new GuildPermissions(0));
			await Actions.makeAndDeleteSecondaryMessage(Context, String.Format("Successfully removed all permissions from `{0}`.", input), Constants.WAIT_TIME);
		}

		[Command("changerolename")]
		[Alias("crn")]
		[Usage(Constants.BOT_PREFIX + "changerolename [Role]/[New Name]")]
		[Summary("Changes the name of the role.")]
		[PermissionRequirements(0, (1U << (int)GuildPermission.ManageRoles))]
		public async Task ChangeRoleName([Remainder] String input)
		{
			//Split at the current role name and the new role name
			String[] values = input.Split(new char[] { '/' }, 2);

			//Check if correct number of arguments
			if (values.Length != 2)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR), Constants.WAIT_TIME);
				return;
			}

			//See if the new name is a valid length
			if (values[1].Length > Constants.ROLE_NAME_LENGTH)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context,
					Actions.ERROR("Roles can only have a name length of up to 32 characters."), Constants.WAIT_TIME);
				return;
			}

			//Determine if the role exists and if it is able to be edited by both the bot and the user
			IRole role = await Actions.getRoleEditAbility(Context, values[0], false);
			if (role == null)
				return;

			//Change the name
			await Context.Guild.GetRole(role.Id).ModifyAsync(x => x.Name = values[1]);
			await Actions.makeAndDeleteSecondaryMessage(Context, String.Format("Successfully changed the name of the role `{0}` to `{1}`.",
				values[0], values[1]), Constants.WAIT_TIME);
		}

		[Command("changerolecolor")]
		[Alias("crc")]
		[Usage(Constants.BOT_PREFIX + "changerolecolor Role/[Hexadecimal|Color Name]")]
		[Summary("Changes the role's color. A color of '0' sets the role back to the default color. " +
			"Colors must either be in hexadecimal format or be a color listed here: https://msdn.microsoft.com/en-us/library/system.drawing.color(v=vs.110).aspx")]
		[PermissionRequirements(0, (1U << (int)GuildPermission.ManageRoles))]
		public async Task ChangeRoleColor([Remainder] String input)
		{
			String[] values = input.Split(new char[] { '/' }, 2);

			//Determine if the role exists and if it is able to be edited by both the bot and the user
			IRole role = await Actions.getRoleEditAbility(Context, values[0], false);
			if (role == null)
				return;

			UInt32 colorID = (UInt32)System.Drawing.Color.FromName(values[1]).ToArgb();
			if (colorID == 0)
			{
				//Couldn't get name
				String hexString = values[1];
				//Remove 0x if someone put that in there
				if (hexString.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
				{
					hexString = hexString.Substring(2);
				}
				//If the color ID isn't a hex number
				if (!UInt32.TryParse(hexString, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out colorID))
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Color is unable to be added."), Constants.WAIT_TIME);
					return;
				}
			}

			//Change the color
			await Context.Guild.GetRole(role.Id).ModifyAsync(x => x.Color = new Color(colorID & 0xffffff));
			await Actions.makeAndDeleteSecondaryMessage(Context, String.Format("Successfully changed the color of the role `{0}` to `{1}`.",
				values[0], values[1]), Constants.WAIT_TIME);
		}

		[Command("createchannel")]
		[Alias("cch")]
		[Usage(Constants.BOT_PREFIX + "createchannel [Name]/[Text|Voice]")]
		[Summary("Adds a channel to the guild of the given type with the given name. The name CANNOT contain any spaces: use underscores or dashes instead.")]
		[PermissionRequirements(0, (1U << (int)GuildPermission.ManageChannels))]
		public async Task CreateChannel([Remainder] String input)
		{
			String[] values = input.Split('/');
			String type = values[1];

			//Test for args
			if (values.Length != 2)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR), Constants.WAIT_TIME);
				return;
			}

			//Test for name validity
			if (values[0].Contains(' '))
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("No spaces allowed in a channel name."), Constants.WAIT_TIME);
				return;
			}
			else if (values[0].Length > 100 || values[0].Length < 2)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Name must be between 2 and 100 characters long."), Constants.WAIT_TIME);
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
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid channel type."), Constants.WAIT_TIME);
				return;
			}

			await Actions.makeAndDeleteSecondaryMessage(Context,
				String.Format("Successfully created `{0} ({1})`.", values[0], char.ToUpper(type[0]) + type.Substring(1)), Constants.WAIT_TIME);
		}

		[Command("softdeletechannel")]
		[Alias("sdch")]
		[Usage(Constants.BOT_PREFIX + "softdeletechannel [#Channel]")]
		[Summary("Makes most roles unable to read the channel and moves it to the bottom of the channel list. Only works for text channels.")]
		[PermissionRequirements((1U << (int)GuildPermission.ManageChannels) | (1U << (int)GuildPermission.ManageRoles), 0)]
		public async Task SoftDeleteChannel([Remainder] String input)
		{
			//See if the input name has spaces
			if (input.Contains(' '))
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.CHANNEL_ERROR), Constants.WAIT_TIME);
				return;
			}

			//See if the user can see and thus edit that channel
			IGuildChannel channel = await Actions.getChannelEditAbility(Context, input);
			if (channel == null)
				return;

			//See if not attempted on a text channel
			if (!channel.GetType().Name.ToLower().Contains(Constants.TEXT_TYPE))
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Softdelete only works on text channels inside a guild."), Constants.WAIT_TIME);
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
		[PermissionRequirements(0, (1U << (int)GuildPermission.ManageChannels))]
		public async Task DeleteChannel([Remainder] String input)
		{
			//See if the user can see and thus edit that channel
			IGuildChannel channel = await Actions.getChannelEditAbility(Context, input);
			if (channel == null)
				return;

			await channel.DeleteAsync();
			await Actions.makeAndDeleteSecondaryMessage(Context, String.Format("Successfully deleted `{0} ({1})`.", channel.Name,
				channel.GetType().Name.ToLower().Contains(Constants.TEXT_TYPE) ? "Text" : "Voice"), Constants.WAIT_TIME);
		}

		[Command("channelpermissions")]
		[Alias("echp", "chp")]
		[Usage(Constants.BOT_PREFIX + "channelpermissions [Show|Allow|Inherit|Deny] " + Constants.OPTIONAL_CHANNEL_INSTRUCTIONS + " <Role|User> <Permission/...>")]
		[Summary("Type '" + Constants.BOT_PREFIX + "chp [Show]' to see the available permissions. Permissions must be separated by a '/'! " +
			"Type '" + Constants.BOT_PREFIX + "chp [Show] [Channel]' to see all permissions on a channel. " +
			"Type '" + Constants.BOT_PREFIX + "chp [Show] [Channel] [Role|User]' to see permissions a role/user has on a channel.")]
		[PermissionRequirements((1U << (int)GuildPermission.ManageChannels) | (1U << (int)GuildPermission.ManageRoles), 0)]
		public async Task ChannelPermissions([Remainder] String input)
		{
			//Set the variables
			List<String> permissions = null;
			IGuildChannel channel = null;
			IGuildUser user = null;
			IRole role = null;

			//Split the input
			String[] values = input.ToLower().Trim().Split(new char[] { ' ' }, 2);
			if (values.Length == 0)
				return;
			String actionName = values[0];

			if (actionName.Equals("show"))
			{
				//If only show, take that as a person wanting to see the permission types
				if (values.Length == 1)
				{
					await Actions.sendChannelMessage(Context.Channel, String.Format("**CHANNEL PERMISSION TYPES:**```\n{0}```", String.Join("\n", Variables.ChannelPermissionNames)));
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
					List<String> overwrites = new List<String>();
					foreach (Overwrite overwrite in channel.PermissionOverwrites)
					{
						if (overwrite.TargetType == PermissionTarget.Role)
						{
							overwrites.Add(Context.Guild.GetRole(overwrite.TargetId).Name);
						}
						else
						{
							overwrites.Add(Context.Guild.GetUserAsync(overwrite.TargetId).Result.Username);
						}
					}
					await Actions.sendChannelMessage(Context.Channel, String.Format("**OVERWRITES FOR `{0} ({1})`:**```\n{2}```",
						channel.Name, channel.GetType().Name.ToLower().Contains(Constants.VOICE_TYPE) ? "VOICE" : "TEXT", String.Join("\n", overwrites.ToArray())));
					return;
				}

				//Check if valid role or user
				role = await Actions.getRole(Context, values[1]);
				if (role == null)
				{
					user = await Actions.getUser(Context.Guild, values[1]);
					if (user == null)
					{
						await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid role or user supplied."), Constants.WAIT_TIME);
						return;
					}
				}

				//Say the permissions of the overwrite
				foreach (Overwrite overwrite in channel.PermissionOverwrites)
				{
					String overwriteString = "";
					if (role != null && overwrite.TargetId.Equals(role.Id))
					{
						Actions.getPerms(overwrite, channel).ToList().ForEach(kvp => overwriteString += kvp.Key + ": " + kvp.Value + '\n');
						await Actions.sendChannelMessage(Context.Channel, String.Format("**PERMISSIONS FOR `{0}` ON `{1} ({2})`:**```\n{3}```",
							role.Name, channel.Name, channel.GetType().Name.ToLower().Contains(Constants.VOICE_TYPE) ? "VOICE" : "TEXT", overwriteString));
						return;
					}
					else if (user != null && overwrite.TargetId.Equals(user.Id))
					{
						Actions.getPerms(overwrite, channel).ToList().ForEach(kvp => overwriteString += kvp.Key + ": " + kvp.Value + '\n');
						await Actions.sendChannelMessage(Context.Channel, String.Format("**PERMISSIONS FOR `{0}#{1}` ON `{2} ({3})`:**```\n{4}```",
							user.Username, user.Discriminator, channel.Name, channel.GetType().Name.ToLower().Contains(Constants.VOICE_TYPE) ? "VOICE" : "TEXT", overwriteString));
						return;
					}
				}
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(String.Format("Unable to show permissions for `{0}` on `{1} ({2})`.",
					values[1], channel.Name, channel.GetType().Name.ToLower().Contains(Constants.VOICE_TYPE) ? "Voice" : "Text")), Constants.WAIT_TIME);
				return;
			}
			else if (actionName.Equals("allow") || actionName.Equals("deny") || actionName.Equals("inherit"))
			{
				values = values[1].Split(new char[] { ' ' }, 2);

				//Check if valid number of arguments
				if (values.Length == 1)
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR), Constants.WAIT_TIME);
					return;
				}

				//See if the user can see and thus edit that channel
				channel = await Actions.getChannelEditAbility(Context, values[0]);
				if (channel == null)
					return;

				//Check if valid perms and potential role/user
				String potentialRoleOrUser;
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
							await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid role or user supplied."), Constants.WAIT_TIME);
							return;
						}
					}
				}
				else
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("No permissions supplied."), Constants.WAIT_TIME);
					return;
				}
			}
			else
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid action."), Constants.WAIT_TIME);
				return;
			}

			//Get the generic permissions
			List<String> genericPerms = Variables.ChannelPermissionNames.Values.ToList();
			//Check if valid permissions
			List<String> validPerms = permissions.Intersect(genericPerms, StringComparer.OrdinalIgnoreCase).ToList();
			if (validPerms.Count != permissions.Count)
			{
				List<String> invalidPerms = new List<String>();
				foreach (String permission in permissions)
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
			foreach (String permission in permissions)
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
			String roleNameOrUsername;
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
				actionName,
				String.Join("`, `", permissions),
				roleNameOrUsername,
				channel.Name,
				channel.GetType().Name.ToLower().Contains(Constants.VOICE_TYPE) ? "Voice" : "Text"),
				7500);
		}

		[Command("copychannelpermissions")]
		[Alias("cchp")]
		[Usage(Constants.BOT_PREFIX + "copychannelpermissions " + Constants.CHANNEL_INSTRUCTIONS + " " + Constants.CHANNEL_INSTRUCTIONS + " [Role|User|All]")]
		[Summary("Copy permissions from one channel to another. Works for a role, a user, or everything.")]
		[PermissionRequirements((1U << (int)GuildPermission.ManageChannels) | (1U << (int)GuildPermission.ManageRoles), 0)]
		public async Task CopyChannelPermissions([Remainder] String input)
		{
			//Get arguments
			String[] inputArray = input.ToLower().Split(new char[] { ' ' }, 3);

			//Separating the channels
			IGuildChannel inputChannel = await Actions.getChannel(Context.Guild, inputArray[0]);
			if (inputChannel == null)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid input channel."), Constants.WAIT_TIME);
				return;
			}
			//See if the user can see and thus edit that channel
			IGuildChannel outputChannel = await Actions.getChannelEditAbility(Context, inputArray[1]);
			if (outputChannel == null)
				return;

			//Copy the selected target
			String target;
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
						await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("No valid role/user or all input."), Constants.WAIT_TIME);
						return;
					}
				}
			}

			await Actions.makeAndDeleteSecondaryMessage(Context, String.Format("Successfully copied `{0}` from `{1} ({2})` to `{3} ({4})`",
				target,
				inputChannel.Name,
				inputChannel.GetType().Name.ToLower().Contains(Constants.TEXT_TYPE) ? "Text" : "Voice",
				outputChannel.Name,
				outputChannel.GetType().Name.ToLower().Contains(Constants.TEXT_TYPE) ? "Text" : "Voice"),
				7500);
		}

		[Command("clearchannelpermissions")]
		[Alias("clchp")]
		[Usage(Constants.BOT_PREFIX + "clearchannelpermissions " + Constants.CHANNEL_INSTRUCTIONS)]
		[Summary("Removes all permissions set on a channel.")]
		[PermissionRequirements((1U << (int)GuildPermission.ManageChannels) | (1U << (int)GuildPermission.ManageRoles), 0)]
		public async Task ClearChannelPermissions([Remainder] String input)
		{
			//See if the user can see and thus edit that channel
			IGuildChannel channel = await Actions.getChannelEditAbility(Context, input);
			if (channel == null)
				return;

			//Check if channel has permissions to clear
			if (channel.PermissionOverwrites.Count < 1)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Channel has no permissions to clear."), Constants.WAIT_TIME);
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
				channel.Name, (channel.GetType().Name.ToLower().Contains(Constants.VOICE_TYPE) ? "Voice" : "Text")),
				7500);
		}

		[Command("changechannelname")]
		[Alias("cchn")]
		[Usage(Constants.BOT_PREFIX + "changechannelname " + Constants.CHANNEL_INSTRUCTIONS + " [New Name]")]
		[Summary("Bans then unbans a user from the guild.")]
		[PermissionRequirements(0, (1U << (int)GuildPermission.ManageChannels))]
		public async Task ChangeChannelName([Remainder] String input)
		{
			String[] inputArray = input.Split(new char[] { ' ' }, 2);
			if (inputArray.Length != 2)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR), Constants.WAIT_TIME);
				return;
			}

			//Checking if valid name
			if (inputArray[1].Contains(' '))
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("No spaces allowed in a channel name."), Constants.WAIT_TIME);
				return;
			}
			if (inputArray[1].Length < 2 || inputArray[1].Length > 100)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Name must be between 2 and 100 characters long."), Constants.WAIT_TIME);
				return;
			}

			//Check if valid channel
			IGuildChannel channel = await Actions.getChannelEditAbility(Context, inputArray[0]);
			if (channel == null)
				return;

			String previousName = channel.Name;
			await channel.ModifyAsync(x => x.Name = inputArray[1]);
			await Actions.makeAndDeleteSecondaryMessage(Context, String.Format("Successfully changed channel `{0}` to `{1}`.", previousName, channel.Name), 5000);
		}

		[Command("changechanneltopic")]
		[Alias("ccht")]
		[Usage(Constants.BOT_PREFIX + "changechanneltopic [#Channel] [New Topic]")]
		[Summary("Changes the subtext of a channel to whatever is input.")]
		[PermissionRequirements(0, (1U << (int)GuildPermission.ManageChannels))]
		public async Task ChangeChannelTopic([Remainder] String input)
		{
			String[] inputArray = input.Split(new char[] { ' ' }, 2);
			if (inputArray.Length != 2)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR), Constants.WAIT_TIME);
				return;
			}
			String newTopic = inputArray[1];

			//See if valid length
			if (newTopic.Length > Constants.TOPIC_LENGTH)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Topics cannot be longer than 1024 characters in length."), Constants.WAIT_TIME);
				return;
			}

			//Test if valid channel
			IGuildChannel channel = await Actions.getChannelEditAbility(Context, inputArray[0]);
			if (channel == null)
				return;

			//See if not a text channel
			if (!channel.GetType().Name.ToLower().Contains(Constants.TEXT_TYPE))
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Only text channels can have their topic set."), Constants.WAIT_TIME);
				return;
			}

			//See what current topic is
			String currentTopic = (channel as ITextChannel).Topic;
			if (String.IsNullOrWhiteSpace(currentTopic))
			{
				currentTopic = "NOTHING";
			}

			await (channel as ITextChannel).ModifyAsync(x => x.Topic = newTopic);
			await Actions.makeAndDeleteSecondaryMessage(Context, String.Format("Successfully changed the topic in `{0}` from `{1}` to `{2}`.",
				channel.Name, currentTopic, newTopic == "" ? "NOTHING" : newTopic), Constants.WAIT_TIME);
		}

		[Command("moveuser")]
		[Alias("mu")]
		[Usage(Constants.BOT_PREFIX + "moveuser [@User] [Channel]")]
		[Summary("Moves the user to the given voice channel.")]
		[PermissionRequirements(0, (1U << (int)GuildPermission.MoveMembers))]
		public async Task MoveUser([Remainder] String input)
		{
			//Input and splitting
			String[] inputArray = input.Split(new char[] { ' ' }, 2);

			//Check if valid user
			IGuildUser user = await Actions.getUser(Context.Guild, inputArray[0]);
			if (user == null)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.USER_ERROR), Constants.WAIT_TIME);
				return;
			}

			//Check if user is in a voice channel
			if (user.VoiceChannel == null)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("User is not in a voice channel."), Constants.WAIT_TIME);
				return;
			}

			//See if the bot can move people from this channel
			if (!await Actions.getChannelEditAbility(user.VoiceChannel, await Context.Guild.GetUserAsync(Variables.Bot_ID)))
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Unable to move this user due to permissions " +
					"or due to the user being in a voice channel before the bot started up."), 5000);
				return;
			}
			//See if the user can move people from this channel
			if (!await Actions.getChannelEditAbility(user.VoiceChannel, Context.User as IGuildUser))
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("You are unable to move people from this channel."), Constants.WAIT_TIME);
				return;
			}

			//Check if valid channel
			IGuildChannel channel = await Actions.getChannelEditAbility(Context, inputArray[1]);
			if (channel == null)
				return;
			if (!channel.GetType().Name.ToLower().Contains(Constants.VOICE_TYPE))
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Users can only be moved to a voice channel."), Constants.WAIT_TIME);
				return;
			}

			//See if trying to put user in the exact same channel
			if (user.VoiceChannel == channel)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("User is already in that channel"), Constants.WAIT_TIME);
				return;
			}

			await user.ModifyAsync(x => x.Channel = Optional.Create(channel as IVoiceChannel));
			await Actions.makeAndDeleteSecondaryMessage(Context, String.Format("Successfully moved {0} to `{1}`.", user.Mention, channel.Name), 5000);
		}

		[Command("mute")]
		[Alias("m")]
		[Usage(Constants.BOT_PREFIX + "mute [@User]")]
		[Summary("If the user is not guild muted, this will mute them. If they are guild muted, this will unmute them.")]
		[PermissionRequirements(0, (1U << (int)GuildPermission.MuteMembers))]
		public async Task Mute([Remainder] String input)
		{
			//Test if valid user mention
			IGuildUser user = await Actions.getUser(Context.Guild, input);
			if (user == null)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.USER_ERROR), Constants.WAIT_TIME);
				return;
			}

			//See if it should mute or unmute
			if (!user.IsMuted)
			{
				await user.ModifyAsync(x => x.Mute = true);
				await Actions.makeAndDeleteSecondaryMessage(Context, String.Format("Successfully guild muted {0}.", user.Mention), Constants.WAIT_TIME);
				return;
			}
			await user.ModifyAsync(x => x.Mute = false);
			await Actions.makeAndDeleteSecondaryMessage(Context, String.Format("Successfully removed the guild mute on {0}.", user.Mention), Constants.WAIT_TIME);
		}

		[Command("deafen")]
		[Alias("dfn", "d")]
		[Usage(Constants.BOT_PREFIX + "deafen [@User]")]
		[Summary("Bans then unbans a user from the guild.")]
		[PermissionRequirements(0, (1U << (int)GuildPermission.DeafenMembers))]
		public async Task Deafen([Remainder] String input)
		{
			//Test if valid user mention
			IGuildUser user = await Actions.getUser(Context.Guild, input);
			if (user == null)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.USER_ERROR), Constants.WAIT_TIME);
				return;
			}

			//See if it should deafen or undeafen
			if (!user.IsDeafened)
			{
				await user.ModifyAsync(x => x.Deaf = true);
				await Actions.makeAndDeleteSecondaryMessage(Context, String.Format("Successfully server deafened {0}.", user.Mention), Constants.WAIT_TIME);
				return;
			}
			await user.ModifyAsync(x => x.Deaf = false);
			await Actions.makeAndDeleteSecondaryMessage(Context, String.Format("Successfully removed the server deafen on {0}.", user.Mention), Constants.WAIT_TIME);
		}

		[Command("nickname")]
		[Alias("nn")]
		[Usage(Constants.BOT_PREFIX + "nickname [@User] [New Nickname|Remove]")]
		[Summary("Gives the user a nickname.")]
		[PermissionRequirements(0, (1U << (int)GuildPermission.ManageNicknames))]
		public async Task Nickname([Remainder] String input)
		{
			//Input and splitting
			String[] inputArray = input.Split(new char[] { ' ' }, 2);
			String nickname;
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
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR), Constants.WAIT_TIME);
				return;
			}

			//Check if valid length
			if (nickname != null && nickname.Length > Constants.NICKNAME_LENGTH)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Nicknames cannot be longer than 32 characters."), Constants.WAIT_TIME);
				return;
			}

			//Check if valid user
			IGuildUser user = await Actions.getUser(Context.Guild, inputArray[0]);
			if (user == null)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.USER_ERROR), Constants.WAIT_TIME);
				return;
			}

			//Checks for positions
			int nicknamePosition = Actions.getPosition(Context.Guild, user);
			if (nicknamePosition > (Actions.userHasOwner(Context.Guild, Context.User as IGuildUser)
				? Constants.OWNER_POSITION : Actions.getPosition(Context.Guild, Context.User as IGuildUser)))
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("User cannot be nicknamed by you."), Constants.WAIT_TIME);
				return;
			}
			if (nicknamePosition > Actions.getPosition(Context.Guild, await Context.Guild.GetUserAsync(Variables.Bot_ID)))
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("User cannot be nicknamed by the bot."), Constants.WAIT_TIME);
				return;
			}

			await user.ModifyAsync(x => x.Nickname = nickname);
			if (nickname != null)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, String.Format("Successfully gave the nickname `{0}` to {1}.", nickname, user.Mention), Constants.WAIT_TIME);
				return;
			}
			await Actions.makeAndDeleteSecondaryMessage(Context, String.Format("Sucessfully removed the nickname from {0}.", user.Mention), Constants.WAIT_TIME);
		}

		[Command("allwithrole")]
		[Alias("awr")]
		[Usage(Constants.BOT_PREFIX + "allwithrole <File|Upload> [Role]")]
		[Summary("Prints out a list of all users with the given role. File specifies a text document which can show more symbols. Uploaded specifies to use a text uploader.")]
		[PermissionRequirements(0, (1U << (int)GuildPermission.ManageRoles))]
		public async Task AllWithRole([Remainder] String input)
		{
			//Split into the bools and role
			List<String> values = input.Split(new char[] { ' ' }, 2).ToList();

			//Initializing input and variables
			IRole role = await Actions.getRole(Context, values.Last());
			if (role == null)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ROLE_ERROR), Constants.WAIT_TIME);
				return;
			}

			//If two args, check what action to take
			bool overwriteBool = false;
			bool uploadBool = true;
			if (values.Count == 2)
			{
				if (values[0].ToLower().Equals("file"))
				{
					uploadBool = false;
					overwriteBool = true;
				}
				else if (values[0].ToLower().Equals("upload"))
				{
					overwriteBool = true;
				}
			}

			//Initialize the lists
			List<String> usersMentions = new List<String>();
			List<String> usersText = new List<String>();
			int characters = 0;
			int count = 0;

			//Grab each user
			IReadOnlyCollection<IGuildUser> guildUsers = await Context.Guild.GetUsersAsync();
			guildUsers.Where(x => x.JoinedAt != null).ToList().OrderBy(x => x.JoinedAt.Value.Ticks).ToList().ForEach(x =>
			{
				++count;
				if (x.RoleIds.ToList().Contains(role.Id))
				{
					usersMentions.Add(x.Mention);
					usersText.Add(count + ": " + x.Username + "#" + x.Discriminator + " ID: " + x.Id);
					characters += (x.Mention.Length + 3);
				}
			});

			//Checking if the message can fit in a single message
			if (characters > 1950 || overwriteBool)
			{
				if (uploadBool)
				{
					await Actions.sendChannelMessage(Context.Channel,
						"`[" + DateTime.UtcNow.ToString("HH:mm:ss") + "]` **AWR " + role.Name.ToUpper() + ":**\n" + Actions.uploadToHastebin(Context.Channel,
						String.Join("\n", usersText)));
					return;
				}
				//Upload the file
				await Actions.uploadTextFile(Context.Guild, Context.Channel, String.Join("\n", usersText), "AWR_" + role.Name.ToUpper() + "_", "AWR");
			}
			else
			{
				await Actions.sendChannelMessage(Context.Channel, String.Format("**ALL USERS WITH `{0}`:**\n{1}", role.Name, String.Join(", ", usersMentions)));
			}
		}

		[Command("forallwithrole")]
		[Alias("fawr")]
		[Usage(Constants.BOT_PREFIX + "forallwithrole [Give|Take|Nickname] [Role]/[Role|Nickname]")]
		[Summary("Can give a role to users, take a role from users, and nickname users who have a specific role. The bot will hit the rate limit of actions " +
			"every 10 users and then have to wait for ~9 seconds. The max limit of 100 can be bypassed by saying 'Badoodle' after the last argument.")]
		[PermissionRequirements((1U << (int)GuildPermission.Administrator), 0)]
		public async Task ForAllWithRole([Remainder] String input)
		{

		}
	}
}
