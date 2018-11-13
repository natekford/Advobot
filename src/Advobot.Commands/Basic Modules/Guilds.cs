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
using Advobot.Classes.Attributes.Preconditions.Permissions;
using Advobot.Classes.ImageResizing;
using Advobot.Classes.Modules;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using CPerm = Discord.ChannelPermission;

namespace Advobot.Commands
{
	public sealed class Guilds : ModuleBase
	{
		[Group(nameof(LeaveGuild)), ModuleInitialismAlias(typeof(LeaveGuild))]
		[Summary("Makes the bot leave the guild. " +
			"Settings and preferences will be preserved.")]
#warning better group name
		[RequireBotOwner(Group = nameof(LeaveGuild)), RequireGuildOwner(Group = nameof(LeaveGuild))]
		[EnabledByDefault(true)]
		public sealed class LeaveGuild : AdvobotModuleBase
		{
			[Command]
			public Task Command()
				=> Context.Guild.LeaveAsync();
			[Command]
			public async Task Command(ulong guildId)
			{
				//Need bot owner check so only the bot owner can make the bot leave servers they don't own
				if (Context.User.Id != await Context.Client.GetOwnerIdAsync().CAF())
				{
					await ReplyErrorAsync("Only the bot owner can use this command targetting other guilds.").CAF();
					return;
				}
				if (!(Context.Client.GetGuild(guildId) is SocketGuild guild))
				{
					await ReplyErrorAsync("Invalid guild supplied.").CAF();
					return;
				}
				await guild.LeaveAsync().CAF();
				await ReplyTimedAsync($"Successfully left the guild `{guild.Format()}`.").CAF();
			}
		}

		[Group(nameof(ModifyGuildName)), ModuleInitialismAlias(typeof(ModifyGuildName))]
		[Summary("Change the name of the guild to the given name.")]
		[UserPermissionRequirement(GuildPermission.ManageGuild)]
		[EnabledByDefault(true)]
		public sealed class ModifyGuildName : AdvobotModuleBase
		{
			[Command]
			public async Task Command([Remainder, ValidateGuildName] string name)
			{
				await Context.Guild.ModifyAsync(x => x.Name = name, GenerateRequestOptions()).CAF();
				await ReplyTimedAsync($"Successfully changed the guild name to `{name}`.").CAF();
			}
		}

		[Group(nameof(ModifyGuildRegion)), ModuleInitialismAlias(typeof(ModifyGuildRegion))]
		[Summary("Changes the guild server region.")]
		[UserPermissionRequirement(GuildPermission.ManageGuild)]
		[EnabledByDefault(true)]
		public sealed class ModifyGuildRegion : AdvobotModuleBase
		{
			[ImplicitCommand, ImplicitAlias, Priority(1)]
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

		[Group(nameof(ModifyGuildAfkTimer)), ModuleInitialismAlias(typeof(ModifyGuildAfkTimer))]
		[Summary("Changes the guild AFK timeout.")]
		[UserPermissionRequirement(GuildPermission.ManageGuild)]
		[EnabledByDefault(true)]
		public sealed class ModifyGuildAfkTimer : AdvobotModuleBase
		{
			[Command]
			public async Task Command([ValidateGuildAfkTime] int time)
			{
				await Context.Guild.ModifyAsync(x => x.AfkTimeout = time, GenerateRequestOptions()).CAF();
				await ReplyTimedAsync($"Successfully set the guild AFK timeout to `{time}` minutes.").CAF();
			}
		}

		[Group(nameof(ModifyGuildAfkChannel)), ModuleInitialismAlias(typeof(ModifyGuildAfkChannel))]
		[Summary("Changes the guild afk channel.")]
		[UserPermissionRequirement(GuildPermission.ManageGuild)]
		[EnabledByDefault(true)]
		public sealed class ModifyGuildAfkChannel : AdvobotModuleBase
		{
			[Command]
			public async Task Command([ValidateVoiceChannel(CPerm.ManageChannels, FromContext = true)] SocketVoiceChannel channel)
			{
				await Context.Guild.ModifyAsync(x => x.AfkChannel = Optional.Create<IVoiceChannel>(channel), GenerateRequestOptions()).CAF();
				await ReplyTimedAsync($"Successfully set the guild AFK channel to `{channel.Format()}`.").CAF();
			}
			[ImplicitCommand, ImplicitAlias]
			public async Task Remove()
			{
				await Context.Guild.ModifyAsync(x => x.AfkChannelId = Optional.Create<ulong?>(null), GenerateRequestOptions()).CAF();
				await ReplyTimedAsync("Successfully removed the guild afk channel.").CAF();
			}
		}

