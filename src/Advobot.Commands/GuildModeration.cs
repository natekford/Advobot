using Advobot.Core;
using Advobot.Core.Utilities;
using Advobot.Core.Utilities.Formatting;
using Advobot.Core.Classes;
using Advobot.Core.Classes.Attributes;
using Advobot.Core.Enums;
using Discord;
using Discord.Commands;
using System;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Advobot.Commands.GuildModeration
{
	[Group(nameof(LeaveGuild)), TopLevelShortAlias(typeof(LeaveGuild))]
	[Summary("Makes the bot leave the guild. " +
		"Settings and preferences will be preserved.")]
	[OtherRequirement(Precondition.GuildOwner | Precondition.BotOwner)]
	[DefaultEnabled(true)]
	public sealed class LeaveGuild : AdvobotModuleBase
	{
		[Command]
		public async Task Command([Optional] ulong guildId)
		{
			if (Context.Guild.Id == guildId || guildId == 0)
			{
				await Context.Guild.LeaveAsync().CAF();
			}
			//Need bot owner check so only the bot owner can make the bot leave servers they don't own
			else if (Context.User.Id == (await ClientUtils.GetBotOwnerAsync(Context.Client).CAF()).Id)
			{
				var guild = await Context.Client.GetGuildAsync(guildId).CAF();
				if (guild == null)
				{
					await MessageUtils.SendErrorMessageAsync(Context, new Error("Invalid server supplied.")).CAF();
					return;
				}

				await guild.LeaveAsync().CAF();
				if (Context.Guild.Id != guildId)
				{
					var resp = $"Successfully left the server `{guild.Name}` with an ID `{guild.Id}`.";
					await MessageUtils.SendMessageAsync(Context.Channel, resp).CAF();
				}
			}
			else
			{
				var error = new Error("Only the bot owner can use this command targetting other guilds.");
				await MessageUtils.SendErrorMessageAsync(Context, error).CAF();
			}
		}
	}

	[Group(nameof(ModifyGuildName)), TopLevelShortAlias(typeof(ModifyGuildName))]
	[Summary("Change the name of the guild to the given name.")]
	[PermissionRequirement(new[] { GuildPermission.ManageGuild }, null)]
	[DefaultEnabled(true)]
	public sealed class ModifyGuildName : AdvobotModuleBase
	{
		[Command]
		public async Task Command([Remainder, VerifyStringLength(Target.Guild)] string name)
		{
			await GuildUtils.ModifyGuildNameAsync(Context.Guild, name, new ModerationReason(Context.User, null)).CAF();
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, $"Successfully changed the guild name to `{name}`.").CAF();
		}
	}

	[Group(nameof(ModifyGuildRegion)), TopLevelShortAlias(typeof(ModifyGuildRegion))]
	[Summary("Shows or changes the guild's server region.")]
	[PermissionRequirement(new[] { GuildPermission.ManageGuild }, null)]
	[DefaultEnabled(true)]
	public sealed class ModifyGuildRegion : AdvobotModuleBase
	{
		private static string[] _ValidRegionIDs =
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
		private static string[] _VIPRegionIDs =
		{
			"vip-amsterdam",
			"vip-us-east",
			"vip-us-west",
		};

		private static string _BaseRegions = String.Join("\n", _BaseRegions);
		private static string _VIPRegions = String.Join("\n", _VIPRegionIDs);
		private static string _AllRegions = _BaseRegions + "\n" + _VIPRegions;

		[Command(nameof(Show)), ShortAlias(nameof(Show)), Priority(1)]
		public async Task Show()
		{
			var embed = new EmbedWrapper
			{
				Title = "Region Ids",
				Description = Context.Guild.Features.CaseInsContains(Constants.VIP_REGIONS) ? _AllRegions : _BaseRegions,
			};
			await MessageUtils.SendEmbedMessageAsync(Context.Channel, embed).CAF();
		}
		[Command(nameof(Current)), ShortAlias(nameof(Current)), Priority(1)]
		public async Task Current()
		{
			var resp = $"The guild's current server region is `{Context.Guild.VoiceRegionId}`.";
			await MessageUtils.SendMessageAsync(Context.Channel, resp).CAF();
		}
		[Command, Priority(0)]
		public async Task Command(string regionId)
		{
			if (true
				&& !_ValidRegionIDs.CaseInsContains(regionId) 
				&& !(Context.Guild.Features.CaseInsContains(Constants.VIP_REGIONS) && _VIPRegionIDs.CaseInsContains(regionId)))
			{
				await MessageUtils.SendErrorMessageAsync(Context, new Error("No valid region ID was input.")).CAF();
				return;
			}

			var beforeRegion = Context.Guild.VoiceRegionId;
			await GuildUtils.ModifyGuildRegionAsync(Context.Guild, regionId, new ModerationReason(Context.User, null)).CAF();
			var resp = $"Successfully changed the server region of the guild from `{beforeRegion}` to `{regionId}`.";
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}
	}

	[Group(nameof(ModifyGuildAFKTimer)), TopLevelShortAlias(typeof(ModifyGuildAFKTimer))]
	[Summary("Updates the guild's AFK timeout.")]
	[PermissionRequirement(new[] { GuildPermission.ManageGuild }, null)]
	[DefaultEnabled(true)]
	public sealed class ModifyGuildAFKTimer : AdvobotModuleBase
	{
		[Command]
		public async Task Command([VerifyNumber(new[] { 60, 300, 900, 1800, 3600 })] uint time)
		{
			await GuildUtils.ModifyGuildAFKTimeAsync(Context.Guild, (int)time, new ModerationReason(Context.User, null)).CAF();
			var resp = $"Successfully set the guild's AFK timeout to `{time}`.";
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}
	}

	[Group(nameof(ModifyGuildAFKChannel)), TopLevelShortAlias(typeof(ModifyGuildAFKChannel))]
	[Summary("Updates the guild's AFK channel.")]
	[PermissionRequirement(new[] { GuildPermission.ManageGuild }, null)]
	[DefaultEnabled(true)]
	public sealed class ModifyGuildAFKChannel : AdvobotModuleBase
	{
		[Command]
		public async Task Command(IVoiceChannel channel)
		{
			await GuildUtils.ModifyGuildAFKChannelAsync(Context.Guild, channel, new ModerationReason(Context.User, null)).CAF();
			var resp = $"Successfully set the guild's AFK channel to `{channel.Format()}`.";
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}
	}

	[Group(nameof(ModifyGuildMsgNotif)), TopLevelShortAlias(typeof(ModifyGuildMsgNotif))]
	[Summary("Changes the message notifications to either all messages or mentions only.")]
	[PermissionRequirement(new[] { GuildPermission.ManageGuild }, null)]
	[DefaultEnabled(true)]
	public sealed class ModifyGuildMsgNotif : AdvobotModuleBase
	{
		[Command]
		public async Task Command(DefaultMessageNotifications msgNotifs)
		{
			await GuildUtils.ModifyGuildDefaultMsgNotificationsAsync(Context.Guild, msgNotifs, new ModerationReason(Context.User, null)).CAF();
			var resp = $"Successfully changed the default message notification setting to `{msgNotifs.EnumName()}`.";
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}
	}

	[Group(nameof(ModifyGuildVerif)), TopLevelShortAlias(typeof(ModifyGuildVerif))]
	[Summary("Changes the verification level. " +
		"None is the most lenient (no requirements to type), extreme is the harshest (phone verification).")]
	[PermissionRequirement(new[] { GuildPermission.ManageGuild }, null)]
	[DefaultEnabled(true)]
	public sealed class ModifyGuildVerif : AdvobotModuleBase
	{
		[Command]
		public async Task Command(VerificationLevel verif)
		{
			await GuildUtils.ModifyGuildVerificationLevelAsync(Context.Guild, verif, new ModerationReason(Context.User, null)).CAF();
			var resp = $"Successfully set the guild verification level as `{verif.EnumName()}`.";
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}
	}

	[Group(nameof(ModifyGuildIcon)), TopLevelShortAlias(typeof(ModifyGuildIcon))]
	[Summary("Changes the guild's icon to the given image. " +
		"The image must be smaller than 2.5MB. " +
		"Inputting nothing removes the guild's icon.")]
	[PermissionRequirement(new[] { GuildPermission.ManageGuild }, null)]
	[DefaultEnabled(true)]
	public sealed class ModifyGuildIcon : AdvobotModuleBase
	{
		[Command(RunMode = RunMode.Async)]
		public async Task Command([Optional, Remainder] string url)
		{
			var success = MessageUtils.GetImageUrl(Context, url, out var imageUrl, out var error);
			if (!success)
			{
				await MessageUtils.SendErrorMessageAsync(Context, error).CAF();
				return;
			}
			else if (imageUrl == null)
			{
				await Context.Guild.ModifyAsync(x => x.Icon = new Image()).CAF();
				await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, "Successfully removed the guild's icon.").CAF();
				return;
			}

			var fileInfo = IOUtils.GetServerDirectoryFile(Context.Guild.Id, Constants.GUILD_ICON_LOC);
			using (var webClient = new WebClient())
			{
				webClient.DownloadFileAsync(imageUrl, fileInfo.FullName);
				webClient.DownloadFileCompleted += async (sender, e) =>
				{
					await GuildUtils.ModifyGuildIconAsync(Context.Guild, fileInfo, new ModerationReason(Context.User, null)).CAF();
					await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, "Successfully changed the guild's icon.").CAF();
					IOUtils.DeleteFile(fileInfo);
				};
			}
		}
	}

	[Group(nameof(CreateGuild)), TopLevelShortAlias(typeof(CreateGuild))]
	[Summary("Creates a guild with the bot as the owner.")]
	[OtherRequirement(Precondition.BotOwner)]
	[DefaultEnabled(true)]
	public sealed class CreateGuild : AdvobotModuleBase
	{
		[Command]
		public async Task Command([Remainder, VerifyStringLength(Target.Guild)] string name)
		{
			var optimalVoiceRegion = await Context.Client.GetOptimalVoiceRegionAsync().CAF();
			var guild = await Context.Client.CreateGuildAsync(name, optimalVoiceRegion).CAF();
			var defaultChannel = await guild.GetDefaultChannelAsync().CAF();
			var invite = await defaultChannel.CreateInviteAsync().CAF();
			await Context.User.SendMessageAsync(invite.Url).CAF();
		}
	}

	[Group(nameof(SwapGuildOwner)), TopLevelShortAlias(typeof(SwapGuildOwner))]
	[Summary("If the bot is the current owner of the guild, this command will give you owner.")]
	[OtherRequirement(Precondition.BotOwner)]
	[DefaultEnabled(true)]
	public sealed class SwapGuildOwner : AdvobotModuleBase
	{
		[Command]
		public async Task Command()
		{
			if (Context.Client.CurrentUser.Id == Context.Guild.OwnerId)
			{
				await Context.Guild.ModifyAsync(x => x.Owner = new Optional<IUser>(Context.User)).CAF();
				await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, $"{Context.User.Mention} is now the owner.").CAF();
				return;
			}

			await MessageUtils.SendErrorMessageAsync(Context, new Error("The bot is not the owner of the guild.")).CAF();
		}
	}

	[Group(nameof(DeleteGuild)), TopLevelShortAlias(typeof(DeleteGuild))]
	[Summary("If the bot is the current owner of the guild, this command will delete the guild.")]
	[OtherRequirement(Precondition.BotOwner)]
	[DefaultEnabled(true)]
	public sealed class DeleteGuild : AdvobotModuleBase
	{
		[Command]
		public async Task Command()
		{
			if (Context.Client.CurrentUser.Id == Context.Guild.OwnerId)
			{
				await Context.Guild.DeleteAsync().CAF();
				return;
			}

			await MessageUtils.SendErrorMessageAsync(Context, new Error("The bot is not the owner of the guild.")).CAF();
		}
	}
}
