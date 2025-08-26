using Advobot.Attributes;
using Advobot.Localization;
using Advobot.Modules;
using Advobot.ParameterPreconditions.Discord.Roles;
using Advobot.ParameterPreconditions.Numbers;
using Advobot.Preconditions.Permissions;
using Advobot.Resources;
using Advobot.Utilities;

using Discord;
using Discord.Commands;

namespace Advobot.Standard.Commands;

[Category(nameof(Roles))]
public sealed class Roles : ModuleBase
{
	[LocalizedGroup(nameof(Groups.ClearRolePerms))]
	[LocalizedAlias(nameof(Aliases.ClearRolePerms))]
	[LocalizedSummary(nameof(Summaries.ClearRolePerms))]
	[Meta("bb5e3639-7287-45d4-a3fe-22359dd25073", IsEnabled = true)]
	[RequireGuildPermissions(GuildPermission.ManageRoles)]
	public sealed class ClearRolePerms : AdvobotModuleBase
	{
		[Command]
		public async Task<RuntimeResult> Command([CanModifyRole] IRole role)
		{
			var immovable = role.Permissions.RawValue & ~Context.User.GuildPermissions.RawValue;
			await role.ModifyAsync(x => x.Permissions = new GuildPermissions(immovable), GetOptions()).ConfigureAwait(false);
			return Responses.Roles.ClearedPermissions(role);
		}
	}

	[LocalizedGroup(nameof(Groups.CopyRolePerms))]
	[LocalizedAlias(nameof(Aliases.CopyRolePerms))]
	[LocalizedSummary(nameof(Summaries.CopyRolePerms))]
	[Meta("bbf7898b-fcb6-4c04-a04a-f343fa129008", IsEnabled = true)]
	[RequireGuildPermissions(GuildPermission.ManageRoles)]
	public sealed class CopyRolePerms : AdvobotModuleBase
	{
		[Command]
		public async Task<RuntimeResult> Command(
			IRole input,
			[CanModifyRole]
			IRole output)
		{
			//Perms which the user can copy from the input role
			var copyable = input.Permissions.RawValue & Context.User.GuildPermissions.RawValue;
			//Output perms which can't be modified by the user
			var immovable = output.Permissions.RawValue & ~Context.User.GuildPermissions.RawValue;
			var permissions = immovable | copyable;

			await output.ModifyAsync(x => x.Permissions = new GuildPermissions(permissions), GetOptions()).ConfigureAwait(false);
			return Responses.Roles.CopiedPermissions(input, output, (GuildPermission)copyable);
		}
	}

	// Moving roles on mobile sucks
	[LocalizedGroup(nameof(Groups.ModifyRolePosition))]
	[LocalizedAlias(nameof(Aliases.ModifyRolePosition))]
	[LocalizedSummary(nameof(Summaries.ModifyRolePosition))]
	[Meta("efb2d8e5-b5d5-4c77-b0f6-66b9c378080d", IsEnabled = true)]
	[RequireGuildPermissions(GuildPermission.ManageRoles)]
	public sealed class ModifyRolePosition : AdvobotModuleBase
	{
		[Command]
		public async Task<RuntimeResult> Command(
			[CanModifyRole]
			IRole role,
			[Positive]
			int position)
		{
			var pos = await role.ModifyRolePositionAsync(position, GetOptions()).ConfigureAwait(false);
			return Responses.Roles.Moved(role, pos);
		}
	}

	[LocalizedGroup(nameof(Groups.SoftDeleteRole))]
	[LocalizedAlias(nameof(Aliases.SoftDeleteRole))]
	[LocalizedSummary(nameof(Summaries.SoftDeleteRole))]
	[Meta("4cecc4b9-9d25-44d2-9de3-3b5fe5bd33c5", IsEnabled = true)]
	[RequireGuildPermissions(GuildPermission.ManageRoles)]
	public sealed class SoftDeleteRole : AdvobotModuleBase
	{
		[Command]
		public async Task<RuntimeResult> Command(
			[CanModifyRole, NotManaged, NotEveryone]
			IRole role)
		{
			await role.DeleteAsync(GetOptions()).ConfigureAwait(false);
			var copy = await Context.Guild.CreateRoleAsync(
				name: role.Name,
				permissions: role.Permissions,
				color: role.Color,
				isHoisted: role.IsHoisted,
				isMentionable: role.IsMentionable,
				options: GetOptions()
			).ConfigureAwait(false);
			await copy.ModifyRolePositionAsync(role.Position, GetOptions()).ConfigureAwait(false);
			return Responses.Snowflakes.SoftDeleted(role);
		}
	}
}