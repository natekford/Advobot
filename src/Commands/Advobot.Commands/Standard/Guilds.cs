using System;
using System.Threading.Tasks;
using Advobot.Attributes;
using Advobot.Attributes.ParameterPreconditions.DiscordObjectValidation.Channels;
using Advobot.Attributes.ParameterPreconditions.Numbers;
using Advobot.Attributes.ParameterPreconditions.Strings;
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
		[CommandMeta("3090730c-1377-4a56-b379-485baed393e7", IsEnabled = true)]
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
		[CommandMeta("54a75c3c-be5a-46d4-93d3-b1cbc1af9def", IsEnabled = true)]
		[RequireGuildPermissions(GuildPermission.ManageGuild)]
		public sealed class ModifyGuildName : AdvobotModuleBase
		{
			[Command]
			public async Task<RuntimeResult> Command([Remainder, GuildName] string name)
			{
				await Context.Guild.ModifyAsync(x => x.Name = name, GenerateRequestOptions()).CAF();
				return Responses.Snowflakes.ModifiedName(Context.Guild, name);
			}
		}

		[Group(nameof(ModifyGuildRegion)), ModuleInitialismAlias(typeof(ModifyGuildRegion))]
		[LocalizedSummary(nameof(Summaries.ModifyGuildRegion))]
		[CommandMeta("e2ab50b0-ce20-48ee-bac9-1fac2e53387d", IsEnabled = true)]
		[RequireGuildPermissions(GuildPermission.ManageGuild)]
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
		[CommandMeta("bb4ceabc-2660-431d-aa0c-d0f6176b88a1", IsEnabled = true)]
		[RequireGuildPermissions(GuildPermission.ManageGuild)]
		public sealed class ModifyGuildAfkTimer : AdvobotModuleBase
		{
			[Command]
			public async Task<RuntimeResult> Command([GuildAfkTime] int time)
			{
				await Context.Guild.ModifyAsync(x => x.AfkTimeout = time, GenerateRequestOptions()).CAF();
				return Responses.Guilds.ModifiedAfkTime(time);
			}
		}

		[Group(nameof(ModifyGuildAfkChannel)), ModuleInitialismAlias(typeof(ModifyGuildAfkChannel))]
		[LocalizedSummary(nameof(Summaries.ModifyGuildAfkChannel))]
		[CommandMeta("ec19c9b8-cc0c-46d6-a207-9eb6abf69c9e", IsEnabled = true)]
		[RequireGuildPermissions(GuildPermission.ManageGuild)]
		public sealed class ModifyGuildAfkChannel : AdvobotModuleBase
		{
			[ImplicitCommand, ImplicitAlias]
			public Task<RuntimeResult> Remove()
				=> Command(null);
			[Command]
			public async Task<RuntimeResult> Command(
				[Channel(ManageChannels)] IVoiceChannel channel)
			{
				await Context.Guild.ModifyAsync(x => x.AfkChannel = Optional.Create<IVoiceChannel?>(channel), GenerateRequestOptions()).CAF();
				return Responses.Guilds.ModifiedAfkChannel(channel);
			}
		}

		[Group(nameof(ModifyGuildSystemChannel)), ModuleInitialismAlias(typeof(ModifyGuildSystemChannel))]
		[LocalizedSummary(nameof(Summaries.ModifyGuildSystemChannel))]
		[CommandMeta("f6cc90d9-ae1d-4eab-ae15-226d88e42092", IsEnabled = true)]
		[RequireGuildPermissions(GuildPermission.ManageGuild)]
		public sealed class ModifyGuildSystemChannel : AdvobotModuleBase
		{
			[ImplicitCommand, ImplicitAlias]
			public Task<RuntimeResult> Remove()
				=> Command(null);
			[Command]
			public async Task<RuntimeResult> Command(
				[Channel(ManageChannels)] ITextChannel channel)
			{
				await Context.Guild.ModifyAsync(x => x.SystemChannel = Optional.Create(channel), GenerateRequestOptions()).CAF();
				return Responses.Guilds.ModifiedSystemChannel(channel);
			}
		}

		[Group(nameof(ModifyGuildMsgNotif)), ModuleInitialismAlias(typeof(ModifyGuildMsgNotif))]
		[LocalizedSummary(nameof(Summaries.ModifyGuildMsgNotif))]
		[CommandMeta("f32a347c-e2d8-4b64-9a3c-53cd46d0f3ed", IsEnabled = true)]
		[RequireGuildPermissions(GuildPermission.ManageGuild)]
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
		[CommandMeta("5640e2e6-7ee8-416c-982c-9efb22634f54", IsEnabled = true)]
		[RequireGuildPermissions(GuildPermission.ManageGuild)]
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
		[CommandMeta("c6f5c58e-4784-4f30-91a9-3727e580ddf2", IsEnabled = true)]
		[RequireGuildPermissions(GuildPermission.ManageGuild)]
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
		[CommandMeta("0a02898e-0e5c-417d-9309-7f93714b61f7", IsEnabled = true)]
		[RequireGuildPermissions(GuildPermission.ManageGuild)]
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
		[CommandMeta("f3e7e812-067a-4be3-9904-42eb9eac8791", IsEnabled = true)]
		[RequireBotOwner]
		public sealed class CreateGuild : AdvobotModuleBase
		{
			[Command]
			public async Task Command([Remainder, GuildName] string name)
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
		[CommandMeta("3cb8a267-5d62-4644-b5af-60281dd8e182", IsEnabled = true)]
		[RequireBotIsOwner]
		[RequireBotOwner]
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
		[CommandMeta("65ec403c-7287-4689-a2f0-c73cf5407540", IsEnabled = true)]
		[RequireBotIsOwner]
		[RequireBotOwner]
		public sealed class DeleteGuild : AdvobotModuleBase
		{
			[Command]
			public Task Command()
				=> Context.Guild.DeleteAsync();
		}
	}
}
