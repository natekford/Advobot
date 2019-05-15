using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Advobot.Classes.Attributes;
using Advobot.Classes.Attributes.ParameterPreconditions.DiscordObjectValidation.Roles;
using Advobot.Classes.Attributes.ParameterPreconditions.NumberValidation;
using Advobot.Classes.Attributes.ParameterPreconditions.StringValidation;
using Advobot.Classes.Attributes.Preconditions.Permissions;
using Advobot.Classes.Modules;
using Advobot.Classes.TypeReaders;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Advobot.Commands
{
	public sealed class Roles : ModuleBase
	{
		[Group(nameof(GiveRole)), ModuleInitialismAlias(typeof(GiveRole))]
		[Summary("Gives the role(s) to the user (assuming the person using the command and bot both have the ability to give that role).")]
		[UserPermissionRequirement(GuildPermission.ManageRoles)]
		[EnabledByDefault(true)]
		public sealed class GiveRole : AdvobotModuleBase
		{
			[Command]
			public async Task<RuntimeResult> Command(SocketGuildUser user, [ValidateRole] params SocketRole[] roles)
			{
				await user.AddRolesAsync(roles, GenerateRequestOptions()).CAF();
				return Responses.Roles.Gave(roles, user);
			}
		}

		[Group(nameof(TakeRole)), ModuleInitialismAlias(typeof(TakeRole))]
		[Summary("Takes the role(s) from the user (assuming the person using the command and bot both have the ability to take that role).")]
		[UserPermissionRequirement(GuildPermission.ManageRoles)]
		[EnabledByDefault(true)]
		public sealed class TakeRole : AdvobotModuleBase
		{
			[Command]
			public async Task<RuntimeResult> Command(SocketGuildUser user, [ValidateRole] params SocketRole[] roles)
			{
				await user.RemoveRolesAsync(roles, GenerateRequestOptions()).CAF();
				return Responses.Roles.Took(roles, user);
			}
		}

		[Group(nameof(CreateRole)), ModuleInitialismAlias(typeof(CreateRole))]
		[Summary("Adds a role to the guild with the chosen name.")]
		[UserPermissionRequirement(GuildPermission.ManageRoles)]
		[EnabledByDefault(true)]
		public sealed class CreateRole : AdvobotModuleBase
		{
			[Command]
			public async Task<RuntimeResult> Command([ValidateRoleName] string name)
			{
				var role = await Context.Guild.CreateRoleAsync(name, new GuildPermissions(0), null, false, GenerateRequestOptions()).CAF();
				return Responses.Snowflakes.Created(role);
			}
		}

		[Group(nameof(SoftDeleteRole)), ModuleInitialismAlias(typeof(SoftDeleteRole))]
		[Summary("Deletes the role, thus removing all channel overwrites the role had and removing the role from everyone. " +
			"Creates a new role with the same color, permissions, and position.")]
		[UserPermissionRequirement(GuildPermission.ManageRoles)]
		[EnabledByDefault(true)]
		public sealed class SoftDeleteRole : AdvobotModuleBase
		{
			[Command]
			public async Task<RuntimeResult> Command([NotEveryoneOrManaged] SocketRole role)
			{
				await role.DeleteAsync(GenerateRequestOptions()).CAF();
				await Context.Guild.CreateRoleAsync(role.Name, role.Permissions, role.Color, false, GenerateRequestOptions()).CAF();
				await DiscordUtils.ModifyRolePositionAsync(role, role.Position, GenerateRequestOptions()).CAF();
				return Responses.Snowflakes.SoftDeleted(role);
			}
		}

		[Group(nameof(DeleteRole)), ModuleInitialismAlias(typeof(DeleteRole))]
		[Summary("Deletes the role.")]
		[UserPermissionRequirement(GuildPermission.ManageRoles)]
		[EnabledByDefault(true)]
		public sealed class DeleteRole : AdvobotModuleBase
		{
			[Command]
			public async Task<RuntimeResult> Command([NotEveryoneOrManaged] SocketRole role)
			{
				await role.DeleteAsync(GenerateRequestOptions()).CAF();
				return Responses.Snowflakes.Deleted(role);
			}
		}

		[Group(nameof(DisplayRolePositions)), ModuleInitialismAlias(typeof(DisplayRolePositions))]
		[Summary("Lists the positions of each role on the guild.")]
		[UserPermissionRequirement(GuildPermission.ManageRoles)]
		[EnabledByDefault(true)]
		public sealed class DisplayRolePositions : AdvobotModuleBase
		{
			[Command]
			public Task<RuntimeResult> Command()
				=> Responses.Roles.Display(Context.Guild.Roles.OrderByDescending(x => x.Position));
		}

		[Group(nameof(ModifyRolePosition)), ModuleInitialismAlias(typeof(ModifyRolePosition))]
		[Summary("If only a role is input its position will be listed, else moves the role to the given position. " +
			"Everyone is the first position and starts at zero.")]
		[UserPermissionRequirement(GuildPermission.ManageRoles)]
		[EnabledByDefault(true)]
		public sealed class ModifyRolePosition : AdvobotModuleBase
		{
			[Command]
			public async Task<RuntimeResult> Command([NotEveryone] SocketRole role, [ValidatePositiveNumber] int position)
			{
				var pos = await DiscordUtils.ModifyRolePositionAsync(role, position, GenerateRequestOptions()).CAF();
				return Responses.Roles.Moved(role, pos);
			}
		}

		[Group(nameof(DisplayRolePerms)), ModuleInitialismAlias(typeof(DisplayRolePerms))]
		[Summary("Shows permissions on a role. Can show permission types or the permission a role has.")]
		[UserPermissionRequirement(GuildPermission.ManageRoles)]
		[EnabledByDefault(true)]
		public sealed class DisplayRolePerms : AdvobotModuleBase
		{
			[Command]
			public Task<RuntimeResult> Command()
				=> Responses.CommandResponses.DisplayEnumValues<GuildPermission>();
			[Command]
			public Task<RuntimeResult> Command(SocketRole role)
				=> Responses.Roles.DisplayPermissions(role);
		}

		[Group(nameof(ModifyRolePerms)), ModuleInitialismAlias(typeof(ModifyRolePerms))]
		[Summary("Permissions must be separated by a `/` or their rawvalue can be said instead.")]
		[UserPermissionRequirement(GuildPermission.ManageRoles)]
		[EnabledByDefault(true)]
		public sealed class ModifyRolePerms : AdvobotModuleBase
		{
			[Command]
			public async Task<RuntimeResult> Command([ValidateRole] SocketRole role,
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
		[Summary("Copies the permissions from the first role to the second role. " +
			"Will not copy roles that the user does not have access to. " +
			"Will not overwrite roles that are above the user's top role.")]
		[UserPermissionRequirement(GuildPermission.ManageRoles)]
		[EnabledByDefault(true)]
		public sealed class CopyRolePerms : AdvobotModuleBase
		{
			[Command]
			public async Task<RuntimeResult> Command(SocketRole input, [ValidateRole] SocketRole output)
			{
				var copyable = input.Permissions.RawValue & Context.User.GuildPermissions.RawValue; //Perms which the user can copy from the input role
				var immovable = output.Permissions.RawValue & ~Context.User.GuildPermissions.RawValue; //Output perms which can't be modified by the user
				var permissions = immovable | copyable;

				await output.ModifyAsync(x => x.Permissions = new GuildPermissions(permissions), GenerateRequestOptions()).CAF();
				return Responses.Roles.CopiedPermissions(input, output, (GuildPermission)permissions);
			}
		}

		[Group(nameof(ClearRolePerms)), ModuleInitialismAlias(typeof(ClearRolePerms))]
		[Summary("Removes all permissions from a role.")]
		[UserPermissionRequirement(GuildPermission.ManageRoles)]
		[EnabledByDefault(true)]
		public sealed class ClearRolePerms : AdvobotModuleBase
		{
			[Command]
			public async Task<RuntimeResult> Command([ValidateRole] SocketRole role)
			{
				var immovable = role.Permissions.RawValue & ~Context.User.GuildPermissions.RawValue;
				await role.ModifyAsync(x => x.Permissions = new GuildPermissions(immovable), GenerateRequestOptions()).CAF();
				return Responses.Roles.ClearedPermissions(role, (GuildPermission)immovable);
			}
		}

		[Group(nameof(ModifyRoleName)), ModuleInitialismAlias(typeof(ModifyRoleName))]
		[Summary("Changes the name of the role.")]
		[UserPermissionRequirement(GuildPermission.ManageRoles)]
		[EnabledByDefault(true)]
		public sealed class ModifyRoleName : AdvobotModuleBase
		{
			[Command, Priority(1)]
			public async Task<RuntimeResult> Command([NotEveryone] SocketRole role, [Remainder, ValidateRoleName] string name)
			{
				await role.ModifyAsync(x => x.Name = name, GenerateRequestOptions()).CAF();
				return Responses.Snowflakes.ModifiedName(role, name);
			}
			[ImplicitCommand]
			public Task<RuntimeResult> Position([OverrideTypeReader(typeof(RolePositionTypeReader)), NotEveryone] SocketRole role,
				[Remainder, ValidateRoleName] string name)
				=> Command(role, name);
		}

		[Group(nameof(ModifyRoleColor)), ModuleInitialismAlias(typeof(ModifyRoleColor))]
		[Summary("Changes the role's color. " +
			"Color must be valid hexadecimal or the name of a default role color. ")]
		[UserPermissionRequirement(GuildPermission.ManageRoles)]
		[EnabledByDefault(true)]
		public sealed class ModifyRoleColor : AdvobotModuleBase
		{
			[Command]
			public async Task<RuntimeResult> Command([NotEveryone] SocketRole role, [Optional] Color color)
			{
				await role.ModifyAsync(x => x.Color = color, GenerateRequestOptions()).CAF();
				return Responses.Roles.ModifiedColor(role, color);
			}
		}

		[Group(nameof(ModifyRoleHoist)), ModuleInitialismAlias(typeof(ModifyRoleHoist))]
		[Summary("Displays a role separately from others on the user list. " +
			"Saying the command again remove it from being hoisted.")]
		[UserPermissionRequirement(GuildPermission.ManageRoles)]
		[EnabledByDefault(true)]
		public sealed class ModifyRoleHoist : AdvobotModuleBase
		{
			[Command]
			public async Task<RuntimeResult> Command([NotEveryone] SocketRole role)
			{
				var hoisted = !role.IsHoisted;
				await role.ModifyAsync(x => x.Hoist = !role.IsHoisted, GenerateRequestOptions()).CAF();
				return Responses.Roles.ModifiedHoistStatus(role, hoisted);
			}
		}

		[Group(nameof(ModifyRoleMentionability)), ModuleInitialismAlias(typeof(ModifyRoleMentionability))]
		[Summary("Allows the role to be mentioned. " +
			"Saying the command again removes its ability to be mentioned.")]
		[UserPermissionRequirement(GuildPermission.ManageRoles)]
		[EnabledByDefault(true)]
		public sealed class ModifyRoleMentionability : AdvobotModuleBase
		{
			[Command]
			public async Task<RuntimeResult> Command([NotEveryone] SocketRole role)
			{
				var mentionability = !role.IsMentionable;
				await role.ModifyAsync(x => x.Mentionable = !role.IsMentionable, GenerateRequestOptions()).CAF();
				return Responses.Roles.ModifiedMentionability(role, mentionability);
			}
		}
	}
}
