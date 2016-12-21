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
//I know I could be using my other files so I don't need to always type Actions. or Constants. but I want to specify where stuff comes from

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
			if (await Actions.getRoleEditAbility(Context.Guild, Context.Channel, Context.Message,
				await Context.Guild.GetUserAsync(Context.User.Id), await Context.Guild.GetUserAsync(Context.Client.CurrentUser.Id), Constants.MUTE_ROLE_NAME) == null)
			{
				return;
			}

			//Test if valid user mention
			IGuildUser user = await Actions.getUser(Context.Guild, input);
			if (user == null)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context.Channel, Context.Message, Actions.ERROR(Constants.USER_ERROR), Constants.WAIT_TIME);
				return;
			}

			//Give the targetted user the role
			await Actions.giveRole(user, muteRole);
			await Actions.makeAndDeleteSecondaryMessage(Context.Channel, Context.Message, String.Format("Successfully muted {0}.", user.Mention), Constants.WAIT_TIME);
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
				await Actions.makeAndDeleteSecondaryMessage(Context.Channel, Context.Message, Actions.ERROR(Constants.USER_ERROR), Constants.WAIT_TIME);
				return;
			}

			//Remove the role
			await Actions.takeRole(user, Actions.getRole(Context.Guild, Constants.MUTE_ROLE_NAME));
			await Actions.makeAndDeleteSecondaryMessage(Context.Channel, Context.Message, String.Format("Successfully unmuted {0}.", user.Mention), Constants.WAIT_TIME);
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
				await Actions.makeAndDeleteSecondaryMessage(Context.Channel, Context.Message, Actions.ERROR(Constants.USER_ERROR), Constants.WAIT_TIME);
				return;
			}

			//Determine if the user is allowed to kick this person
			int kickerPosition = Actions.userHasOwner(Context.Guild, (Context.User as IGuildUser)) ?
				Constants.OWNER_POSITION : Actions.getPosition(Context.Guild, (Context.User as IGuildUser));
			int kickeePosition = Actions.getPosition(Context.Guild, inputUser);
			if (kickerPosition <= kickeePosition)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context.Channel, Context.Message, Actions.ERROR("User is unable to be kicked by you."), Constants.WAIT_TIME);
				return;
			}

			//Determine if the bot can kick this person
			if (Actions.getPosition(Context.Guild, await Context.Guild.GetUserAsync(Context.Client.CurrentUser.Id)) <= kickeePosition)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context.Channel, Context.Message, Actions.ERROR("Bot is unable to kick user."), Constants.WAIT_TIME);
				return;
			}

			//Kick the targetted user
			await inputUser.KickAsync();
			await Actions.makeAndDeleteSecondaryMessage(Context.Channel, Context.Message, String.Format("Successfully kicked {0}#{1} with the ID `{2}`.",
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
				await Actions.makeAndDeleteSecondaryMessage(Context.Channel, Context.Message, Actions.ERROR(Constants.USER_ERROR), Constants.WAIT_TIME);
				return;
			}

			//Determine if the user is allowed to softban this person
			int sberPosition = Actions.userHasOwner(Context.Guild, (Context.User as IGuildUser)) ?
				Constants.OWNER_POSITION : Actions.getPosition(Context.Guild, (Context.User as IGuildUser));
			int sbeePosition = Actions.getPosition(Context.Guild, inputUser);
			if (sberPosition <= sbeePosition)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context.Channel, Context.Message, Actions.ERROR("User is unable to be soft-banned by you."), Constants.WAIT_TIME);
				return;
			}

			//Determine if the bot can softban this person
			if (Actions.getPosition(Context.Guild, await Context.Guild.GetUserAsync(Context.Client.CurrentUser.Id)) <= sbeePosition)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context.Channel, Context.Message, Actions.ERROR("Bot is unable to soft-ban user."), Constants.WAIT_TIME);
				return;
			}

			//Softban the targetted use
			await Context.Guild.AddBanAsync(inputUser);
			await Context.Guild.RemoveBanAsync(inputUser);
			await Actions.makeAndDeleteSecondaryMessage(Context.Channel, Context.Message, String.Format("Successfully banned and unbanned {0}.", inputUser.Mention), Constants.WAIT_TIME);
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
				await Actions.makeAndDeleteSecondaryMessage(Context.Channel, Context.Message, Actions.ERROR(Constants.ARGUMENTS_ERROR), Constants.WAIT_TIME);
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

			if (null == inputUser)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context.Channel, Context.Message, Actions.ERROR(Constants.USER_ERROR), Constants.WAIT_TIME);
				return;
			}

			//Determine if the user is allowed to ban this person
			int bannerPosition = Actions.userHasOwner(Context.Guild, await Context.Guild.GetUserAsync(Context.User.Id) as IGuildUser) ?
				Constants.OWNER_POSITION : Actions.getPosition(Context.Guild, await Context.Guild.GetUserAsync(Context.User.Id) as IGuildUser);
			int banneePosition = Actions.getPosition(Context.Guild, inputUser);
			if (bannerPosition <= banneePosition)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context.Channel, Context.Message, Actions.ERROR("User is unable to be banned by you."), Constants.WAIT_TIME);
				return;
			}

			//Determine if the bot can ban this person
			if (Actions.getPosition(Context.Guild, await Context.Guild.GetUserAsync(Context.Client.CurrentUser.Id)) <= banneePosition)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context.Channel, Context.Message, Actions.ERROR("Bot is unable to ban user."), Constants.WAIT_TIME);
				return;
			}

			//Checking for valid days requested
			int pruneDays = 0;
			if (values.Length == 2 && !Int32.TryParse(values[1], out pruneDays))
			{
				await Actions.makeAndDeleteSecondaryMessage(Context.Channel, Context.Message, Actions.ERROR("Incorrect input for days of messages to be deleted."), Constants.WAIT_TIME);
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
			await Actions.makeAndDeleteSecondaryMessage(Context.Channel, Context.Message, String.Format("Successfully banned {0}#{1} `{2}`{3}",
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
				await Actions.makeAndDeleteSecondaryMessage(Context.Channel, Context.Message, Actions.ERROR(Constants.ARGUMENTS_ERROR), Constants.WAIT_TIME);
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
					await Actions.makeAndDeleteSecondaryMessage(Context.Channel, Context.Message, Actions.ERROR("Invalid discriminator provided."), Constants.WAIT_TIME);
				}
				String username = values[0].Replace("@", "");

				//Get a list of users with the name username and discriminator
				List<IBan> bannedUserWithNameAndDiscriminator = bans.ToList().Where(x => x.User.Username.Equals(username) && x.User.Discriminator.Equals(discriminator)).ToList();

				//Unban the user
				IUser bannedUser = bannedUserWithNameAndDiscriminator[0].User;
				await Context.Guild.RemoveBanAsync(bannedUser);
				secondHalfOfTheSecondaryMessage = String.Format("unbanned the user `{0}#{1}` with the ID `{2}`.", bannedUser.Username, bannedUser.Discriminator, bannedUser.Id);
			}
			else if (!ulong.TryParse(input, out inputUserID))
			{
				//Unban given just a username
				List<IBan> bannedUsersWithSameName = bans.ToList().Where(x => x.User.Username.Equals(input)).ToList();
				if (bannedUsersWithSameName.Count() > 1)
				{
					//Return a message saying if there are multiple users
					await Actions.sendChannelMessage(Context.Channel, String.Format("The following users have that name: `{0}`.", String.Join("`, `", bannedUsersWithSameName)));
					return;
				}
				else if (bannedUsersWithSameName.Count == 1)
				{
					//Unban the user
					IUser bannedUser = bannedUsersWithSameName[0].User;
					await Context.Guild.RemoveBanAsync(bannedUser);
					secondHalfOfTheSecondaryMessage = String.Format("unbanned the user `{0}#{1}` with the ID `{2}`.", bannedUser.Username, bannedUser.Discriminator, bannedUser.Id);
				}
				else
				{
					await Actions.makeAndDeleteSecondaryMessage(Context.Channel, Context.Message, Actions.ERROR("No user on the ban list has that username."), Constants.WAIT_TIME);
					return;
				}
			}
			else
			{
				//Unban given a user ID
				if (Actions.getUlong(input).Equals(0))
				{
					await Actions.makeAndDeleteSecondaryMessage(Context.Channel, Context.Message, Actions.ERROR("Invalid user ID."), Constants.WAIT_TIME);
					return;
				}
				await Context.Guild.RemoveBanAsync(bans.FirstOrDefault(x => x.User.Id.Equals(inputUserID)).User);
				secondHalfOfTheSecondaryMessage = String.Format("unbanned the user with the ID `{0}`.", inputUserID);
			}
			await Actions.makeAndDeleteSecondaryMessage(Context.Channel, Context.Message, String.Format("Successfully {0}", secondHalfOfTheSecondaryMessage), 10000);
		}

		[Command("currentbanlist")]
		[Alias("cbl")]
		[Usage(Constants.BOT_PREFIX + "softban [@User]")]
		[Summary("Bans then unbans a user from the guild.")]
		[PermissionRequirements(0, (1U << (int)GuildPermission.BanMembers))]
		public async Task CurrentBanList([Remainder] String input)
		{

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
				await Actions.makeAndDeleteSecondaryMessage(Context.Channel, Context.Message, Actions.ERROR(Constants.ARGUMENTS_ERROR), Constants.WAIT_TIME);
				return;
			}

			int argIndex = 0;
			int argCount = values.Length;

			//Testing if starts with user mention
			IGuildUser inputUser = null;
			if (argIndex < argCount && values[argIndex].StartsWith("<@"))
			{
				inputUser = await Actions.getUser(Context.Guild, values[argIndex]);
				if (null == inputUser)
				{
					await Actions.makeAndDeleteSecondaryMessage(Context.Channel, Context.Message, Actions.ERROR(Constants.USER_ERROR), Constants.WAIT_TIME);
					return;
				}
				++argIndex;
			}

			//Testing if starts with channel mention
			IMessageChannel inputChannel = Context.Channel;
			if (argIndex < argCount && values[argIndex].StartsWith("<#"))
			{
				inputChannel = await Actions.getChannelID(Context.Guild, values[argIndex]);
				if (null == inputChannel)
				{
					await Actions.makeAndDeleteSecondaryMessage(Context.Channel, Context.Message, Actions.ERROR(Constants.CHANNEL_ERROR), Constants.WAIT_TIME);
					return;
				}
				++argIndex;
			}

			//Checking for valid request count
			int requestCount = (argIndex == argCount - 1) ? Actions.getInteger(values[argIndex]) : -1;
			if (requestCount < 1)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context.Channel, Context.Message,
					Actions.ERROR("Incorrect input for number of messages to be removed."), Constants.WAIT_TIME);
				return;
			}

			//See if the user is trying to delete more than 100 messages at a time
			if (requestCount > 100 && !(await Context.Guild.GetUserAsync(Context.User.Id)).GuildPermissions.Administrator)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context.Channel, Context.Message,
					Actions.ERROR("You need administrator to remove more than 100 messages at a time."), Constants.WAIT_TIME);
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
			await Actions.makeAndDeleteSecondaryMessage(Context.Channel, Context.Message, String.Format("Successfully deleted {0} messages{1}{2}.",
				requestCount,
				inputUser == null ? "" : " from " + inputUser.Mention,
				inputChannel == null ? "" : " on `" + inputChannel.Name + "`"),
				2000);
		}

		[Command("giverole")]
		[Alias("gr")]
		[Usage(Constants.BOT_PREFIX + "giverole [@User] [Role]")]
		[Summary("Gives the user the role (assuming the person using the command and bot both have the ability to give that role).")]
		[PermissionRequirements(0, (1U << (int)GuildPermission.ManageRoles))]
		public async Task GiveRole([Remainder] String input)
		{

		}

		[Command("takerole")]
		[Alias("tr")]
		[Usage(Constants.BOT_PREFIX + "takerole [@User] [Role]")]
		[Summary("Take the role from the user (assuming the person using the command and bot both have the ability to take that role).")]
		[PermissionRequirements(0, (1U << (int)GuildPermission.ManageRoles))]
		public async Task TakeRole([Remainder] String input)
		{

		}

		[Command("createrole")]
		[Alias("cr")]
		[Usage(Constants.BOT_PREFIX + "createrole [Role]")]
		[Summary("Adds a role to the guild with the chosen name.")]
		[PermissionRequirements(0, (1U << (int)GuildPermission.ManageRoles))]
		public async Task CreateRole([Remainder] String input)
		{

		}

		[Command("softdeleterole")]
		[Alias("sdrole", "sdr")]
		[Usage(Constants.BOT_PREFIX + "softdeleterole [Role]")]
		[Summary("Removes all permissions from a role (and all channels the role had permissions on) and removes the role from everyone. Leaves the name and color behind.")]
		[PermissionRequirements(0, (1U << (int)GuildPermission.ManageRoles))]
		public async Task SoftDeleteRole([Remainder] String input)
		{

		}

		[Command("deleterole")]
		[Alias("drole", "dr")]
		[Usage(Constants.BOT_PREFIX + "deleterole [Role]")]
		[Summary("Deletes the role. 'Drole' is a pretty funny alias.")]
		[PermissionRequirements(0, (1U << (int)GuildPermission.ManageRoles))]
		public async Task DeleteRole([Remainder] String input)
		{

		}

		[Command("rolepermissions")]
		[Alias("erp", "rp")]
		[Usage(Constants.BOT_PREFIX + "rolepermissions [Show|Add|Remove] [Role] [Permission/...]")]
		[Summary("Add/remove the selected permissions to/from the role. Permissions must be separated by a '/'!" +
			"Type '>rolepermissions [Show]' to see the available permissions. Type '>rolepermissions [Show] [Role]' to see the permissions of that role.")]
		[PermissionRequirements(0, (1U << (int)GuildPermission.ManageRoles))]
		public async Task RolePermissions([Remainder] String input)
		{

		}

		[Command("copyrolepermissions")]
		[Alias("crp")]
		[Usage(Constants.BOT_PREFIX + "copyrolepermissions [Role]/[Role]")]
		[Summary("Copies the permissions from the first role to the second role. Will not copy roles that the user does not have access to." +
			"Will not overwrite roles that are above the user's top role.")]
		[PermissionRequirements(0, (1U << (int)GuildPermission.ManageRoles))]
		public async Task CopyRolePermissions([Remainder] String input)
		{

		}

		[Command("clearrolepermissions")]
		[Alias("clrrole")]
		[Usage(Constants.BOT_PREFIX + "clearrolepermissions [Role]")]
		[Summary("Removes all permissions from a role.")]
		[PermissionRequirements(0, (1U << (int)GuildPermission.ManageRoles))]
		public async Task ClearRolePermissions([Remainder] String input)
		{

		}

		[Command("changerolename")]
		[Alias("crn")]
		[Usage(Constants.BOT_PREFIX + "changerolename [Role]/[New Name]")]
		[Summary("Changes the name of the role.")]
		[PermissionRequirements(0, (1U << (int)GuildPermission.ManageRoles))]
		public async Task ChangeRoleName([Remainder] String input)
		{

		}

		[Command("changerolecolor")]
		[Alias("crc")]
		[Usage(Constants.BOT_PREFIX + "changerolecolor Role/[Hexadecimal|Color Name]")]
		[Summary("Changes the role's color. A color of '0' sets the role back to the default color. " +
			"Colors must either be in hexadecimal format or be a color listed in the System.Drawing namespace of C#." +
			"\nFor a list of acceptable color names: https://msdn.microsoft.com/en-us/library/system.drawing.color(v=vs.110).aspx")]
		[PermissionRequirements(0, (1U << (int)GuildPermission.ManageRoles))]
		public async Task ChangeRoleColor([Remainder] String input)
		{

		}

		[Command("createchannel")]
		[Alias("cch")]
		[Usage(Constants.BOT_PREFIX + "createchannel [Name]/[Text|Voice]")]
		[Summary("Adds a channel to the guild of the given type with the given name. The name CANNOT contain any spaces: use underscores or dashes instead.")]
		[PermissionRequirements(0, (1U << (int)GuildPermission.ManageChannels))]
		public async Task CreateChannel([Remainder] String input)
		{

		}

		[Command("softdeletechannel")]
		[Alias("sdch")]
		[Usage(Constants.BOT_PREFIX + "softdeletechannel [#Channel]")]
		[Summary("Makes most roles unable to read the channel and moves it to the bottom of the channel list. Only works for text channels.")]
		[PermissionRequirements((1U << (int)GuildPermission.ManageChannels) | (1U << (int)GuildPermission.ManageRoles), 0)]
		public async Task SoftDeleteChannel([Remainder] String input)
		{

		}

		[Command("deletechannel")]
		[Alias("dch")]
		[Usage(Constants.BOT_PREFIX + "deletechannel " + Constants.CHANNEL_INSTRUCTIONS)]
		[Summary("Deletes the channel. Deleting a voice channel requires '[Channel] [Voice]' since it is not possible to mention a voice channel.")]
		[PermissionRequirements(0, (1U << (int)GuildPermission.ManageChannels))]
		public async Task DeleteChannel([Remainder] String input)
		{

		}

		[Command("channelpermissions")]
		[Alias("echp", "chp")]
		[Usage(Constants.BOT_PREFIX + "channelpermissions [Show|Allow|Inherit|Deny] " + Constants.CHANNEL_INSTRUCTIONS + " [Role|User] <Permission/...>")]
		[Summary("Type '>chp [Show]' to see the available permissions. Permissions must be separated by a '/'! " +
			"Type '>chp [Show] [Channel]' to see all permissions on a channel. Type '>chp [Show] [Channel] [Role|User]' to see permissions a role/user has on a channel.")]
		[PermissionRequirements((1U << (int)GuildPermission.ManageChannels) | (1U << (int)GuildPermission.ManageRoles), 0)]
		public async Task ChannelPermissions([Remainder] String input)
		{

		}

		[Command("copychannelpermissions")]
		[Alias("cchp")]
		[Usage(Constants.BOT_PREFIX + "copychannelpermissions [Channel]/[Channel] [Role|User|All]")]
		[Summary("Copy permissions from one channel to another. Works for a role, a user, or everything.")]
		[PermissionRequirements((1U << (int)GuildPermission.ManageChannels) | (1U << (int)GuildPermission.ManageRoles), 0)]
		public async Task CopyChannelPermissions([Remainder] String input)
		{

		}

		[Command("clearchannelpermissions")]
		[Alias("clchp")]
		[Usage(Constants.BOT_PREFIX + "clearchannelpermissions " + Constants.CHANNEL_INSTRUCTIONS)]
		[Summary("Removes all permissions set on a channel.")]
		[PermissionRequirements((1U << (int)GuildPermission.ManageChannels) | (1U << (int)GuildPermission.ManageRoles), 0)]
		public async Task ClearChannelPermissions([Remainder] String input)
		{

		}

		[Command("changechannelname")]
		[Alias("cchn")]
		[Usage(Constants.BOT_PREFIX + "changechannelname " + Constants.CHANNEL_INSTRUCTIONS + " [New Name]")]
		[Summary("Bans then unbans a user from the guild.")]
		[PermissionRequirements(0, (1U << (int)GuildPermission.ManageChannels))]
		public async Task ChangeChannelName([Remainder] String input)
		{

		}

		[Command("changechanneltopic")]
		[Alias("ccht")]
		[Usage(Constants.BOT_PREFIX + "changechanneltopic [#Channel] [New Topic]")]
		[Summary("Changes the subtext of a channel to whatever is input.")]
		[PermissionRequirements(0, (1U << (int)GuildPermission.ManageChannels))]
		public async Task ChangeChannelTopic([Remainder] String input)
		{

		}

		[Command("createinstantinvite")]
		[Alias("crinv")]
		[Usage(Constants.BOT_PREFIX + "createinstantinvite " + Constants.CHANNEL_INSTRUCTIONS + " [Forever|1800|3600|21600|43200|86400] [Infinite|1|5|10|25|50|100] [True|False]")]
		[Summary("The first argument is the channel. The second is how long the invite will last for. " +
			"The third is how many users can use the invite. The fourth is the temporary membership option.")]
		[PermissionRequirements(0, (1U << (int)GuildPermission.CreateInstantInvite))]
		public async Task CreateInstantInvite([Remainder] String input)
		{

		}

		[Command("moveuser")]
		[Alias("mu")]
		[Usage(Constants.BOT_PREFIX + "moveuser [@User] [Channel]")]
		[Summary("Moves the user to the given voice channel.")]
		[PermissionRequirements(0, (1U << (int)GuildPermission.MoveMembers))]
		public async Task MoveUser([Remainder] String input)
		{

		}

		[Command("mute")]
		[Alias("m")]
		[Usage(Constants.BOT_PREFIX + "mute [@User]")]
		[Summary("If the user is not guild muted, this will mute them. If they are guild muted, this will unmute them.")]
		[PermissionRequirements(0, (1U << (int)GuildPermission.MuteMembers))]
		public async Task Mute([Remainder] String input)
		{

		}

		[Command("deafen")]
		[Alias("dfn", "d")]
		[Usage(Constants.BOT_PREFIX + "deafen [@User]")]
		[Summary("Bans then unbans a user from the guild.")]
		[PermissionRequirements(0, (1U << (int)GuildPermission.DeafenMembers))]
		public async Task Deafen([Remainder] String input)
		{

		}

		[Command("nickname")]
		[Alias("nn")]
		[Usage(Constants.BOT_PREFIX + "nickname [@User] [New Nickname|Remove]")]
		[Summary("Gives the user a nickname.")]
		[PermissionRequirements(0, (1U << (int)GuildPermission.ManageNicknames))]
		public async Task Nickname([Remainder] String input)
		{

		}

		[Command("allwithrole")]
		[Alias("awr")]
		[Usage(Constants.BOT_PREFIX + "allwithrole [Role]")]
		[Summary("Prints out a list of all users with the given role.")]
		[PermissionRequirements(0, (1U << (int)GuildPermission.ManageRoles))]
		public async Task AllWithRole([Remainder] String input)
		{

		}

		[Command("forallwithrole")]
		[Alias("fawr")]
		[Usage(Constants.BOT_PREFIX + "forallwithrole [Give|Take|Nickname] [Role]/[Role|Nickname]")]
		[Summary("Can give a role to users, take a role from users, and nickname users who have a specific role. " +
			"The bot will hit the rate limit of actions every 10 users and then have to wait for ~9 seconds. " +
			"The max limit of 100 can be bypassed by saying 'i_understand' after the last argument. \n" +
			"Do not abuse this command.")]
		[PermissionRequirements((1U << (int)GuildPermission.Administrator), 0)]
		public async Task ForAllWithRole([Remainder] String input)
		{

		}
	}
}
