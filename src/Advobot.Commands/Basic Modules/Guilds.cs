using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Advobot.Classes;
using Advobot.Classes.Attributes;
using Advobot.Classes.Attributes.ParameterPreconditions.DiscordObjectValidation.Channels;
using Advobot.Classes.Attributes.ParameterPreconditions.NumberValidation;
using Advobot.Classes.Attributes.ParameterPreconditions.StringValidation;
using Advobot.Classes.Attributes.Preconditions;
using Advobot.Classes.ImageResizing;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using CPerm = Discord.ChannelPermission;

namespace Advobot.Commands.Guilds
{
	[Category(typeof(LeaveGuild)), Group(nameof(LeaveGuild)), TopLevelShortAlias(typeof(LeaveGuild))]
	[Summary("Makes the bot leave the guild. " +
		"Settings and preferences will be preserved.")]
#warning better group name
	[RequireBotOwner(Group = nameof(LeaveGuild)), RequireGuildOwner(Group = nameof(LeaveGuild))]
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
				await ReplyErrorAsync(new Error("Only the bot owner can use this command targetting other guilds.")).CAF();
				return;
			}
			if (!(Context.Client.GetGuild(guildId) is SocketGuild guild))
			{
				await ReplyErrorAsync(new Error("Invalid guild supplied.")).CAF();
				return;
			}
			await guild.LeaveAsync().CAF();
			await ReplyTimedAsync($"Successfully left the guild `{guild.Format()}`.").CAF();
		}
	}

	[Category(typeof(ModifyGuildName)), Group(nameof(ModifyGuildName)), TopLevelShortAlias(typeof(ModifyGuildName))]
	[Summary("Change the name of the guild to the given name.")]
	[PermissionRequirement(new[] { GuildPermission.ManageGuild }, null)]
	[DefaultEnabled(true)]
	public sealed class ModifyGuildName : AdvobotModuleBase
	{
		[Command]
		public async Task Command([Remainder, ValidateGuildName] string name)
		{
			await Context.Guild.ModifyAsync(x => x.Name = name, GenerateRequestOptions()).CAF();
			await ReplyTimedAsync($"Successfully changed the guild name to `{name}`.").CAF();
		}
	}

	[Category(typeof(ModifyGuildRegion)), Group(nameof(ModifyGuildRegion)), TopLevelShortAlias(typeof(ModifyGuildRegion))]
	[Summary("Changes the guild server region.")]
	[PermissionRequirement(new[] { GuildPermission.ManageGuild }, null)]
	[DefaultEnabled(true)]
	public sealed class ModifyGuildRegion : AdvobotModuleBase
	{
		[Command(nameof(Show)), ShortAlias(nameof(Show)), Priority(1)]
		public async Task Show()
		{
			var regions = await Context.Guild.GetVoiceRegionsAsync().CAF();
			await ReplyEmbedAsync(new EmbedWrapper
			{
				Title = "Region Ids",
				Description = $"`{regions.Join("`, `", x => x.Name)}`",
			}).CAF();
		}
		[Command]
		public async Task Command(IVoiceRegion region)
		{
			await Context.Guild.ModifyAsync(x => x.Region = Optional.Create(region), GenerateRequestOptions()).CAF();
			await ReplyTimedAsync($"Successfully changed the server region of the guild to `{region.Id}`.").CAF();
		}
	}

	[Category(typeof(ModifyGuildAfkTimer)), Group(nameof(ModifyGuildAfkTimer)), TopLevelShortAlias(typeof(ModifyGuildAfkTimer))]
	[Summary("Changes the guild AFK timeout.")]
	[PermissionRequirement(new[] { GuildPermission.ManageGuild }, null)]
	[DefaultEnabled(true)]
	public sealed class ModifyGuildAfkTimer : AdvobotModuleBase
	{
		[Command]
		public async Task Command([ValidateGuildAfkTime] int time)
		{
			await Context.Guild.ModifyAsync(x => x.AfkTimeout = time, GenerateRequestOptions()).CAF();
			await ReplyTimedAsync($"Successfully set the guild AFK timeout to `{time}` minutes.").CAF();
		}
	}

	[Category(typeof(ModifyGuildAfkChannel)), Group(nameof(ModifyGuildAfkChannel)), TopLevelShortAlias(typeof(ModifyGuildAfkChannel))]
	[Summary("Changes the guild afk channel.")]
	[PermissionRequirement(new[] { GuildPermission.ManageGuild }, null)]
	[DefaultEnabled(true)]
	public sealed class ModifyGuildAfkChannel : AdvobotModuleBase
	{
		[Command]
		public async Task Command([ValidateVoiceChannel(CPerm.ManageChannels, FromContext = true)] SocketVoiceChannel channel)
		{
			await Context.Guild.ModifyAsync(x => x.AfkChannel = Optional.Create<IVoiceChannel>(channel), GenerateRequestOptions()).CAF();
			await ReplyTimedAsync($"Successfully set the guild AFK channel to `{channel.Format()}`.").CAF();
		}
		[Command(nameof(Remove)), ShortAlias(nameof(Remove))]
		public async Task Remove()
		{
			await Context.Guild.ModifyAsync(x => x.AfkChannelId = Optional.Create<ulong?>(null), GenerateRequestOptions()).CAF();
			await ReplyTimedAsync("Successfully removed the guild afk channel.").CAF();
		}
	}

	[Category(typeof(ModifyGuildSystemChannel)), Group(nameof(ModifyGuildSystemChannel)), TopLevelShortAlias(typeof(ModifyGuildSystemChannel))]
	[Summary("Changes the guild system channel.")]
	[PermissionRequirement(new[] { GuildPermission.ManageGuild }, null)]
	[DefaultEnabled(true)]
	public sealed class ModifyGuildSystemChannel : AdvobotModuleBase
	{
		[Command]
		public async Task Command([ValidateTextChannel(CPerm.ManageChannels, FromContext = true)] SocketTextChannel channel)
		{
			await Context.Guild.ModifyAsync(x => x.SystemChannel = Optional.Create<ITextChannel>(channel), GenerateRequestOptions()).CAF();
			await ReplyTimedAsync($"Successfully set the guild system channel to `{channel.Format()}`.").CAF();
		}
		[Command(nameof(Remove)), ShortAlias(nameof(Remove))]
		public async Task Remove()
		{
			await Context.Guild.ModifyAsync(x => x.SystemChannelId = Optional.Create<ulong?>(null), GenerateRequestOptions()).CAF();
			await ReplyTimedAsync("Successfully removed the guild system channel.").CAF();
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
			await Context.Guild.ModifyAsync(x => x.DefaultMessageNotifications = msgNotifs, GenerateRequestOptions()).CAF();
			await ReplyTimedAsync($"Successfully changed the default message notification setting to `{msgNotifs}`.").CAF();
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
			await Context.Guild.ModifyAsync(x => x.VerificationLevel = verif, GenerateRequestOptions()).CAF();
			await ReplyTimedAsync($"Successfully set the guild verification level as `{verif}`.").CAF();
		}
	}

	[Category(typeof(ModifyGuildIcon)), Group(nameof(ModifyGuildIcon)), TopLevelShortAlias(typeof(ModifyGuildIcon))]
	[Summary("Changes the guild's icon to the given image.")]
	[PermissionRequirement(new[] { GuildPermission.ManageGuild }, null)]
	[DefaultEnabled(true)]
	public sealed class ModifyGuildIcon : ImageResizerModule
	{
		[Command]
		public async Task Command(Uri url)
		{
			await ProcessAsync(new IconCreationArgs("Guild Icon", Context, url, default, async (ctx, ms) =>
			{
				await ctx.Guild.ModifyAsync(x => x.Icon = new Image(ms), ctx.GenerateRequestOptions()).CAF();
			})).CAF();
		}
		[Command(nameof(Remove)), ShortAlias(nameof(Remove))]
		public async Task Remove()
		{
			await Context.Guild.ModifyAsync(x => x.Icon = new Image(), GenerateRequestOptions()).CAF();
			await ReplyTimedAsync("Successfully removed the guild icon.").CAF();
		}
	}

	[Category(typeof(ModifyGuildSplash)), Group(nameof(ModifyGuildSplash)), TopLevelShortAlias(typeof(ModifyGuildSplash))]
	[Summary("Changes the guild splash to the given image. Won't be modified unless the server is a partnered server.")]
	[PermissionRequirement(new[] { GuildPermission.ManageGuild }, null)]
	[DefaultEnabled(true)]
	[RequirePartneredGuild]
	public sealed class ModifyGuildSplash : ImageResizerModule
	{
		[Command]
		public async Task Command(Uri url)
		{
			await ProcessAsync(new IconCreationArgs("Guild Splash", Context, url, default, async (ctx, ms) =>
			{
				await ctx.Guild.ModifyAsync(x => x.Splash = new Image(ms), ctx.GenerateRequestOptions()).CAF();
			})).CAF();
		}
		[Command(nameof(Remove)), ShortAlias(nameof(Remove))]
		public async Task Remove()
		{
			await Context.Guild.ModifyAsync(x => x.Splash = new Image(), GenerateRequestOptions()).CAF();
			await ReplyTimedAsync("Successfully removed the guild splash.").CAF();
		}
	}

	[Category(typeof(CreateGuild)), Group(nameof(CreateGuild)), TopLevelShortAlias(typeof(CreateGuild))]
	[Summary("Creates a guild with the bot as the owner.")]
	[RequireBotOwner]
	[DefaultEnabled(true)]
	public sealed class CreateGuild : AdvobotModuleBase
	{
		[Command]
		public async Task Command([Remainder, ValidateGuildName] string name)
		{
			var optimalVoiceRegion = await Context.Client.GetOptimalVoiceRegionAsync().CAF();
			var newGuild = await Context.Client.CreateGuildAsync(name, optimalVoiceRegion).CAF();
			var defaultChannel = await newGuild.GetDefaultChannelAsync().CAF();
			var invite = await defaultChannel.CreateInviteAsync().CAF();
			await Context.User.SendMessageAsync(invite.Url).CAF();
		}
	}

	[Category(typeof(SwapGuildOwner)), Group(nameof(SwapGuildOwner)), TopLevelShortAlias(typeof(SwapGuildOwner))]
	[Summary("If the bot is the current owner of the guild, this command will give you owner.")]
	[RequireBotIsOwner]
	[RequireBotOwner]
	[DefaultEnabled(true)]
	public sealed class SwapGuildOwner : AdvobotModuleBase
	{
		[Command]
		public async Task Command()
		{
			await Context.Guild.ModifyAsync(x => x.Owner = new Optional<IUser>(Context.User)).CAF();
			await ReplyTimedAsync($"{Context.User.Mention} is now the owner.").CAF();
		}
	}

	[Category(typeof(DeleteGuild)), Group(nameof(DeleteGuild)), TopLevelShortAlias(typeof(DeleteGuild))]
	[Summary("If the bot is the current owner of the guild, this command will delete the guild.")]
	[RequireBotIsOwner]
	[RequireBotOwner]
	[DefaultEnabled(true)]
	public sealed class DeleteGuild : AdvobotModuleBase
	{
		[Command]
		public async Task Command()
			=> await Context.Guild.DeleteAsync().CAF();
	}
}
