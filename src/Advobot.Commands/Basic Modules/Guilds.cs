using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Advobot.Classes;
using Advobot.Classes.Attributes;
using Advobot.Classes.ImageResizing;
using Advobot.Enums;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Advobot.Commands.Guilds
{
	[Category(typeof(LeaveGuild)), Group(nameof(LeaveGuild)), TopLevelShortAlias(typeof(LeaveGuild))]
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
				return;
			}
			//Need bot owner check so only the bot owner can make the bot leave servers they don't own
			if (Context.User.Id != await ClientUtils.GetOwnerIdAsync(Context.Client).CAF())
			{
				var error = new Error("Only the bot owner can use this command targetting other guilds.");
				await MessageUtils.SendErrorMessageAsync(Context, error).CAF();
				return;
			}
			if (!(Context.Client.GetGuild(guildId) is SocketGuild guild))
			{
				await MessageUtils.SendErrorMessageAsync(Context, new Error("Invalid guild supplied.")).CAF();
				return;
			}
			await guild.LeaveAsync().CAF();
			if (Context.Guild.Id != guildId)
			{
				await MessageUtils.SendMessageAsync(Context.Channel, $"Successfully left the guild `{guild.Format()}`.").CAF();
			}
		}
	}

	[Category(typeof(ModifyGuildName)), Group(nameof(ModifyGuildName)), TopLevelShortAlias(typeof(ModifyGuildName))]
	[Summary("Change the name of the guild to the given name.")]
	[PermissionRequirement(new[] { GuildPermission.ManageGuild }, null)]
	[DefaultEnabled(true)]
	public sealed class ModifyGuildName : AdvobotModuleBase
	{
		[Command]
		public async Task Command([Remainder, ValidateString(Target.Guild)] string name)
		{
			await Context.Guild.ModifyAsync(x => x.Name = name, GetRequestOptions()).CAF();
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, $"Successfully changed the guild name to `{name}`.").CAF();
		}
	}

	[Category(typeof(ModifyGuildRegion)), Group(nameof(ModifyGuildRegion)), TopLevelShortAlias(typeof(ModifyGuildRegion))]
	[Summary("Changes the guild server region.")]
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
			"japan",
			"russia",
			"singapore",
			"sydney",
			"us-east",
			"us-central",
			"us-south",
			"us-west"
		};
		private static string[] _VIPRegionIDs =
		{
			"vip-amsterdam",
			"vip-us-east",
			"vip-us-west"
		};

		private static readonly string _BaseRegions = string.Join("\n", _ValidRegionIDs);
		private static readonly string _VIPRegions = string.Join("\n", _VIPRegionIDs);
		private static readonly string _AllRegions = _BaseRegions + "\n" + _VIPRegions;

		//TODO: use bot voice regions field
		[Command(nameof(Show)), ShortAlias(nameof(Show)), Priority(1)]
		public async Task Show()
		{
			var embed = new EmbedWrapper
			{
				Title = "Region Ids",
				Description = Context.Guild.Features.CaseInsContains(Constants.VIP_REGIONS) ? _AllRegions : _BaseRegions
			};
			await MessageUtils.SendMessageAsync(Context.Channel, null, embed).CAF();
		}
		[Command]
		public async Task Command(string regionId)
		{
			if (!_ValidRegionIDs.CaseInsContains(regionId)
				&& !(Context.Guild.Features.CaseInsContains(Constants.VIP_REGIONS) && _VIPRegionIDs.CaseInsContains(regionId)))
			{
				await MessageUtils.SendErrorMessageAsync(Context, new Error("No valid region ID was input.")).CAF();
				return;
			}

			var beforeRegion = Context.Guild.VoiceRegionId;
			await Context.Guild.ModifyAsync(x => x.RegionId = regionId, GetRequestOptions()).CAF();
			var resp = $"Successfully changed the server region of the guild from `{beforeRegion}` to `{regionId}`.";
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}
	}

	[Category(typeof(ModifyGuildAfkTimer)), Group(nameof(ModifyGuildAfkTimer)), TopLevelShortAlias(typeof(ModifyGuildAfkTimer))]
	[Summary("Changes the guild AFK timeout.")]
	[PermissionRequirement(new[] { GuildPermission.ManageGuild }, null)]
	[DefaultEnabled(true)]
	public sealed class ModifyGuildAfkTimer : AdvobotModuleBase
	{
		[Command]
		public async Task Command([ValidateNumber(new[] { 60, 300, 900, 1800, 3600 })] uint time)
		{
			await Context.Guild.ModifyAsync(x => x.AfkTimeout = (int)time, GetRequestOptions()).CAF();
			var resp = $"Successfully set the guild AFK timeout to `{time}`.";
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}
	}

	[Category(typeof(ModifyGuildAfkChannel)), Group(nameof(ModifyGuildAfkChannel)), TopLevelShortAlias(typeof(ModifyGuildAfkChannel))]
	[Summary("Changes the guild afk channel.")]
	[PermissionRequirement(new[] { GuildPermission.ManageGuild }, null)]
	[DefaultEnabled(true)]
	public sealed class ModifyGuildAfkChannel : AdvobotModuleBase
	{
		[Command]
		public async Task Command([ValidateObject(true, Verif.CanBeViewed, Verif.CanBeManaged)] SocketVoiceChannel channel)
		{
			await Context.Guild.ModifyAsync(x => x.AfkChannel = Optional.Create<IVoiceChannel>(channel), GetRequestOptions()).CAF();
			var resp = $"Successfully set the guild AFK channel to `{channel.Format()}`.";
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}
		[Command(nameof(Remove)), ShortAlias(nameof(Remove))]
		public async Task Remove()
		{
			await Context.Guild.ModifyAsync(x => x.AfkChannelId = Optional.Create<ulong?>(null), GetRequestOptions()).CAF();
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, "Successfully removed the guild afk channel.").CAF();
		}
	}

	[Category(typeof(ModifyGuildSystemChannel)), Group(nameof(ModifyGuildSystemChannel)), TopLevelShortAlias(typeof(ModifyGuildSystemChannel))]
	[Summary("Changes the guild system channel.")]
	[PermissionRequirement(new[] { GuildPermission.ManageGuild }, null)]
	[DefaultEnabled(true)]
	public sealed class ModifyGuildSystemChannel : AdvobotModuleBase
	{
		[Command]
		public async Task Command([ValidateObject(true, Verif.CanBeViewed, Verif.CanBeManaged)] SocketTextChannel channel)
		{
			await Context.Guild.ModifyAsync(x => x.SystemChannel = Optional.Create<ITextChannel>(channel), GetRequestOptions()).CAF();
			var resp = $"Successfully set the guild system channel to `{channel.Format()}`.";
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}
		[Command(nameof(Remove)), ShortAlias(nameof(Remove))]
		public async Task Remove()
		{
			await Context.Guild.ModifyAsync(x => x.SystemChannelId = Optional.Create<ulong?>(null), GetRequestOptions()).CAF();
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, "Successfully removed the guild system channel.").CAF();
		}
	}

	[Category(typeof(ModifyGuildMsgNotif)), Group(nameof(ModifyGuildMsgNotif)), TopLevelShortAlias(typeof(ModifyGuildMsgNotif))]
	[Summary("Changes the message notifications to either all messages or mentions only.")]
	[PermissionRequirement(new[] { GuildPermission.ManageGuild }, null)]
	[DefaultEnabled(true)]
	public sealed class ModifyGuildMsgNotif : AdvobotModuleBase
	{
		[Command]
		public async Task Command(DefaultMessageNotifications msgNotifs)
		{
			await Context.Guild.ModifyAsync(x => x.DefaultMessageNotifications = msgNotifs, GetRequestOptions()).CAF();
			var resp = $"Successfully changed the default message notification setting to `{msgNotifs}`.";
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}
	}

	[Category(typeof(ModifyGuildVerif)), Group(nameof(ModifyGuildVerif)), TopLevelShortAlias(typeof(ModifyGuildVerif))]
	[Summary("Changes the verification level. " +
		"None is the most lenient (no requirements to type), extreme is the harshest (phone verification).")]
	[PermissionRequirement(new[] { GuildPermission.ManageGuild }, null)]
	[DefaultEnabled(true)]
	public sealed class ModifyGuildVerif : AdvobotModuleBase
	{
		[Command]
		public async Task Command(VerificationLevel verif)
		{
			await Context.Guild.ModifyAsync(x => x.VerificationLevel = verif, GetRequestOptions()).CAF();
			var resp = $"Successfully set the guild verification level as `{verif}`.";
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}
	}

	[Category(typeof(ModifyGuildIcon)), Group(nameof(ModifyGuildIcon)), TopLevelShortAlias(typeof(ModifyGuildIcon))]
	[Summary("Changes the guild's icon to the given image.")]
	[PermissionRequirement(new[] { GuildPermission.ManageGuild }, null)]
	[DefaultEnabled(true)]
	public sealed class ModifyGuildIcon : AdvobotModuleBase
	{
		private static GuildIconResizer _Resizer = new GuildIconResizer(4);

		[Command]
		public async Task Command(Uri url)
		{
			if (_Resizer.IsGuildAlreadyProcessing(Context.Guild))
			{
				await MessageUtils.SendErrorMessageAsync(Context, new Error("Currently already working on the guild icon.")).CAF();
				return;
			}

			_Resizer.EnqueueArguments(Context, new IconResizerArguments(), url, GetRequestOptions());
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, $"Position in guild icon creation queue: {_Resizer.QueueCount}.").CAF();
			if (_Resizer.CanStart)
			{
				_Resizer.StartProcessing();
			}
		}
		[Command(nameof(Remove)), ShortAlias(nameof(Remove))]
		public async Task Remove()
		{
			if (_Resizer.IsGuildAlreadyProcessing(Context.Guild))
			{
				await MessageUtils.SendErrorMessageAsync(Context, new Error("Currently already working on the guild icon.")).CAF();
				return;
			}

			await Context.Guild.ModifyAsync(x => x.Icon = new Image(), GetRequestOptions()).CAF();
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, "Successfully removed the guild icon.").CAF();
		}
	}

	[Category(typeof(ModifyGuildSplash)), Group(nameof(ModifyGuildSplash)), TopLevelShortAlias(typeof(ModifyGuildSplash))]
	[Summary("Changes the guild splash to the given image. Won't be modified unless the server is a partnered server.")]
	[PermissionRequirement(new[] { GuildPermission.ManageGuild }, null)]
	[DefaultEnabled(true)]
	public sealed class ModifyGuildSplash : AdvobotModuleBase
	{
		private static GuildSplashResizer _Resizer = new GuildSplashResizer(4);

		[Command]
		public async Task Command(Uri url)
		{
			if (!Context.Guild.Features.CaseInsContains(Constants.INVITE_SPLASH))
			{
				await MessageUtils.SendErrorMessageAsync(Context, new Error("The guild needs to be partnered before a splash can be set."));
				return;
			}
			if (_Resizer.IsGuildAlreadyProcessing(Context.Guild))
			{
				await MessageUtils.SendErrorMessageAsync(Context, new Error("Currently already working on the guild splash.")).CAF();
				return;
			}

			_Resizer.EnqueueArguments(Context, new IconResizerArguments(), url, GetRequestOptions());
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, $"Position in guild splash creation queue: {_Resizer.QueueCount}.").CAF();
			if (_Resizer.CanStart)
			{
				_Resizer.StartProcessing();
			}
		}
		[Command(nameof(Remove)), ShortAlias(nameof(Remove))]
		public async Task Remove()
		{
			if (!Context.Guild.Features.CaseInsContains(Constants.INVITE_SPLASH))
			{
				await MessageUtils.SendErrorMessageAsync(Context, new Error("The guild needs to be partnered before a splah can be removed."));
				return;
			}
			if (_Resizer.IsGuildAlreadyProcessing(Context.Guild))
			{
				await MessageUtils.SendErrorMessageAsync(Context, new Error("Currently already working on the guild splash.")).CAF();
				return;
			}

			await Context.Guild.ModifyAsync(x => x.Splash = new Image(), GetRequestOptions()).CAF();
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, "Successfully removed the guild splash.").CAF();
		}
	}

	[Category(typeof(CreateGuild)), Group(nameof(CreateGuild)), TopLevelShortAlias(typeof(CreateGuild))]
	[Summary("Creates a guild with the bot as the owner.")]
	[OtherRequirement(Precondition.BotOwner)]
	[DefaultEnabled(true)]
	public sealed class CreateGuild : AdvobotModuleBase
	{
		[Command]
		public async Task Command([Remainder, ValidateString(Target.Guild)] string name)
		{
			var optimalVoiceRegion = await Context.Client.GetOptimalVoiceRegionAsync().CAF();
			var guild = await Context.Client.CreateGuildAsync(name, optimalVoiceRegion).CAF();
			var defaultChannel = await guild.GetDefaultChannelAsync().CAF();
			var invite = await defaultChannel.CreateInviteAsync().CAF();
			await Context.User.SendMessageAsync(invite.Url).CAF();
		}
	}

	[Category(typeof(SwapGuildOwner)), Group(nameof(SwapGuildOwner)), TopLevelShortAlias(typeof(SwapGuildOwner))]
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

	[Category(typeof(DeleteGuild)), Group(nameof(DeleteGuild)), TopLevelShortAlias(typeof(DeleteGuild))]
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
