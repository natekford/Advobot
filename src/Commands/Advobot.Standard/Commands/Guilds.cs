using System;
using System.Threading.Tasks;

using Advobot.Attributes;
using Advobot.Attributes.ParameterPreconditions.DiscordObjectValidation.Channels;
using Advobot.Attributes.ParameterPreconditions.Numbers;
using Advobot.Attributes.ParameterPreconditions.Strings;
using Advobot.Attributes.Preconditions;
using Advobot.Attributes.Preconditions.Permissions;
using Advobot.Localization;
using Advobot.Modules;
using Advobot.Resources;
using Advobot.Services.ImageResizing;
using Advobot.Utilities;

using AdvorangesUtils;

using Discord;
using Discord.Commands;

using static Discord.ChannelPermission;

namespace Advobot.Standard.Commands
{
	[Category(nameof(Guilds))]
	public sealed class Guilds : ModuleBase
	{
		[LocalizedGroup(nameof(Groups.CreateGuild))]
		[LocalizedAlias(nameof(Aliases.CreateGuild))]
		[LocalizedSummary(nameof(Summaries.CreateGuild))]
		[Meta("f3e7e812-067a-4be3-9904-42eb9eac8791", IsEnabled = true)]
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

		[LocalizedGroup(nameof(Groups.DeleteGuild))]
		[LocalizedAlias(nameof(Aliases.DeleteGuild))]
		[LocalizedSummary(nameof(Summaries.DeleteGuild))]
		[Meta("65ec403c-7287-4689-a2f0-c73cf5407540", IsEnabled = true)]
		[RequireBotIsOwner]
		[RequireBotOwner]
		public sealed class DeleteGuild : AdvobotModuleBase
		{
			[Command]
			public Task Command()
				=> Context.Guild.DeleteAsync();
		}

		[LocalizedGroup(nameof(Groups.LeaveGuild))]
		[LocalizedAlias(nameof(Aliases.LeaveGuild))]
		[LocalizedSummary(nameof(Summaries.LeaveGuild))]
		[Meta("3090730c-1377-4a56-b379-485baed393e7", IsEnabled = true)]
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

		[LocalizedGroup(nameof(Groups.ModifyGuildAfkChannel))]
		[LocalizedAlias(nameof(Aliases.ModifyGuildAfkChannel))]
		[LocalizedSummary(nameof(Summaries.ModifyGuildAfkChannel))]
		[Meta("ec19c9b8-cc0c-46d6-a207-9eb6abf69c9e", IsEnabled = true)]
		[RequireGuildPermissions(GuildPermission.ManageGuild)]
		public sealed class ModifyGuildAfkChannel : AdvobotModuleBase
		{
			[Command]
			public async Task<RuntimeResult> Command(
				[CanModifyChannel(ManageChannels)] IVoiceChannel? channel)
			{
				await Context.Guild.ModifyAsync(x => x.AfkChannel = Optional.Create(channel), GenerateRequestOptions()).CAF();
				return Responses.Guilds.ModifiedAfkChannel(channel);
			}

			[LocalizedCommand(nameof(Groups.Remove))]
			[LocalizedAlias(nameof(Aliases.Remove))]
			public Task<RuntimeResult> Remove()
				=> Command(null);
		}

		[LocalizedGroup(nameof(Groups.ModifyGuildAfkTimer))]
		[LocalizedAlias(nameof(Aliases.ModifyGuildAfkTimer))]
		[LocalizedSummary(nameof(Summaries.ModifyGuildAfkTimer))]
		[Meta("bb4ceabc-2660-431d-aa0c-d0f6176b88a1", IsEnabled = true)]
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

		[LocalizedGroup(nameof(Groups.ModifyGuildExplicitContentFilter))]
		[LocalizedAlias(nameof(Aliases.ModifyGuildExplicitContentFilter))]
		[LocalizedSummary(nameof(Summaries.ModifyGuildExplicitContentFilter))]
		[Meta("a60b0a3c-a890-40c7-83f7-9856da3808fe", IsEnabled = true)]
		[RequireGuildPermissions(GuildPermission.ManageGuild)]
		public sealed class ModifyGuildExplicitContentFilter : AdvobotModuleBase
		{
			[Command]
			public async Task<RuntimeResult> Command(ExplicitContentFilterLevel filter)
			{
				await Context.Guild.ModifyAsync(x => x.ExplicitContentFilter = filter, GenerateRequestOptions()).CAF();
				return Responses.Guilds.ModifiedGuildContentFilter(filter);
			}
		}

		[LocalizedGroup(nameof(Groups.ModifyGuildIcon))]
		[LocalizedAlias(nameof(Aliases.ModifyGuildIcon))]
		[LocalizedSummary(nameof(Summaries.ModifyGuildIcon))]
		[Meta("c6f5c58e-4784-4f30-91a9-3727e580ddf2", IsEnabled = true)]
		[RequireGuildPermissions(GuildPermission.ManageGuild)]
		public sealed class ModifyGuildIcon : ImageResizerModule
		{
			[Command]
			public Task<RuntimeResult> Command(Uri url)
			{
				var position = Enqueue(new IconCreationContext(Context, url, null, "Guild Icon",
					(ctx, ms) => ctx.Guild.ModifyAsync(x => x.Icon = new Image(ms), ctx.GenerateRequestOptions())));
				return Responses.Snowflakes.EnqueuedIcon(Context.Guild, position);
			}

			[LocalizedCommand(nameof(Groups.Remove))]
			[LocalizedAlias(nameof(Aliases.Remove))]
			public async Task<RuntimeResult> Remove()
			{
				await Context.Guild.ModifyAsync(x => x.Icon = new Image(), GenerateRequestOptions()).CAF();
				return Responses.Snowflakes.RemovedIcon(Context.Guild);
			}
		}

