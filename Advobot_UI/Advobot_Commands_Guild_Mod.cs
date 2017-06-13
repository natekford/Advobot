using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Advobot
{
	//Guild Moderation commands are commands that affect the guild itself and nothing else
	[Name("Guild_Moderation")]
	public class Advobot_Commands_Guild_Mod : ModuleBase
	{
		[Command("changeguildname")]
		[Alias("cgn")]
		[Usage("[New Name]")]
		[Summary("Change the name of the guild to the given name.")]
		[PermissionRequirement(1U << (int)GuildPermission.ManageGuild)]
		[DefaultEnabled(true)]
		public async Task ChangeGuildName([Remainder] string input)
		{
			//Guild names have the same length requirements as channel names, so I'm not changing the variable names

			//Check if valid length
			if (input.Length > Constants.MAX_CHANNEL_NAME_LENGTH)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(String.Format("Name cannot be more than `{0}` characters.", Constants.MAX_CHANNEL_NAME_LENGTH)));
				return;
			}
			else if (input.Length < Constants.MIN_CHANNEL_NAME_LENGTH)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(String.Format("Name cannot be less than `{0}` characters.", Constants.MIN_CHANNEL_NAME_LENGTH)));
				return;
			}

			//Change the name and say what it was changed to
			await Context.Guild.ModifyAsync(x => x.Name = input);
			await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully changed the guild name to `{0}`.", input));
		}

		[Command("changeguildregion")]
		[Alias("cgr")]
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
				if (Context.Guild.Features.CaseInsContains(Constants.VIP_REGIONS))
				{
					text += "\n" + String.Join("\n", Constants.VIP_REGIONIDS);
				}
				await Actions.SendEmbedMessage(Context.Channel, Actions.MakeNewEmbed("Region IDs", text));
			}
			else if (Actions.CaseInsEquals(input, "current"))
			{
				await Actions.SendChannelMessage(Context, String.Format("The guild's current server region is `{0}`.", Context.Guild.VoiceRegionId));
			}
			else if (Constants.VALID_REGION_IDS.CaseInsContains(input))
			{
				//Capture the previous region
				var bRegion = Context.Guild.VoiceRegionId;

				//Change the region
				await Context.Guild.ModifyAsync(x => x.RegionId = input);
				await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully changed the server region of the guild from `{0}` to `{1}`.", bRegion, input));
			}
			else if (Constants.VIP_REGIONIDS.CaseInsContains(input))
			{
				//Check if the guild can access vip regions
				if (Context.Guild.Features.CaseInsContains(Constants.VIP_REGIONS))
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

		[Command("changeguildafk")]
		[Alias("cgafk")]
		[Usage("[Channel] <Time:Time in Seconds>")]
		[Summary("Updates the guild's afk channel and timeout.")]
		[PermissionRequirement(1U << (int)GuildPermission.ManageGuild)]
		[DefaultEnabled(true)]
		public async Task ChangeGuildAFK([Remainder] string input)
		{
			//Split at space into two args
			var returnedArgs = Actions.GetArgs(Context, input, new ArgNumbers(1, 2), new[] { "time" });
			if (returnedArgs.Reason != ArgFailureReason.Not_Failure)
			{
				await Actions.HandleArgsGettingErrors(Context, returnedArgs);
				return;
			}
			var chanStr = returnedArgs.Arguments[0];
			var timeStr = returnedArgs.GetSpecifiedArg("time");

			//Check if valid action
			if (!String.IsNullOrWhiteSpace(chanStr))
			{
				//Check if valid channel
				var returnedChannel = Actions.GetChannel(Context, new[] { ChannelCheck.Can_Modify_Permissions, ChannelCheck.Is_Voice }, false, chanStr);
				if (returnedChannel.Reason != FailureReason.Not_Failure)
				{
					await Actions.HandleObjectGettingErrors(Context, returnedChannel);
					return;
				}
				var channel = returnedChannel.Object;

				var bChan = Context.Guild.AFKChannelId.HasValue ? (await Context.Guild.GetChannelAsync(Context.Guild.AFKChannelId.Value)) : null;
				await Context.Guild.ModifyAsync(x => x.AfkChannelId = channel.Id);
				await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully changed the guild's AFK channel from `{0}` to `{1}`", bChan.FormatChannel(), channel.FormatChannel()));
			}
			else if (!String.IsNullOrWhiteSpace(timeStr))
			{
				//Check if valid time
				if (!int.TryParse(timeStr, out int time))
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid input for time."));
					return;
				}

				//Check if valid amount
				int[] validAmount = { 60, 300, 900, 1800, 3600 };
				if (!validAmount.Contains(time))
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(String.Format("Invalid time input, must be one of the following: `{0}`.", String.Join("`, `", validAmount))));
					return;
				}

				var bTime = Context.Guild.AFKTimeout.ToString();
				await Context.Guild.ModifyAsync(x => x.AfkTimeout = time);
				await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully changed the guild's AFK timer from `{0}` to `{1}`.", bTime, timeStr));
			}
			else
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ACTION_ERROR));
			}
		}

		[Command("changeguildmsgnotif")]
		[Alias("cgmn")]
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

		[Command("changeguildverif")]
		[Alias("cgv")]
		[Usage("[None|Low|Medium|High]")]
		[Summary("Changes the verification level. None is the most lenient (no requirements to type), high is the harshest (10 minutes in the guild before new members can type).")]
		[PermissionRequirement(1U << (int)GuildPermission.ManageGuild)]
		[DefaultEnabled(true)]
		public async Task ChangeGuildVerification([Remainder] string input)
		{
			if (Enum.TryParse(input, true, out VerificationLevel vLevel))
			{
				await Context.Guild.ModifyAsync(x => x.VerificationLevel = vLevel);
				await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully set the guild verification level as `{0}`.", Enum.GetName(typeof(VerificationLevel), vLevel)));
			}
			else
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, "Invalid verification level provided.");
			}
		}

		[Command("changeguildicon")]
		[Alias("cgi")]
		[Usage("[Attached Image|Embedded Image|Remove]")]
		[Summary("Changes the guild's icon to the given image. Typing `" + Constants.BOT_PREFIX + "gdi remove` will remove the icon. The image must be smaller than 2.5MB.")]
		[PermissionRequirement(1U << (int)GuildPermission.ManageGuild)]
		[DefaultEnabled(true)]
		public async Task ChangeGuildIcon([Optional] string input)
		{
			await Actions.SetPicture(Context, input, false);
		}

		[Command("createguild")]
		[Alias("cg")]
		[Usage("[Name]")]
		[Summary("Creates a guild with the bot as the owner.")]
		[OtherRequirement(1U << (int)Precondition.Bot_Owner)]
		[DefaultEnabled(true)]
		public async Task GuildCreate([Remainder] string input)
		{
			//Get a region for the bot to create the guild with
			var region = await Variables.Client.GetOptimalVoiceRegionAsync();
			//Create the guild
			var guild = await Variables.Client.CreateGuildAsync(input, region);
			//Add the guild to the list of guilds
			Variables.Guilds.Add(guild.Id, new BotGuildInfo(guild.Id));
			//Create an invite
			var invite = await (await guild.GetTextChannelsAsync()).FirstOrDefault().CreateInviteAsync(null);
			//Send that invite to the user who used this command
			await (await Context.User.CreateDMChannelAsync()).SendMessageAsync(invite.Url);
		}

		[Command("changeguildowner")]
		[Alias("cgo")]
		[Usage("")]
		[Summary("If the bot is the current owner of the guild, this command will give you owner.")]
		[OtherRequirement(1U << (int)Precondition.Bot_Owner)]
		[DefaultEnabled(true)]
		public async Task GuildAdmin()
		{
			//Check if the user is the only person in the guild
			var owner = await Context.Guild.GetOwnerAsync();
			if (owner.Id != Variables.BotID)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(String.Format("The bot is not the owner of the guild. The owner is: `{0}`.", owner.FormatUser())));
				return;
			}

			await Context.Guild.ModifyAsync(x => x.Owner = new Optional<IUser>(Context.User));
			await Actions.MakeAndDeleteSecondaryMessage(Context, "You are now the owner.");
		}

		[Command("deleteguild")]
		[Alias("dg")]
		[Usage("")]
		[Summary("If the bot is the current owner of the guild, this command will delete the guild.")]
		[OtherRequirement(1U << (int)Precondition.Bot_Owner)]
		[DefaultEnabled(true)]
		public async Task GuildDelete()
		{
			//Check if the bot can delete the guild
			if (Variables.BotID != Context.Guild.OwnerId)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("The bot is not the owner of the guild and thus cannot delete it."));
				return;
			}

			//Delete the guild
			await Context.Guild.DeleteAsync();
		}
	}
}
