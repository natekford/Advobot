using Advobot.Attributes;
using Advobot.Localization;
using Advobot.Modules;
using Advobot.ParameterPreconditions.Discord.Channels;
using Advobot.Preconditions.Permissions;
using Advobot.Resources;
using Advobot.Utilities;

using Discord;
using Discord.Commands;

using static Discord.ChannelPermission;

namespace Advobot.Standard.Commands;

[Category(nameof(Channels))]
public sealed class Channels : ModuleBase
{
	// On mobile this has to be done 1 by 1
	[LocalizedGroup(nameof(Groups.ClearChannelPerms))]
	[LocalizedAlias(nameof(Aliases.ClearChannelPerms))]
	[LocalizedSummary(nameof(Summaries.ClearChannelPerms))]
	[Meta("5710430c-ce62-4474-9296-071eca65c9b1", IsEnabled = true)]
	[RequireGuildPermissions(GuildPermission.ManageChannels | GuildPermission.ManageRoles)]
	public sealed class ClearChannelPerms : AdvobotModuleBase
	{
		[Command]
		public async Task<RuntimeResult> Command(
			[CanModifyChannel(ManageChannels | ManageRoles)]
			IGuildChannel channel
		)
		{
			var count = await channel.ClearOverwritesAsync(GetOptions()).ConfigureAwait(false);
			return Responses.Channels.ClearedOverwrites(channel, count);
		}
	}

	// Can copy an entire channel on mobile, but not individual overwrites
	[LocalizedGroup(nameof(Groups.CopyChannelPerms))]
	[LocalizedAlias(nameof(Aliases.CopyChannelPerms))]
	[LocalizedSummary(nameof(Summaries.CopyChannelPerms))]
	[Meta("621f61a8-f3ba-41d1-b9b8-9e2075bcfa11", IsEnabled = true)]
	[RequireGuildPermissions(GuildPermission.ManageChannels | GuildPermission.ManageRoles)]
	public sealed class CopyChannelPerms : AdvobotModuleBase
	{
		[Command]
		public Task<RuntimeResult> Command(
			[CanModifyChannel(ManageChannels | ManageRoles)]
			IGuildChannel input,
			[CanModifyChannel(ManageChannels | ManageRoles)]
			IGuildChannel output
		) => CommandRunner(input, output, default(IGuildUser));

		[Command]
		public Task<RuntimeResult> Command(
			[CanModifyChannel(ManageChannels | ManageRoles)]
			IGuildChannel input,
			[CanModifyChannel(ManageChannels | ManageRoles)]
			IGuildChannel output,
			IRole role
		) => CommandRunner(input, output, role);

		[Command]
		public Task<RuntimeResult> Command(
			[CanModifyChannel(ManageChannels | ManageRoles)]
			IGuildChannel input,
			[CanModifyChannel(ManageChannels | ManageRoles)]
			IGuildChannel output,
			IGuildUser user
		) => CommandRunner(input, output, user);

		private async Task<RuntimeResult> CommandRunner(
			IGuildChannel input,
			IGuildChannel output,
			ISnowflakeEntity? obj
		)
		{
			// Make sure channels are the same type
			if (input.GetType() != output.GetType())
			{
				return Responses.Channels.MismatchType(input, output);
			}

			var overwrites = await input.CopyOverwritesAsync(output, obj?.Id, GetOptions()).ConfigureAwait(false);
			return Responses.Channels.CopiedOverwrites(input, output, obj, overwrites);
		}
	}
}