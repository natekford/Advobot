using System;
using System.Threading.Tasks;
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

namespace Advobot.Commands
{
	public sealed class Guilds : ModuleBase
	{
		[Group(nameof(LeaveGuild)), ModuleInitialismAlias(typeof(LeaveGuild))]
		[Summary("Makes the bot leave the guild. " +
			"Settings and preferences will be preserved.")]
		[EnabledByDefault(true)]
		public sealed class LeaveGuild : AdvobotModuleBase
		{
#warning change help entry to show the overloads of each command
			[Command]
			[RequireGuildOwner]
			public Task Command()
				=> Context.Guild.LeaveAsync();
			[Command]
			[RequireBotOwner]
			public async Task<RuntimeResult> Command([Remainder] IGuild guild)
			{
				await guild.LeaveAsync().CAF();
				return Responses.Guilds.LeftGuild(guild);
			}
		}

		[Group(nameof(ModifyGuildName)), ModuleInitialismAlias(typeof(ModifyGuildName))]
		[Summary("Change the name of the guild to the given name.")]
		[UserPermissionRequirement(GuildPermission.ManageGuild)]
		[EnabledByDefault(true)]
		public sealed class ModifyGuildName : AdvobotModuleBase
		{
			[Command]
			public async Task<RuntimeResult> Command([Remainder, ValidateGuildName] string name)
			{
				await Context.Guild.ModifyAsync(x => x.Name = name, GenerateRequestOptions()).CAF();
				return Responses.Snowflakes.ModifiedName(Context.Guild, name);
			}
		}

		[Group(nameof(ModifyGuildRegion)), ModuleInitialismAlias(typeof(ModifyGuildRegion))]
		[Summary("Changes the guild server region.")]
		[UserPermissionRequirement(GuildPermission.ManageGuild)]
		[EnabledByDefault(true)]
		public sealed class ModifyGuildRegion : AdvobotModuleBase
		{
			[ImplicitCommand, ImplicitAlias, Priority(1)]
			public async Task<RuntimeResult> Show()
			{
				var regions = await Context.Guild.GetVoiceRegionsAsync().CAF();
				return Responses.Guilds.DisplayRegions(regions);
			}
			[Command]
			public async Task<RuntimeResult> Command(IVoiceRegion region)
			{
				await Context.Guild.ModifyAsync(x => x.Region = Optional.Create(region), GenerateRequestOptions()).CAF();
				return Responses.Guilds.ModifiedRegion(region);
			}
		}

		[Group(nameof(ModifyGuildAfkTimer)), ModuleInitialismAlias(typeof(ModifyGuildAfkTimer))]
		[Summary("Changes the guild AFK timeout.")]
		[UserPermissionRequirement(GuildPermission.ManageGuild)]
		[EnabledByDefault(true)]
		public sealed class ModifyGuildAfkTimer : AdvobotModuleBase
		{
			[Command]
			public async Task<RuntimeResult> Command([ValidateGuildAfkTime] int time)
			{
				await Context.Guild.ModifyAsync(x => x.AfkTimeout = time, GenerateRequestOptions()).CAF();
				return Responses.Guilds.ModifiedAfkTime(time);
			}
		}

		[Group(nameof(ModifyGuildAfkChannel)), ModuleInitialismAlias(typeof(ModifyGuildAfkChannel))]
		[Summary("Changes the guild afk channel.")]
		[UserPermissionRequirement(GuildPermission.ManageGuild)]
		[EnabledByDefault(true)]
		public sealed class ModifyGuildAfkChannel : AdvobotModuleBase
		{
			[Command]
			public async Task<RuntimeResult> Command([ValidateVoiceChannel(ChannelPermission.ManageChannels, FromContext = true)] SocketVoiceChannel? channel)
			{
				await Context.Guild.ModifyAsync(x => x.AfkChannel = Optional.Create<IVoiceChannel?>(channel), GenerateRequestOptions()).CAF();
				return Responses.Guilds.ModifiedAfkChannel(channel);
			}
			[ImplicitCommand, ImplicitAlias]
			public Task<RuntimeResult> Remove()
				=> Command(null);
		}

