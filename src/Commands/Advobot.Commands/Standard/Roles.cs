using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Advobot.Attributes;
using Advobot.Attributes.ParameterPreconditions.DiscordObjectValidation.Roles;
using Advobot.Attributes.ParameterPreconditions.Numbers;
using Advobot.Attributes.ParameterPreconditions.Strings;
using Advobot.Attributes.Preconditions.Permissions;
using Advobot.Commands.Localization;
using Advobot.Commands.Resources;
using Advobot.Modules;
using Advobot.TypeReaders;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Discord.Commands;

namespace Advobot.Commands.Standard
{
	public sealed class Roles : ModuleBase
	{
		[Group(nameof(GiveRole)), ModuleInitialismAlias(typeof(GiveRole))]
		[LocalizedSummary(nameof(Summaries.GiveRole))]
		[GuildPermissionRequirement(GuildPermission.ManageRoles)]
		[EnabledByDefault(true)]
		public sealed class GiveRole : AdvobotModuleBase
		{
			[Command]
			public async Task<RuntimeResult> Command(
				IGuildUser user,
				[Role] params IRole[] roles)
			{
				await user.AddRolesAsync(roles, GenerateRequestOptions()).CAF();
				return Responses.Roles.Gave(roles, user);
			}
		}

		[Group(nameof(TakeRole)), ModuleInitialismAlias(typeof(TakeRole))]
		[LocalizedSummary(nameof(Summaries.TakeRole))]
		[GuildPermissionRequirement(GuildPermission.ManageRoles)]
		[EnabledByDefault(true)]
		public sealed class TakeRole : AdvobotModuleBase
		{
			[Command]
			public async Task<RuntimeResult> Command(
				IGuildUser user,
				[Role] params IRole[] roles)
			{
				await user.RemoveRolesAsync(roles, GenerateRequestOptions()).CAF();
				return Responses.Roles.Took(roles, user);
			}
		}

		[Group(nameof(CreateRole)), ModuleInitialismAlias(typeof(CreateRole))]
		[LocalizedSummary(nameof(Summaries.CreateRole))]
		[GuildPermissionRequirement(GuildPermission.ManageRoles)]
		[EnabledByDefault(true)]
		public sealed class CreateRole : AdvobotModuleBase
		{
			[Command]
			public async Task<RuntimeResult> Command([RoleName] string name)
			{
				var role = await Context.Guild.CreateRoleAsync(name, new GuildPermissions(0), null, false, GenerateRequestOptions()).CAF();
				return Responses.Snowflakes.Created(role);
			}
		}

		[Group(nameof(SoftDeleteRole)), ModuleInitialismAlias(typeof(SoftDeleteRole))]
		[LocalizedSummary(nameof(Summaries.SoftDeleteRole))]
		[GuildPermissionRequirement(GuildPermission.ManageRoles)]
		[EnabledByDefault(true)]
		public sealed class SoftDeleteRole : AdvobotModuleBase
		{
			[Command]
			public async Task<RuntimeResult> Command([NotEveryoneOrManaged] IRole role)
			{
				await role.DeleteAsync(GenerateRequestOptions()).CAF();
				await Context.Guild.CreateRoleAsync(role.Name, role.Permissions, role.Color, false, GenerateRequestOptions()).CAF();
				await DiscordUtils.ModifyRolePositionAsync(role, role.Position, GenerateRequestOptions()).CAF();
				return Responses.Snowflakes.SoftDeleted(role);
			}
		}

		[Group(nameof(DeleteRole)), ModuleInitialismAlias(typeof(DeleteRole))]
		[LocalizedSummary(nameof(Summaries.DeleteRole))]
		[GuildPermissionRequirement(GuildPermission.ManageRoles)]
		[EnabledByDefault(true)]
		public sealed class DeleteRole : AdvobotModuleBase
		{
			[Command]
			public async Task<RuntimeResult> Command([NotEveryoneOrManaged] IRole role)
			{
				await role.DeleteAsync(GenerateRequestOptions()).CAF();
				return Responses.Snowflakes.Deleted(role);
			}
		}