		[Group(nameof(ModifyGuildSystemChannel)), ModuleInitialismAlias(typeof(ModifyGuildSystemChannel))]
		[Summary("Changes the guild system channel.")]
		[UserPermissionRequirement(GuildPermission.ManageGuild)]
		[EnabledByDefault(true)]
		public sealed class ModifyGuildSystemChannel : AdvobotModuleBase
		{
			[Command]
			public async Task Command([ValidateTextChannel(CPerm.ManageChannels, FromContext = true)] SocketTextChannel channel)
			{
				await Context.Guild.ModifyAsync(x => x.SystemChannel = Optional.Create<ITextChannel>(channel), GenerateRequestOptions()).CAF();
				await ReplyTimedAsync($"Successfully set the guild system channel to `{channel.Format()}`.").CAF();
			}
			[ImplicitCommand, ImplicitAlias]
			public async Task Remove()
			{
				await Context.Guild.ModifyAsync(x => x.SystemChannelId = Optional.Create<ulong?>(null), GenerateRequestOptions()).CAF();
				await ReplyTimedAsync("Successfully removed the guild system channel.").CAF();
			}
		}

		[Group(nameof(ModifyGuildMsgNotif)), ModuleInitialismAlias(typeof(ModifyGuildMsgNotif))]
		[Summary("Changes the message notifications to either all messages or mentions only.")]
		[UserPermissionRequirement(GuildPermission.ManageGuild)]
		[EnabledByDefault(true)]
		public sealed class ModifyGuildMsgNotif : AdvobotModuleBase
		{
			[Command]
			public async Task Command(DefaultMessageNotifications msgNotifs)
			{
				await Context.Guild.ModifyAsync(x => x.DefaultMessageNotifications = msgNotifs, GenerateRequestOptions()).CAF();
				await ReplyTimedAsync($"Successfully changed the default message notification setting to `{msgNotifs}`.").CAF();
			}
		}

		[Group(nameof(ModifyGuildVerif)), ModuleInitialismAlias(typeof(ModifyGuildVerif))]
		[Summary("Changes the verification level. " +
			"None is the most lenient (no requirements to type), extreme is the harshest (phone verification).")]
		[UserPermissionRequirement(GuildPermission.ManageGuild)]
		[EnabledByDefault(true)]
		public sealed class ModifyGuildVerif : AdvobotModuleBase
		{
			[Command]
			public async Task Command(VerificationLevel verif)
			{
				await Context.Guild.ModifyAsync(x => x.VerificationLevel = verif, GenerateRequestOptions()).CAF();
				await ReplyTimedAsync($"Successfully set the guild verification level as `{verif}`.").CAF();
			}
		}

		[Group(nameof(ModifyGuildIcon)), ModuleInitialismAlias(typeof(ModifyGuildIcon))]
		[Summary("Changes the guild's icon to the given image.")]
		[UserPermissionRequirement(GuildPermission.ManageGuild)]
		[EnabledByDefault(true)]
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
			[ImplicitCommand, ImplicitAlias]
			public async Task Remove()
			{
				await Context.Guild.ModifyAsync(x => x.Icon = new Image(), GenerateRequestOptions()).CAF();
				await ReplyTimedAsync("Successfully removed the guild icon.").CAF();
			}
		}

		[Group(nameof(ModifyGuildSplash)), ModuleInitialismAlias(typeof(ModifyGuildSplash))]
		[Summary("Changes the guild splash to the given image. Won't be modified unless the server is a partnered server.")]
		[UserPermissionRequirement(GuildPermission.ManageGuild)]
		[EnabledByDefault(true)]
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
			[ImplicitCommand, ImplicitAlias]
			public async Task Remove()
			{
				await Context.Guild.ModifyAsync(x => x.Splash = new Image(), GenerateRequestOptions()).CAF();
				await ReplyTimedAsync("Successfully removed the guild splash.").CAF();
			}
		}

		[Group(nameof(CreateGuild)), ModuleInitialismAlias(typeof(CreateGuild))]
		[Summary("Creates a guild with the bot as the owner.")]
		[RequireBotOwner]
		[EnabledByDefault(true)]
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

		[Group(nameof(SwapGuildOwner)), ModuleInitialismAlias(typeof(SwapGuildOwner))]
		[Summary("If the bot is the current owner of the guild, this command will give you owner.")]
		[RequireBotIsOwner]
		[RequireBotOwner]
		[EnabledByDefault(true)]
		public sealed class SwapGuildOwner : AdvobotModuleBase
		{
			[Command]
			public async Task Command()
			{
				await Context.Guild.ModifyAsync(x => x.Owner = new Optional<IUser>(Context.User)).CAF();
				await ReplyTimedAsync($"{Context.User.Mention} is now the owner.").CAF();
			}
		}

		[Group(nameof(DeleteGuild)), ModuleInitialismAlias(typeof(DeleteGuild))]
		[Summary("If the bot is the current owner of the guild, this command will delete the guild.")]
		[RequireBotIsOwner]
		[RequireBotOwner]
		[EnabledByDefault(true)]
		public sealed class DeleteGuild : AdvobotModuleBase
		{
			[Command]
			public async Task Command()
				=> await Context.Guild.DeleteAsync().CAF();
		}
	}
}
