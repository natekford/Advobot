using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Advobot.Classes;
using Advobot.Classes.Attributes;
using Advobot.Classes.TypeReaders;
using Advobot.Enums;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Advobot.Commands.Roles
{
	[Category(typeof(GiveRole)), Group(nameof(GiveRole)), TopLevelShortAlias(typeof(GiveRole))]
	[Summary("Gives the role(s) to the user (assuming the person using the command and bot both have the ability to give that role).")]
	[PermissionRequirement(new[] { GuildPermission.ManageRoles }, null)]
	[DefaultEnabled(true)]
	public sealed class GiveRole : AdvobotModuleBase
	{
		[Command]
		public async Task Command(
			SocketGuildUser user,
			[ValidateObject(false, Verif.CanBeEdited, Verif.IsNotEveryone, Verif.IsNotManaged)] params SocketRole[] roles)
		{
			await user.AddRolesAsync(roles, GetRequestOptions()).CAF();
			var resp = $"Successfully gave `{string.Join("`, `", roles.Select(x => x.Format()))}` to `{user.Format()}`.";
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}
	}

	[Category(typeof(TakeRole)), Group(nameof(TakeRole)), TopLevelShortAlias(typeof(TakeRole))]
	[Summary("Takes the role(s) from the user (assuming the person using the command and bot both have the ability to take that role).")]
	[PermissionRequirement(new[] { GuildPermission.ManageRoles }, null)]
	[DefaultEnabled(true)]
	public sealed class TakeRole : AdvobotModuleBase
	{
		[Command]
		public async Task Command(
			SocketGuildUser user,
			[ValidateObject(false, Verif.CanBeEdited, Verif.IsNotEveryone, Verif.IsNotManaged)] params SocketRole[] roles)
		{
			await user.RemoveRolesAsync(roles, GetRequestOptions()).CAF();
			var resp = $"Successfully took `{string.Join("`, `", roles.Select(x => x.Format()))}` from `{user.Format()}`.";
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}
	}

	[Category(typeof(CreateRole)), Group(nameof(CreateRole)), TopLevelShortAlias(typeof(CreateRole))]
	[Summary("Adds a role to the guild with the chosen name.")]
	[PermissionRequirement(new[] { GuildPermission.ManageRoles }, null)]
	[DefaultEnabled(true)]
	public sealed class CreateRole : AdvobotModuleBase
	{
		[Command]
		public async Task Command([ValidateString(Target.Role)] string name)
		{
			await Context.Guild.CreateRoleAsync(name, new GuildPermissions(0), null, false, GetRequestOptions()).CAF();
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, $"Successfully created the role `{name}`.").CAF();
		}
	}

	[Category(typeof(SoftDeleteRole)), Group(nameof(SoftDeleteRole)), TopLevelShortAlias(typeof(SoftDeleteRole))]
	[Summary("Deleted the role, thus removing all permissions from a role (and all channels the role had permissions on) and removing the role from everyone. " +
		"Leaves the name, color, and position behind in a newly created role.")]
	[PermissionRequirement(new[] { GuildPermission.ManageRoles }, null)]
	[DefaultEnabled(true)]
	public sealed class SoftDeleteRole : AdvobotModuleBase
	{
		[Command]
		public async Task Command([ValidateObject(false, Verif.CanBeEdited, Verif.IsNotEveryone, Verif.IsNotManaged)] SocketRole role)
		{
			//Get the properties of the role before it's deleted
			var name = role.Name;
			var color = role.Color;
			var position = role.Position;

			await role.DeleteAsync(GetRequestOptions()).CAF();
			var newRole = await Context.Guild.CreateRoleAsync(name, new GuildPermissions(0), color).CAF();
			await DiscordUtils.ModifyRolePositionAsync(role, position, GetRequestOptions()).CAF();
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, $"Successfully softdeleted `{newRole.Name}`.").CAF();
		}
	}

	[Category(typeof(DeleteRole)), Group(nameof(DeleteRole)), TopLevelShortAlias(typeof(DeleteRole))]
	[Summary("Deletes the role.")]
	[PermissionRequirement(new[] { GuildPermission.ManageRoles }, null)]
	[DefaultEnabled(true)]
	public sealed class DeleteRole : AdvobotModuleBase
	{
		[Command]
		public async Task Command([ValidateObject(false, Verif.CanBeEdited, Verif.IsNotEveryone, Verif.IsNotManaged)] IRole role)
		{
			await role.DeleteAsync(GetRequestOptions()).CAF();
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, $"Successfully deleted `{role.Format()}`.").CAF();
		}
	}

	[Category(typeof(ModifyRolePosition)), Group(nameof(ModifyRolePosition)), TopLevelShortAlias(typeof(ModifyRolePosition))]
	[Summary("If only a role is input its position will be listed, else moves the role to the given position. " +
		"Everyone is the first position and starts at zero.")]
	[PermissionRequirement(new[] { GuildPermission.ManageRoles }, null)]
	[DefaultEnabled(true)]
	public sealed class ModifyRolePosition : AdvobotModuleBase
	{
		[Command]
		public async Task Command([ValidateObject(false, Verif.CanBeEdited, Verif.IsNotEveryone)] SocketRole role, [ValidateNumber(1, 250)] uint position)
		{
			var pos = await DiscordUtils.ModifyRolePositionAsync(role, (int)position, GetRequestOptions()).CAF();
			await MessageUtils.SendMessageAsync(Context.Channel, $"Successfully gave `{role.Format()}` the position `{pos}`.").CAF();
		}
	}

	[Category(typeof(DisplayRolePositions)), Group(nameof(DisplayRolePositions)), TopLevelShortAlias(typeof(DisplayRolePositions))]
	[Summary("Lists the positions of each role on the guild.")]
	[PermissionRequirement(new[] { GuildPermission.ManageRoles }, null)]
	[DefaultEnabled(true)]
	public sealed class DisplayRolePositions : AdvobotModuleBase
	{
		[Command]
		public async Task Command()
		{
			var embed = new EmbedWrapper
			{
				Title = "Role Positions",
				Description = string.Join("\n", Context.Guild.Roles.OrderByDescending(x => x.Position).Select(x =>
				{
					return x.Id == Context.Guild.EveryoneRole.Id
						? $"`{x.Position.ToString("00")}.` Everyone"
						: $"`{x.Position.ToString("00")}.` {x.Name}";
				})),
			};
			await MessageUtils.SendMessageAsync(Context.Channel, null, embed).CAF();
		}
	}

	[Category(typeof(ModifyRolePerms)), Group(nameof(ModifyRolePerms)), TopLevelShortAlias(typeof(ModifyRolePerms))]
	[Summary("Permissions must be separated by a `/` or their rawvalue can be said instead. " +
		"Type `" + nameof(ModifyRolePerms) + " [Show]` to see the available permissions. " +
		"Type `" + nameof(ModifyRolePerms) + " [Show] [Role]` to see the permissions of that role.")]
	[PermissionRequirement(new[] { GuildPermission.ManageRoles }, null)]
	[DefaultEnabled(true)]
	public sealed class ModifyRolePerms : AdvobotModuleBase
	{
		[Group(nameof(Show)), ShortAlias(nameof(Show))]
		public sealed class Show : AdvobotModuleBase
		{
			[Command]
			public async Task Command()
			{
				var embed = new EmbedWrapper
				{
					Title = "Guild Permission Types",
					Description = $"`{string.Join("`, `", Enum.GetNames(typeof(GuildPermission)))}`"
				};
				await MessageUtils.SendMessageAsync(Context.Channel, null, embed).CAF();
			}
			[Command]
			public async Task Command([ValidateObject(false, Verif.CanBeEdited)] SocketRole role)
			{
				var currentRolePerms = EnumUtils.GetFlagNames((GuildPermission)role.Permissions.RawValue);
				var embed = new EmbedWrapper
				{
					Title = role.Name,
					Description = $"`{(currentRolePerms.Any() ? string.Join("`, `", currentRolePerms) : "No permission")}`"
				};
				await MessageUtils.SendMessageAsync(Context.Channel, null, embed).CAF();
			}
		}
		[Command(nameof(Allow)), ShortAlias(nameof(Allow))]
		public async Task Allow(
			[ValidateObject(false, Verif.CanBeEdited)] SocketRole role,
			[Remainder, OverrideTypeReader(typeof(GuildPermissionsTypeReader))] ulong permissions)
		{
			var givenPerms = (await CommandRunner(role, PermValue.Allow, permissions).CAF()).ToList();
			var resp = $"Successfully allowed `{(givenPerms.Any() ? string.Join("`, `", givenPerms) : "Nothing")}` for `{role.Format()}`.";
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}
		[Command(nameof(Deny)), ShortAlias(nameof(Deny))]
		public async Task Deny(
			[ValidateObject(false, Verif.CanBeEdited)] SocketRole role,
			[Remainder, OverrideTypeReader(typeof(GuildPermissionsTypeReader))] ulong permissions)
		{
			var givenPerms = (await CommandRunner(role, PermValue.Deny, permissions).CAF()).ToList();
			var resp = $"Successfully denied `{(givenPerms.Any() ? string.Join("`, `", givenPerms) : "Nothing")}` for `{role.Format()}`.";
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}

		private async Task<IEnumerable<string>> CommandRunner(SocketRole role, PermValue permValue, ulong permissions)
		{
			if (role == null)
			{
				return Enumerable.Empty<string>();
			}

			//Only modify permissions the user has the ability to
			permissions &= (Context.User as IGuildUser).GuildPermissions.RawValue;

			var roleBits = role.Permissions.RawValue;
			switch (permValue)
			{
				case PermValue.Allow:
					roleBits |= permissions;
					break;
				case PermValue.Deny:
					roleBits &= ~permissions;
					break;
				default:
					throw new ArgumentException("invalid value provided", nameof(permValue));
			}

			await role.ModifyAsync(x => x.Permissions = new GuildPermissions(roleBits), GetRequestOptions()).CAF();
			return EnumUtils.GetFlagNames((GuildPermission)permissions);
		}
	}

	[Category(typeof(CopyRolePerms)), Group(nameof(CopyRolePerms)), TopLevelShortAlias(typeof(CopyRolePerms))]
	[Summary("Copies the permissions from the first role to the second role. " +
		"Will not copy roles that the user does not have access to. " +
		"Will not overwrite roles that are above the user's top role.")]
	[PermissionRequirement(new[] { GuildPermission.ManageRoles }, null)]
	[DefaultEnabled(true)]
	public sealed class CopyRolePerms : AdvobotModuleBase
	{
		[Command]
		public async Task Command(
			[ValidateObject(false, Verif.CanBeEdited)] SocketRole inputRole,
			[ValidateObject(false, Verif.CanBeEdited)] SocketRole outputRole)
		{
			var userBits = ((IGuildUser)Context.User).GuildPermissions.RawValue;
			var immovableBits = outputRole.Permissions.RawValue & ~userBits;
			var copyBits = inputRole.Permissions.RawValue & userBits;
			var newRoleBits = immovableBits | copyBits;
			var immovableEnums = EnumUtils.GetFlagNames((GuildPermission)immovableBits);
			var failedEnums = EnumUtils.GetFlagNames((GuildPermission)(inputRole.Permissions.RawValue & ~copyBits));
			var newEnums = EnumUtils.GetFlagNames((GuildPermission)newRoleBits);
			var response = new[]
			{
				immovableEnums.Any() ? "Output role had some permissions unable to be removed by you." : null,
				failedEnums.Any() ? "Input role had some permission unable to be copied by you." : null,
				$"`{outputRole.Format()}` now has the following permissions: `{(newEnums.Any() ? string.Join("`, `", newEnums) : "Nothing")}`.",
			}.JoinNonNullStrings(" ");

			await outputRole.ModifyAsync(x => x.Permissions = new GuildPermissions(newRoleBits), GetRequestOptions()).CAF();
			await MessageUtils.SendMessageAsync(Context.Channel, response).CAF();
		}
	}

	[Category(typeof(ClearRolePerms)), Group(nameof(ClearRolePerms)), TopLevelShortAlias(typeof(ClearRolePerms))]
	[Summary("Removes all permissions from a role.")]
	[PermissionRequirement(new[] { GuildPermission.ManageRoles }, null)]
	[DefaultEnabled(true)]
	public sealed class ClearRolePerms : AdvobotModuleBase
	{
		[Command]
		public async Task Command([ValidateObject(false, Verif.CanBeEdited)] SocketRole role)
		{
			var immovableBits = role.Permissions.RawValue & ~((IGuildUser)Context.User).GuildPermissions.RawValue;
			var immovablePerms = EnumUtils.GetFlagNames((GuildPermission)immovableBits);
			var response = new[]
			{
				immovablePerms.Any() ? "Role had some permissions unable to be cleared by you." : null,
				$"`{role.Format()}` now has the following permissions: `{(immovablePerms.Any() ? string.Join("`, `", immovablePerms) : "Nothing")}`.",
			}.JoinNonNullStrings(" ");

			await role.ModifyAsync(x => x.Permissions = new GuildPermissions(immovableBits), GetRequestOptions()).CAF();
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, response).CAF();
		}
	}

	[Category(typeof(ModifyRoleName)), Group(nameof(ModifyRoleName)), TopLevelShortAlias(typeof(ModifyRoleName))]
	[Summary("Changes the name of the role.")]
	[PermissionRequirement(new[] { GuildPermission.ManageRoles }, null)]
	[DefaultEnabled(true)]
	public sealed class ModifyRoleName : AdvobotModuleBase
	{
		[Command, Priority(1)]
		public async Task Command(
			[ValidateObject(false, Verif.CanBeEdited, Verif.IsNotEveryone)] SocketRole role,
			[Remainder, ValidateString(Target.Role)] string name)
		{
			await role.ModifyAsync(x => x.Name = name, GetRequestOptions()).CAF();
			var resp = $"Successfully changed the name of `{role.Format()}` to `{name}`.";
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}
		[Command(nameof(Position))]
		public async Task Position([ValidateNumber(1, 250)] uint rolePosition, [Remainder, ValidateString(Target.Role)] string name)
		{
			var roles = Context.Guild.Roles.Where(x => x.Position == rolePosition).ToList();
			if (!roles.Any())
			{
				await MessageUtils.SendErrorMessageAsync(Context, new Error($"No object has the position `{rolePosition}`.")).CAF();
				return;
			}
			if (roles.Count() > 1)
			{
				await MessageUtils.SendErrorMessageAsync(Context, new Error($"Multiple objects have the position `{rolePosition}`.")).CAF();
				return;
			}

			var role = roles.First();
			var result = role.Verify(Context, new[] { Verif.IsNotManaged, Verif.IsNotEveryone });
			if (!result.IsSuccess)
			{
				await MessageUtils.SendErrorMessageAsync(Context, new Error(result.ErrorReason)).CAF();
			}

			await role.ModifyAsync(x => x.Name = name, GetRequestOptions()).CAF();
			var resp = $"Successfully changed the name of `{role.Format()}` to `{name}`.";
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}
	}

	[Category(typeof(ModifyRoleColor)), Group(nameof(ModifyRoleColor)), TopLevelShortAlias(typeof(ModifyRoleColor))]
	[Summary("Changes the role's color. " +
		"Color must be valid hexadecimal or the name of a default role color. ")]
	[PermissionRequirement(new[] { GuildPermission.ManageRoles }, null)]
	[DefaultEnabled(true)]
	public sealed class ModifyRoleColor : AdvobotModuleBase
	{
		[Command]
		public async Task Command([Optional, ValidateObject(false, Verif.CanBeEdited, Verif.IsNotEveryone)] IRole role, [Optional] Color color)
		{
			await role.ModifyAsync(x => x.Color = color, GetRequestOptions()).CAF();
			var resp = $"Successfully changed the color of `{role.Format()}` to `#{color.RawValue.ToString("X6")}`."; //X6 to get hex
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}
	}

	[Category(typeof(ModifyRoleHoist)), Group(nameof(ModifyRoleHoist)), TopLevelShortAlias(typeof(ModifyRoleHoist))]
	[Summary("Displays a role separately from others on the user list. " +
		"Saying the command again remove it from being hoisted.")]
	[PermissionRequirement(new[] { GuildPermission.ManageRoles }, null)]
	[DefaultEnabled(true)]
	public sealed class ModifyRoleHoist : AdvobotModuleBase
	{
		[Command]
		public async Task Command([ValidateObject(false, Verif.CanBeEdited, Verif.IsNotEveryone)] IRole role)
		{
			await role.ModifyAsync(x => x.Hoist = !role.IsHoisted, GetRequestOptions()).CAF();
			var resp = $"Successfully made `{role.Format()}` {(role.IsHoisted ? "unhoisted" : "hoisted")}.";
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}
	}

	[Category(typeof(ModifyRoleMentionability)), Group(nameof(ModifyRoleMentionability)), TopLevelShortAlias(typeof(ModifyRoleMentionability))]
	[Summary("Allows the role to be mentioned. " +
		"Saying the command again removes its ability to be mentioned.")]
	[PermissionRequirement(new[] { GuildPermission.ManageRoles }, null)]
	[DefaultEnabled(true)]
	public sealed class ModifyRoleMentionability : AdvobotModuleBase
	{
		[Command]
		public async Task Command([ValidateObject(false, Verif.CanBeEdited, Verif.IsNotEveryone)] IRole role)
		{
			await role.ModifyAsync(x => x.Mentionable = !role.IsMentionable, GetRequestOptions()).CAF();
			var resp = $"Successfully made `{role.Format()}` {(role.IsMentionable ? "unmentionable" : "mentionable")}.";
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}
	}
}