		[Group(nameof(DisplayRolePositions)), ModuleInitialismAlias(typeof(DisplayRolePositions))]
		[LocalizedSummary(nameof(Summaries.DisplayRolePositions))]
		[GuildPermissionRequirement(GuildPermission.ManageRoles)]
		[EnabledByDefault(true)]
		public sealed class DisplayRolePositions : AdvobotModuleBase
		{
			[Command]
			public Task<RuntimeResult> Command()
				=> Responses.Roles.Display(Context.Guild.Roles.OrderByDescending(x => x.Position));
		}

		[Group(nameof(ModifyRolePosition)), ModuleInitialismAlias(typeof(ModifyRolePosition))]
		[LocalizedSummary(nameof(Summaries.ModifyRolePosition))]
		[GuildPermissionRequirement(GuildPermission.ManageRoles)]
		[EnabledByDefault(true)]
		public sealed class ModifyRolePosition : AdvobotModuleBase
		{
			[Command]
			public async Task<RuntimeResult> Command(
				[NotEveryone] IRole role,
				[Positive] int position)
			{
				var pos = await DiscordUtils.ModifyRolePositionAsync(role, position, GenerateRequestOptions()).CAF();
				return Responses.Roles.Moved(role, pos);
			}
		}

		[Group(nameof(DisplayRolePerms)), ModuleInitialismAlias(typeof(DisplayRolePerms))]
		[LocalizedSummary(nameof(Summaries.DisplayRolePerms))]
		[GuildPermissionRequirement(GuildPermission.ManageRoles)]
		[EnabledByDefault(true)]
		public sealed class DisplayRolePerms : AdvobotModuleBase
		{
			[Command]
			public Task<RuntimeResult> Command()
				=> Responses.CommandResponses.DisplayEnumValues<GuildPermission>();
			[Command]
			public Task<RuntimeResult> Command(IRole role)
				=> Responses.Roles.DisplayPermissions(role);
		}

		[Group(nameof(ModifyRolePerms)), ModuleInitialismAlias(typeof(ModifyRolePerms))]
		[LocalizedSummary(nameof(Summaries.ModifyRolePerms))]
		[GuildPermissionRequirement(GuildPermission.ManageRoles)]
		[EnabledByDefault(true)]
		public sealed class ModifyRolePerms : AdvobotModuleBase
		{
			[Command]
			public async Task<RuntimeResult> Command(
				[Role] IRole role,
				bool allow,
				[Remainder, OverrideTypeReader(typeof(PermissionsTypeReader<GuildPermission>))] ulong permissions)
			{
				//Only modify permissions the user has the ability to
				permissions &= Context.User.GuildPermissions.RawValue;

				var rolePermissions = allow ? role.Permissions.RawValue | permissions : role.Permissions.RawValue & ~permissions;
				await role.ModifyAsync(x => x.Permissions = new GuildPermissions(rolePermissions), GenerateRequestOptions()).CAF();
				return Responses.Roles.ModifiedPermissions(role, (GuildPermission)permissions, allow);
			}
		}

		[Group(nameof(CopyRolePerms)), ModuleInitialismAlias(typeof(CopyRolePerms))]
		[LocalizedSummary(nameof(Summaries.CopyRolePerms))]
		[GuildPermissionRequirement(GuildPermission.ManageRoles)]
		[EnabledByDefault(true)]
		public sealed class CopyRolePerms : AdvobotModuleBase
		{
			[Command]
			public async Task<RuntimeResult> Command(
				IRole input,
				[Role] IRole output)
			{
				var copyable = input.Permissions.RawValue & Context.User.GuildPermissions.RawValue; //Perms which the user can copy from the input role
				var immovable = output.Permissions.RawValue & ~Context.User.GuildPermissions.RawValue; //Output perms which can't be modified by the user
				var permissions = immovable | copyable;

				await output.ModifyAsync(x => x.Permissions = new GuildPermissions(permissions), GenerateRequestOptions()).CAF();
				return Responses.Roles.CopiedPermissions(input, output, (GuildPermission)permissions);
			}
		}

