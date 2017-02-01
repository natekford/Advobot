using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Advobot
{
	//User Moderation commands are commands that affect the users of a guild
	[Name("User Moderation")]
	public class Advobot_Commands_User_Mod : ModuleBase
	{
		[Command("textmute")]
		[Alias("tm")]
		[Usage("textmute [@User]")]
		[Summary("Removes the user's ability to type via the 'Muted' role.")]
		[PermissionRequirements((1U << (int)GuildPermission.ManageRoles) | (1U << (int)GuildPermission.ManageMessages))]
		public async Task FullMute([Remainder] string input)
		{
			//Check if role already exists, if not, create it
			var muteRole = await Actions.createRoleIfNotFound(Context.Guild, Constants.MUTE_ROLE_NAME);
			if (muteRole == null)
				return;

			//See if both the bot and the user can edit/use this role
			if (await Actions.getRoleEditAbility(Context, role: muteRole) == null)
				return;

			//Always make sure it's muting correctly
			await muteRole.ModifyAsync(x => x.Permissions = new GuildPermissions(sendMessages: false));
			(await Context.Guild.GetTextChannelsAsync()).ToList().ForEach(x =>
			{
				x.AddPermissionOverwriteAsync(muteRole, new OverwritePermissions(0, 805316689));
			});

			//Test if valid user mention
			var user = await Actions.getUser(Context.Guild, input);
			if (user == null)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.USER_ERROR));
				return;
			}

			//Give the targetted user the role
			await Actions.giveRole(user, muteRole);
			await Actions.makeAndDeleteSecondaryMessage(Context, String.Format("Successfully muted `{0}#{1}`.", user.Username, user.Discriminator));
		}

		[Command("textunmute")]
		[Alias("tum")]
		[Usage("textunmute [@User]")]
		[Summary("Gives the user back the ability type via removing the 'Muted' role.")]
		[PermissionRequirements((1U << (int)GuildPermission.ManageRoles) | (1U << (int)GuildPermission.ManageMessages))]
		public async Task FullUnmute([Remainder] string input)
		{
			//Test if valid user mention
			var user = await Actions.getUser(Context.Guild, input);
			if (user == null)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.USER_ERROR));
				return;
			}

			//Remove the role
			await Actions.takeRole(user, Actions.getRole(Context.Guild, Constants.MUTE_ROLE_NAME));
			await Actions.makeAndDeleteSecondaryMessage(Context, String.Format("Successfully unmuted `{0}#{1}`.", user.Username, user.Discriminator));
		}

		[Command("voicemute")]
		[Alias("vm")]
		[Usage("voicemute [@User]")]
		[Summary("If the user is not voice muted, this will mute them. If they are voice muted, this will unmute them.")]
		[PermissionRequirements(1U << (int)GuildPermission.MuteMembers)]
		public async Task Mute([Remainder] string input)
		{
			//Test if valid user mention
			var user = await Actions.getUser(Context.Guild, input);
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
		[Usage("deafen [@User]")]
		[Summary("Bans then unbans a user from the guild.")]
		[PermissionRequirements(1U << (int)GuildPermission.DeafenMembers)]
		public async Task Deafen([Remainder] string input)
		{
			//Test if valid user mention
			var user = await Actions.getUser(Context.Guild, input);
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

		[Command("moveuser")]
		[Alias("mu")]
		[Usage("moveuser [@User] [Channel]")]
		[Summary("Moves the user to the given voice channel.")]
		[PermissionRequirements(1U << (int)GuildPermission.MoveMembers)]
		public async Task MoveUser([Remainder] string input)
		{
			//Input and splitting
			var inputArray = input.Split(new char[] { ' ' }, 2);

			//Check if valid user
			var user = await Actions.getUser(Context.Guild, inputArray[0]);
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
			var channel = await Actions.getChannelEditAbility(Context, inputArray[1] + "/voice");
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

		[Command("nickname")]
		[Alias("nn")]
		[Usage("nickname [@User] [New Nickname|Remove]")]
		[Summary("Gives the user a nickname.")]
		[PermissionRequirements(1U << (int)GuildPermission.ManageNicknames)]
		public async Task Nickname([Remainder] string input)
		{
			//Input and splitting
			var inputArray = input.Split(new char[] { ' ' }, 2);
			var nickname = "";
			if (inputArray.Length == 2)
			{
				if (inputArray[1].Equals("remove", StringComparison.OrdinalIgnoreCase))
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
			if (nickname != null && nickname.Length > Constants.NICKNAME_MAX_LENGTH)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Nicknames cannot be longer than 32 characters."));
				return;
			}
			else if (nickname != null && nickname.Length < Constants.NICKNAME_MIN_LENGTH)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Nicknames cannot be less than 2 characters.."));
				return;
			}

			//Make sure it's a mention
			if (!inputArray[0].StartsWith("<@"))
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Please mention a user."));
				return;
			}

			//Check if valid user
			var user = await Actions.getUser(Context.Guild, inputArray[0]);
			if (user == null)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.USER_ERROR));
				return;
			}

			//Checks for positions
			var nicknamePosition = Actions.getPosition(Context.Guild, user);
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

		[Command("prunemembers")]
		[Alias("pmems")]
		[Usage("prunemembers [1|7|30] <Real>")]
		[Summary("Removes users who have no roles and have not been seen in the past given amount of days. Real means an actual prune, otherwise this returns the number of users that would have been pruned.")]
		[PermissionRequirements]
		public async Task PruneMembers([Remainder] string input)
		{
			//Split into the ints and 'bool'
			var inputArray = input.Split(new char[] { ' ' }, 2);
			int[] validDays = { 1, 7, 30 };

			//Get the int
			var amountOfDays = 0;
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
			else if (inputArray[1].Equals("real", StringComparison.OrdinalIgnoreCase))
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
		[Usage("softban [@User]")]
		[Summary("Bans then unbans a user from the guild.")]
		[PermissionRequirements(1U << (int)GuildPermission.BanMembers)]
		public async Task SoftBan([Remainder] string input)
		{
			//Test if valid user mention
			var inputUser = await Actions.getUser(Context.Guild, input);
			if (inputUser == null)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.USER_ERROR));
				return;
			}

			//Determine if the user is allowed to softban this person
			var sberPosition = Actions.getPosition(Context.Guild, Context.User as IGuildUser);
			var sbeePosition = Actions.getPosition(Context.Guild, inputUser);
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
		[Usage("ban [@User] [Days]")]
		[Summary("Bans the user from the guild.")]
		[PermissionRequirements(1U << (int)GuildPermission.BanMembers)]
		public async Task Ban([Remainder] string input)
		{
			//Test number of arguments
			var values = input.Split(' ');
			if (values.Length > 2 || values.Length == 0)
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
			var bannerPosition = Actions.getPosition(Context.Guild, Context.User as IGuildUser);
			var banneePosition = Actions.getPosition(Context.Guild, inputUser);
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
			var pruneDays = 0;
			if (values.Length == 2 && !Int32.TryParse(values[1], out pruneDays))
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Incorrect input for days of messages to be deleted."));
				return;
			}

			//Forming the second half of the string that prints out when a user is successfully banned
			var plurality = "days";
			if (pruneDays == 1)
			{
				plurality = "day";
			}
			var latterHalfOfString = "";
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
		[Usage("unban [User|User#Discriminator|User ID]")]
		[Summary("Unbans the user from the guild.")]
		[PermissionRequirements(1U << (int)GuildPermission.BanMembers)]
		public async Task Unban([Remainder] string input)
		{
			//Cut the user mention into the username and the discriminator
			var values = input.Split('#');
			if (values.Length != 2)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR));
				return;
			}

			//Get a list of the bans
			var bans = await Context.Guild.GetBansAsync();

			//Get their name and discriminator or ulong
			ulong inputUserID;
			var secondHalfOfTheSecondaryMessage = "";

			if (values.Length == 2)
			{
				//Unban given a username and discriminator
				ushort discriminator;
				if (!ushort.TryParse(values[1], out discriminator))
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid discriminator provided."));
				}
				var username = values[0].Replace("@", "");

				//Get a list of users with the name username and discriminator
				var bannedUserWithNameAndDiscriminator = bans.ToList().Where(x => x.User.Username.Equals(username) && x.User.Discriminator.Equals(discriminator)).ToList();

				//Unban the user
				var bannedUser = bannedUserWithNameAndDiscriminator[0].User;
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
				var bannedUsersWithSameName = bans.ToList().Where(x => x.User.Username.Equals(input)).ToList();
				if (bannedUsersWithSameName.Count > 1)
				{
					//Return a message saying if there are multiple users
					await Actions.sendChannelMessage(Context, String.Format("The following users have that name: `{0}`.", String.Join("`, `", bannedUsersWithSameName)));
					return;
				}
				else if (bannedUsersWithSameName.Count == 1)
				{
					//Unban the user
					var bannedUser = bannedUsersWithSameName[0].User;
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

				var bannedUser = bans.FirstOrDefault(x => x.User.Id.Equals(inputUserID)).User;
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

		[Command("kick")]
		[Alias("k")]
		[Usage("kick [@User]")]
		[Summary("Kicks the user from the guild.")]
		[PermissionRequirements(1U << (int)GuildPermission.KickMembers)]
		public async Task Kick([Remainder] string input)
		{
			//Test if valid user mention
			var inputUser = await Actions.getUser(Context.Guild, input);
			if (inputUser == null)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.USER_ERROR));
				return;
			}

			//Determine if the user is allowed to kick this person
			var kickerPosition = Actions.getPosition(Context.Guild, Context.User as IGuildUser);
			var kickeePosition = Actions.getPosition(Context.Guild, inputUser);
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

		[Command("currentbanlist")]
		[Alias("cbl")]
		[Usage("currentbanlist")]
		[Summary("Displays all the bans on the guild.")]
		[PermissionRequirements(1U << (int)GuildPermission.BanMembers)]
		public async Task CurrentBanList()
		{
			//Get and add the ban information
			var bans = await Context.Guild.GetBansAsync();

			//Initialize the ban string
			var description = "";

			//Initialize the count
			var count = 0;

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
					var hastebin = Actions.uploadToHastebin(Actions.replaceMessageCharacters(description));
					await Actions.sendEmbedMessage(Context.Channel, Actions.makeNewEmbed("Current Bans", hastebin));
				}
				else
				{
					await Actions.uploadTextFile(Context.Guild, Context.Channel, Actions.replaceMessageCharacters(description), "Current_Ban_List_", "Current Bans");
				}
				return;
			}
			else if (bans.Count == 0)
			{
				await Actions.sendChannelMessage(Context, "This guild has no bans.");
				return;
			}

			//Make and send the embed
			await Actions.sendEmbedMessage(Context.Channel, Actions.makeNewEmbed("Current Bans", description));
		}

		[Command("removemessages")]
		[Alias("rm")]
		[Usage("removemessages <@User> <#Channel> [Number of Messages]")]
		[Summary("Removes the selected number of messages from either the user, the channel, both, or, if neither is input, the current channel." +
			"People without administrator can only delete up to 100 messages at a time.")]
		[PermissionRequirements(1U << (int)GuildPermission.ManageMessages)]
		public async Task RemoveMessages([Remainder] string input)
		{
			var values = input.Split(' ');
			if (values.Length < 1 || values.Length > 3)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR));
				return;
			}

			var argIndex = 0;
			var argCount = values.Length;

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
			var inputChannel = Context.Channel as ITextChannel;
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
			var serverlogChannel = await Actions.logChannelCheck(Context.Guild, Constants.SERVER_LOG_CHECK_STRING);
			var modlogChannel = await Actions.logChannelCheck(Context.Guild, Constants.MOD_LOG_CHECK_STRING);
			if (Context.User.Id != Context.Guild.OwnerId && (inputChannel == serverlogChannel || inputChannel == modlogChannel))
			{
				//Send a message in the channel
				await Actions.sendChannelMessage(serverlogChannel == null ? modlogChannel : serverlogChannel,
					String.Format("Hey, @here, {0} is trying to delete stuff.", Context.User.Mention));

				//DM the owner of the server
				await Actions.sendDMMessage(await (await Context.Guild.GetOwnerAsync()).CreateDMChannelAsync(),
					String.Format("`{0}#{1}` ID: `{2}` is trying to delete stuff from the server/mod log.", Context.User.Username, Context.User.Discriminator, Context.User.Id));
				return;
			}

			//Checking for valid request count
			var requestCount = (argIndex == argCount - 1) ? Actions.getInteger(values[argIndex]) : -1;
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
				requestCount != 1 ? "messages" : "message",
				inputUser == null ? "" : " from `" + inputUser.Username + "#" + inputUser.Discriminator + "`",
				inputChannel == null ? "" : " on `#" + inputChannel.Name + "`"));
		}

		[Command("slowmode")]
		[Alias("sm")]
		[Usage("slowmode <Roles:.../.../> <Messages:1 to 5> <Time:1 to 30> <Guild:Yes> | Off [Guild|Channel|All]")]
		[Summary("The first argument is the roles that get ignored by slowmode, the second is the amount of messages, and the third is the time period. Default is: none, 1, 5." +
			"Bots are unaffected by slowmode. Any users who are immune due to roles stay immune even if they lose said role until a new slowmode is started.")]
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
			if (inputArray != null && inputArray[0].Equals("off", StringComparison.OrdinalIgnoreCase))
			{
				if (inputArray.Length != 2)
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR));
				}
				else if (inputArray[1].Equals("guild", StringComparison.OrdinalIgnoreCase))
				{
					//Remove the guild
					Variables.SlowmodeGuilds.Remove(Context.Guild.Id);

					//Send a success message
					await Actions.makeAndDeleteSecondaryMessage(Context, "Successfully removed the slowmode on the guild.");
				}
				else if (inputArray[1].Equals("channel", StringComparison.OrdinalIgnoreCase))
				{
					//Remove the channel
					Variables.SlowmodeChannels.Remove(Context.Channel as IGuildChannel);

					//Send a success message
					await Actions.makeAndDeleteSecondaryMessage(Context, "Successfully removed the slowmode on the channel.");
				}
				else if (inputArray[1].Equals("all", StringComparison.OrdinalIgnoreCase))
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
			var roleString = Actions.getVariable(inputArray, "roles");
			var messageString = Actions.getVariable(inputArray, "messages");
			var timeString = Actions.getVariable(inputArray, "time");
			var targetString = Actions.getVariable(inputArray, "guild");

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
					var role = Actions.getRole(Context.Guild, x);
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
			var msgsLimit = 1;
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
			var timeLimit = 5;
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
			await Actions.makeAndDeleteSecondaryMessage(Context, String.Format("Successfully enabled slowmode on `{0}` with a message limit of `{1}` and time interval of `{2}` seconds.{3}",
				targetString == null ? Context.Channel.Name : Context.Guild.Name,
				msgsLimit,
				timeLimit,
				roleNames.Count == 0 ? "" : String.Format("\nImmune roles: `{0}`.", String.Join("`, `", roleNames))));
		}

		[Command("forallwithrole")]
		[Alias("fawr")]
		[Usage("forallwithrole [Give|Take|Nickname] [Role]/[Role|Nickname] <" + Constants.BYPASS_STRING + ">")]
		[Summary("Only self hosted bots are allowed to go past ten members per use. When used on a self bot, \"" + Constants.BYPASS_STRING + "\" removes the 10 user limit.")]
		[PermissionRequirements]
		public async Task ForAllWithRole([Remainder] string input)
		{
			//Separating input into the action and role/role or nickname + bypass
			var inputArray = input.Split(new char[] { ' ' }, 2);
			var action = inputArray[0];
			if (inputArray.Length < 2)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR));
				return;
			}

			//Separate role/role or nickname + bypass into role and role or nickname + bypass
			var values = inputArray[1].Split('/');
			if (values.Length != 2)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR));
				return;
			}

			//Check if bypass, up the max limit, and remove the bypass string from the values array
			var maxLength = 10;
			if (values[1].EndsWith(Constants.BYPASS_STRING) && Context.User.Id.Equals(Properties.Settings.Default.BotOwner))
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
				var roleToGather = Actions.getRole(Context.Guild, values[0]);
				if (roleToGather == null)
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid role to gather."));
					return;
				}

				//Get the roles and their edit ability
				var roleToGive = await Actions.getRoleEditAbility(Context, values[1]);
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
				var listUsersWithRole = new List<IGuildUser>();
				foreach (var user in (await Context.Guild.GetUsersAsync()).ToList())
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
				foreach (var user in listUsersWithRole)
				{
					await Actions.giveRole(user, roleToGive);
				}

				await Actions.sendChannelMessage(Context, String.Format("Successfully gave `{0}` to all users{1} ({2} users).",
					roleToGive.Name, Context.Guild.EveryoneRole.Id.Equals(roleToGather.Id) ? "" : " with `" + roleToGather.Name + "`", listUsersWithRole.Count()));
			}
			else if (action.Equals("take"))
			{
				//Check if valid roles
				var roleToGather = Actions.getRole(Context.Guild, values[0]);
				if (roleToGather == null)
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid role to gather."));
					return;
				}
				var roleToTake = await Actions.getRoleEditAbility(Context, values[1]);
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
				var listUsersWithRole = new List<IGuildUser>();
				foreach (var user in (await Context.Guild.GetUsersAsync()).ToList())
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
				foreach (var user in listUsersWithRole)
				{
					await Actions.takeRole(user, roleToTake);
				}

				await Actions.sendChannelMessage(Context, String.Format("Successfully took `{0}` from all users{1} ({2} users).",
					roleToTake.Name, Context.Guild.EveryoneRole.Id.Equals(roleToGather.Id) ? "" : " with `" + roleToGather.Name + "`", listUsersWithRole.Count()));
			}
			else if (action.Equals("nickname"))
			{
				//Check if valid role
				var roleToGather = Actions.getRole(Context.Guild, values[0]);
				if (roleToGather == null)
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid role to gather."));
					return;
				}

				//Check if valid nickname length
				var inputNickname = values[1];
				if (inputNickname.Length > Constants.NICKNAME_MAX_LENGTH)
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Nicknames cannot be longer than 32 charaters."));
					return;
				}
				else if (inputNickname.Length < Constants.NICKNAME_MIN_LENGTH)
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Nicknames cannot be less than 2 characters."));
					return;
				}

				//Rename each user who has the role
				var botPosition = Actions.getPosition(Context.Guild, await Context.Guild.GetUserAsync(Variables.Bot_ID));
				var commandUserPosition = Actions.getPosition(Context.Guild, Context.User as IGuildUser);
				var listUsersWithRole = new List<IGuildUser>();
				foreach (var user in (await Context.Guild.GetUsersAsync()).ToList())
				{
					if (user.RoleIds.Contains(roleToGather.Id))
					{
						var userPosition = Actions.getPosition(Context.Guild, Context.User as IGuildUser);
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

				await Actions.sendChannelMessage(Context, String.Format("Successfully gave the nickname `{0}` to all users{1} ({2} users).",
					inputNickname, Context.Guild.EveryoneRole.Id.Equals(roleToGather.Id) ? "" : " with `" + roleToGather.Name + "`", listUsersWithRole.Count()));
			}
			else
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ACTION_ERROR));
				return;
			}
		}
	}
}
