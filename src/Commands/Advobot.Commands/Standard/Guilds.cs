using System;
using System.Threading.Tasks;
using Advobot.Attributes;
using Advobot.Attributes.ParameterPreconditions.DiscordObjectValidation.Channels;
using Advobot.Attributes.ParameterPreconditions.NumberValidation;
using Advobot.Attributes.ParameterPreconditions.StringLengthValidation;
using Advobot.Attributes.Preconditions;
using Advobot.Attributes.Preconditions.Permissions;
using Advobot.Commands.Localization;
using Advobot.Commands.Resources;
using Advobot.Modules;
using Advobot.Services.ImageResizing;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Discord.Commands;
using static Discord.ChannelPermission;

namespace Advobot.Commands.Standard
{
	public sealed class Guilds : ModuleBase
	{
		[Group(nameof(LeaveGuild)), ModuleInitialismAlias(typeof(LeaveGuild))]
		[LocalizedSummary(nameof(Summaries.LeaveGuild))]
		[EnabledByDefault(true)]
		public sealed class LeaveGuild : AdvobotModuleBase
		{
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
		[LocalizedSummary(nameof(Summaries.ModifyGuildName))]
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
		[LocalizedSummary(nameof(Summaries.ModifyGuildRegion))]
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
		[LocalizedSummary(nameof(Summaries.ModifyGuildAfkTimer))]
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
		[LocalizedSummary(nameof(Summaries.ModifyGuildAfkChannel))]
		[UserPermissionRequirement(GuildPermission.ManageGuild)]
		[EnabledByDefault(true)]
		public sealed class ModifyGuildAfkChannel : AdvobotModuleBase
		{
			[Command]
			public async Task<RuntimeResult> Command(
				[ValidateVoiceChannel(ManageChannels, FromContext = true)] IVoiceChannel? channel)
			{
				await Context.Guild.ModifyAsync(x => x.AfkChannel = Optional.Create<IVoiceChannel?>(channel), GenerateRequestOptions()).CAF();
				return Responses.Guilds.ModifiedAfkChannel(channel);
			}
			[ImplicitCommand, ImplicitAlias]
			public Task<RuntimeResult> Remove()
				=> Command(null);
		}

		[Group(nameof(ModifyGuildSystemChannel)), ModuleInitialismAlias(typeof(ModifyGuildSystemChannel))]
		[LocalizedSummary(nameof(Summaries.ModifyGuildSystemChannel))]
		[UserPermissionRequirement(GuildPermission.ManageGuild)]
		[EnabledByDefault(true)]
		public sealed class ModifyGuildSystemChannel : AdvobotModuleBase
		{
			[Command]
			public async Task<RuntimeResult> Command(
				[ValidateTextChannel(ManageChannels, FromContext = true)] ITextChannel? channel)
			{
				await Context.Guild.ModifyAsync(x => x.SystemChannel = Optional.Create(channel), GenerateRequestOptions()).CAF();
				return Responses.Guilds.ModifiedSystemChannel(channel);
			}
			[ImplicitCommand, ImplicitAlias]
			public Task<RuntimeResult> Remove()
				=> Command(null);
		}

		[Group(nameof(ModifyGuildMsgNotif)), ModuleInitialismAlias(typeof(ModifyGuildMsgNotif))]
		[LocalizedSummary(nameof(Summaries.ModifyGuildMsgNotif))]
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
		[LocalizedSummary(nameof(Summaries.ModifyGuildVerif))]
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
		[LocalizedSummary(nameof(Summaries.ModifyGuildIcon))]
		[UserPermissionRequirement(GuildPermission.ManageGuild)]
		[EnabledByDefault(true)]
		public sealed class ModifyGuildIcon : ImageResizerModule
		{
			[Command]
			public Task<RuntimeResult> Command(Uri url)
			{
				var position = Enqueue(new IconCreationContext(Context, url, default, "Guild Icon",
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
		[LocalizedSummary(nameof(Summaries.ModifyGuildSplash))]
		[UserPermissionRequirement(GuildPermission.ManageGuild)]
		[EnabledByDefault(true)]
		[RequirePartneredGuild]
		public sealed class ModifyGuildSplash : ImageResizerModule
		{
			[Command]
			public Task<RuntimeResult> Command(Uri url)
			{
				var position = Enqueue(new IconCreationContext(Context, url, default, "Guild Splash",
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
		[LocalizedSummary(nameof(Summaries.CreateGuild))]
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
		[LocalizedSummary(nameof(Summaries.SwapGuildOwner))]
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
		[LocalizedSummary(nameof(Summaries.DeleteGuild))]
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
