using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

using Advobot.Attributes;
using Advobot.Attributes.ParameterPreconditions.DiscordObjectValidation.Roles;
using Advobot.Attributes.ParameterPreconditions.Numbers;
using Advobot.Attributes.ParameterPreconditions.Strings;
using Advobot.Attributes.Preconditions.Permissions;
using Advobot.Modules;
using Advobot.Standard.Localization;
using Advobot.Standard.Resources;
using Advobot.TypeReaders;
using Advobot.Utilities;

using AdvorangesUtils;

using Discord;
using Discord.Commands;

namespace Advobot.Standard.Commands
{
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
				await role.ModifyAsync(x => x.Permissions = new GuildPermissions(immovable), GenerateRequestOptions()).CAF();
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
				[CanModifyRole] IRole output)
			{
				//Perms which the user can copy from the input role
				var copyable = input.Permissions.RawValue & Context.User.GuildPermissions.RawValue;
				//Output perms which can't be modified by the user
				var immovable = output.Permissions.RawValue & ~Context.User.GuildPermissions.RawValue;
				var permissions = immovable | copyable;

				await output.ModifyAsync(x => x.Permissions = new GuildPermissions(permissions), GenerateRequestOptions()).CAF();
				return Responses.Roles.CopiedPermissions(input, output, (GuildPermission)copyable);
			}
		}

		[LocalizedGroup(nameof(Groups.CreateRole))]
		[LocalizedAlias(nameof(Aliases.CreateRole))]
		[LocalizedSummary(nameof(Summaries.CreateRole))]
		[Meta("15f6ac1f-8975-42c4-ba19-8fc8e6a5e4cb", IsEnabled = true)]
		[RequireGuildPermissions(GuildPermission.ManageRoles)]
		public sealed class CreateRole : AdvobotModuleBase
		{
			[Command]
			public async Task<RuntimeResult> Command([RoleName] string name)
			{
				var role = await Context.Guild.CreateRoleAsync(name, new GuildPermissions(0), null, false, GenerateRequestOptions()).CAF();
				return Responses.Snowflakes.Created(role);
			}
		}

		[LocalizedGroup(nameof(Groups.DeleteRole))]
		[LocalizedAlias(nameof(Aliases.DeleteRole))]
		[LocalizedSummary(nameof(Summaries.DeleteRole))]
		[Meta("280c2d19-d045-4b01-a1a1-2749e183b4b4", IsEnabled = true)]
		[RequireGuildPermissions(GuildPermission.ManageRoles)]
		public sealed class DeleteRole : AdvobotModuleBase
		{
			[Command]
			public async Task<RuntimeResult> Command(
				[CanModifyRole, NotManaged, NotEveryone] IRole role)
			{
				await role.DeleteAsync(GenerateRequestOptions()).CAF();
				return Responses.Snowflakes.Deleted(role);
			}
		}

		[LocalizedGroup(nameof(Groups.DisplayRolePerms))]
		[LocalizedAlias(nameof(Aliases.DisplayRolePerms))]
		[LocalizedSummary(nameof(Summaries.DisplayRolePerms))]
		[Meta("429c4817-9b92-4b6e-8525-f0b94690fb6f", IsEnabled = true)]
		[RequireGuildPermissions(GuildPermission.ManageRoles)]
		public sealed class DisplayRolePerms : AdvobotModuleBase
		{
			/* TODO: reimplement localized
			[Command]
			public Task<RuntimeResult> Command()
				=> Responses.Gets.ShowEnumValues(typeof(GuildPermission));*/

			[Command]
			public Task<RuntimeResult> Command(IRole role)
				=> Responses.Roles.DisplayPermissions(role);
		}

		[LocalizedGroup(nameof(Groups.DisplayRolePositions))]
		[LocalizedAlias(nameof(Aliases.DisplayRolePositions))]
		[LocalizedSummary(nameof(Summaries.DisplayRolePositions))]
		[Meta("f27c560c-6814-42a1-90aa-3dcdc1db0855", IsEnabled = true)]
		[RequireGuildPermissions(GuildPermission.ManageRoles)]
		public sealed class DisplayRolePositions : AdvobotModuleBase
		{
			[Command]
			public Task<RuntimeResult> Command()
				=> Responses.Roles.Display(Context.Guild.Roles.OrderByDescending(x => x.Position));
		}

		[LocalizedGroup(nameof(Groups.GiveRole))]
		[LocalizedAlias(nameof(Aliases.GiveRole))]
		[LocalizedSummary(nameof(Summaries.GiveRole))]
		[Meta("3920cfd0-e4cd-474a-b2b6-aa65b1f52804", IsEnabled = true)]
		[RequireGuildPermissions(GuildPermission.ManageRoles)]
		public sealed class GiveRole : AdvobotModuleBase
		{
			[Command]
			public async Task<RuntimeResult> Command(
				IGuildUser user,
				[CanModifyRole, NotManaged, NotEveryone] params IRole[] roles)
			{
				await user.AddRolesAsync(roles, GenerateRequestOptions()).CAF();
				return Responses.Roles.Gave(roles, user);
			}
		}

		[LocalizedGroup(nameof(Groups.ModifyRoleColor))]
		[LocalizedAlias(nameof(Aliases.ModifyRoleColor))]
		[LocalizedSummary(nameof(Summaries.ModifyRoleColor))]
		[Meta("41f8a8df-ac9f-48af-aec2-b82b242dcd9b", IsEnabled = true)]
		[RequireGuildPermissions(GuildPermission.ManageRoles)]
		public sealed class ModifyRoleColor : AdvobotModuleBase
		{
			[Command]
			public async Task<RuntimeResult> Command(
				[CanModifyRole]
				IRole role,
				Color color = default)
			{
				await role.ModifyAsync(x => x.Color = color, GenerateRequestOptions()).CAF();
				return Responses.Roles.ModifiedColor(role, color);
			}
		}

		[LocalizedGroup(nameof(Groups.ModifyRoleHoist))]
		[LocalizedAlias(nameof(Aliases.ModifyRoleHoist))]
		[LocalizedSummary(nameof(Summaries.ModifyRoleHoist))]
		[Meta("7bf99434-abb9-443b-a420-5b91bf73834e", IsEnabled = true)]
		[RequireGuildPermissions(GuildPermission.ManageRoles)]
		public sealed class ModifyRoleHoist : AdvobotModuleBase
		{
			[Command]
			public async Task<RuntimeResult> Command([CanModifyRole] IRole role)
			{
				var hoisted = !role.IsHoisted;
				await role.ModifyAsync(x => x.Hoist = !role.IsHoisted, GenerateRequestOptions()).CAF();
				return Responses.Roles.ModifiedHoistStatus(role, hoisted);
			}
		}

		[LocalizedGroup(nameof(Groups.ModifyRoleMentionability))]
		[LocalizedAlias(nameof(Aliases.ModifyRoleMentionability))]
		[LocalizedSummary(nameof(Summaries.ModifyRoleMentionability))]
		[Meta("847e8d15-df9c-41a7-87d0-4f5570044a3d", IsEnabled = true)]
		[RequireGuildPermissions(GuildPermission.ManageRoles)]
		public sealed class ModifyRoleMentionability : AdvobotModuleBase
		{
			[Command]
			public async Task<RuntimeResult> Command([CanModifyRole] IRole role)
			{
				var mentionability = !role.IsMentionable;
				await role.ModifyAsync(x => x.Mentionable = !role.IsMentionable, GenerateRequestOptions()).CAF();
				return Responses.Roles.ModifiedMentionability(role, mentionability);
			}
		}

		[LocalizedGroup(nameof(Groups.ModifyRoleName))]
		[LocalizedAlias(nameof(Aliases.ModifyRoleName))]
		[LocalizedSummary(nameof(Summaries.ModifyRoleName))]
		[Meta("c1b6a365-c66c-4485-bbae-e9d75f507440", IsEnabled = true)]
		[RequireGuildPermissions(GuildPermission.ManageRoles)]
		public sealed class ModifyRoleName : AdvobotModuleBase
		{
			[Command, Priority(1)]
			public async Task<RuntimeResult> Command(
				[CanModifyRole] IRole role,
				[Remainder, RoleName] string name)
			{
				await role.ModifyAsync(x => x.Name = name, GenerateRequestOptions()).CAF();
				return Responses.Snowflakes.ModifiedName(role, name);
			}

			[LocalizedCommand(nameof(Groups.Position))]
			public Task<RuntimeResult> Position(
				[CanModifyRole, OverrideTypeReader(typeof(RolePositionTypeReader))] IRole role,
				[Remainder, RoleName] string name)
				=> Command(role, name);
		}

		[LocalizedGroup(nameof(Groups.ModifyRolePerms))]
		[LocalizedAlias(nameof(Aliases.ModifyRolePerms))]
		[LocalizedSummary(nameof(Summaries.ModifyRolePerms))]
		[Meta("dcfbf444-9e48-4f72-b137-be1e5e25a934", IsEnabled = true)]
		[RequireGuildPermissions(GuildPermission.ManageRoles)]
		public sealed class ModifyRolePerms : AdvobotModuleBase
		{
			[Command]
			public async Task<RuntimeResult> Command(
				[CanModifyRole]
				IRole role,
				bool allow,
				[Remainder]
				[OverrideTypeReader(typeof(PermissionsTypeReader<GuildPermission>))]
				ulong permissions
			)
			{
				//Only modify permissions the user has the ability to
				permissions &= Context.User.GuildPermissions.RawValue;

				var rolePermissions = allow
					? role.Permissions.RawValue | permissions
					: role.Permissions.RawValue & ~permissions;
				await role.ModifyAsync(x => x.Permissions = new GuildPermissions(rolePermissions), GenerateRequestOptions()).CAF();
				return Responses.Roles.ModifiedPermissions(role, (GuildPermission)permissions, allow);
			}
		}

		[LocalizedGroup(nameof(Groups.ModifyRolePosition))]
		[LocalizedAlias(nameof(Aliases.ModifyRolePosition))]
		[LocalizedSummary(nameof(Summaries.ModifyRolePosition))]
		[Meta("efb2d8e5-b5d5-4c77-b0f6-66b9c378080d", IsEnabled = true)]
		[RequireGuildPermissions(GuildPermission.ManageRoles)]
		public sealed class ModifyRolePosition : AdvobotModuleBase
		{
			[Command]
			public async Task<RuntimeResult> Command(
				[CanModifyRole] IRole role,
				[Positive] int position)
			{
				var pos = await DiscordUtils.ModifyRolePositionAsync(role, position, GenerateRequestOptions()).CAF();
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
				[CanModifyRole, NotManaged, NotEveryone] IRole role)
			{
				await role.DeleteAsync(GenerateRequestOptions()).CAF();
				await Context.Guild.CreateRoleAsync(role.Name, role.Permissions, role.Color, false, GenerateRequestOptions()).CAF();
				await DiscordUtils.ModifyRolePositionAsync(role, role.Position, GenerateRequestOptions()).CAF();
				return Responses.Snowflakes.SoftDeleted(role);
			}
		}

		[LocalizedGroup(nameof(Groups.TakeRole))]
		[LocalizedAlias(nameof(Aliases.TakeRole))]
		[LocalizedSummary(nameof(Summaries.TakeRole))]
		[Meta("ab678dd2-4108-49b2-8b6f-7e21c6cb8fda", IsEnabled = true)]
		[RequireGuildPermissions(GuildPermission.ManageRoles)]
		public sealed class TakeRole : AdvobotModuleBase
		{
			[Command]
			public async Task<RuntimeResult> Command(
				IGuildUser user,
				[CanModifyRole, NotManaged, NotEveryone] params IRole[] roles)
			{
				await user.RemoveRolesAsync(roles, GenerateRequestOptions()).CAF();
				return Responses.Roles.Took(roles, user);
			}
		}
	}
}