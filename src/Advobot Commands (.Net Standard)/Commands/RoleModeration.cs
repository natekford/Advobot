using Advobot.Core.Actions;
using Advobot.Core.Actions.Formatting;
using Advobot.Core.Classes;
using Advobot.Core.Classes.Attributes;
using Advobot.Core.Classes.Permissions;
using Advobot.Core.Classes.TypeReaders;
using Advobot.Core.Enums;
using Discord;
using Discord.Commands;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Advobot.Commands.RoleModeration
{
	[Group(nameof(GiveRole)), TopLevelShortAlias(typeof(GiveRole))]
	[Summary("Gives the role(s) to the user (assuming the person using the command and bot both have the ability to give that role).")]
	[PermissionRequirement(new[] { GuildPermission.ManageRoles }, null)]
	[DefaultEnabled(true)]
	public sealed class GiveRole : AdvobotModuleBase
	{
		[Command]
		public async Task Command(IGuildUser user,
			[VerifyObject(false, ObjectVerification.CanBeEdited, ObjectVerification.IsEveryone, ObjectVerification.IsManaged)] params IRole[] roles)
		{
			await RoleActions.GiveRolesAsync(user, roles, new ModerationReason(Context.User, null)).CAF();
			var resp = $"Successfully gave `{String.Join("`, `", roles.Select(x => x.FormatRole()))}` to `{user.FormatUser()}`.";
			await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}
	}

	[Group(nameof(TakeRole)), TopLevelShortAlias(typeof(TakeRole))]
	[Summary("Takes the role(s) from the user (assuming the person using the command and bot both have the ability to take that role).")]
	[PermissionRequirement(new[] { GuildPermission.ManageRoles }, null)]
	[DefaultEnabled(true)]
	public sealed class TakeRole : AdvobotModuleBase
	{
		[Command]
		public async Task Command(IGuildUser user, [VerifyObject(false, ObjectVerification.CanBeEdited, ObjectVerification.IsEveryone, ObjectVerification.IsManaged)] params IRole[] roles)
		{
			await RoleActions.TakeRolesAsync(user, roles, new ModerationReason(Context.User, null)).CAF();
			var resp = $"Successfully took `{String.Join("`, `", roles.Select(x => x.FormatRole()))}` from `{user.FormatUser()}`.";
			await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
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
			await RoleActions.CreateRoleAsync(Context.Guild, name, new ModerationReason(Context.User, null)).CAF();
			await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, $"Successfully created the role `{name}`.").CAF();
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

			await RoleActions.DeleteRoleAsync(role, new ModerationReason(Context.User, null)).CAF();
			var newRole = await Context.Guild.CreateRoleAsync(name, new GuildPermissions(0), color).CAF();

			await RoleActions.ModifyRolePositionAsync(newRole, position, new ModerationReason(Context.User, null)).CAF();
			await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, $"Successfully softdeleted `{role.Name}`.").CAF();
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
			await RoleActions.DeleteRoleAsync(role, new ModerationReason(Context.User, null)).CAF();
			await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, $"Successfully deleted `{role.FormatRole()}`.").CAF();
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
			var newPos = await RoleActions.ModifyRolePositionAsync(role, (int)position, new ModerationReason(Context.User, null)).CAF();
			if (newPos == -1)
			{
				var error = new ErrorReason($"Failed to give `{role.FormatRole()}` the position `{position}`.");
				await MessageActions.SendErrorMessageAsync(Context, error).CAF();
				return;
			}

			await MessageActions.SendMessageAsync(Context.Channel, $"Successfully gave `{role.FormatRole()}` the position `{newPos}`.").CAF();
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
			await MessageActions.SendEmbedMessageAsync(Context.Channel, new AdvobotEmbed("Role Positions", desc)).CAF();
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
				var desc = $"`{String.Join("`, `", GuildPerms.Permissions.Select(x => x.Name))}`";
				await MessageActions.SendEmbedMessageAsync(Context.Channel, new AdvobotEmbed("Guild Permission Types", desc)).CAF();
			}
			[Command]
			public async Task Command([VerifyObject(false, ObjectVerification.CanBeEdited)] IRole role)
			{
				var currentRolePerms = GuildPerms.ConvertValueToNames(role.Permissions.RawValue);
				var permissions = currentRolePerms.Any() ? String.Join("`, `", currentRolePerms) : "No permission";
				await MessageActions.SendEmbedMessageAsync(Context.Channel, new AdvobotEmbed(role.Name, $"`{permissions}`")).CAF();
			}
		}
		[Command(nameof(Allow)), ShortAlias(nameof(Allow))]
		public async Task Allow([VerifyObject(false, ObjectVerification.CanBeEdited)] IRole role, [Remainder, OverrideTypeReader(typeof(GuildPermissionsTypeReader))] ulong permissions)
		{
			var givenPerms = await RoleActions.ModifyRolePermissionsAsync(role, PermValue.Allow, permissions, Context.User as IGuildUser).CAF();
			var resp = $"Successfully allowed `{(givenPerms.Any() ? String.Join("`, `", givenPerms) : "Nothing")}` for `{role.FormatRole()}`.";
			await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}
		[Command(nameof(Deny)), ShortAlias(nameof(Deny))]
		public async Task Deny([VerifyObject(false, ObjectVerification.CanBeEdited)] IRole role, [Remainder, OverrideTypeReader(typeof(GuildPermissionsTypeReader))] ulong permissions)
		{
			var givenPerms = await RoleActions.ModifyRolePermissionsAsync(role, PermValue.Deny, permissions, Context.User as IGuildUser).CAF();
			var resp = $"Successfully denied `{(givenPerms.Any() ? String.Join("`, `", givenPerms) : "Nothing")}` for `{role.FormatRole()}`.";
			await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
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
			var userBits = (Context.User as IGuildUser).GuildPermissions.RawValue;
			var inputRoleBits = inputRole.Permissions.RawValue;
			var outputRoleBits = outputRole.Permissions.RawValue;
			var immovableBits = outputRoleBits & ~userBits;
			var copyBits = inputRoleBits & userBits;
			var newRoleBits = immovableBits | copyBits;

			await RoleActions.ModifyRolePermissionsAsync(outputRole, newRoleBits, new ModerationReason(Context.User, null)).CAF();

			var immovablePerms = GuildPerms.ConvertValueToNames(immovableBits);
			var failedToCopy = GuildPerms.ConvertValueToNames(inputRoleBits & ~copyBits);
			var newPerms = GuildPerms.ConvertValueToNames(newRoleBits);
			var immovablePermsStr = immovablePerms.Any() ? "Output role had some permissions unable to be removed by you." : null;
			var failedToCopyStr = failedToCopy.Any() ? "Input role had some permission unable to be copied by you." : null;
			var newPermsStr = $"`{outputRole.FormatRole()}` now has the following permissions: `{(newPerms.Any() ? String.Join("`, `", newPerms) : "Nothing")}`.";

			var response = GeneralFormatting.JoinNonNullStrings(" ", immovablePermsStr, failedToCopyStr, newPermsStr);
			await MessageActions.SendMessageAsync(Context.Channel, response).CAF();
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
			var userBits = (Context.User as IGuildUser).GuildPermissions.RawValue;
			var roleBits = role.Permissions.RawValue;
			var immovableBits = roleBits & ~userBits;

			await RoleActions.ModifyRolePermissionsAsync(role, immovableBits, new ModerationReason(Context.User, null)).CAF();

			var immovablePerms = GuildPerms.ConvertValueToNames(immovableBits);
			var immovablePermsStr = immovablePerms.Any() ? "Role had some permissions unable to be cleared by you." : null;
			var newPermsStr = $"`{role.FormatRole()}` now has the following permissions: `{(immovablePerms.Any() ? String.Join("`, `", immovablePerms) : "Nothing")}`.";

			var response = GeneralFormatting.JoinNonNullStrings(" ", immovablePermsStr, newPermsStr);
			await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, response).CAF();
		}
	}

	[Group(nameof(ModifyRoleName)), TopLevelShortAlias(typeof(ModifyRoleName))]
	[Summary("Changes the name of the role.")]
	[PermissionRequirement(new[] { GuildPermission.ManageRoles }, null)]
	[DefaultEnabled(true)]
	public sealed class ModifyRoleName : AdvobotModuleBase
	{
		[Command]
		public async Task Command([VerifyObject(false, ObjectVerification.CanBeEdited, ObjectVerification.IsEveryone)] IRole role,
			[Remainder, VerifyStringLength(Target.Role)] string name)
		{
			await RoleActions.ModifyRoleNameAsync(role, name, new ModerationReason(Context.User, null)).CAF();
			var resp = $"Successfully changed the name of `{role.FormatRole()}` to `{name}`.";
			await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}
		[Command]
		public async Task CommandByPosition([OverrideTypeReader(typeof(ObjectByPositionTypeReader<IRole>)), VerifyObject(false, ObjectVerification.CanBeEdited, ObjectVerification.IsEveryone)] IRole role,
			[Remainder, VerifyStringLength(Target.Role)] string name)
		{
			await RoleActions.ModifyRoleNameAsync(role, name, new ModerationReason(Context.User, null)).CAF();
			var resp = $"Successfully changed the name of `{role.FormatRole()}` to `{name}`.";
			await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
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
				var desc = $"`{String.Join("`, `", Colors.COLORS.Keys)}`";
				await MessageActions.SendEmbedMessageAsync(Context.Channel, new AdvobotEmbed("Colors", desc)).CAF();
				return;
			}

			await RoleActions.ModifyRoleColorAsync(role, color, new ModerationReason(Context.User, null)).CAF();
			var resp = $"Successfully changed the color of `{role.FormatRole()}` to `#{color.RawValue.ToString("X6")}`."; //X6 to get hex
			await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
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
			await RoleActions.ModifyRoleHoistAsync(role, new ModerationReason(Context.User, null)).CAF();
			var resp = $"Successfully {(role.IsHoisted ? "dehoisted" : "hoisted")} `{role.FormatRole()}`.";
			await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
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
			await RoleActions.ModifyRoleMentionabilityAsync(role, new ModerationReason(Context.User, null)).CAF();
			var resp = $"Successfully made `{role.FormatRole()}` {(role.IsMentionable ? "unmentionable" : "mentionable")}.";
			await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}
	}
}