		[Group(nameof(ClearRolePerms)), ModuleInitialismAlias(typeof(ClearRolePerms))]
		[LocalizedSummary(nameof(Summaries.ClearRolePerms))]
		[GuildPermissionRequirement(GuildPermission.ManageRoles)]
		[EnabledByDefault(true)]
		public sealed class ClearRolePerms : AdvobotModuleBase
		{
			[Command]
			public async Task<RuntimeResult> Command([Role] IRole role)
			{
				var immovable = role.Permissions.RawValue & ~Context.User.GuildPermissions.RawValue;
				await role.ModifyAsync(x => x.Permissions = new GuildPermissions(immovable), GenerateRequestOptions()).CAF();
				return Responses.Roles.ClearedPermissions(role, (GuildPermission)immovable);
			}
		}

		[Group(nameof(ModifyRoleName)), ModuleInitialismAlias(typeof(ModifyRoleName))]
		[LocalizedSummary(nameof(Summaries.ModifyRoleName))]
		[GuildPermissionRequirement(GuildPermission.ManageRoles)]
		[EnabledByDefault(true)]
		public sealed class ModifyRoleName : AdvobotModuleBase
		{
			[Command, Priority(1)]
			public async Task<RuntimeResult> Command(
				[NotEveryone] IRole role,
				[Remainder, RoleName] string name)
			{
				await role.ModifyAsync(x => x.Name = name, GenerateRequestOptions()).CAF();
				return Responses.Snowflakes.ModifiedName(role, name);
			}
			[ImplicitCommand]
			public Task<RuntimeResult> Position(
				[OverrideTypeReader(typeof(RolePositionTypeReader)), NotEveryone] IRole role,
				[Remainder, RoleName] string name)
				=> Command(role, name);
		}

		[Group(nameof(ModifyRoleColor)), ModuleInitialismAlias(typeof(ModifyRoleColor))]
		[LocalizedSummary(nameof(Summaries.ModifyRoleColor))]
		[GuildPermissionRequirement(GuildPermission.ManageRoles)]
		[EnabledByDefault(true)]
		public sealed class ModifyRoleColor : AdvobotModuleBase
		{
			[Command]
			public async Task<RuntimeResult> Command(
				[NotEveryone] IRole role,
				[Optional] Color color)
			{
				await role.ModifyAsync(x => x.Color = color, GenerateRequestOptions()).CAF();
				return Responses.Roles.ModifiedColor(role, color);
			}
		}

		[Group(nameof(ModifyRoleHoist)), ModuleInitialismAlias(typeof(ModifyRoleHoist))]
		[LocalizedSummary(nameof(Summaries.ModifyRoleHoist))]
		[GuildPermissionRequirement(GuildPermission.ManageRoles)]
		[EnabledByDefault(true)]
		public sealed class ModifyRoleHoist : AdvobotModuleBase
		{
			[Command]
			public async Task<RuntimeResult> Command([NotEveryone] IRole role)
			{
				var hoisted = !role.IsHoisted;
				await role.ModifyAsync(x => x.Hoist = !role.IsHoisted, GenerateRequestOptions()).CAF();
				return Responses.Roles.ModifiedHoistStatus(role, hoisted);
			}
		}

		[Group(nameof(ModifyRoleMentionability)), ModuleInitialismAlias(typeof(ModifyRoleMentionability))]
		[LocalizedSummary(nameof(Summaries.ModifyRoleMentionability))]
		[GuildPermissionRequirement(GuildPermission.ManageRoles)]
		[EnabledByDefault(true)]
		public sealed class ModifyRoleMentionability : AdvobotModuleBase
		{
			[Command]
			public async Task<RuntimeResult> Command([NotEveryone] IRole role)
			{
				var mentionability = !role.IsMentionable;
				await role.ModifyAsync(x => x.Mentionable = !role.IsMentionable, GenerateRequestOptions()).CAF();
				return Responses.Roles.ModifiedMentionability(role, mentionability);
			}
		}
	}
}
