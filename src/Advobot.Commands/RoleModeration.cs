using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Advobot.Core;
using Advobot.Core.Classes;
using Advobot.Core.Classes.Attributes;
using Advobot.Core.Classes.TypeReaders;
using Advobot.Core.Enums;
using Advobot.Core.Utilities;
using Advobot.Core.Utilities.Formatting;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Advobot.Commands.RoleModeration
{
	[Group(nameof(GiveRole)), TopLevelShortAlias(typeof(GiveRole))]
	[Summary("Gives the role(s) to the user (assuming the person using the command and bot both have the ability to give that role).")]
	[PermissionRequirement(new[] { GuildPermission.ManageRoles }, null)]
	[DefaultEnabled(true)]
	public sealed class GiveRole : AdvobotModuleBase
	{
		[Command]
		public async Task Command(SocketGuildUser user,
			[VerifyObject(false, ObjectVerification.CanBeEdited, ObjectVerification.IsEveryone, ObjectVerification.IsManaged)] params IRole[] roles)
		{
			await RoleUtils.GiveRolesAsync(user, roles, new ModerationReason(Context.User, null)).CAF();
			var resp = $"Successfully gave `{String.Join("`, `", roles.Select(x => x.Format()))}` to `{user.Format()}`.";
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}
	}

	[Group(nameof(TakeRole)), TopLevelShortAlias(typeof(TakeRole))]
	[Summary("Takes the role(s) from the user (assuming the person using the command and bot both have the ability to take that role).")]
	[PermissionRequirement(new[] { GuildPermission.ManageRoles }, null)]
	[DefaultEnabled(true)]
	public sealed class TakeRole : AdvobotModuleBase
	{
		[Command]
		public async Task Command(SocketGuildUser user,
			[VerifyObject(false, ObjectVerification.CanBeEdited, ObjectVerification.IsEveryone, ObjectVerification.IsManaged)] params IRole[] roles)
		{
			await RoleUtils.TakeRolesAsync(user, roles, new ModerationReason(Context.User, null)).CAF();
			var resp = $"Successfully took `{String.Join("`, `", roles.Select(x => x.Format()))}` from `{user.Format()}`.";
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}
	}

	[Group(nameof(CreateRole)), TopLevelShortAlias(typeof(CreateRole))]
	[Summary("Adds a role to the guild with the chosen name.")]
	[PermissionRequirement(new[] { GuildPermission.ManageRoles }, null)]
	[DefaultEnabled(true)]
	public sealed class CreateRole : AdvobotModuleBase
	{
		[Command]
		public async Task Command([VerifyStringLength(Target.Role)] string name)
		{
			await RoleUtils.CreateRoleAsync(Context.Guild, name, new ModerationReason(Context.User, null)).CAF();
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, $"Successfully created the role `{name}`.").CAF();
		}
	}

	[Group(nameof(SoftDeleteRole)), TopLevelShortAlias(typeof(SoftDeleteRole))]
	[Summary("Removes all permissions from a role (and all channels the role had permissions on) and removes the role from everyone. " +
		"Leaves the name and color behind.")]
	[PermissionRequirement(new[] { GuildPermission.ManageRoles }, null)]
	[DefaultEnabled(true)]
	public sealed class SoftDeleteRole : AdvobotModuleBase
	{
		[Command]
		public async Task Command([VerifyObject(false, ObjectVerification.CanBeEdited, ObjectVerification.IsEveryone, ObjectVerification.IsManaged)] IRole role)
		{
			//Get the properties of the role before it's deleted
			var name = role.Name;
			var color = role.Color;
			var position = role.Position;

			await RoleUtils.DeleteRoleAsync(role, new ModerationReason(Context.User, null)).CAF();
			var newRole = await Context.Guild.CreateRoleAsync(name, new GuildPermissions(0), color).CAF();

			await RoleUtils.ModifyRolePositionAsync(newRole, position, new ModerationReason(Context.User, null)).CAF();
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, $"Successfully softdeleted `{role.Name}`.").CAF();
		}
	}

	[Group(nameof(DeleteRole)), TopLevelShortAlias(typeof(DeleteRole))]
	[Summary("Deletes the role.")]
	[PermissionRequirement(new[] { GuildPermission.ManageRoles }, null)]
	[DefaultEnabled(true)]
	public sealed class DeleteRole : AdvobotModuleBase
	{
		[Command]
		public async Task Command([VerifyObject(false, ObjectVerification.CanBeEdited, ObjectVerification.IsEveryone, ObjectVerification.IsManaged)] IRole role)
		{
			await RoleUtils.DeleteRoleAsync(role, new ModerationReason(Context.User, null)).CAF();
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, $"Successfully deleted `{role.Format()}`.").CAF();
		}
	}

	[Group(nameof(ModifyRolePosition)), TopLevelShortAlias(typeof(ModifyRolePosition))]
	[Summary("If only a role is input its position will be listed, else moves the role to the given position. " +
		Constants.FAKE_EVERYONE + " is the first position and starts at zero.")]
	[PermissionRequirement(new[] { GuildPermission.ManageRoles }, null)]
	[DefaultEnabled(true)]
	public sealed class ModifyRolePosition : AdvobotModuleBase
	{
		[Command]
		public async Task Command([VerifyObject(false, ObjectVerification.CanBeEdited, ObjectVerification.IsEveryone)] IRole role, uint position)
		{
			var newPos = await RoleUtils.ModifyRolePositionAsync(role, (int)position, new ModerationReason(Context.User, null)).CAF();
			if (newPos == -1)
			{
				var error = new Error($"Failed to give `{role.Format()}` the position `{position}`.");
				await MessageUtils.SendErrorMessageAsync(Context, error).CAF();
				return;
			}

			await MessageUtils.SendMessageAsync(Context.Channel, $"Successfully gave `{role.Format()}` the position `{newPos}`.").CAF();
		}
	}

	[Group(nameof(DisplayRolePositions)), TopLevelShortAlias(typeof(DisplayRolePositions))]
	[Summary("Lists the positions of each role on the guild.")]
	[PermissionRequirement(new[] { GuildPermission.ManageRoles }, null)]
	[DefaultEnabled(true)]
	public sealed class DisplayRolePositions : AdvobotModuleBase
	{
		[Command]
		public async Task Command()
		{
			var desc = String.Join("\n", Context.Guild.Roles.OrderByDescending(x => x.Position).Select(x =>
			{
				return x.Id == Context.Guild.EveryoneRole.Id
					? $"`{x.Position.ToString("00")}.` {Constants.FAKE_EVERYONE}"
					: $"`{x.Position.ToString("00")}.` {x.Name}";
			}));
			var embed = new EmbedWrapper
			{
				Title = "Role Positions",
				Description = desc
			};
			await MessageUtils.SendEmbedMessageAsync(Context.Channel, embed).CAF();
		}
	}

	[Group(nameof(ModifyRolePerms)), TopLevelShortAlias(typeof(ModifyRolePerms))]
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
					Description = $"`{String.Join("`, `", Enum.GetNames(typeof(GuildPermission)))}`"
				};
				await MessageUtils.SendEmbedMessageAsync(Context.Channel, embed).CAF();
			}
			[Command]
			public async Task Command([VerifyObject(false, ObjectVerification.CanBeEdited)] IRole role)
			{
				var currentRolePerms = Utils.GetNamesFromEnum((GuildPermission)role.Permissions.RawValue);
				var embed = new EmbedWrapper
				{
					Title = role.Name,
					Description = $"`{(currentRolePerms.Any() ? String.Join("`, `", currentRolePerms) : "No permission")}`"
				};
				await MessageUtils.SendEmbedMessageAsync(Context.Channel, embed).CAF();
			}
		}
		[Command(nameof(Allow)), ShortAlias(nameof(Allow))]
		public async Task Allow([VerifyObject(false, ObjectVerification.CanBeEdited)] IRole role, [Remainder, OverrideTypeReader(typeof(GuildPermissionsTypeReader))] ulong permissions)
		{
			var givenPerms = (await RoleUtils.ModifyRolePermissionsAsync(role, PermValue.Allow, permissions, Context.User as IGuildUser).CAF()).ToList();
			var resp = $"Successfully allowed `{(givenPerms.Any() ? String.Join("`, `", givenPerms) : "Nothing")}` for `{role.Format()}`.";
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}
		[Command(nameof(Deny)), ShortAlias(nameof(Deny))]
		public async Task Deny([VerifyObject(false, ObjectVerification.CanBeEdited)] IRole role, [Remainder, OverrideTypeReader(typeof(GuildPermissionsTypeReader))] ulong permissions)
		{
			var givenPerms = (await RoleUtils.ModifyRolePermissionsAsync(role, PermValue.Deny, permissions, Context.User as IGuildUser).CAF()).ToList();
			var resp = $"Successfully denied `{(givenPerms.Any() ? String.Join("`, `", givenPerms) : "Nothing")}` for `{role.Format()}`.";
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}
	}

	[Group(nameof(CopyRolePerms)), TopLevelShortAlias(typeof(CopyRolePerms))]
	[Summary("Copies the permissions from the first role to the second role. " +
		"Will not copy roles that the user does not have access to. " +
		"Will not overwrite roles that are above the user's top role.")]
	[PermissionRequirement(new[] { GuildPermission.ManageRoles }, null)]
	[DefaultEnabled(true)]
	public sealed class CopyRolePerms : AdvobotModuleBase
	{
		[Command]
		public async Task Command([VerifyObject(false, ObjectVerification.CanBeEdited)] IRole inputRole,
			[VerifyObject(false, ObjectVerification.CanBeEdited)] IRole outputRole)
		{
			var userBits = ((IGuildUser)Context.User).GuildPermissions.RawValue;
			var inputRoleBits = inputRole.Permissions.RawValue;
			var outputRoleBits = outputRole.Permissions.RawValue;
			var immovableBits = outputRoleBits & ~userBits;
			var copyBits = inputRoleBits & userBits;
			var newRoleBits = immovableBits | copyBits;

			await RoleUtils.ModifyRolePermissionsAsync(outputRole, newRoleBits, new ModerationReason(Context.User, null)).CAF();

			var immovablePerms = Utils.GetNamesFromEnum((GuildPermission)immovableBits);
			var failedToCopy = Utils.GetNamesFromEnum((GuildPermission)(inputRoleBits & ~copyBits));
			var newPerms = Utils.GetNamesFromEnum((GuildPermission)newRoleBits);
			var immovablePermsStr = immovablePerms.Any() ? "Output role had some permissions unable to be removed by you." : null;
			var failedToCopyStr = failedToCopy.Any() ? "Input role had some permission unable to be copied by you." : null;
			var newPermsStr = $"`{outputRole.Format()}` now has the following permissions: `{(newPerms.Any() ? String.Join("`, `", newPerms) : "Nothing")}`.";

			var response = GeneralFormatting.JoinNonNullStrings(" ", immovablePermsStr, failedToCopyStr, newPermsStr);
			await MessageUtils.SendMessageAsync(Context.Channel, response).CAF();
		}
	}

	[Group(nameof(ClearRolePerms)), TopLevelShortAlias(typeof(ClearRolePerms))]
	[Summary("Removes all permissions from a role.")]
	[PermissionRequirement(new[] { GuildPermission.ManageRoles }, null)]
	[DefaultEnabled(true)]
	public sealed class ClearRolePerms : AdvobotModuleBase
	{
		[Command]
		public async Task Command([VerifyObject(false, ObjectVerification.CanBeEdited)] IRole role)
		{
			var userBits = ((IGuildUser)Context.User).GuildPermissions.RawValue;
			var roleBits = role.Permissions.RawValue;
			var immovableBits = roleBits & ~userBits;

			await RoleUtils.ModifyRolePermissionsAsync(role, immovableBits, new ModerationReason(Context.User, null)).CAF();

			var immovablePerms = Utils.GetNamesFromEnum((GuildPermission)immovableBits);
			var immovablePermsStr = immovablePerms.Any() ? "Role had some permissions unable to be cleared by you." : null;
			var newPermsStr = $"`{role.Format()}` now has the following permissions: `{(immovablePerms.Any() ? String.Join("`, `", immovablePerms) : "Nothing")}`.";

			var response = GeneralFormatting.JoinNonNullStrings(" ", immovablePermsStr, newPermsStr);
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, response).CAF();
		}
	}

	[Group(nameof(ModifyRoleName)), TopLevelShortAlias(typeof(ModifyRoleName))]
	[Summary("Changes the name of the role.")]
	[PermissionRequirement(new[] { GuildPermission.ManageRoles }, null)]
	[DefaultEnabled(true)]
	public sealed class ModifyRoleName : AdvobotModuleBase
	{
		[Command, Priority(1)]
		public async Task Command([VerifyObject(false, ObjectVerification.CanBeEdited, ObjectVerification.IsEveryone)] IRole role,
			[Remainder, VerifyStringLength(Target.Role)] string name)
		{
			await RoleUtils.ModifyRoleNameAsync(role, name, new ModerationReason(Context.User, null)).CAF();
			var resp = $"Successfully changed the name of `{role.Format()}` to `{name}`.";
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}
		[Command(nameof(Position))]
		public async Task Position(uint rolePosition, [Remainder, VerifyStringLength(Target.Role)] string name)
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
			var result = role.Verify(Context, new[] { ObjectVerification.CanBeManaged, ObjectVerification.IsEveryone });
			if (!result.IsSuccess)
			{
				await MessageUtils.SendErrorMessageAsync(Context, new Error(result.ErrorReason)).CAF();
			}

			await RoleUtils.ModifyRoleNameAsync(role, name, new ModerationReason(Context.User, null)).CAF();
			var resp = $"Successfully changed the name of `{role.Format()}` to `{name}`.";
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}
	}

	[Group(nameof(ModifyRoleColor)), TopLevelShortAlias(typeof(ModifyRoleColor))]
	[Summary("Changes the role's color. " +
		"Color must be valid hexadecimal or the name of a default role color. " +
		"Inputting nothing displays the colors.")]
	[PermissionRequirement(new[] { GuildPermission.ManageRoles }, null)]
	[DefaultEnabled(true)]
	public sealed class ModifyRoleColor : AdvobotModuleBase
	{
		[Command]
		public async Task Command([Optional, VerifyObject(false, ObjectVerification.CanBeEdited, ObjectVerification.IsEveryone)] IRole role, [Optional] Color color)
		{
			if (role == null)
			{
				var embed = new EmbedWrapper
				{
					Title = "Colors",
					Description = $"`{String.Join("`, `", Constants.Colors.Keys)}`"
				};
				await MessageUtils.SendEmbedMessageAsync(Context.Channel, embed).CAF();
				return;
			}

			await RoleUtils.ModifyRoleColorAsync(role, color, new ModerationReason(Context.User, null)).CAF();
			var resp = $"Successfully changed the color of `{role.Format()}` to `#{color.RawValue.ToString("X6")}`."; //X6 to get hex
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}
	}

	[Group(nameof(ModifyRoleHoist)), TopLevelShortAlias(typeof(ModifyRoleHoist))]
	[Summary("Displays a role separately from others on the user list. " +
		"Saying the command again remove it from being hoisted.")]
	[PermissionRequirement(new[] { GuildPermission.ManageRoles }, null)]
	[DefaultEnabled(true)]
	public sealed class ModifyRoleHoist : AdvobotModuleBase
	{
		[Command]
		public async Task Command([VerifyObject(false, ObjectVerification.CanBeEdited, ObjectVerification.IsEveryone)] IRole role)
		{
			await RoleUtils.ModifyRoleHoistAsync(role, new ModerationReason(Context.User, null)).CAF();
			var resp = $"Successfully {(role.IsHoisted ? "dehoisted" : "hoisted")} `{role.Format()}`.";
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}
	}

	[Group(nameof(ModifyRoleMentionability)), TopLevelShortAlias(typeof(ModifyRoleMentionability))]
	[Summary("Allows the role to be mentioned. " +
		"Saying the command again removes its ability to be mentioned.")]
	[PermissionRequirement(new[] { GuildPermission.ManageRoles }, null)]
	[DefaultEnabled(true)]
	public sealed class ModifyRoleMentionability : AdvobotModuleBase
	{
		[Command]
		public async Task Command([VerifyObject(false, ObjectVerification.CanBeEdited, ObjectVerification.IsEveryone)] IRole role)
		{
			await RoleUtils.ModifyRoleMentionabilityAsync(role, new ModerationReason(Context.User, null)).CAF();
			var resp = $"Successfully made `{role.Format()}` {(role.IsMentionable ? "unmentionable" : "mentionable")}.";
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}
	}
}