		[Group(nameof(ModifyGuildSystemChannel)), ModuleInitialismAlias(typeof(ModifyGuildSystemChannel))]
		[Summary("Changes the guild system channel.")]
		[UserPermissionRequirement(GuildPermission.ManageGuild)]
		[EnabledByDefault(true)]
		public sealed class ModifyGuildSystemChannel : AdvobotModuleBase
		{
			[Command]
			public async Task<RuntimeResult> Command([ValidateTextChannel(ChannelPermission.ManageChannels, FromContext = true)] SocketTextChannel? channel)
			{
				await Context.Guild.ModifyAsync(x => x.SystemChannel = Optional.Create<ITextChannel?>(channel), GenerateRequestOptions()).CAF();
				return Responses.Guilds.ModifiedSystemChannel(channel);
			}
			[ImplicitCommand, ImplicitAlias]
			public Task<RuntimeResult> Remove()
				=> Command(null);
		}

		[Group(nameof(ModifyGuildMsgNotif)), ModuleInitialismAlias(typeof(ModifyGuildMsgNotif))]
		[Summary("Changes the message notifications to either all messages or mentions only.")]
		[UserPermissionRequirement(GuildPermission.ManageGuild)]
		[EnabledByDefault(true)]
		public sealed class ModifyGuildMsgNotif : AdvobotModuleBase
		{
			[Command]
			public async Task<RuntimeResult> Command(DefaultMessageNotifications msgNotifs)
			{
				await Context.Guild.ModifyAsync(x => x.DefaultMessageNotifications = msgNotifs, GenerateRequestOptions()).CAF();
				return Responses.Guilds.ModifiedMsgNotif(msgNotifs);
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
			public async Task<RuntimeResult> Command(VerificationLevel verif)
			{
				await Context.Guild.ModifyAsync(x => x.VerificationLevel = verif, GenerateRequestOptions()).CAF();
				return Responses.Guilds.ModifiedVerif(verif);
			}
		}

		[Group(nameof(ModifyGuildIcon)), ModuleInitialismAlias(typeof(ModifyGuildIcon))]
		[Summary("Changes the guild's icon to the given image.")]
		[UserPermissionRequirement(GuildPermission.ManageGuild)]
		[EnabledByDefault(true)]
		public sealed class ModifyGuildIcon : ImageResizerModule
		{
			[Command]
			public Task<RuntimeResult> Command(Uri url)
			{
				var position = Enqueue(new IconCreationArgs("Guild Icon", Context, url, default,
					(ctx, ms) => ctx.Guild.ModifyAsync(x => x.Icon = new Image(ms), ctx.GenerateRequestOptions())));
				return Responses.Snowflakes.EnqueuedIcon(Context.Guild, position);
			}
			[ImplicitCommand, ImplicitAlias]
			public async Task<RuntimeResult> Remove()
			{
				await Context.Guild.ModifyAsync(x => x.Icon = new Image(), GenerateRequestOptions()).CAF();
				return Responses.Snowflakes.RemovedIcon(Context.Guild);
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
			public Task<RuntimeResult> Command(Uri url)
			{
				var position = Enqueue(new IconCreationArgs("Guild Splash", Context, url, default,
					(ctx, ms) => ctx.Guild.ModifyAsync(x => x.Splash = new Image(ms), ctx.GenerateRequestOptions())));
				return Responses.Guilds.EnqueuedSplash(position);
			}
			[ImplicitCommand, ImplicitAlias]
			public async Task<RuntimeResult> Remove()
			{
				await Context.Guild.ModifyAsync(x => x.Splash = new Image(), GenerateRequestOptions()).CAF();
				return Responses.Guilds.RemovedSplash();
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
			public async Task<RuntimeResult> Command()
			{
				await Context.Guild.ModifyAsync(x => x.Owner = new Optional<IUser>(Context.User)).CAF();
				return Responses.Guilds.ModifiedOwner(Context.User);
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
			public Task Command()
				=> Context.Guild.DeleteAsync();
		}
	}
}
