using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Advobot.Classes;
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
			public async Task Command(SocketGuildUser user, [ValidateRole] params SocketRole[] roles)
			{
				await user.AddRolesAsync(roles, GenerateRequestOptions()).CAF();
				await ReplyTimedAsync($"Successfully gave `{roles.Join("`, `", x => x.Format())}` to `{user.Format()}`.").CAF();
			}
		}

		[Group(nameof(TakeRole)), ModuleInitialismAlias(typeof(TakeRole))]
		[Summary("Takes the role(s) from the user (assuming the person using the command and bot both have the ability to take that role).")]
		[UserPermissionRequirement(GuildPermission.ManageRoles)]
		[EnabledByDefault(true)]
		public sealed class TakeRole : AdvobotModuleBase
		{
			[Command]
			public async Task Command(SocketGuildUser user, [ValidateRole] params SocketRole[] roles)
			{
				await user.RemoveRolesAsync(roles, GenerateRequestOptions()).CAF();
				await ReplyTimedAsync($"Successfully took `{roles.Join("`, `", x => x.Format())}` from `{user.Format()}`.").CAF();
			}
		}

		[Group(nameof(CreateRole)), ModuleInitialismAlias(typeof(CreateRole))]
		[Summary("Adds a role to the guild with the chosen name.")]
		[UserPermissionRequirement(GuildPermission.ManageRoles)]
		[EnabledByDefault(true)]
		public sealed class CreateRole : AdvobotModuleBase
		{
			[Command]
			public async Task Command([ValidateRoleName] string name)
			{
				var role = await Context.Guild.CreateRoleAsync(name, new GuildPermissions(0), null, false, GenerateRequestOptions()).CAF();
				await ReplyTimedAsync($"Successfully created the role `{role.Format()}`.").CAF();
			}
		}

		[Group(nameof(SoftDeleteRole)), ModuleInitialismAlias(typeof(SoftDeleteRole))]
		[Summary("Deletes the role, thus removing all channel overwrites the role had and removing the role from everyone. " +
			"Leaves the name, color, permissions, and position behind in a newly created role.")]
		[UserPermissionRequirement(GuildPermission.ManageRoles)]
		[EnabledByDefault(true)]
		public sealed class SoftDeleteRole : AdvobotModuleBase
		{
			[Command]
			public async Task Command([NotEveryoneOrManaged] SocketRole role)
			{
				await role.DeleteAsync(GenerateRequestOptions()).CAF();
				var newRole = await Context.Guild.CreateRoleAsync(role.Name, role.Permissions, role.Color, false, GenerateRequestOptions()).CAF();
				await DiscordUtils.ModifyRolePositionAsync(role, role.Position, GenerateRequestOptions()).CAF();
				await ReplyTimedAsync($"Successfully softdeleted `{role.Format()}`.").CAF();
			}
		}

		[Group(nameof(DeleteRole)), ModuleInitialismAlias(typeof(DeleteRole))]
		[Summary("Deletes the role.")]
		[UserPermissionRequirement(GuildPermission.ManageRoles)]
		[EnabledByDefault(true)]
		public sealed class DeleteRole : AdvobotModuleBase
		{
			[Command]
			public async Task Command([NotEveryoneOrManaged] SocketRole role)
			{
				await role.DeleteAsync(GenerateRequestOptions()).CAF();
				await ReplyTimedAsync($"Successfully deleted `{role.Format()}`.").CAF();
			}
		}

		[Group(nameof(ModifyRolePosition)), ModuleInitialismAlias(typeof(ModifyRolePosition))]
		[Summary("If only a role is input its position will be listed, else moves the role to the given position. " +
			"Everyone is the first position and starts at zero.")]
		[UserPermissionRequirement(GuildPermission.ManageRoles)]
		[EnabledByDefault(true)]
		public sealed class ModifyRolePosition : AdvobotModuleBase
		{
			[Command]
			public async Task Command([NotEveryone] SocketRole role, [ValidatePositiveNumber] int position)
			{
				var pos = await DiscordUtils.ModifyRolePositionAsync(role, position, GenerateRequestOptions()).CAF();
				await ReplyTimedAsync($"Successfully gave `{role.Format()}` the position `{pos}`.").CAF();
			}
		}

		[Group(nameof(DisplayRolePositions)), ModuleInitialismAlias(typeof(DisplayRolePositions))]
		[Summary("Lists the positions of each role on the guild.")]
		[UserPermissionRequirement(GuildPermission.ManageRoles)]
		[EnabledByDefault(true)]
		public sealed class DisplayRolePositions : AdvobotModuleBase
		{
			[Command]
			public Task Command()
			{
				var roles = Context.Guild.Roles.OrderByDescending(x => x.Position);
				return ReplyEmbedAsync(new EmbedWrapper
				{
					Title = "Role Positions",
					Description = roles.Join("\n", x => $"`{x.Position.ToString("00")}.` {x.Name}"),
				});
			}
		}

		[Group(nameof(ModifyRolePerms)), ModuleInitialismAlias(typeof(ModifyRolePerms))]
		[Summary("Permissions must be separated by a `/` or their rawvalue can be said instead. " +
			"Type `" + nameof(ModifyRolePerms) + " [Show]` to see the available permissions. " +
			"Type `" + nameof(ModifyRolePerms) + " [Show] [Role]` to see the permissions of that role.")]
		[UserPermissionRequirement(GuildPermission.ManageRoles)]
		[EnabledByDefault(true)]
		public sealed class ModifyRolePerms : AdvobotModuleBase
		{
			[ImplicitCommand, ImplicitAlias]
			public Task Show([Optional, ValidateRole] SocketRole role)
			{
				if (role == null)
				{
					return ReplyEmbedAsync(new EmbedWrapper
					{
						Title = "Guild Permission Types",
						Description = $"`{string.Join("`, `", Enum.GetNames(typeof(GuildPermission)))}`",
					});
				}
				return ReplyIfAny(EnumUtils.GetFlagNames((GuildPermission)role.Permissions.RawValue), role.Format(), "Permissions", x => x);
			}
			[ImplicitCommand, ImplicitAlias]
			public Task Allow(
				[ValidateRole] SocketRole role,
				[Remainder, OverrideTypeReader(typeof(PermissionsTypeReader<GuildPermission>))] ulong permissions)
				=> CommandRunner(role, permissions, true);
			[ImplicitCommand, ImplicitAlias]
			public Task Deny(
				[ValidateRole] SocketRole role,
				[Remainder, OverrideTypeReader(typeof(PermissionsTypeReader<GuildPermission>))] ulong permissions)
				=> CommandRunner(role, permissions, false);

			private async Task CommandRunner(SocketRole role, ulong permissions, bool add)
			{
				//Only modify permissions the user has the ability to
				var validPermissions = permissions & ((IGuildUser)Context.User).GuildPermissions.RawValue;
				var rolePermissions = add ? role.Permissions.RawValue | validPermissions : role.Permissions.RawValue & ~validPermissions;

				await role.ModifyAsync(x => x.Permissions = new GuildPermissions(rolePermissions), GenerateRequestOptions()).CAF();
				var perms = EnumUtils.GetFlagNames((GuildPermission)permissions);
				var modified = perms.Any() ? string.Join("`, `", perms) : "Nothing";
				await ReplyTimedAsync($"Successfully {(add ? "added" : "removed")} `{perms}` for `{role.Format()}`.").CAF();
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
			public async Task Command(SocketRole input, [ValidateRole] SocketRole output)
			{
				var userBits = ((IGuildUser)Context.User).GuildPermissions.RawValue;
				var copyableBits = input.Permissions.RawValue & userBits; //Perms which the user can copy from the input role
				var uncopyableBits = input.Permissions.RawValue & ~copyableBits; //Input perms which can't be modified by the user
				var unremovableBits = output.Permissions.RawValue & ~userBits; //Output perms which can't be modified by the user
				var newOutputBits = unremovableBits | copyableBits;

				if (newOutputBits == output.Permissions.RawValue)
				{
					await ReplyErrorAsync("Either you are copying the same values or with the permissions you have you cannot change anything.");
					return;
				}

				var copyable = EnumUtils.GetFlagNames((GuildPermission)copyableBits);
				var uncopyable = EnumUtils.GetFlagNames((GuildPermission)uncopyableBits);
				var unremovable = EnumUtils.GetFlagNames((GuildPermission)unremovableBits);
				var newOutput = EnumUtils.GetFlagNames((GuildPermission)newOutputBits);

				var parts = new[]
				{
					copyable.Any() ? $"Successfully copied to output role: `{string.Join("`, `", copyable)}`." : null,
					unremovable.Any() ? $"Unable to be removed from output role: `{string.Join("`, `", unremovable)}`." : null,
					uncopyable.Any() ? $"Unable to be copied to output role: `{string.Join("`, `", uncopyable)}`." : null,
					$"Current output role: `{(newOutput.Any() ? string.Join("`, `", newOutput) : "Nothing")}`.",
				};
				await output.ModifyAsync(x => x.Permissions = new GuildPermissions(newOutputBits), GenerateRequestOptions()).CAF();
				await ReplyAsync(parts.JoinNonNullStrings("\n")).CAF();
			}
		}

		[Group(nameof(ClearRolePerms)), ModuleInitialismAlias(typeof(ClearRolePerms))]
		[Summary("Removes all permissions from a role.")]
		[UserPermissionRequirement(GuildPermission.ManageRoles)]
		[EnabledByDefault(true)]
		public sealed class ClearRolePerms : AdvobotModuleBase
		{
			[Command]
			public async Task Command([ValidateRole] SocketRole role)
			{
				var immovableBits = role.Permissions.RawValue & ~((IGuildUser)Context.User).GuildPermissions.RawValue;
				var immovablePerms = EnumUtils.GetFlagNames((GuildPermission)immovableBits);
				var response = new[]
				{
					immovablePerms.Any() ? "Role had some permissions unable to be cleared by you." : null,
					$"`{role.Format()}` now has the following permissions: `{(immovablePerms.Any() ? string.Join("`, `", immovablePerms) : "Nothing")}`.",
				}.JoinNonNullStrings(" ");

				await role.ModifyAsync(x => x.Permissions = new GuildPermissions(immovableBits), GenerateRequestOptions()).CAF();
				await ReplyTimedAsync(response).CAF();
			}
		}

		[Group(nameof(ModifyRoleName)), ModuleInitialismAlias(typeof(ModifyRoleName))]
		[Summary("Changes the name of the role.")]
		[UserPermissionRequirement(GuildPermission.ManageRoles)]
		[EnabledByDefault(true)]
		public sealed class ModifyRoleName : AdvobotModuleBase
		{
			[Command, Priority(1)]
			public Task Command([NotEveryone] SocketRole role, [Remainder, ValidateRoleName] string name)
				=> CommandRunner(role, name);
			[ImplicitCommand]
			public Task Position(
				[OverrideTypeReader(typeof(RolePositionTypeReader)), NotEveryone] SocketRole role,
				[Remainder, ValidateRoleName] string name)
				=> CommandRunner(role, name);

			private async Task CommandRunner(SocketRole role, string name)
			{
				await role.ModifyAsync(x => x.Name = name, GenerateRequestOptions()).CAF();
				await ReplyTimedAsync($"Successfully changed the name of `{role.Format()}` to `{name}`.").CAF();
			}
		}

		[Group(nameof(ModifyRoleColor)), ModuleInitialismAlias(typeof(ModifyRoleColor))]
		[Summary("Changes the role's color. " +
			"Color must be valid hexadecimal or the name of a default role color. ")]
		[UserPermissionRequirement(GuildPermission.ManageRoles)]
		[EnabledByDefault(true)]
		public sealed class ModifyRoleColor : AdvobotModuleBase
		{
			[Command]
			public async Task Command([NotEveryone] SocketRole role, [Optional] Color color)
			{
				await role.ModifyAsync(x => x.Color = color, GenerateRequestOptions()).CAF();
				//X6 to get hex
				await ReplyTimedAsync($"Successfully changed the color of `{role.Format()}` to `#{color.RawValue.ToString("X6")}`.").CAF();
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
			public async Task Command([NotEveryone] SocketRole role)
			{
				await role.ModifyAsync(x => x.Hoist = !role.IsHoisted, GenerateRequestOptions()).CAF();
				await ReplyTimedAsync($"Successfully {(role.IsHoisted ? "un" : "")}hoisted `{role.Format()}`.").CAF();
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
			public async Task Command([NotEveryone] SocketRole role)
			{
				await role.ModifyAsync(x => x.Mentionable = !role.IsMentionable, GenerateRequestOptions()).CAF();
				await ReplyTimedAsync($"Successfully made `{role.Format()}` {(role.IsMentionable ? "un" : "")}mentionable.").CAF();
			}
		}
	}
}
