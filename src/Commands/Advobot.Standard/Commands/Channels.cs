using Advobot.Attributes;
using Advobot.Modules;
using Advobot.ParameterPreconditions.Discord.Channels;
using Advobot.Preconditions.Permissions;
using Advobot.Resources;
using Advobot.Utilities;

using Discord;

using YACCS.Commands.Attributes;
using YACCS.Localization;

using static Discord.ChannelPermission;

namespace Advobot.Standard.Commands;

[LocalizedCategory(nameof(Channels))]
public sealed class Channels : AdvobotModuleBase
{
	// On mobile this has to be done 1 by 1
	[LocalizedCommand(nameof(Groups.ClearChannelPerms), nameof(Aliases.ClearChannelPerms))]
	[LocalizedSummary(nameof(Summaries.ClearChannelPerms))]
	[Id("5710430c-ce62-4474-9296-071eca65c9b1")]
	[Meta(IsEnabled = true)]
	[RequireGuildPermissions(GuildPermission.ManageChannels | GuildPermission.ManageRoles)]
	public sealed class ClearChannelPerms : AdvobotModuleBase
	{
		[LocalizedCommand]
		public async Task<AdvobotResult> Command(
			[CanModifyChannel(ManageChannels | ManageRoles)]
			IGuildChannel channel
		)
		{
			var count = await channel.ClearOverwritesAsync(GetOptions()).ConfigureAwait(false);
			return Responses.Channels.ClearedOverwrites(channel, count);
		}
	}

	// Can copy an entire channel on mobile, but not individual overwrites
	[LocalizedCommand(nameof(Groups.CopyChannelPerms), nameof(Aliases.CopyChannelPerms))]
	[LocalizedSummary(nameof(Summaries.CopyChannelPerms))]
	[Id("621f61a8-f3ba-41d1-b9b8-9e2075bcfa11")]
	[Meta(IsEnabled = true)]
	[RequireGuildPermissions(GuildPermission.ManageChannels | GuildPermission.ManageRoles)]
	public sealed class CopyChannelPerms : AdvobotModuleBase
	{
		[LocalizedCommand]
		public Task<AdvobotResult> Command(
			[CanModifyChannel(ManageChannels | ManageRoles)]
			IGuildChannel input,
			[CanModifyChannel(ManageChannels | ManageRoles)]
			IGuildChannel output
		) => CommandRunner(input, output, default(IGuildUser));

		[LocalizedCommand]
		public Task<AdvobotResult> Command(
			[CanModifyChannel(ManageChannels | ManageRoles)]
			IGuildChannel input,
			[CanModifyChannel(ManageChannels | ManageRoles)]
			IGuildChannel output,
			IRole role
		) => CommandRunner(input, output, role);

		[LocalizedCommand]
		public Task<AdvobotResult> Command(
			[CanModifyChannel(ManageChannels | ManageRoles)]
			IGuildChannel input,
			[CanModifyChannel(ManageChannels | ManageRoles)]
			IGuildChannel output,
			IGuildUser user
		) => CommandRunner(input, output, user);

		private async Task<AdvobotResult> CommandRunner(
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