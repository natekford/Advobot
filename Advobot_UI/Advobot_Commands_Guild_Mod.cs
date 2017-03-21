using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Advobot
{
	//Guild Moderation commands are commands that affect the guild itself and nothing else
	[Name("Guild_Moderation")]
	public class Advobot_Commands_Guild_Mod : ModuleBase
	{
		[Command("guildname")]
		[Alias("gdn")]
		[Usage("[New Name]")]
		[Summary("Change the name of the guild to the given name.")]
		[PermissionRequirement(1U << (int)GuildPermission.ManageGuild)]
		[DefaultEnabled(true)]
		public async Task ChangeGuildName([Remainder] string input)
		{
			//Guild names have the same length requirements as channel names, so I'm not changing the variable names

			//Check if valid length
			if (input.Length > Constants.CHANNEL_NAME_MAX_LENGTH)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(String.Format("Name cannot be more than `{0}` characters.", Constants.CHANNEL_NAME_MAX_LENGTH)));
				return;
			}
			else if (input.Length < Constants.CHANNEL_NAME_MIN_LENGTH)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(String.Format("Name cannot be less than `{0}` characters.", Constants.CHANNEL_NAME_MIN_LENGTH)));
				return;
			}

			//Change the name and say what it was changed to
			await Context.Guild.ModifyAsync(x => x.Name = input);
			await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully changed the guild name to `{0}`.", input));
		}

		[Command("guildregion")]
		[Alias("gdr")]
		[Usage("[Regions|Current|Region ID]")]
		[Summary("Shows or changes the guild's server region. `Regions` lists all valid region IDs.")]
		[PermissionRequirement(1U << (int)GuildPermission.ManageGuild)]
		[DefaultEnabled(true)]
		public async Task ChangeGuildLocation([Remainder] string input)
		{
			//Check if a valid region or asking to see the region types
			if (Actions.CaseInsEquals(input, "regions"))
			{
				var text = String.Join("\n", Constants.VALID_REGION_IDS);
				//Check whether to show the VIP regions
				if (Actions.CaseInsContains(Context.Guild.Features.ToList(), Constants.VIP_REGIONS))
				{
					text += "\n" + String.Join("\n", Constants.VIP_REGIONIDS);
				}
				await Actions.SendEmbedMessage(Context.Channel, Actions.MakeNewEmbed("Region IDs", text));
			}
			else if (Actions.CaseInsEquals(input, "current"))
			{
				await Actions.SendChannelMessage(Context, String.Format("The guild's current server region is `{0}`.", Context.Guild.VoiceRegionId));
			}
			else if (Actions.CaseInsContains(Constants.VALID_REGION_IDS, input))
			{
				//Capture the previous region
				var bRegion = Context.Guild.VoiceRegionId;

				//Change the region
				await Context.Guild.ModifyAsync(x => x.RegionId = input);
				await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully changed the server region of the guild from `{0}` to `{1}`.", bRegion, input));
			}
			else if (Actions.CaseInsContains(Constants.VIP_REGIONIDS, input))
			{
				//Check if the guild can access vip regions
				if (Actions.CaseInsContains(Context.Guild.Features.ToList(), Constants.VIP_REGIONS))
				{
					//Capture the previous region
					var bRegion = Context.Guild.VoiceRegionId;

					//Change the region
					await Context.Guild.ModifyAsync(x => x.RegionId = input);
					await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully changed the server region of the guild from `{0}` to `{1}`.", bRegion, input));
				}
			}
			else
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("No valid region ID was input."));
			}
		}

		[Command("guildafk")]
		[Alias("gdafk")]
		[Usage("[Channel|Time] [Voice Channel Name|Time in Seconds]")]
		[Summary("The first argument tells if the channel or timer is going to be changed. The second is what it will be changed to.")]
		[PermissionRequirement(1U << (int)GuildPermission.ManageGuild)]
		[DefaultEnabled(true)]
		public async Task ChangeGuildAFK([Remainder] string input)
		{
			//Split at space into two args
			var inputArray = input.Split(new char[] { ' ' }, 2);

			//Check if valid number of args
			if (inputArray.Length != 2)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR));
				return;
			}

			//Check if valid action
			if (Actions.CaseInsEquals(inputArray[0], "channel"))
			{
				//Check if valid channel
				var channel = await Actions.GetChannelEditAbility(Context, inputArray[1] + "/voice");
				if (channel == null)
					return;
				else if (Actions.GetChannelType(channel) != Constants.VOICE_TYPE)
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("The guild's afk channel has to be a voice channel."));
					return;
				}

				//Capture the before channel
				var bChan = Context.Guild.AFKChannelId.HasValue ? (await Context.Guild.GetChannelAsync(Context.Guild.AFKChannelId.Value)).Name : "Nothing";

				//Change the afk channel to the input
				await Context.Guild.ModifyAsync(x => x.AfkChannelId = channel.Id);
				await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully changed the guild's AFK channel from `{0}` to `{1}`", bChan, inputArray[1]));
			}
			else if (Actions.CaseInsEquals(inputArray[0], "time"))
			{
				//Check if valid time
				if (!int.TryParse(inputArray[1], out int time))
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid input for time."));
					return;
				}

				//Check if valid amount
				int[] validAmount = { 60, 300, 900, 1800, 3600 };
				if (!validAmount.Contains(time))
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid time input, must be one of the following: `" + String.Join("`, `", validAmount) + "`."));
					return;
				}

				//Capture the before timer
				var bTime = Context.Guild.AFKTimeout.ToString();

				//Change the afk timer
				await Context.Guild.ModifyAsync(x => x.AfkTimeout = time);
				await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully changed the guild's AFK timer from `{0}` to `{1}`.", bTime, inputArray[1]));
			}
			else
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("No valid action was input."));
			}
		}

		[Command("guildmsgnotifications")]
		[Alias("gdmn")]
		[Usage("[All|Mentions]")]
		[Summary("Changes the message notifications to either all messages or mentions only.")]
		[PermissionRequirement(1U << (int)GuildPermission.ManageGuild)]
		[DefaultEnabled(true)]
		public async Task ChangeGuildMsgNotifications([Remainder] string input)
		{
			if (Actions.CaseInsEquals(input, "all"))
			{
				await Context.Guild.ModifyAsync(x => x.DefaultMessageNotifications = DefaultMessageNotifications.AllMessages);
				await Actions.MakeAndDeleteSecondaryMessage(Context, "Successfully changed the default message notification setting to all messages.");
			}
			else if (Actions.CaseInsEquals(input, "mentions"))
			{
				await Context.Guild.ModifyAsync(x => x.DefaultMessageNotifications = DefaultMessageNotifications.MentionsOnly);
				await Actions.MakeAndDeleteSecondaryMessage(Context, "Successfully changed the default message notification setting to mentions only.");
			}
			else
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid message notification setting."));
			}
		}

		[Command("guildverification")]
		[Alias("gdv")]
		[Usage("[0|1|2|3]")]
		[Summary("Changes the verification level. 0 is the most lenient (no requirements to type), 3 is the harshest (10 minutes in the guild before new members can type).")]
		[PermissionRequirement(1U << (int)GuildPermission.ManageGuild)]
		[DefaultEnabled(true)]
		public async Task ChangeGuildVerification([Remainder] string input)
		{
			//Check if valid int
			var vLevel = -1;
			if (!int.TryParse(input, out vLevel))
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid verification level."));
				return;
			}

			//Check if valid verification level position
			if (vLevel > 3 || vLevel < 0)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid verification level. Verification levels range from 0 to 3."));
				return;
			}
			else
			{
				//Change the verification level
				await Context.Guild.ModifyAsync(x => x.VerificationLevel = (VerificationLevel)vLevel);

				//Get the verification level's name as a string
				var vString = Enum.GetName(typeof(VerificationLevel), vLevel);
				//Send a success message
				await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully set the guild verification level as `{0}`.", vString));
			}
		}

		[Command("guildicon")]
		[Alias("gdi")]
		[Usage("[Attached Image|Embedded Image|Remove]")]
		[Summary("Changes the guild icon to the given image. Must be less than 2.5MB simply because the bot would use more data and be slower otherwise.")]
		[PermissionRequirement(1U << (int)GuildPermission.ManageGuild)]
		[DefaultEnabled(true)]
		public async Task ChangeGuildIcon([Optional] string input)
		{
			await Actions.SetPicture(Context, input, false);
		}

