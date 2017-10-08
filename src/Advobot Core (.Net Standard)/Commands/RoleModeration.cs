using Advobot.Actions;
using Advobot.Actions.Formatting;
using Advobot.Classes;
using Advobot.Classes.Attributes;
using Advobot.Classes.Permissions;
using Advobot.Classes.TypeReaders;
using Advobot.Enums;
using Discord;
using Discord.Commands;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Advobot.Commands.RoleModeration
{
	[Group(nameof(GiveRole)), TopLevelShortAlias(nameof(GiveRole))]
	[Usage("[User] [Role] <Role> ...")]
	[Summary("Gives the role(s) to the user (assuming the person using the command and bot both have the ability to give that role).")]
	[PermissionRequirement(new[] { GuildPermission.ManageRoles }, null)]
	[DefaultEnabled(true)]
	public sealed class GiveRole : AdvobotModuleBase
	{
		[Command]
		public async Task Command(IGuildUser user, [VerifyObject(false, ObjectVerification.CanBeEdited, ObjectVerification.IsEveryone, ObjectVerification.IsManaged)] params IRole[] roles)
		{
			await RoleActions.GiveRoles(user, roles, new ModerationReason(Context.User, null));
			await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully gave `{String.Join("`, `", roles.Select(x => x.FormatRole()))}` to `{user.FormatUser()}`.");
		}
	}

	[Group(nameof(TakeRole)), TopLevelShortAlias(nameof(TakeRole))]
	[Usage("[User] [Role] <Role> ...")]
	[Summary("Takes the role(s) from the user (assuming the person using the command and bot both have the ability to take that role).")]
	[PermissionRequirement(new[] { GuildPermission.ManageRoles }, null)]
	[DefaultEnabled(true)]
	public sealed class TakeRole : AdvobotModuleBase
	{
		[Command]
		public async Task Command(IGuildUser user, [VerifyObject(false, ObjectVerification.CanBeEdited, ObjectVerification.IsEveryone, ObjectVerification.IsManaged)] params IRole[] roles)
		{
			await RoleActions.TakeRoles(user, roles, new ModerationReason(Context.User, null));
			await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully took `{String.Join("`, `", roles.Select(x => x.FormatRole()))}` from `{user.FormatUser()}`.");
		}
	}

	[Group(nameof(CreateRole)), TopLevelShortAlias(nameof(CreateRole))]
	[Usage("[Name]")]
	[Summary("Adds a role to the guild with the chosen name.")]
	[PermissionRequirement(new[] { GuildPermission.ManageRoles }, null)]
	[DefaultEnabled(true)]
	public sealed class CreateRole : AdvobotModuleBase
	{
		[Command]
		public async Task Command([VerifyStringLength(Target.Role)] string name)
		{
			await RoleActions.CreateRole(Context.Guild, name, new ModerationReason(Context.User, null));
			await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully created the role `{name}`.");
		}
	}

	[Group(nameof(SoftDeleteRole)), TopLevelShortAlias(nameof(SoftDeleteRole))]
	[Usage("[Role]")]
	[Summary("Removes all permissions from a role (and all channels the role had permissions on) and removes the role from everyone. Leaves the name and color behind.")]
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

			await RoleActions.DeleteRole(role, new ModerationReason(Context.User, null));
			var newRole = await Context.Guild.CreateRoleAsync(name, new GuildPermissions(0), color);

			await RoleActions.ModifyRolePosition(newRole, position, new ModerationReason(Context.User, null));
			await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully removed all permissions from the role `{role.Name}` and removed the role from all users on the guild.");
		}
	}

	[Group(nameof(DeleteRole)), TopLevelShortAlias(nameof(DeleteRole))]
	[Usage("[Role]")]
	[Summary("Deletes the role.")]
	[PermissionRequirement(new[] { GuildPermission.ManageRoles }, null)]
	[DefaultEnabled(true)]
	public sealed class DeleteRole : AdvobotModuleBase
	{
		[Command]
		public async Task Command([VerifyObject(false, ObjectVerification.CanBeEdited, ObjectVerification.IsEveryone, ObjectVerification.IsManaged)] IRole role)
		{
			await RoleActions.DeleteRole(role, new ModerationReason(Context.User, null));
			await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully deleted `{role.FormatRole()}`.");
		}
	}

	[Group(nameof(ModifyRolePosition)), TopLevelShortAlias(nameof(ModifyRolePosition))]
	[Usage("[Role] <Position>")]
	[Summary("If only a role is input its position will be listed, else moves the role to the given position. " + Constants.FAKE_EVERYONE + " is the first position and starts at zero.")]
	[PermissionRequirement(new[] { GuildPermission.ManageRoles }, null)]
	[DefaultEnabled(true)]
	public sealed class ModifyRolePosition : AdvobotModuleBase
	{
		[Command]
		public async Task Command([VerifyObject(false, ObjectVerification.CanBeEdited, ObjectVerification.IsEveryone)] IRole role, uint position)
		{
			var newPos = await RoleActions.ModifyRolePosition(role, (int)position, new ModerationReason(Context.User, null));
			if (newPos != -1)
			{
				await MessageActions.SendMessage(Context.Channel, $"Successfully gave `{role.FormatRole()}` the position `{newPos}`.");
			}
			else
			{
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Failed to give `{role.FormatRole()}` the position `{position}`.");
			}
		}
	}

	[Group(nameof(DisplayRolePositions)), TopLevelShortAlias(nameof(DisplayRolePositions))]
	[Usage("")]
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
				if (x.Id == Context.Guild.EveryoneRole.Id)
				{
					return $"`{x.Position.ToString("00")}.` {Constants.FAKE_EVERYONE}";
				}
				else
				{
					return $"`{x.Position.ToString("00")}.` {x.Name}";
				}
			}));
			await MessageActions.SendEmbedMessage(Context.Channel, EmbedActions.MakeNewEmbed("Role Positions", desc));
		}
	}

	[Group(nameof(ModifyRolePerms)), TopLevelShortAlias(nameof(ModifyRolePerms))]
	[Usage("[Show|Allow|Deny] <Role> <Permission/...>")]
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
				await MessageActions.SendEmbedMessage(Context.Channel, EmbedActions.MakeNewEmbed("Guild Permission Types", desc));
			}
			[Command]
			public async Task Command([VerifyObject(false, ObjectVerification.CanBeEdited)] IRole role)
			{
				var currentRolePerms = GuildPerms.ConvertValueToNames(role.Permissions.RawValue);
				var permissions = currentRolePerms.Any() ? String.Join("`, `", currentRolePerms) : "No permission";
				await MessageActions.SendEmbedMessage(Context.Channel, EmbedActions.MakeNewEmbed(role.Name, $"`{permissions}`"));
			}
		}
		[Command(nameof(Allow)), ShortAlias(nameof(Allow))]
		public async Task Allow([VerifyObject(false, ObjectVerification.CanBeEdited)] IRole role, [Remainder, OverrideTypeReader(typeof(GuildPermissionsTypeReader))] ulong rawValue)
		{
			var givenPerms = await RoleActions.ModifyRolePermissions(role, PermValue.Allow, rawValue, Context.User as IGuildUser);
			await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully allowed `{(givenPerms.Any() ? String.Join("`, `", givenPerms) : "Nothing")}` for `{role.FormatRole()}`.");
		}
		[Command(nameof(Deny)), ShortAlias(nameof(Deny))]
		public async Task Deny([VerifyObject(false, ObjectVerification.CanBeEdited)] IRole role, [Remainder, OverrideTypeReader(typeof(GuildPermissionsTypeReader))] ulong rawValue)
		{
			var givenPerms = await RoleActions.ModifyRolePermissions(role, PermValue.Deny, rawValue, Context.User as IGuildUser);
			await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully denied `{(givenPerms.Any() ? String.Join("`, `", givenPerms) : "Nothing")}` for `{role.FormatRole()}`.");
		}
	}

	[Group(nameof(CopyRolePerms)), TopLevelShortAlias(nameof(CopyRolePerms))]
	[Usage("[Role] [Role]")]
	[Summary("Copies the permissions from the first role to the second role. Will not copy roles that the user does not have access to. Will not overwrite roles that are above the user's top role.")]
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

			await RoleActions.ModifyRolePermissions(outputRole, newRoleBits, new ModerationReason(Context.User, null));

			var immovablePerms = GuildPerms.ConvertValueToNames(immovableBits);
			var failedToCopy = GuildPerms.ConvertValueToNames(inputRoleBits & ~copyBits);
			var newPerms = GuildPerms.ConvertValueToNames(newRoleBits);
			var immovablePermsStr = immovablePerms.Any() ? "Output role had some permissions unable to be removed by you." : null;
			var failedToCopyStr = failedToCopy.Any() ? "Input role had some permission unable to be copied by you." : null;
			var newPermsStr = $"`{outputRole.FormatRole()}` now has the following permissions: `{(newPerms.Any() ? String.Join("`, `", newPerms) : "Nothing")}`.";

			var response = GeneralFormatting.JoinNonNullStrings(" ", immovablePermsStr, failedToCopyStr, newPermsStr);
			await MessageActions.SendMessage(Context.Channel, response);
		}
	}

	[Group(nameof(ClearRolePerms)), TopLevelShortAlias(nameof(ClearRolePerms))]
	[Usage("[Role]")]
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

			await RoleActions.ModifyRolePermissions(role, immovableBits, new ModerationReason(Context.User, null));

			var immovablePerms = GuildPerms.ConvertValueToNames(immovableBits);
			var immovablePermsStr = immovablePerms.Any() ? "Role had some permissions unable to be cleared by you." : null;
			var newPermsStr = $"`{role.FormatRole()}` now has the following permissions: `{(immovablePerms.Any() ? String.Join("`, `", immovablePerms) : "Nothing")}`.";

			var response = GeneralFormatting.JoinNonNullStrings(" ", immovablePermsStr, newPermsStr);
			await MessageActions.MakeAndDeleteSecondaryMessage(Context, response);
		}
	}

	[Group(nameof(ModifyRoleName)), TopLevelShortAlias(nameof(ModifyRoleName))]
	[Usage("[Role] [Name]")]
	[Summary("Changes the name of the role.")]
	[PermissionRequirement(new[] { GuildPermission.ManageRoles }, null)]
	[DefaultEnabled(true)]
	public sealed class ModifyRoleName : AdvobotModuleBase
	{
		[Command]
		public async Task Command([VerifyObject(false, ObjectVerification.CanBeEdited, ObjectVerification.IsEveryone)] IRole role, [Remainder, VerifyStringLength(Target.Role)] string name)
		{
			await RoleActions.ModifyRoleName(role, name, new ModerationReason(Context.User, null));
			await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully changed the name of `{role.FormatRole()}` to `{name}`.");
		}
	}

	/*
	[Group(nameof(ChangeRoleNameByPosition)), Alias("crnbp")]
	[Usage("[Number] [Name]")]
	[Summary("Changes the name of the role with the given position. This is *extremely* useful for when multiple roles have the same name but you want to edit things")]
	[PermissionRequirement(new[] { GuildPermission.ManageRoles }, null)]
	[DefaultEnabled(true)]
	public sealed class ChangeRoleNameByPosition : AdvobotModuleBase
	{
		[Command]
		public async Task Command(uint position, [Remainder, VerifyStringLength(Target.Role)] string name)
		{
			if (position > Context.User.GetPosition())
			{
				await MessageActions.SendErrorMessage(Context, new ErrorReason("Your position is less than the role's."));
				return;
			}

			var role = Context.Guild.Roles.FirstOrDefault(x => x.Position == (int)position);
			if (role == null)
			{
				await MessageActions.SendErrorMessage(Context, new ErrorReason("No role has the given position."));
				return;
			}

			await RoleActions.ModifyRoleName(role, name, new ModerationReason(Context.User, null));
			await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully changed the name of `{role.FormatRole()}` to `{name}`.");
		}
	}*/

	[Group(nameof(ModifyRoleColor)), TopLevelShortAlias(nameof(ModifyRoleColor))]
	[Usage("<Role> <Hexadecimal|Color Name>")]
	[Summary("Changes the role's color. Color must be valid hexadecimal or the name of a default role color. Inputting nothing displays the colors.")]
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
				await MessageActions.SendEmbedMessage(Context.Channel, EmbedActions.MakeNewEmbed("Colors", desc));
				return;
			}

			await RoleActions.ModifyRoleColor(role, color, new ModerationReason(Context.User, null)); //Use .ToString("X6") to get a hex string that's 6 characters long
			await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully changed the color of `{role.FormatRole()}` to `#{color.RawValue.ToString("X6")}`.");
		}
	}

	[Group(nameof(ModifyRoleHoist)), TopLevelShortAlias(nameof(ModifyRoleHoist))]
	[Usage("[Role]")]
	[Summary("Displays a role separately from others on the user list. Saying the command again remove it from being hoisted.")]
	[PermissionRequirement(new[] { GuildPermission.ManageRoles }, null)]
	[DefaultEnabled(true)]
	public sealed class ModifyRoleHoist : AdvobotModuleBase
	{
		[Command]
		public async Task Command([VerifyObject(false, ObjectVerification.CanBeEdited, ObjectVerification.IsEveryone)] IRole role)
		{
			await RoleActions.ModifyRoleHoist(role, new ModerationReason(Context.User, null));
			await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully {(role.IsHoisted ? "dehoisted" : "hoisted")} `{role.FormatRole()}`.");
		}
	}

	[Group(nameof(ModifyRoleMentionability)), TopLevelShortAlias(nameof(ModifyRoleMentionability))]
	[Usage("[Role]")]
	[Summary("Allows the role to be mentioned. Saying the command again removes its ability to be mentioned.")]
	[PermissionRequirement(new[] { GuildPermission.ManageRoles }, null)]
	[DefaultEnabled(true)]
	public sealed class ModifyRoleMentionability : AdvobotModuleBase
	{
		[Command]
		public async Task Command([VerifyObject(false, ObjectVerification.CanBeEdited, ObjectVerification.IsEveryone)] IRole role)
		{
			await RoleActions.ModifyRoleMentionability(role, new ModerationReason(Context.User, null));
			await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully made `{role.FormatRole()}` {(role.IsMentionable ? "unmentionable" : "mentionable")}.");
		}
	}
}
