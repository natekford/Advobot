using Advobot.Actions;
using Advobot.Classes.Attributes;
using Advobot.Classes;
using Advobot.Enums;
using Advobot.Actions.Formatting;
using Discord;
using Discord.Commands;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Advobot.Commands.GuildModeration
{
	[Group(nameof(LeaveGuild))]
	[Usage("<Guild ID>")]
	[Summary("Makes the bot leave the guild. Settings and preferences will be preserved.")]
	[OtherRequirement(Precondition.GuildOwner | Precondition.BotOwner)]
	[DefaultEnabled(true)]
	public sealed class LeaveGuild : MyModuleBase
	{
		[Command]
		public async Task Command([Optional] ulong guildId)
		{
			if (guildId == 0)
			{
				await Context.Guild.LeaveAsync();
				return;
			}

			//Need bot owner check so only the bot owner can make the bot leave servers they don't own
			if (Context.User.Id == (await UserActions.GetBotOwner(Context.Client)).Id)
			{
				var guild = await Context.Client.GetGuildAsync(guildId);
				if (guild == null)
				{
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, GeneralFormatting.ERROR("Invalid server supplied."));
					return;
				}

				await guild.LeaveAsync();
				if (Context.Guild.Id != guildId)
				{
					await MessageActions.SendChannelMessage(Context.Channel, $"Successfully left the server `{guild.Name}` with an ID `{guild.Id}`.");
				}
			}
			else if (Context.Guild.Id == guildId)
			{
				await Context.Guild.LeaveAsync();
			}
			else
			{
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, GeneralFormatting.ERROR("Only the bot owner can use this command targetting other guilds."));
			}
		}
	}

	[Group(nameof(ModifyGuildName)), Alias("mgn")]
	[Usage("[Name]")]
	[Summary("Change the name of the guild to the given name.")]
	[PermissionRequirement(new[] { GuildPermission.ManageGuild }, null)]
	[DefaultEnabled(true)]
	public sealed class ModifyGuildName : MyModuleBase
	{
		[Command]
		public async Task Command([Remainder, VerifyStringLength(Target.Guild)] string name)
		{
			await GuildActions.ModifyGuildName(Context.Guild, name, GeneralFormatting.FormatUserReason(Context.User));
			await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully changed the guild name to `{name}`.");
		}
	}

	[Group(nameof(ModifyGuildRegion)), Alias("mgr")]
	[Usage("<Current|Region ID>")]
	[Summary("Shows or changes the guild's server region. Inputting nothing lists all valid region IDs.")]
	[PermissionRequirement(new[] { GuildPermission.ManageGuild }, null)]
	[DefaultEnabled(true)]
	public sealed class ModifyGuildRegion : MyModuleBase
	{
		private static readonly string[] _ValidRegionIDs =
		{
			"brazil",
			"eu-central",
			"eu-west",
			"hongkong",
			"russia",
			"singapore",
			"sydney",
			"us-east",
			"us-central",
			"us-south",
			"us-west",
		};
		private static readonly string[] _VIPRegionIDs =
		{
			"vip-amsterdam",
			"vip-us-east",
			"vip-us-west",
		};

		private static readonly string _BaseRegions = String.Join("\n", _BaseRegions);
		private static readonly string _VIPRegions = String.Join("\n", _VIPRegionIDs);
		private static readonly string _AllRegions = _BaseRegions + "\n" + _VIPRegions;

		[Command(nameof(ActionType.Current)), Priority(1)]
		public async Task CommandCurrent()
		{
			await MessageActions.SendChannelMessage(Context.Channel, $"The guild's current server region is `{Context.Guild.VoiceRegionId}`.");
		}
		[Command, Priority(0)]
		public async Task Command(string region)
		{
			if (!(_ValidRegionIDs.CaseInsContains(region) || (Context.Guild.Features.CaseInsContains(Constants.VIP_REGIONS) && _VIPRegionIDs.CaseInsContains(region))))
			{
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, GeneralFormatting.ERROR("No valid region ID was input."));
				return;
			}

			var beforeRegion = Context.Guild.VoiceRegionId;
			await GuildActions.ModifyGuildRegion(Context.Guild, region, GeneralFormatting.FormatUserReason(Context.User));
			await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully changed the server region of the guild from `{beforeRegion}` to `{region}`.");
		}
		[Command]
		public async Task Command()
		{
			var desc = Context.Guild.Features.CaseInsContains(Constants.VIP_REGIONS) ? _AllRegions : _BaseRegions;
			await MessageActions.SendEmbedMessage(Context.Channel, EmbedActions.MakeNewEmbed("Region IDs", desc));
		}
	}

	[Group(nameof(ModifyGuildAFKTimer)), Alias("mgafkt")]
	[Usage("[Number]")]
	[Summary("Updates the guild's AFK timeout.")]
	[PermissionRequirement(new[] { GuildPermission.ManageGuild }, null)]
	[DefaultEnabled(true)]
	public sealed class ModifyGuildAFKTimer : MyModuleBase
	{
		private static readonly uint[] _AFKTimes = { 60, 300, 900, 1800, 3600 };

		[Command]
		public async Task Command(uint time)
		{
			if (!_AFKTimes.Contains(time))
			{
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, GeneralFormatting.ERROR($"Invalid time input, must be one of the following: `{String.Join("`, `", _AFKTimes)}`."));
				return;
			}

			await GuildActions.ModifyGuildAFKTime(Context.Guild, (int)time, GeneralFormatting.FormatUserReason(Context.User));
			await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully set the guild's AFK timeout to `{time}`.");
		}
	}

	[Group(nameof(ModifyGuildAFKChannel)), Alias("mgafkc")]
	[Usage("[Channel]")]
	[Summary("Updates the guild's AFK channel.")]
	[PermissionRequirement(new[] { GuildPermission.ManageGuild }, null)]
	[DefaultEnabled(true)]
	public sealed class ModifyGuildAFKChannel : MyModuleBase
	{
		[Command]
		public async Task Command(IVoiceChannel channel)
		{
			await GuildActions.ModifyGuildAFKChannel(Context.Guild, channel, GeneralFormatting.FormatUserReason(Context.User));
			await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully set the guild's AFK channel to `{channel.FormatChannel()}`.");
		}
	}

	[Group(nameof(ModifyGuildMsgNotif)), Alias("mgmn")]
	[Usage("[AllMessages|MentionsOnly]")]
	[Summary("Changes the message notifications to either all messages or mentions only.")]
	[PermissionRequirement(new[] { GuildPermission.ManageGuild }, null)]
	[DefaultEnabled(true)]
	public sealed class ModifyGuildMsgNotif : MyModuleBase
	{
		[Command]
		public async Task Command(DefaultMessageNotifications msgNotifs)
		{
			await GuildActions.ModifyGuildDefaultMsgNotifications(Context.Guild, msgNotifs, GeneralFormatting.FormatUserReason(Context.User));
			await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully changed the default message notification setting to `{msgNotifs.EnumName()}`.");
		}
	}

	[Group(nameof(ModifyGuildVerif)), Alias("mgv")]
	[Usage("[None|Low|Medium|High|Extreme]")]
	[Summary("Changes the verification level. None is the most lenient (no requirements to type), high is the harshest (10 minutes in the guild before new members can type).")]
	[PermissionRequirement(new[] { GuildPermission.ManageGuild }, null)]
	[DefaultEnabled(true)]
	public sealed class ModifyGuildVerif : MyModuleBase
	{
		[Command]
		public async Task Command(VerificationLevel verif)
		{
			await GuildActions.ModifyGuildVerificationLevel(Context.Guild, verif, GeneralFormatting.FormatUserReason(Context.User));
			await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully set the guild verification level as `{verif.EnumName()}`.");
		}
	}

	[Group(nameof(ModifyGuildIcon)), Alias("mgi")]
	[Usage("<Attached Image|Embedded Image>")]
	[Summary("Changes the guild's icon to the given image. The image must be smaller than 2.5MB. Inputting nothing removes the guild's icon.")]
	[PermissionRequirement(new[] { GuildPermission.ManageGuild }, null)]
	[DefaultEnabled(true)]
	public sealed class ModifyGuildIcon : MyModuleBase
	{
		[Command(RunMode = RunMode.Async)]
		public async Task Command([Optional] string input)
		{
			var attach = Context.Message.Attachments.Where(x => x.Width != null && x.Height != null).Select(x => x.Url);
			var embeds = Context.Message.Embeds.Where(x => x.Image.HasValue).Select(x => x.Image?.Url);
			var validImages = attach.Concat(embeds);
			if (!validImages.Any())
			{
				await Context.Guild.ModifyAsync(x => x.Icon = new Image());
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, "Successfully removed the guild's icon.");
				return;
			}
			else if (validImages.Count() > 1)
			{
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, GeneralFormatting.ERROR("Too many attached or embedded images."));
				return;
			}

			var imageUrl = validImages.First();
			if (!GetActions.TryGetFileType(Context, imageUrl, out string fileType, out string errorReason))
			{
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, GeneralFormatting.ERROR(errorReason));
				return;
			}

			var fileInfo = GetActions.GetServerDirectoryFile(Context.Guild.Id, Constants.GUILD_ICON_LOCATION + fileType);
			using (var webClient = new System.Net.WebClient())
			{
				webClient.DownloadFileAsync(new Uri(imageUrl), fileInfo.FullName);
				webClient.DownloadFileCompleted += async (sender, e) =>
				{
					await GuildActions.ModifyGuildIcon(Context.Guild, fileInfo, GeneralFormatting.FormatUserReason(Context.User));
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, "Successfully changed the guild's icon.");
					SavingAndLoadingActions.DeleteFile(fileInfo);
				};
			}
		}
	}

	[Group(nameof(CreateGuild)), Alias("cg")]
	[Usage("[Name]")]
	[Summary("Creates a guild with the bot as the owner.")]
	[OtherRequirement(Precondition.BotOwner)]
	[DefaultEnabled(true)]
	public sealed class CreateGuild : MyModuleBase
	{
		[Command]
		public async Task Command([Remainder, VerifyStringLength(Target.Guild)] string name)
		{
			var optimalVoiceRegion = await Context.Client.GetOptimalVoiceRegionAsync();
			var guild = await Context.Client.CreateGuildAsync(name, optimalVoiceRegion);

			var defaultChannel = await guild.GetDefaultChannelAsync();
			var invite = await defaultChannel.CreateInviteAsync();
			var DMChannel = await Context.User.GetOrCreateDMChannelAsync();
			await MessageActions.SendChannelMessage(DMChannel, invite.Url);
		}
	}

	[Group(nameof(SwapGuildOwner)), Alias("sgo")]
	[Usage("")]
	[Summary("If the bot is the current owner of the guild, this command will give you owner.")]
	[OtherRequirement(Precondition.BotOwner)]
	[DefaultEnabled(true)]
	public sealed class SwapGuildOwner : MyModuleBase
	{
		[Command]
		public async Task Command()
		{
			if (Context.Client.CurrentUser.Id == Context.Guild.OwnerId)
			{
				await Context.Guild.ModifyAsync(x => x.Owner = new Optional<IUser>(Context.User));
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"{Context.User.Mention} is now the owner.");
				return;
			}

			await MessageActions.MakeAndDeleteSecondaryMessage(Context, GeneralFormatting.ERROR("The bot is not the owner of the guild."));
		}
	}

	[Group(nameof(DeleteGuild)), Alias("dg")]
	[Usage("")]
	[Summary("If the bot is the current owner of the guild, this command will delete the guild.")]
	[OtherRequirement(Precondition.BotOwner)]
	[DefaultEnabled(true)]
	public sealed class DeleteGuild : MyModuleBase
	{
		[Command]
		public async Task Command()
		{
			if (Context.Client.CurrentUser.Id == Context.Guild.OwnerId)
			{
				await Context.Guild.DeleteAsync();
				return;
			}

			await MessageActions.MakeAndDeleteSecondaryMessage(Context, GeneralFormatting.ERROR("The bot is not the owner of the guild and thus cannot delete it."));
		}
	}
}
