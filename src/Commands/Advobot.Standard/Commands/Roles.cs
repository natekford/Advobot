using Advobot.Attributes;
using Advobot.Modules;
using Advobot.ParameterPreconditions.Discord.Roles;
using Advobot.ParameterPreconditions.Numbers;
using Advobot.Preconditions.Permissions;
using Advobot.Resources;
using Advobot.Utilities;

using Discord;

using YACCS.Commands.Attributes;
using YACCS.Localization;

namespace Advobot.Standard.Commands;

[LocalizedCategory(nameof(Names.RolesCategory))]
public sealed class Roles
{
	[Command(nameof(Names.ClearRolePerms), nameof(Names.ClearRolePermsAlias))]
	[LocalizedSummary(nameof(Summaries.ClearRolePermsSummary))]
	[Meta("bb5e3639-7287-45d4-a3fe-22359dd25073", IsEnabled = true)]
	[RequireGuildPermissions(GuildPermission.ManageRoles)]
	public sealed class ClearRolePerms : AdvobotModuleBase
	{
		[Command]
		public async Task<AdvobotResult> All([CanModifyRole] IRole role)
		{
			var immovable = role.Permissions.RawValue & ~Context.User.GuildPermissions.RawValue;
			await role.ModifyAsync(x => x.Permissions = new GuildPermissions(immovable), GetOptions()).ConfigureAwait(false);
			return Responses.Roles.ClearedPermissions(role);
		}
	}

	[Command(nameof(Names.CopyRolePerms), nameof(Names.CopyRolePermsAlias))]
	[LocalizedSummary(nameof(Summaries.CopyRolePermsSummary))]
	[Meta("bbf7898b-fcb6-4c04-a04a-f343fa129008", IsEnabled = true)]
	[RequireGuildPermissions(GuildPermission.ManageRoles)]
	public sealed class CopyRolePerms : AdvobotModuleBase
	{
		[Command]
		public async Task<AdvobotResult> All(
			IRole input,
			[CanModifyRole]
			IRole output)
		{
			// Perms that the user can copy from the input role
			var copyable = input.Permissions.RawValue & Context.User.GuildPermissions.RawValue;
			// Perms that can't be modified by the user
			var immovable = output.Permissions.RawValue & ~Context.User.GuildPermissions.RawValue;
			var permissions = immovable | copyable;

			await output.ModifyAsync(x => x.Permissions = new GuildPermissions(permissions), GetOptions()).ConfigureAwait(false);
			return Responses.Roles.CopiedPermissions(input, output, (GuildPermission)copyable);
		}
	}

	[Command(nameof(Names.SoftDeleteRole), nameof(Names.SoftDeleteRoleAlias))]
	[LocalizedSummary(nameof(Summaries.SoftDeleteRoleSummary))]
	[Meta("4cecc4b9-9d25-44d2-9de3-3b5fe5bd33c5", IsEnabled = true)]
	[RequireGuildPermissions(GuildPermission.ManageRoles)]
	public sealed class SoftDeleteRole : AdvobotModuleBase
	{
		[Command]
		public async Task<AdvobotResult> Targeted(
			[CanModifyRole]
			[NotManaged]
			[NotEveryone]
			IRole role)
		{
			var position = role.Position;
			await role.DeleteAsync(GetOptions()).ConfigureAwait(false);
			var copy = await Context.Guild.CreateRoleAsync(
				name: role.Name,
				permissions: role.Permissions,
				color: role.Color,
				isHoisted: role.IsHoisted,
				isMentionable: role.IsMentionable,
				options: GetOptions()
			).ConfigureAwait(false);
			await copy.ModifyAsync(x => x.Position = role.Position, GetOptions()).ConfigureAwait(false);
			return Responses.Snowflakes.SoftDeleted(role);
		}
	}
}