using Advobot.Actions;
using Advobot.Actions.Formatting;
using Advobot.Classes;
using Advobot.Classes.Attributes;
using Advobot.Enums;
using Discord;
using Discord.Commands;
using System;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Advobot.Commands.GuildModeration
{
	[Group(nameof(LeaveGuild)), TopLevelShortAlias(typeof(LeaveGuild))]
	[Summary("Makes the bot leave the guild. Settings and preferences will be preserved.")]
	[OtherRequirement(Precondition.GuildOwner | Precondition.BotOwner)]
	[DefaultEnabled(true)]
	public sealed class LeaveGuild : AdvobotModuleBase
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
					await MessageActions.SendErrorMessage(Context, new ErrorReason("Invalid server supplied."));
					return;
				}

				await guild.LeaveAsync();
				if (Context.Guild.Id != guildId)
				{
					await MessageActions.SendMessage(Context.Channel, $"Successfully left the server `{guild.Name}` with an ID `{guild.Id}`.");
				}
			}
			else if (Context.Guild.Id == guildId)
			{
				await Context.Guild.LeaveAsync();
			}
			else
			{
				await MessageActions.SendErrorMessage(Context, new ErrorReason("Only the bot owner can use this command targetting other guilds."));
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
			await GuildActions.ModifyGuildName(Context.Guild, name, new ModerationReason(Context.User, null));
			await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully changed the guild name to `{name}`.");
		}
	}

	[Group(nameof(ModifyGuildRegion)), TopLevelShortAlias(typeof(ModifyGuildRegion))]
	[Summary("Shows or changes the guild's server region. Inputting nothing lists all valid region IDs.")]
	[PermissionRequirement(new[] { GuildPermission.ManageGuild }, null)]
	[DefaultEnabled(true)]
	public sealed class ModifyGuildRegion : AdvobotModuleBase
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

		[Command(nameof(Show)), ShortAlias(nameof(Show)), Priority(1)]
		public async Task Show()
		{
			var desc = Context.Guild.Features.CaseInsContains(Constants.VIP_REGIONS) ? _AllRegions : _BaseRegions;
			await MessageActions.SendEmbedMessage(Context.Channel, new MyEmbed("Region IDs", desc));
		}
		[Command(nameof(Current)), ShortAlias(nameof(Current)), Priority(1)]
		public async Task Current()
		{
			await MessageActions.SendMessage(Context.Channel, $"The guild's current server region is `{Context.Guild.VoiceRegionId}`.");
		}
		[Command, Priority(0)]
		public async Task Command(string regionId)
		{
			if (!(_ValidRegionIDs.CaseInsContains(regionId) || (Context.Guild.Features.CaseInsContains(Constants.VIP_REGIONS) && _VIPRegionIDs.CaseInsContains(regionId))))
			{
				await MessageActions.SendErrorMessage(Context, new ErrorReason("No valid region ID was input."));
				return;
			}

			var beforeRegion = Context.Guild.VoiceRegionId;
			await GuildActions.ModifyGuildRegion(Context.Guild, regionId, new ModerationReason(Context.User, null));
			await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully changed the server region of the guild from `{beforeRegion}` to `{regionId}`.");
		}
	}

	[Group(nameof(ModifyGuildAFKTimer)), TopLevelShortAlias(typeof(ModifyGuildAFKTimer))]
	[Summary("Updates the guild's AFK timeout.")]
	[PermissionRequirement(new[] { GuildPermission.ManageGuild }, null)]
	[DefaultEnabled(true)]
	public sealed class ModifyGuildAFKTimer : AdvobotModuleBase
	{
		[Command]
		public async Task Command([VerifyNumber(60, 300, 900, 1800, 3600)] uint time)
		{
			await GuildActions.ModifyGuildAFKTime(Context.Guild, (int)time, new ModerationReason(Context.User, null));
			await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully set the guild's AFK timeout to `{time}`.");
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
			await GuildActions.ModifyGuildAFKChannel(Context.Guild, channel, new ModerationReason(Context.User, null));
			await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully set the guild's AFK channel to `{channel.FormatChannel()}`.");
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
			await GuildActions.ModifyGuildDefaultMsgNotifications(Context.Guild, msgNotifs, new ModerationReason(Context.User, null));
			await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully changed the default message notification setting to `{msgNotifs.EnumName()}`.");
		}
	}

	[Group(nameof(ModifyGuildVerif)), TopLevelShortAlias(typeof(ModifyGuildVerif))]
	[Summary("Changes the verification level. None is the most lenient (no requirements to type), extreme is the harshest (phone verification).")]
	[PermissionRequirement(new[] { GuildPermission.ManageGuild }, null)]
	[DefaultEnabled(true)]
	public sealed class ModifyGuildVerif : AdvobotModuleBase
	{
		[Command]
		public async Task Command(VerificationLevel verif)
		{
			await GuildActions.ModifyGuildVerificationLevel(Context.Guild, verif, new ModerationReason(Context.User, null));
			await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully set the guild verification level as `{verif.EnumName()}`.");
		}
	}

	[Group(nameof(ModifyGuildIcon)), TopLevelShortAlias(typeof(ModifyGuildIcon))]
	[Summary("Changes the guild's icon to the given image. The image must be smaller than 2.5MB. Inputting nothing removes the guild's icon.")]
	[PermissionRequirement(new[] { GuildPermission.ManageGuild }, null)]
	[DefaultEnabled(true)]
	public sealed class ModifyGuildIcon : AdvobotModuleBase
	{
		[Command(RunMode = RunMode.Async)]
		public async Task Command([Optional] ImageUrl imageUrl)
		{
			if (imageUrl?.Url == null)
			{
				await Context.Guild.ModifyAsync(x => x.Icon = new Image());
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, "Successfully removed the guild's icon.");
				return;
			}

			var fileInfo = GetActions.GetServerDirectoryFile(Context.Guild.Id, Constants.GUILD_ICON_LOCATION + imageUrl.FileType);
			using (var webClient = new WebClient())
			{
				webClient.DownloadFileAsync(imageUrl.Url, fileInfo.FullName);
				webClient.DownloadFileCompleted += async (sender, e) =>
				{
					await GuildActions.ModifyGuildIcon(Context.Guild, fileInfo, new ModerationReason(Context.User, null));
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, "Successfully changed the guild's icon.");
					SavingAndLoadingActions.DeleteFile(fileInfo);
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
			var optimalVoiceRegion = await Context.Client.GetOptimalVoiceRegionAsync();
			var guild = await Context.Client.CreateGuildAsync(name, optimalVoiceRegion);

			var defaultChannel = await guild.GetDefaultChannelAsync();
			var invite = await defaultChannel.CreateInviteAsync();
			await Context.User.SendMessageAsync(invite.Url);
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
				await Context.Guild.ModifyAsync(x => x.Owner = new Optional<IUser>(Context.User));
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"{Context.User.Mention} is now the owner.");
				return;
			}

			await MessageActions.SendErrorMessage(Context, new ErrorReason("The bot is not the owner of the guild."));
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
				await Context.Guild.DeleteAsync();
				return;
			}

			await MessageActions.SendErrorMessage(Context, new ErrorReason("The bot is not the owner of the guild."));
		}
	}
}
