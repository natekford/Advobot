using Discord;
using Discord.Commands;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Advobot
{
	//Guild Moderation commands are commands that affect the guild itself and nothing else
	[Name("Guild Moderation")]
	public class Advobot_Commands_Guild_Mod : ModuleBase
	{
		[Command("guildname")]
		[Alias("gn")]
		[Usage("guildname [New Name]")]
		[Summary("Change the name of the guild to the given name.")]
		[PermissionRequirements(1U << (int)GuildPermission.ManageGuild)]
		public async Task ChangeGuildName([Remainder] string input)
		{
			//Guild names have the same length requirements as channel names, so I'm not changing the variable names

			//Check if valid length
			if (input.Length > Constants.CHANNEL_NAME_MAX_LENGTH)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Guild names cannot be longer than 100 characters."));
				return;
			}
			else if (input.Length < Constants.CHANNEL_NAME_MIN_LENGTH)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Guild names cannot be shorter than 2 characters."));
				return;
			}

			//Change the name and say what it was changed to
			await Context.Guild.ModifyAsync(x => x.Name = input);
			await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully changed the guild name to `{0}`.", input));
		}

		[Command("guildregion")]
		[Alias("greg")]
		[Usage("guildregion [Regions|Current|Region ID]")]
		[Summary("Shows or changes the guild's server region. `Regions` lists all valid region IDs.")]
		[PermissionRequirements(1U << (int)GuildPermission.ManageGuild)]
		public async Task ChangeGuildLocation([Remainder] string input)
		{
			//Check if a valid region or asking to see the region types
			if (input.Equals("regions", StringComparison.OrdinalIgnoreCase))
			{
				await Actions.SendEmbedMessage(Context.Channel, Actions.MakeNewEmbed("Region IDs", String.Join("\n", Constants.VALIDREGIONIDS)));
			}
			else if (input.Equals("current", StringComparison.OrdinalIgnoreCase))
			{
				await Actions.SendChannelMessage(Context, String.Format("The guild's current server region is `{0}`.", Context.Guild.VoiceRegionId));
			}
			else if (Constants.VALIDREGIONIDS.Contains(input, StringComparer.OrdinalIgnoreCase))
			{
				//Capture the previous region
				var bRegion = Context.Guild.VoiceRegionId;

				//Change the region
				await Context.Guild.ModifyAsync(x => x.RegionId = input);
				await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully changed the server region of the guild from `{0}` to `{1}`.", bRegion, input));
			}
			//else if (Constants.VIPREGIONIDS.Contains(input, StringComparer.OrdinalIgnoreCase))
			//{
			//	//Figure out how to check if the guild is VIP
			//	if ()
			//	{
			//		//Capture the previous region
			//		var bRegion = Context.Guild.VoiceRegionId;

			//		//Change the region
			//		await Context.Guild.ModifyAsync(x => x.RegionId = input);
			//		await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully changed the server region of the guild from `{0}` to `{1}`.", bRegion, input));
			//	}
			//}
			else
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("No valid region ID was input."));
			}
		}

		[Command("guildafk")]
		[Alias("gafk")]
		[Usage("guildafk [Channel|Time] [Voice Channel Name|Time in Seconds]")]
		[Summary("The first argument tells if the channel or timer is going to be changed. The second is what it will be changed to.")]
		[PermissionRequirements(1U << (int)GuildPermission.ManageGuild)]
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
			if (inputArray[0].Equals("channel", StringComparison.OrdinalIgnoreCase))
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
			else if (inputArray[0].Equals("time", StringComparison.OrdinalIgnoreCase))
			{
				//Check if valid time
				var time = 0;
				if (!int.TryParse(inputArray[1], out time))
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
		[Alias("gmn")]
		[Usage("guildmsgnotifications [All Messages|Mentions Only]")]
		[Summary("Changes the message notifications to either all messages or mentions only.")]
		[PermissionRequirements(1U << (int)GuildPermission.ManageGuild)]
		public async Task ChangeGuildMsgNotifications([Remainder] string input)
		{
			if (input.Equals("all messages", StringComparison.OrdinalIgnoreCase))
			{
				await Context.Guild.ModifyAsync(x => x.DefaultMessageNotifications = DefaultMessageNotifications.AllMessages);
				await Actions.MakeAndDeleteSecondaryMessage(Context, "Successfully changed the default message notification setting to all messages.");
			}
			else if (input.Equals("mentions only", StringComparison.OrdinalIgnoreCase))
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
		[Alias("gv")]
		[Usage("guildverification [0|1|2|3]")]
		[Summary("Changes the verification level. 0 is the most lenient (no requirements to type), 3 is the harshest (10 minutes in the guild before new members can type).")]
		[PermissionRequirements(1U << (int)GuildPermission.ManageGuild)]
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
		[Alias("gi")]
		[Usage("guildicon [Attached Image|Embedded Image|Remove]")]
		[Summary("Changes the guild icon to the given image. Must be less than 2.5MB simply because the bot would use more data and be slower otherwise.")]
		[PermissionRequirements(1U << (int)GuildPermission.ManageGuild)]
		public async Task ChangeGuildIcon([Optional] string input)
		{
			await Actions.SetPicture(Context, input, false);
		}
	}
}
