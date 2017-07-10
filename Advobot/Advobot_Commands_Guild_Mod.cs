using Discord;
using Discord.Commands;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Advobot
{
	namespace GuildModeration
	{
		[Usage("[Name]")]
		[Summary("Change the name of the guild to the given name.")]
		[PermissionRequirement(1U << (int)GuildPermission.ManageGuild)]
		[DefaultEnabled(true)]
		public class ChangeGuildName : ModuleBase<MyCommandContext>
		{
			[Command("changeguildname")]
			[Alias("cgn")]
			public async Task Command(string name)
			{
				await CommandRunner(name);
			}

			private async Task CommandRunner(string name)
			{
				if (name.Length > Constants.MAX_GUILD_NAME_LENGTH)
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(String.Format("Guild names cannot be more than `{0}` characters.", Constants.MAX_GUILD_NAME_LENGTH)));
					return;
				}
				else if (name.Length < Constants.MIN_GUILD_NAME_LENGTH)
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(String.Format("Guild names cannot be less than `{0}` characters.", Constants.MIN_GUILD_NAME_LENGTH)));
					return;
				}

				await Context.Guild.ModifyAsync(x => x.Name = name);
				await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully changed the guild name to `{0}`.", name));
			}
		}

		[Usage("<Current|Region ID>")]
		[Summary("Shows or changes the guild's server region. Inputting nothing lists all valid region IDs.")]
		[PermissionRequirement(1U << (int)GuildPermission.ManageGuild)]
		[DefaultEnabled(true)]
		public class ChangeGuildRegion : ModuleBase<MyCommandContext>
		{
			[Command("changeguildregion")]
			[Alias("cgr")]
			public async Task Command(string region)
			{
				await CommandRunner(region);
			}

			private static readonly string mRegionIDs = String.Join("\n", Constants.VALID_REGION_IDS);
			private static readonly string mVIPRegionIDs = String.Join("\n", Constants.VIP_REGIONIDS);
			private static readonly string mAllRegionIDs = mRegionIDs + "\n" + mVIPRegionIDs;

			private async Task CommandRunner(string region)
			{
				if (String.IsNullOrWhiteSpace(region))
				{
					var desc = Context.Guild.Features.CaseInsContains(Constants.VIP_REGIONS) ? mAllRegionIDs : mRegionIDs;
					await Actions.SendEmbedMessage(Context.Channel, Actions.MakeNewEmbed("Region IDs", desc));
				}
				else if (Actions.CaseInsEquals(region, "current"))
				{
					await Actions.SendChannelMessage(Context, String.Format("The guild's current server region is `{0}`.", Context.Guild.VoiceRegionId));
				}
				else if (Constants.VALID_REGION_IDS.CaseInsContains(region) || (Context.Guild.Features.CaseInsContains(Constants.VIP_REGIONS) && Constants.VIP_REGIONIDS.CaseInsContains(region)))
				{
					var beforeRegion = Context.Guild.VoiceRegionId;
					await Context.Guild.ModifyAsync(x => x.RegionId = region);
					await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully changed the server region of the guild from `{0}` to `{1}`.", beforeRegion, region));
				}
				else
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("No valid region ID was input."));
				}
			}
		}

		[Usage("[Number]")]
		[Summary("Updates the guild's AFK timeout.")]
		[PermissionRequirement(1U << (int)GuildPermission.ManageGuild)]
		[DefaultEnabled(true)]
		public class ChangeGuildAFKTimer : ModuleBase<MyCommandContext>
		{
			[Command("changeguildafktimer")]
			[Alias("cgafkt")]
			public async Task Command(uint time)
			{
				await CommandRunner(time);
			}

			private async Task CommandRunner(uint time)
			{
				if (!Constants.VALID_AFK_TIMES.Contains(time))
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(String.Format("Invalid time input, must be one of the following: `{0}`.", String.Join("`, `", Constants.VALID_AFK_TIMES))));
					return;
				}

				await Context.Guild.ModifyAsync(x => x.AfkTimeout = (int)time);
				await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully set the guild's AFK timeout to `{0}`.", time));
			}
		}

		[Usage("[Channel]")]
		[Summary("Updates the guild's AFK channel.")]
		[PermissionRequirement(1U << (int)GuildPermission.ManageGuild)]
		[DefaultEnabled(true)]
		public class ChangeGuildAFKChannel : ModuleBase<MyCommandContext>
		{
			[Command("changeguildafkchannel")]
			[Alias("cgafkc")]
			public async Task Command(IVoiceChannel channel)
			{
				await CommandRunner(channel);
			}

			private async Task CommandRunner(IVoiceChannel channel)
			{
				await Context.Guild.ModifyAsync(x => x.AfkChannel = new Optional<IVoiceChannel>(channel));
				await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully set the guild's AFK channel to `{0}`.", channel.FormatChannel()));
			}
		}

		[Usage("[AllMessages|MentionsOnly]")]
		[Summary("Changes the message notifications to either all messages or mentions only.")]
		[PermissionRequirement(1U << (int)GuildPermission.ManageGuild)]
		[DefaultEnabled(true)]
		public class ChangeGuildMsgNotif : ModuleBase<MyCommandContext>
		{
			[Command("changeguildmsgnotif")]
			[Alias("cgmn")]
			public async Task Command(DefaultMessageNotifications msgNotifs)
			{
				await CommandRunner(msgNotifs);
			}

			private async Task CommandRunner(DefaultMessageNotifications msgNotifs)
			{
				await Context.Guild.ModifyAsync(x => x.DefaultMessageNotifications = msgNotifs);
				await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully changed the default message notification setting to `{0}`.", msgNotifs.EnumName()));
			}
		}

		[Usage("[None|Low|Medium|High|Extreme]")]
		[Summary("Changes the verification level. None is the most lenient (no requirements to type), high is the harshest (10 minutes in the guild before new members can type).")]
		[PermissionRequirement(1U << (int)GuildPermission.ManageGuild)]
		[DefaultEnabled(true)]
		public class ChangeGuildVerif : ModuleBase<MyCommandContext>
		{
			[Command("changeguildverif")]
			[Alias("cgv")]
			public async Task Command(VerificationLevel verif)
			{
				await CommandRunner(verif);
			}

			private async Task CommandRunner(VerificationLevel verif)
			{
				await Context.Guild.ModifyAsync(x => x.VerificationLevel = verif);
				await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully set the guild verification level as `{0}`.", verif.EnumName()));
			}
		}

		[Usage("[Attached Image|Embedded Image|Remove]")]
		[Summary("Changes the guild's icon to the given image. Typing `" + Constants.BOT_PREFIX + "gdi remove` will remove the icon. The image must be smaller than 2.5MB.")]
		[PermissionRequirement(1U << (int)GuildPermission.ManageGuild)]
		[DefaultEnabled(true)]
		public class ChangeGuildIcon : ModuleBase<MyCommandContext>
		{
			[Command("changeguildicon")]
			[Alias("cgi")]
			//TODO: TypeReader for images from Attachments or embeds
			public async Task Command(string other)
			{
				await CommandRunner(other);
			}

			//TODO:separate out setpicture and rework this command
			private async Task CommandRunner(string other)
			{
				await Actions.SetPicture(Context, other, false);
			}
		}

		[Usage("[Name]")]
		[Summary("Creates a guild with the bot as the owner.")]
		[OtherRequirement(Precondition.BotOwner)]
		[DefaultEnabled(true)]
		public class CreateGuild : ModuleBase<MyCommandContext>
		{
			[Command("createguild")]
			[Alias("cg")]
			public async Task Command(string name)
			{
				await CommandRunner(name);
			}

			private async Task CommandRunner(string name)
			{
				var optimalVoiceRegion = await Variables.Client.GetOptimalVoiceRegionAsync();
				var guild = await Variables.Client.CreateGuildAsync(name, optimalVoiceRegion);
				await Actions.CreateOrGetGuildInfo(guild);

				var defaultChannel = await guild.GetDefaultChannelAsync();
				var invite = await defaultChannel.CreateInviteAsync();
				var DMChannel = await Context.User.GetOrCreateDMChannelAsync();
				await Actions.SendDMMessage(DMChannel, invite.Url);
			}
		}

		[Usage("")]
		[Summary("If the bot is the current owner of the guild, this command will give you owner.")]
		[OtherRequirement(Precondition.BotOwner)]
		[DefaultEnabled(true)]
		public class ChangeGuildOwner : ModuleBase<MyCommandContext>
		{
			[Command("changeguildowner")]
			[Alias("cgo")]
			public async Task Command()
			{
				await CommandRunner();
			}

			private async Task CommandRunner()
			{
				if ((await Context.Guild.GetOwnerAsync()).Id == Variables.BotID)
				{
					await Context.Guild.ModifyAsync(x => x.Owner = new Optional<IUser>(Context.User));
					await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("{0} is now the owner.", Context.User.Mention));
					return;
				}

				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("The bot is not the owner of the guild."));
			}
		}

		[Usage("")]
		[Summary("If the bot is the current owner of the guild, this command will delete the guild.")]
		[OtherRequirement(Precondition.BotOwner)]
		[DefaultEnabled(true)]
		public class DeleteGuild : ModuleBase<MyCommandContext>
		{
			[Command("deleteguild")]
			[Alias("dg")]
			public async Task Command()
			{
				await CommandRunner();
			}

			private async Task CommandRunner()
			{
				if (Variables.BotID == Context.Guild.OwnerId)
				{
					await Context.Guild.DeleteAsync();
					return;
				}

				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("The bot is not the owner of the guild and thus cannot delete it."));
			}
		}
	}
}
