using Advobot.Attributes;
using Advobot.Modules;
using Advobot.ParameterPreconditions.Discord.Channels;
using Advobot.Preconditions.Permissions;
using Advobot.Resources;
using Advobot.Utilities;

using Discord;

using YACCS.Commands.Attributes;
using YACCS.Localization;

namespace Advobot.Standard.Commands;

[LocalizedCategory(nameof(Names.ChannelsCategory))]
public sealed class Channels : AdvobotModuleBase
{
	public const ChannelPermission ManageChannelPermissions = 0
		| ChannelPermission.ManageChannels
		| ChannelPermission.ManageRoles;

	// On mobile this has to be done 1 by 1
	[LocalizedCommand(nameof(Names.ClearChannelPerms), nameof(Names.ClearChannelPermsAlias))]
	[LocalizedSummary(nameof(Summaries.ClearChannelPermsSummary))]
	[Meta("5710430c-ce62-4474-9296-071eca65c9b1", IsEnabled = true)]
	[RequireGuildPermissions(GuildPermission.ManageChannels | GuildPermission.ManageRoles)]
	public sealed class ClearChannelPerms : AdvobotModuleBase
	{
		[Command]
		public async Task<AdvobotResult> All(
			[CanModifyChannel(ManageChannelPermissions)]
			IGuildChannel channel
		)
		{
			var count = await channel.ClearOverwritesAsync(GetOptions()).ConfigureAwait(false);
			return Responses.Channels.ClearedOverwrites(channel, count);
		}
	}

	// Can copy an entire channel on mobile, but not individual overwrites
	[LocalizedCommand(nameof(Names.CopyChannelPerms), nameof(Names.CopyChannelPermsAlias))]
	[LocalizedSummary(nameof(Summaries.CopyChannelPermsSummary))]
	[Meta("621f61a8-f3ba-41d1-b9b8-9e2075bcfa11", IsEnabled = true)]
	[RequireGuildPermissions(GuildPermission.ManageChannels | GuildPermission.ManageRoles)]
	public sealed class CopyChannelPerms : AdvobotModuleBase
	{
		[Command]
		public Task<AdvobotResult> All(
			[CanModifyChannel(ManageChannelPermissions)]
			IGuildChannel input,
			[CanModifyChannel(ManageChannelPermissions)]
			IGuildChannel output
		) => CopyAsync(input, output, default);

		[Command]
		public Task<AdvobotResult> Role(
			[CanModifyChannel(ManageChannelPermissions)]
			IGuildChannel input,
			[CanModifyChannel(ManageChannelPermissions)]
			IGuildChannel output,
			IRole role
		) => CopyAsync(input, output, role);

		[Command]
		public Task<AdvobotResult> User(
			[CanModifyChannel(ManageChannelPermissions)]
			IGuildChannel input,
			[CanModifyChannel(ManageChannelPermissions)]
			IGuildChannel output,
			IGuildUser user
		) => CopyAsync(input, output, user);

		private async Task<AdvobotResult> CopyAsync(
			IGuildChannel input,
			IGuildChannel output,
			ISnowflakeEntity? entity
		)
		{
			// Make sure channels are the same type
			if (input.GetType() != output.GetType())
			{
				return Responses.Channels.MismatchType(input, output);
			}

			var overwrites = await input.CopyOverwritesAsync(output, entity?.Id, GetOptions()).ConfigureAwait(false);
			return Responses.Channels.CopiedOverwrites(input, output, entity, overwrites);
		}
	}
}