#if false
		[Command("guildowner")]
		[Alias("gdo")]
		[Usage("<@User>")]
		[Summary("Changes the guild's owner to the given user.")]
		[PermissionRequirement]
		[DefaultEnabled(true)]
		public async Task GuildOwner([Optional, Remainder] string input)
		{
			var user = await (String.IsNullOrWhiteSpace(input) ? Context.Guild.GetUserAsync(Context.User.Id) : Actions.GetUser(Context.Guild, input));
			if (user == null)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.USER_ERROR));
				return;
			}

			//Create the guild owner role
			var role = await Actions.GetRole(Context, "Guild Owner") ?? await Context.Guild.CreateRoleAsync("Guild Owner");
			//Give the role to the user
			await user.AddRoleAsync(role);
			//Change the position of the newly created role
			await Actions.ModifyRolePosition(role, int.MaxValue);

			//Have the bot leave and thus give the owner position to the highest ranking person
			await Context.Guild.LeaveAsync();
		}
#endif
		[Command("guildcreate")]
		[Alias("gdc")]
		[Usage("[Name]")]
		[Summary("Creates a guild.")]
		[BotOwnerRequirement]
		[DefaultEnabled(true)]
		public async Task GuildCreate([Remainder] string input)
		{
			//Get a region for the bot to create the guild with
			var region = await Variables.Client.GetOptimalVoiceRegionAsync();
			//Create the guild
			var guild = await Variables.Client.CreateGuildAsync(input, region);
			//Add the guild to the list of guilds
			Variables.Guilds.Add(guild.Id, new BotGuildInfo(guild));
			//Create an invite
			var invite = await (await guild.GetTextChannelsAsync()).FirstOrDefault().CreateInviteAsync(null);
			//Send that invite to the user who used this command
			await (await Context.User.CreateDMChannelAsync()).SendMessageAsync(invite.Url);
		}

		[Command("guildadmin")]
		[Alias("gda")]
		[Usage("")]
		[Summary("Gives you admin assuming only you and the bot are in the server.")]
		[BotOwnerRequirement]
		[DefaultEnabled(true)]
		public async Task GuildAdmin()
		{
			//Gather the owner and roles in the guild
			var owner = await Context.Guild.GetOwnerAsync();
			var roles = Context.Guild.Roles.OrderBy(x => x.Position).ToList();

			//Check if the user is the only person in the guild
			if ((Context.Guild as SocketGuild).MemberCount > 2)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("You are not the only person in the guild."));
				return;
			}
			//Check if the bot's not the owner of the guild
			else if (owner.Id != Variables.Bot_ID)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(String.Format("The bot is not the owner of the guild. The owner is: `{0}`.", Actions.FormatUser(owner))));
				return;
			}

			//Get or make the roles
			const string botRoleName = "Bot";
			var botRole = roles.Where(x => x.Name == botRoleName).FirstOrDefault() ?? await Context.Guild.CreateRoleAsync(botRoleName, new GuildPermissions(0));
			const string userRoleName = "Owner";
			var userRole = roles.Where(x => x.Name == userRoleName).FirstOrDefault() ?? await Context.Guild.CreateRoleAsync(userRoleName, new GuildPermissions(8));

			//Update roles
			roles = Context.Guild.Roles.ToList();

			//Make sure their positions are good
			await Actions.ModifyRolePosition(botRole, Math.Max(roles.Count - 1, 2));
			await Actions.ModifyRolePosition(userRole, Math.Max(roles.Count - 2, 1));

			//Give the bot and user their roles
			await Actions.GiveRole(owner, botRole);
			await Actions.GiveRole(Context.User as IGuildUser, userRole);

			//Send the success message
			await Actions.MakeAndDeleteSecondaryMessage(Context, "You have successfully effectively become the owner.");
		}

		[Command("guilddelete")]
		[Alias("gdd")]
		[Usage("")]
		[Summary("If the bot is the current owner of the guild it will delete it.")]
		[BotOwnerRequirement]
		[DefaultEnabled(true)]
		public async Task GuildDelete()
		{
			//Check if the bot can delete the guild
			if (Variables.Bot_ID != Context.Guild.OwnerId)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("The bot is not the owner of the guild and thus cannot delete it."));
				return;
			}

			//Delete the guild
			await Context.Guild.DeleteAsync();
		}
	}
}
