using Advobot.Actions;
using Advobot.Attributes;
using Advobot.Classes;
using Advobot.Enums;
using Advobot.TypeReaders;
using Discord;
using Discord.Commands;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Advobot.Commands.RoleModeration
{
	[Group(nameof(GiveRole)), Alias("gr")]
	[Usage("[User] [Role] <Role> ...")]
	[Summary("Gives the role(s) to the user (assuming the person using the command and bot both have the ability to give that role).")]
	[PermissionRequirement(new[] { GuildPermission.ManageRoles }, null)]
	[DefaultEnabled(true)]
	public sealed class GiveRole : MyModuleBase
	{
		[Command]
		public async Task Command(IGuildUser user, [VerifyRole(false, RoleVerification.CanBeEdited, RoleVerification.IsEveryone, RoleVerification.IsManaged)] params IRole[] roles)
		{
			await RoleActions.GiveRoles(user, roles, FormattingActions.FormatUserReason(Context.User));
			await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully gave `{String.Join("`, `", roles.Select(x => x.FormatRole()))}` to `{user.FormatUser()}`.");
		}
	}

	[Group(nameof(TakeRole)), Alias("tr")]
	[Usage("[User] [Role] <Role> ...")]
	[Summary("Takes the role(s) from the user (assuming the person using the command and bot both have the ability to take that role).")]
	[PermissionRequirement(new[] { GuildPermission.ManageRoles }, null)]
	[DefaultEnabled(true)]
	public sealed class TakeRole : MyModuleBase
	{
		[Command]
		public async Task Command(IGuildUser user, [VerifyRole(false, RoleVerification.CanBeEdited, RoleVerification.IsEveryone, RoleVerification.IsManaged)] params IRole[] roles)
		{
			await RoleActions.TakeRoles(user, roles, FormattingActions.FormatUserReason(Context.User));
			await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully took `{String.Join("`, `", roles.Select(x => x.FormatRole()))}` from `{user.FormatUser()}`.");
		}
	}

	[Group(nameof(CreateRole)), Alias("cr")]
	[Usage("[Name]")]
	[Summary("Adds a role to the guild with the chosen name.")]
	[PermissionRequirement(new[] { GuildPermission.ManageRoles }, null)]
	[DefaultEnabled(true)]
	public sealed class CreateRole : MyModuleBase
	{
		[Command]
		public async Task Command([VerifyStringLength(Target.Role)] string name)
		{
			await RoleActions.CreateRole(Context.Guild, name, FormattingActions.FormatUserReason(Context.User));
			await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully created the role `{name}`.");
		}
	}

	[Group(nameof(SoftDeleteRole)), Alias("sdr")]
	[Usage("[Role]")]
	[Summary("Removes all permissions from a role (and all channels the role had permissions on) and removes the role from everyone. Leaves the name and color behind.")]
	[PermissionRequirement(new[] { GuildPermission.ManageRoles }, null)]
	[DefaultEnabled(true)]
	public sealed class SoftDeleteRole : MyModuleBase
	{
		[Command]
		public async Task Command([VerifyRole(false, RoleVerification.CanBeEdited, RoleVerification.IsEveryone, RoleVerification.IsManaged)] IRole role)
		{
			//Get the properties of the role before it's deleted
			var name = role.Name;
			var color = role.Color;
			var position = role.Position;

			await RoleActions.DeleteRole(role, FormattingActions.FormatUserReason(Context.User));
			var newRole = await Context.Guild.CreateRoleAsync(name, new GuildPermissions(0), color);

			await RoleActions.ModifyRolePosition(newRole, position, FormattingActions.FormatUserReason(Context.User));
			await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully removed all permissions from the role `{role.Name}` and removed the role from all users on the guild.");
		}
	}

	[Group(nameof(DeleteRole)), Alias("dr")]
	[Usage("[Role]")]
	[Summary("Deletes the role.")]
	[PermissionRequirement(new[] { GuildPermission.ManageRoles }, null)]
	[DefaultEnabled(true)]
	public sealed class DeleteRole : MyModuleBase
	{
		[Command]
		public async Task Command([VerifyRole(false, RoleVerification.CanBeEdited, RoleVerification.IsEveryone, RoleVerification.IsManaged)] IRole role)
		{
			await RoleActions.DeleteRole(role, FormattingActions.FormatUserReason(Context.User));
			await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully deleted `{role.FormatRole()}`.");
		}
	}

	[Group(nameof(ChangeRolePosition)), Alias("crpo")]
	[Usage("[Role] <Position>")]
	[Summary("If only a role is input its position will be listed, else moves the role to the given position. " + Constants.FAKE_EVERYONE + " is the first position and starts at zero.")]
	[PermissionRequirement(new[] { GuildPermission.ManageRoles }, null)]
	[DefaultEnabled(true)]
	public sealed class ChangeRolePosition : MyModuleBase
	{
		[Command]
		public async Task Command([VerifyRole(false, RoleVerification.CanBeEdited, RoleVerification.IsEveryone)] IRole role, uint position)
		{
			var newPos = await RoleActions.ModifyRolePosition(role, (int)position, FormattingActions.FormatUserReason(Context.User));
			if (newPos != -1)
			{
				await MessageActions.SendChannelMessage(Context, $"Successfully gave `{role.FormatRole()}` the position `{newPos}`.");
			}
			else
			{
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Failed to give `{role.FormatRole()}` the position `{position}`.");
			}
		}
	}

	[Group(nameof(DisplayRolePositions)), Alias("drp")]
	[Usage("")]
	[Summary("Lists the positions of each role on the guild.")]
	[PermissionRequirement(new[] { GuildPermission.ManageRoles }, null)]
	[DefaultEnabled(true)]
	public sealed class DisplayRolePositions : MyModuleBase
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

	[Group(nameof(ChangeRolePerms)), Alias("crpe")]
	[Usage("[Show|Allow|Deny] <Role> <Permission/...>")]
	[Summary("Permissions must be separated by a `/`. Type `" + Constants.BOT_PREFIX + "rp [Show]` to see the available permissions. " +
		"Type `" + Constants.BOT_PREFIX + "rp [Show] [Role]` to see the permissions of that role. If you know the rawvalue of the perms you can say that instead.")]
	[PermissionRequirement(new[] { GuildPermission.ManageRoles }, null)]
	[DefaultEnabled(true)]
	public sealed class ChangeRolePerms : MyModuleBase
	{
		[Group(nameof(ActionType.Show)), Alias("s")]
		public sealed class Show : MyModuleBase
		{
			[Command]
			public async Task Command()
			{
				await MessageActions.SendEmbedMessage(Context.Channel, EmbedActions.MakeNewEmbed("Guild Permission Types", $"`{String.Join("`, `", Constants.GUILD_PERMISSIONS.Select(x => x.Name))}`"));
			}
			[Command]
			public async Task Command([VerifyRole(false, RoleVerification.CanBeEdited)] IRole role)
			{
				var currentRolePerms = Constants.GUILD_PERMISSIONS.Where(x => (role.Permissions.RawValue & x.Bit) != 0).Select(x => x.Name);
				var permissions = currentRolePerms.Any() ? String.Join("`, `", currentRolePerms) : "No permission";
				await MessageActions.SendEmbedMessage(Context.Channel, EmbedActions.MakeNewEmbed(role.Name, $"`{permissions}`"));
			}
		}
		[Group(nameof(ActionType.Allow)), Alias("a")]
		public sealed class Allow : MyModuleBase
		{
			[Command]
			public async Task Command([VerifyRole(false, RoleVerification.CanBeEdited)] IRole role, [Remainder, OverrideTypeReader(typeof(GuildPermissionsTypeReader))] ulong rawValue)
			{
				var givenPerms = await RoleActions.ModifyRolePermissions(role, ActionType.Allow, rawValue, Context.User as IGuildUser);
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully allowed `{(givenPerms.Any() ? String.Join("`, `", givenPerms) : "Nothing")}` for `{role.FormatRole()}`.");
			}
		}
		[Group(nameof(ActionType.Deny)), Alias("d")]
		public sealed class Deny : MyModuleBase
		{
			[Command]
			public async Task Command([VerifyRole(false, RoleVerification.CanBeEdited)] IRole role, [Remainder, OverrideTypeReader(typeof(GuildPermissionsTypeReader))] ulong rawValue)
			{
				var givenPerms = await RoleActions.ModifyRolePermissions(role, ActionType.Deny, rawValue, Context.User as IGuildUser);
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully denied `{(givenPerms.Any() ? String.Join("`, `", givenPerms) : "Nothing")}` for `{role.FormatRole()}`.");
			}
		}
	}

	[Group(nameof(CopyRolePerms)), Alias("corp")]
	[Usage("[Role] [Role]")]
	[Summary("Copies the permissions from the first role to the second role. Will not copy roles that the user does not have access to. Will not overwrite roles that are above the user's top role.")]
	[PermissionRequirement(new[] { GuildPermission.ManageRoles }, null)]
	[DefaultEnabled(true)]
	public sealed class CopyRolePerms : MyModuleBase
	{
		[Command]
		public async Task Command([VerifyRole(false, RoleVerification.CanBeEdited)] IRole inputRole,
									[VerifyRole(false, RoleVerification.CanBeEdited)] IRole outputRole)
		{
			var userBits = (Context.User as IGuildUser).GuildPermissions.RawValue;
			var inputRoleBits = inputRole.Permissions.RawValue;
			var outputRoleBits = outputRole.Permissions.RawValue;
			/* Keep perms on the ouput which the user is unable to edit. E.G:
				* Role:			1001	1001
				* User:			0001 -> 1110
				* Immovable:		1000	1000
				*/
			var immovableBits = outputRoleBits & ~userBits;
			/* Only add in perms the user can edit. E.G:
				* Role:			1111
				* User:			0001
				* Copyable:		0001
				*/
			var copyBits = inputRoleBits & userBits;
			/* Keep immovable bits and add in copyable bits. E.G:
				* Immovable:		1000
				* Copyable:		0001
				* Output:			1001
				*/
			var newRoleBits = immovableBits | copyBits;

			await RoleActions.ModifyRolePermissions(outputRole, newRoleBits, FormattingActions.FormatUserReason(Context.User));

			var immovablePerms = GetActions.GetGuildPermissionNames(immovableBits);
			var failedToCopy = GetActions.GetGuildPermissionNames(inputRoleBits & ~copyBits);
			var newPerms = GetActions.GetGuildPermissionNames(newRoleBits);
			var immovablePermsStr = immovablePerms.Any() ? "Output role had some permissions unable to be removed by you." : null;
			var failedToCopyStr = failedToCopy.Any() ? "Input role had some permission unable to be copied by you." : null;
			var newPermsStr = $"`{outputRole.FormatRole()}` now has the following permissions: `{(newPerms.Any() ? String.Join("`, `", newPerms) : "Nothing")}`.";

			var response = FormattingActions.JoinNonNullStrings(" ", immovablePermsStr, failedToCopyStr, newPermsStr);
			await MessageActions.SendChannelMessage(Context, response);
		}
	}

	[Group(nameof(ClearRolePerms)), Alias("clrp")]
	[Usage("[Role]")]
	[Summary("Removes all permissions from a role.")]
	[PermissionRequirement(new[] { GuildPermission.ManageRoles }, null)]
	[DefaultEnabled(true)]
	public sealed class ClearRolePerms : MyModuleBase
	{
		[Command]
		public async Task Command([VerifyRole(false, RoleVerification.CanBeEdited)] IRole role)
		{
			var userBits = (Context.User as IGuildUser).GuildPermissions.RawValue;
			var roleBits = role.Permissions.RawValue;
			var immovableBits = roleBits & ~userBits;

			await RoleActions.ModifyRolePermissions(role, immovableBits, FormattingActions.FormatUserReason(Context.User));

			var immovablePerms = GetActions.GetGuildPermissionNames(immovableBits);
			var immovablePermsStr = immovablePerms.Any() ? "Role had some permissions unable to be cleared by you." : null;
			var newPermsStr = $"`{role.FormatRole()}` now has the following permissions: `{(immovablePerms.Any() ? String.Join("`, `", immovablePerms) : "Nothing")}`.";

			var response = FormattingActions.JoinNonNullStrings(" ", immovablePermsStr, newPermsStr);
			await MessageActions.MakeAndDeleteSecondaryMessage(Context, response);
		}
	}

	[Group(nameof(ChangeRoleName)), Alias("crn")]
	[Usage("[Role] [Name]")]
	[Summary("Changes the name of the role.")]
	[PermissionRequirement(new[] { GuildPermission.ManageRoles }, null)]
	[DefaultEnabled(true)]
	public sealed class ChangeRoleName : MyModuleBase
	{
		[Command]
		public async Task Command([VerifyRole(false, RoleVerification.CanBeEdited, RoleVerification.IsEveryone)] IRole role, [Remainder, VerifyStringLength(Target.Role)] string name)
		{
			await RoleActions.ModifyRoleName(role, name, FormattingActions.FormatUserReason(Context.User));
			await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully changed the name of `{role.FormatRole()}` to `{name}`.");
		}
	}

	[Group(nameof(ChangeRoleNameByPosition)), Alias("crnbp")]
	[Usage("[Number] [Name]")]
	[Summary("Changes the name of the role with the given position. This is *extremely* useful for when multiple roles have the same name but you want to edit things")]
	[PermissionRequirement(new[] { GuildPermission.ManageRoles }, null)]
	[DefaultEnabled(true)]
	public sealed class ChangeRoleNameByPosition : MyModuleBase
	{
		[Command]
		public async Task Command(uint position, [Remainder, VerifyStringLength(Target.Role)] string name)
		{
			if (position > UserActions.GetUserPosition(Context.User))
			{
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, FormattingActions.ERROR("Your position is less than the role's."));
				return;
			}

			var role = Context.Guild.Roles.FirstOrDefault(x => x.Position == (int)position);
			if (role == null)
			{
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, FormattingActions.ERROR("No role has the given position."));
				return;
			}

			await RoleActions.ModifyRoleName(role, name, FormattingActions.FormatUserReason(Context.User));
			await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully changed the name of `{role.FormatRole()}` to `{name}`.");
		}
	}

	[Group(nameof(ChangeRoleColor)), Alias("crc")]
	[Usage("<Role> <Hexadecimal|Color Name>")]
	[Summary("Changes the role's color. Color must be valid hexadecimal or the name of a default role color. Inputting nothing displays the colors.")]
	[PermissionRequirement(new[] { GuildPermission.ManageRoles }, null)]
	[DefaultEnabled(true)]
	public sealed class ChangeRoleColor : MyModuleBase
	{
		[Command]
		public async Task Command([Optional, VerifyRole(false, RoleVerification.CanBeEdited, RoleVerification.IsEveryone)] IRole role, [Optional] Color color)
		{
			if (role == null)
			{
				var desc = $"`{String.Join("`, `", Constants.COLORS.Keys)}`";
				await MessageActions.SendEmbedMessage(Context.Channel, EmbedActions.MakeNewEmbed("Colors", desc));
				return;
			}

			await RoleActions.ModifyRoleColor(role, color, FormattingActions.FormatUserReason(Context.User)); //Use .ToString("X6") to get a hex string that's 6 characters long
			await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully changed the color of `{role.FormatRole()}` to `#{color.RawValue.ToString("X6")}`.");
		}
	}

	[Group(nameof(ChangeRoleHoist)), Alias("crh")]
	[Usage("[Role]")]
	[Summary("Displays a role separately from others on the user list. Saying the command again remove it from being hoisted.")]
	[PermissionRequirement(new[] { GuildPermission.ManageRoles }, null)]
	[DefaultEnabled(true)]
	public sealed class ChangeRoleHoist : MyModuleBase
	{
		[Command]
		public async Task Command([VerifyRole(false, RoleVerification.CanBeEdited, RoleVerification.IsEveryone)] IRole role)
		{
			await RoleActions.ModifyRoleHoist(role, FormattingActions.FormatUserReason(Context.User));
			await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully {(role.IsHoisted ? "dehoisted" : "hoisted")} `{role.FormatRole()}`.");
		}
	}

	[Group(nameof(ChangeRoleMentionability)), Alias("crma")]
	[Usage("[Role]")]
	[Summary("Allows the role to be mentioned. Saying the command again removes its ability to be mentioned.")]
	[PermissionRequirement(new[] { GuildPermission.ManageRoles }, null)]
	[DefaultEnabled(true)]
	public sealed class ChangeRoleMentionability : MyModuleBase
	{
		[Command]
		public async Task Command([VerifyRole(false, RoleVerification.CanBeEdited, RoleVerification.IsEveryone)] IRole role)
		{
			await RoleActions.ModifyRoleMentionability(role, FormattingActions.FormatUserReason(Context.User));
			await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully made `{role.FormatRole()}` {(role.IsMentionable ? "unmentionable" : "mentionable")}.");
		}
	}
}