		[LocalizedGroup(nameof(Groups.ModifyGuildMsgNotif))]
		[LocalizedAlias(nameof(Aliases.ModifyGuildMsgNotif))]
		[LocalizedSummary(nameof(Summaries.ModifyGuildMsgNotif))]
		[Meta("f32a347c-e2d8-4b64-9a3c-53cd46d0f3ed", IsEnabled = true)]
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

		[LocalizedGroup(nameof(Groups.ModifyGuildName))]
		[LocalizedAlias(nameof(Aliases.ModifyGuildName))]
		[LocalizedSummary(nameof(Summaries.ModifyGuildName))]
		[Meta("54a75c3c-be5a-46d4-93d3-b1cbc1af9def", IsEnabled = true)]
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

		[LocalizedGroup(nameof(Groups.ModifyGuildRegion))]
		[LocalizedAlias(nameof(Aliases.ModifyGuildRegion))]
		[LocalizedSummary(nameof(Summaries.ModifyGuildRegion))]
		[Meta("e2ab50b0-ce20-48ee-bac9-1fac2e53387d", IsEnabled = true)]
		[RequireGuildPermissions(GuildPermission.ManageGuild)]
		public sealed class ModifyGuildRegion : AdvobotModuleBase
		{
			[Command]
			public async Task<RuntimeResult> Command(IVoiceRegion region)
			{
				await Context.Guild.ModifyAsync(x => x.Region = Optional.Create(region), GenerateRequestOptions()).CAF();
				return Responses.Guilds.ModifiedRegion(region);
			}

			[LocalizedCommand(nameof(Groups.Show))]
			[LocalizedAlias(nameof(Aliases.Show))]
			[Priority(1)]
			public async Task<RuntimeResult> Show()
			{
				var regions = await Context.Guild.GetVoiceRegionsAsync().CAF();
				return Responses.Guilds.DisplayRegions(regions);
			}
		}

		[LocalizedGroup(nameof(Groups.ModifyGuildSplash))]
		[LocalizedAlias(nameof(Aliases.ModifyGuildSplash))]
		[LocalizedSummary(nameof(Summaries.ModifyGuildSplash))]
		[Meta("0a02898e-0e5c-417d-9309-7f93714b61f7", IsEnabled = true)]
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

			[LocalizedCommand(nameof(Groups.Remove))]
			[LocalizedAlias(nameof(Aliases.Remove))]
			public async Task<RuntimeResult> Remove()
			{
				await Context.Guild.ModifyAsync(x => x.Splash = new Image(), GenerateRequestOptions()).CAF();
				return Responses.Guilds.RemovedSplash();
			}
		}

		[LocalizedGroup(nameof(Groups.ModifyGuildSystemChannel))]
		[LocalizedAlias(nameof(Aliases.ModifyGuildSystemChannel))]
		[LocalizedSummary(nameof(Summaries.ModifyGuildSystemChannel))]
		[Meta("f6cc90d9-ae1d-4eab-ae15-226d88e42092", IsEnabled = true)]
		[RequireGuildPermissions(GuildPermission.ManageGuild)]
		public sealed class ModifyGuildSystemChannel : AdvobotModuleBase
		{
			[Command]
			public async Task<RuntimeResult> Command(
				[CanModifyChannel(ManageChannels)] ITextChannel? channel)
			{
				await Context.Guild.ModifyAsync(x => x.SystemChannel = Optional.Create(channel), GenerateRequestOptions()).CAF();
				return Responses.Guilds.ModifiedSystemChannel(channel);
			}

			[LocalizedCommand(nameof(Groups.MessageBoost))]
			[LocalizedAlias(nameof(Aliases.MessageBoost))]
			public async Task<RuntimeResult> MessageBoost(bool enable)
			{
				const SystemChannelMessageDeny FLAG = SystemChannelMessageDeny.GuildBoost;
				await Context.Guild.ModifySystemChannelFlags(FLAG, enable, GenerateRequestOptions()).CAF();
				return Responses.Guilds.ModifySystemMessageBoost(enable);
			}

			[LocalizedCommand(nameof(Groups.MessageWelcome))]
			[LocalizedAlias(nameof(Aliases.MessageWelcome))]
			public async Task<RuntimeResult> MessageWelcome(bool enable)
			{
				const SystemChannelMessageDeny FLAG = SystemChannelMessageDeny.WelcomeMessage;
				await Context.Guild.ModifySystemChannelFlags(FLAG, enable, GenerateRequestOptions()).CAF();
				return Responses.Guilds.ModifySystemMessageWelcome(enable);
			}

			[LocalizedCommand(nameof(Groups.Remove))]
			[LocalizedAlias(nameof(Aliases.Remove))]
			public Task<RuntimeResult> Remove()
				=> Command(null);
		}

		[LocalizedGroup(nameof(Groups.ModifyGuildVerif))]
		[LocalizedAlias(nameof(Aliases.ModifyGuildVerif))]
		[LocalizedSummary(nameof(Summaries.ModifyGuildVerif))]
		[Meta("5640e2e6-7ee8-416c-982c-9efb22634f54", IsEnabled = true)]
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

		[LocalizedGroup(nameof(Groups.SwapGuildOwner))]
		[LocalizedAlias(nameof(Aliases.SwapGuildOwner))]
		[LocalizedSummary(nameof(Summaries.SwapGuildOwner))]
		[Meta("3cb8a267-5d62-4644-b5af-60281dd8e182", IsEnabled = true)]
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
	}
}