using Advobot.Actions;
using Discord;
using Discord.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Advobot
{
	namespace GuildModeration
	{
		[Group("changeguildname"), Alias("cgn")]
		[Usage("[Name]")]
		[Summary("Change the name of the guild to the given name.")]
		[PermissionRequirement(new[] { GuildPermission.ManageGuild }, null)]
		[DefaultEnabled(true)]
		public class ChangeGuildName : MyModuleBase
		{
			[Command]
			public async Task Command([Remainder, VerifyStringLength(Target.Guild)] string name)
			{
				await CommandRunner(name);
			}

			private async Task CommandRunner(string name)
			{
				await Context.Guild.ModifyAsync(x => x.Name = name);
				await Messages.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully changed the guild name to `{0}`.", name));
			}
		}

		[Group("changeguildregion"), Alias("cgr")]
		[Usage("<Current|Region ID>")]
		[Summary("Shows or changes the guild's server region. Inputting nothing lists all valid region IDs.")]
		[PermissionRequirement(new[] { GuildPermission.ManageGuild }, null)]
		[DefaultEnabled(true)]
		public class ChangeGuildRegion : MyModuleBase
		{
			[Command]
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
					await Messages.SendEmbedMessage(Context.Channel, Messages.MakeNewEmbed("Region IDs", desc));
				}
				else if ("current".CaseInsEquals(region))
				{
					await Messages.SendChannelMessage(Context, String.Format("The guild's current server region is `{0}`.", Context.Guild.VoiceRegionId));
				}
				else if (Constants.VALID_REGION_IDS.CaseInsContains(region) || (Context.Guild.Features.CaseInsContains(Constants.VIP_REGIONS) && Constants.VIP_REGIONIDS.CaseInsContains(region)))
				{
					var beforeRegion = Context.Guild.VoiceRegionId;
					await Context.Guild.ModifyAsync(x => x.RegionId = region);
					await Messages.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully changed the server region of the guild from `{0}` to `{1}`.", beforeRegion, region));
				}
				else
				{
					await Messages.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("No valid region ID was input."));
				}
			}
		}

		[Group("changeguildafktimer"), Alias("cgafkt")]
		[Usage("[Number]")]
		[Summary("Updates the guild's AFK timeout.")]
		[PermissionRequirement(new[] { GuildPermission.ManageGuild }, null)]
		[DefaultEnabled(true)]
		public class ChangeGuildAFKTimer : MyModuleBase
		{
			[Command]
			public async Task Command(uint time)
			{
				await CommandRunner(time);
			}

			private async Task CommandRunner(uint time)
			{
				if (!Constants.VALID_AFK_TIMES.Contains(time))
				{
					await Messages.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR(String.Format("Invalid time input, must be one of the following: `{0}`.", String.Join("`, `", Constants.VALID_AFK_TIMES))));
					return;
				}

				await Context.Guild.ModifyAsync(x => x.AfkTimeout = (int)time);
				await Messages.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully set the guild's AFK timeout to `{0}`.", time));
			}
		}

		[Group("changeguildafkchannel"), Alias("cgafkc")]
		[Usage("[Channel]")]
		[Summary("Updates the guild's AFK channel.")]
		[PermissionRequirement(new[] { GuildPermission.ManageGuild }, null)]
		[DefaultEnabled(true)]
		public class ChangeGuildAFKChannel : MyModuleBase
		{
			[Command]
			public async Task Command(IVoiceChannel channel)
			{
				await CommandRunner(channel);
			}

			private async Task CommandRunner(IVoiceChannel channel)
			{
				await Context.Guild.ModifyAsync(x => x.AfkChannel = new Optional<IVoiceChannel>(channel));
				await Messages.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully set the guild's AFK channel to `{0}`.", channel.FormatChannel()));
			}
		}

		[Group("changeguildmsgnotif"), Alias("cgmn")]
		[Usage("[AllMessages|MentionsOnly]")]
		[Summary("Changes the message notifications to either all messages or mentions only.")]
		[PermissionRequirement(new[] { GuildPermission.ManageGuild }, null)]
		[DefaultEnabled(true)]
		public class ChangeGuildMsgNotif : MyModuleBase
		{
			[Command]
			public async Task Command(DefaultMessageNotifications msgNotifs)
			{
				await CommandRunner(msgNotifs);
			}

			private async Task CommandRunner(DefaultMessageNotifications msgNotifs)
			{
				await Context.Guild.ModifyAsync(x => x.DefaultMessageNotifications = msgNotifs);
				await Messages.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully changed the default message notification setting to `{0}`.", msgNotifs.EnumName()));
			}
		}

		[Group("changeguildverif"), Alias("cgv")]
		[Usage("[None|Low|Medium|High|Extreme]")]
		[Summary("Changes the verification level. None is the most lenient (no requirements to type), high is the harshest (10 minutes in the guild before new members can type).")]
		[PermissionRequirement(new[] { GuildPermission.ManageGuild }, null)]
		[DefaultEnabled(true)]
		public class ChangeGuildVerif : MyModuleBase
		{
			[Command]
			public async Task Command(VerificationLevel verif)
			{
				await CommandRunner(verif);
			}

			private async Task CommandRunner(VerificationLevel verif)
			{
				await Context.Guild.ModifyAsync(x => x.VerificationLevel = verif);
				await Messages.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully set the guild verification level as `{0}`.", verif.EnumName()));
			}
		}

		[Group("changeguildicon"), Alias("cgi")]
		[Usage("<Attached Image|Embedded Image>")]
		[Summary("Changes the guild's icon to the given image. The image must be smaller than 2.5MB. Inputting nothing removes the guild's icon.")]
		[PermissionRequirement(new[] { GuildPermission.ManageGuild }, null)]
		[DefaultEnabled(true)]
		public class ChangeGuildIcon : MyModuleBase
		{
			[Command]
			public async Task Command()
			{
				await CommandRunner();
			}

			private async Task CommandRunner()
			{
				var attach = Context.Message.Attachments.Where(x => x.Width != null && x.Height != null).Select(x => x.Url);
				var embeds = Context.Message.Embeds.Where(x => x.Image.HasValue).Select(x => x.Image?.Url);
				var validImages = attach.Concat(embeds);
				if (validImages.Count() == 0)
				{
					await Context.Guild.ModifyAsync(x => x.Icon = new Image());
					await Messages.MakeAndDeleteSecondaryMessage(Context, "Successfully removed the guild's icon.");
					return;
				}
				else if (validImages.Count() > 1)
				{
					await Messages.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("Too many attached or embedded images."));
					return;
				}

				var imageURL = validImages.First();
				var fileType = await Uploads.GetFileTypeOrSayErrors(Context, imageURL);
				if (fileType == null)
					return;

				var path = Gets.GetServerFilePath(Context.Guild.Id, Constants.GUILD_ICON_LOCATION + fileType);
				using (var webClient = new System.Net.WebClient())
				{
					webClient.DownloadFileAsync(new Uri(imageURL), path);
					webClient.DownloadFileCompleted += (sender, e) => Uploads.SetIcon(sender, e, Context.Guild.ModifyAsync(x => x.Icon = new Image(path)), Context, path);
				}
			}
		}

		[Group("createguild"), Alias("cg")]
		[Usage("[Name]")]
		[Summary("Creates a guild with the bot as the owner.")]
		[OtherRequirement(Precondition.BotOwner)]
		[DefaultEnabled(true)]
		public class CreateGuild : MyModuleBase
		{
			[Command]
			public async Task Command([Remainder, VerifyStringLength(Target.Guild)] string name)
			{
				await CommandRunner(name);
			}

			private async Task CommandRunner(string name)
			{
				var optimalVoiceRegion = await Context.Client.GetOptimalVoiceRegionAsync();
				var guild = await Context.Client.CreateGuildAsync(name, optimalVoiceRegion);
				await SavingAndLoading.CreateOrGetGuildInfo(guild);

				var defaultChannel = await guild.GetDefaultChannelAsync();
				var invite = await defaultChannel.CreateInviteAsync();
				var DMChannel = await Context.User.GetOrCreateDMChannelAsync();
				await Messages.SendDMMessage(DMChannel, invite.Url);
			}
		}

		[Group("changeguildowner"), Alias("cgo")]
		[Usage("")]
		[Summary("If the bot is the current owner of the guild, this command will give you owner.")]
		[OtherRequirement(Precondition.BotOwner)]
		[DefaultEnabled(true)]
		public class ChangeGuildOwner : MyModuleBase
		{
			[Command]
			public async Task Command()
			{
				await CommandRunner();
			}

			private async Task CommandRunner()
			{
				if (Context.Client.CurrentUser.Id == Context.Guild.OwnerId)
				{
					await Context.Guild.ModifyAsync(x => x.Owner = new Optional<IUser>(Context.User));
					await Messages.MakeAndDeleteSecondaryMessage(Context, String.Format("{0} is now the owner.", Context.User.Mention));
					return;
				}

				await Messages.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("The bot is not the owner of the guild."));
			}
		}

		[Group("deleteguild"), Alias("dg")]
		[Usage("")]
		[Summary("If the bot is the current owner of the guild, this command will delete the guild.")]
		[OtherRequirement(Precondition.BotOwner)]
		[DefaultEnabled(true)]
		public class DeleteGuild : MyModuleBase
		{
			[Command]
			public async Task Command()
			{
				await CommandRunner();
			}

			private async Task CommandRunner()
			{
				if (Context.Client.CurrentUser.Id == Context.Guild.OwnerId)
				{
					await Context.Guild.DeleteAsync();
					return;
				}

				await Messages.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("The bot is not the owner of the guild and thus cannot delete it."));
			}
		}
	}
}
