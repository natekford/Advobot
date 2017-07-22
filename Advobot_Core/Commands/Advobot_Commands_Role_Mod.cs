using Advobot.Actions;
using Advobot.Attributes;
using Advobot.Enums;
using Advobot.NonSavedClasses;
using Discord;
using Discord.Commands;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Advobot
{
	namespace RoleModeration
	{
		[Group("giverole"), Alias("gr")]
		[Usage("[User] [Role] <Role> ...")]
		[Summary("Gives the user the role (assuming the person using the command and bot both have the ability to give that role).")]
		[PermissionRequirement(new[] { GuildPermission.ManageRoles }, null)]
		[DefaultEnabled(true)]
		public sealed class GiveRole : MyModuleBase
		{
			[Command]
			public async Task Command(IGuildUser user, [VerifyObject(false, ObjectVerification.CanBeEdited, ObjectVerification.IsEveryone, ObjectVerification.IsManaged)] params IRole[] roles)
			{
				await CommandRunner(user, roles);
			}

			private async Task CommandRunner(IGuildUser user, IRole[] roles)
			{
				await RoleActions.GiveRoles(user, roles);
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully gave the following roles to `{0}`: `{1}`.", user.FormatUser(), String.Join("`, `", roles.Select(x => x.FormatRole()))));
			}
		}

		[Group("takerole"), Alias("tr")]
		[Usage("[User] [Role] <Role> ...")]
		[Summary("Take the role from the user (assuming the person using the command and bot both have the ability to take that role).")]
		[PermissionRequirement(new[] { GuildPermission.ManageRoles }, null)]
		[DefaultEnabled(true)]
		public sealed class TakeRole : MyModuleBase
		{
			[Command]
			public async Task Command(IGuildUser user, [VerifyObject(false, ObjectVerification.CanBeEdited, ObjectVerification.IsEveryone, ObjectVerification.IsManaged)] params IRole[] roles)
			{
				await CommandRunner(user, roles);
			}

			private async Task CommandRunner(IGuildUser user, IRole[] roles)
			{
				await RoleActions.TakeRoles(user, roles);
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully took the following roles from `{0}`: `{1}`.", user.FormatUser(), String.Join("`, `", roles.Select(x => x.FormatRole()))));
			}
		}

		[Group("createrole"), Alias("cr")]
		[Usage("[Name]")]
		[Summary("Adds a role to the guild with the chosen name.")]
		[PermissionRequirement(new[] { GuildPermission.ManageRoles }, null)]
		[DefaultEnabled(true)]
		public sealed class CreateRole : MyModuleBase
		{
			[Command]
			public async Task Command([Remainder, VerifyStringLength(Target.Role)] string name)
			{
				await CommandRunner(name);
			}

			private async Task CommandRunner(string name)
			{
				await Context.Guild.CreateRoleAsync(name, new GuildPermissions(0));
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully created the role `{0}`.", name));
			}
		}

		[Group("softdeleterole"), Alias("sdr")]
		[Usage("[Role]")]
		[Summary("Removes all permissions from a role (and all channels the role had permissions on) and removes the role from everyone. Leaves the name and color behind.")]
		[PermissionRequirement(new[] { GuildPermission.ManageRoles }, null)]
		[DefaultEnabled(true)]
		public sealed class SoftDeleteRole : MyModuleBase
		{
			[Command]
			public async Task Command([VerifyObject(false, ObjectVerification.CanBeEdited, ObjectVerification.IsEveryone, ObjectVerification.IsManaged)] IRole role)
			{
				await CommandRunner(role);
			}

			private async Task CommandRunner(IRole role)
			{
				//Get the properties of the role before it's deleted
				var name = role.Name;
				var color = role.Color;
				var position = role.Position;

				await role.DeleteAsync();
				var newRole = await Context.Guild.CreateRoleAsync(name, new GuildPermissions(0), color);

				await RoleActions.ModifyRolePosition(newRole, position);
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully removed all permissions from the role `{0}` and removed the role from all users on the guild.", role.Name));
			}
		}

		[Group("deleterole"), Alias("dr")]
		[Usage("[Role]")]
		[Summary("Deletes the role.")]
		[PermissionRequirement(new[] { GuildPermission.ManageRoles }, null)]
		[DefaultEnabled(true)]
		public sealed class DeleteRole : MyModuleBase
		{
			[Command]
			public async Task Command([VerifyObject(false, ObjectVerification.CanBeEdited, ObjectVerification.IsEveryone, ObjectVerification.IsManaged)] IRole role)
			{
				await CommandRunner(role);
			}

			private async Task CommandRunner(IRole role)
			{
				await role.DeleteAsync();
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully deleted `{0}`.", role.FormatRole()));
			}
		}

		[Group("changeroleposition"), Alias("crpo")]
		[Usage("[Role] <Position>")]
		[Summary("If only a role is input its position will be listed, else moves the role to the given position. " + Constants.FAKE_EVERYONE + " is the first position and starts at zero.")]
		[PermissionRequirement(new[] { GuildPermission.ManageRoles }, null)]
		[DefaultEnabled(true)]
		public sealed class ChangeRolePosition : MyModuleBase
		{
			[Command]
			public async Task Command([VerifyObject(false, ObjectVerification.CanBeEdited, ObjectVerification.IsEveryone)] IRole role, uint position)
			{
				await CommandRunner(role, position);
			}

			private async Task CommandRunner(IRole role, uint position)
			{
				var newPos = await RoleActions.ModifyRolePosition(role, (int)position);
				if (newPos != -1)
				{
					await MessageActions.SendChannelMessage(Context, String.Format("Successfully gave `{0}` the position `{1}`.", role.FormatRole(), newPos));
				}
				else
				{
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, String.Format("Failed to give `{0}` the position `{1}`.", role.FormatRole(), position));
				}
			}
		}

		[Group("displayrolepositions"), Alias("drp")]
		[Usage("")]
		[Summary("Lists the positions of each role on the guild.")]
		[PermissionRequirement(new[] { GuildPermission.ManageRoles }, null)]
		[DefaultEnabled(true)]
		public sealed class DisplayRolePositions : MyModuleBase
		{
			[Command]
			public async Task Command()
			{
				await CommandRunner();
			}

			private async Task CommandRunner()
			{
				var desc = String.Join("\n", Context.Guild.Roles.OrderByDescending(x => x.Position).Select(x =>
				{
					if (x.Id == Context.Guild.EveryoneRole.Id)
					{
						return String.Format("`{0}.` {1}", x.Position.ToString("00"), Constants.FAKE_EVERYONE);
					}
					else
					{
						return String.Format("`{0}.` {1}", x.Position.ToString("00"), x.Name);
					}
				}));
				await MessageActions.SendEmbedMessage(Context.Channel, EmbedActions.MakeNewEmbed("Role Positions", desc));
			}
		}

		[Group("changeroleperms"), Alias("crpe")]
		[Usage("[Show|Allow|Deny] <Role> <Permission/...>")]
		[Summary("Permissions must be separated by a `/`. Type `" + Constants.BOT_PREFIX + "rp [Show]` to see the available permissions. " +
			"Type `" + Constants.BOT_PREFIX + "rp [Show] [Role]` to see the permissions of that role.")]
		[PermissionRequirement(new[] { GuildPermission.ManageRoles }, null)]
		[DefaultEnabled(true)]
		public sealed class ChangeRolePerms : MyModuleBase
		{
			[Command]
			public async Task Command([VerifyEnum((uint)(ActionType.Allow | ActionType.Deny))] ActionType actionType,
									  [VerifyObject(false, ObjectVerification.CanBeEdited)] IRole role,
									  [Remainder] string uncutPermissions)
			{
				await CommandRunner(actionType, role, uncutPermissions);
			}
			[Command]
			public async Task Command([VerifyEnum((uint)ActionType.Show)] ActionType actionType,
									  [Optional, VerifyObject(false, ObjectVerification.CanBeEdited)] IRole role)
			{
				await CommandRunner(role);
			}

			private async Task CommandRunner(ActionType actionType, IRole role, string uncutPermissions)
			{
				var permissions = uncutPermissions.Split('/', ' ').Select(x => x.Trim(',')).ToList();
				var validPerms = permissions.Where(x => Constants.GUILD_PERMISSIONS.Select(y => y.Name).CaseInsContains(x));
				var invalidPerms = permissions.Where(x => !Constants.GUILD_PERMISSIONS.Select(y => y.Name).CaseInsContains(x));
				if (invalidPerms.Any())
				{
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, FormattingActions.ERROR(String.Format("Invalid permission{0} provided: `{1}`.",
						GetActions.GetPlural(invalidPerms.Count()),
						String.Join("`, `", invalidPerms))));
					return;
				}

				ulong changeValue = 0;
				foreach (var permission in permissions)
				{
					changeValue = GuildActions.AddGuildPermissionBit(permission, changeValue);
				}

				//Only modify permissions the user has the ability to
				changeValue &= (Context.User as IGuildUser).GuildPermissions.RawValue;

				var actionStr = "";
				var roleBits = role.Permissions.RawValue;
				switch (actionType)
				{
					case ActionType.Allow:
					{
						actionStr = "allowed";
						roleBits |= changeValue;
						break;
					}
					case ActionType.Deny:
					{
						actionStr = "denied";
						roleBits &= ~changeValue;
						break;
					}
				}

				await role.ModifyAsync(x => x.Permissions = new GuildPermissions(roleBits));

				var changedPerms = GetActions.GetPermissionNames(changeValue);
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully {0} `{1}` for `{2}`.",
					actionStr,
					changedPerms.Any() ? String.Join("`, `", changedPerms) : "Nothing",
					role.FormatRole()));
			}
			private async Task CommandRunner(IRole role)
			{
				if (role == null)
				{
					await MessageActions.SendEmbedMessage(Context.Channel, EmbedActions.MakeNewEmbed("Guild Permission Types", String.Format("`{0}`", String.Join("`, `", Constants.GUILD_PERMISSIONS.Select(x => x.Name)))));
					return;
				}

				var currentRolePerms = Constants.GUILD_PERMISSIONS.Where(x => (role.Permissions.RawValue & x.Bit) != 0).Select(x => x.Name);
				var permissions = currentRolePerms.Any() ? String.Join("`, `", currentRolePerms) : "No permission";
				await MessageActions.SendEmbedMessage(Context.Channel, EmbedActions.MakeNewEmbed(role.Name, String.Format("`{0}`", permissions)));
			}
		}

		[Group("copyroleperms"), Alias("corp")]
		[Usage("[Role] [Role]")]
		[Summary("Copies the permissions from the first role to the second role. Will not copy roles that the user does not have access to. Will not overwrite roles that are above the user's top role.")]
		[PermissionRequirement(new[] { GuildPermission.ManageRoles }, null)]
		[DefaultEnabled(true)]
		public sealed class CopyRolePerms : MyModuleBase
		{
			[Command]
			public async Task Command([VerifyObject(false, ObjectVerification.CanBeEdited)] IRole inputRole,
									  [VerifyObject(false, ObjectVerification.CanBeEdited)] IRole outputRole)
			{
				await CommandRunner(inputRole, outputRole);
			}

			private async Task CommandRunner(IRole inputRole, IRole outputRole)
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

				await outputRole.ModifyAsync(x => x.Permissions = new GuildPermissions(newRoleBits));

				var immovablePerms = GetActions.GetPermissionNames(immovableBits);
				var failedToCopy = GetActions.GetPermissionNames(inputRoleBits & ~copyBits);
				var newPerms = GetActions.GetPermissionNames(newRoleBits);
				var immovablePermsStr = immovablePerms.Any() ? "Output role had some permissions unable to be removed by you." : null;
				var failedToCopyStr = failedToCopy.Any() ? "Input role had some permission unable to be copied by you." : null;
				var newPermsStr = String.Format("`{0}` now has the following permissions: `{1}`.", outputRole.FormatRole(), newPerms.Any() ? String.Join("`, `", newPerms) : "Nothing");

				var response = FormattingActions.JoinNonNullStrings(" ", immovablePermsStr, failedToCopyStr, newPermsStr);
				await MessageActions.SendChannelMessage(Context, response);
			}
		}

		[Group("clearroleperms"), Alias("clrp")]
		[Usage("[Role]")]
		[Summary("Removes all permissions from a role.")]
		[PermissionRequirement(new[] { GuildPermission.ManageRoles }, null)]
		[DefaultEnabled(true)]
		public sealed class ClearRolePerms : MyModuleBase
		{
			[Command]
			public async Task Command([VerifyObject(false, ObjectVerification.CanBeEdited)] IRole role)
			{
				await CommandRunner(role);
			}

			private async Task CommandRunner(IRole role)
			{
				var userBits = (Context.User as IGuildUser).GuildPermissions.RawValue;
				var roleBits = role.Permissions.RawValue;
				var immovableBits = roleBits & ~userBits;

				await role.ModifyAsync(x => x.Permissions = new GuildPermissions(immovableBits));

				var immovablePerms = GetActions.GetPermissionNames(immovableBits);
				var immovablePermsStr = immovablePerms.Any() ? "Role had some permissions unable to be cleared by you." : null;
				var newPermsStr = String.Format("`{0}` now has the following permissions: `{1}`.", role.FormatRole(), immovablePerms.Any() ? String.Join("`, `", immovablePerms) : "Nothing");

				var response = FormattingActions.JoinNonNullStrings(" ", immovablePermsStr, newPermsStr);
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, response);
			}
		}

		[Group("changerolename"), Alias("crn")]
		[Usage("[Role] [Name]")]
		[Summary("Changes the name of the role. This is *extremely* useful for when multiple roles have the same name but you want to edit things.")]
		[PermissionRequirement(new[] { GuildPermission.ManageRoles }, null)]
		[DefaultEnabled(true)]
		public sealed class ChangeRoleName : MyModuleBase
		{
			[Command]
			public async Task Command([VerifyObject(false, ObjectVerification.CanBeEdited, ObjectVerification.IsEveryone)] IRole role, [Remainder, VerifyStringLength(Target.Role)] string name)
			{
				await CommandRunner(role, name);
			}

			private async Task CommandRunner(IRole role, string name)
			{
				await Context.Guild.GetRole(role.Id).ModifyAsync(x => x.Name = name);
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully changed the name of `{0}` to `{1}`.", role.FormatRole(), name));
			}
		}

		//TODO: Role name change by position command

		[Group("changerolecolor"), Alias("crc")]
		[Usage("<Role> <Hexadecimal|Color Name>")]
		[Summary("Changes the role's color. Color must be valid hexadecimal or the name of a default role color. Inputting nothing displays the colors.")]
		[PermissionRequirement(new[] { GuildPermission.ManageRoles }, null)]
		[DefaultEnabled(true)]
		public sealed class ChangeRoleColor : MyModuleBase
		{
			[Command]
			public async Task Command([Optional, VerifyObject(false, ObjectVerification.CanBeEdited, ObjectVerification.IsEveryone)] IRole role, [Optional] Color color)
			{
				await CommandRunner(role, color);
			}

			private async Task CommandRunner(IRole role, Color color)
			{
				if (role == null)
				{
					var desc = String.Format("`{0}`", String.Join("`, `", Constants.COLORS.Keys));
					await MessageActions.SendEmbedMessage(Context.Channel, EmbedActions.MakeNewEmbed("Colors", desc));
					return;
				}

				await role.ModifyAsync(x => x.Color = color);
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully changed the color of `{0}` to `{1}/{2}/{3}`.", role.FormatRole(), color.R, color.G, color.B));
			}
		}

		[Group("changerolehoist"), Alias("crh")]
		[Usage("[Role]")]
		[Summary("Displays a role separately from others on the user list. Saying the command again remove it from being hoisted.")]
		[PermissionRequirement(new[] { GuildPermission.ManageRoles }, null)]
		[DefaultEnabled(true)]
		public sealed class ChangeRoleHoist : MyModuleBase
		{
			[Command]
			public async Task Command([VerifyObject(false, ObjectVerification.CanBeEdited, ObjectVerification.IsEveryone)] IRole role)
			{
				await CommandRunner(role);
			}

			private async Task CommandRunner(IRole role)
			{
				await role.ModifyAsync(x => x.Hoist = !role.IsHoisted);
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully {0} `{1}`.", (role.IsHoisted ? "dehoisted" : "hoisted"), role.FormatRole()));
			}
		}

		[Group("changerolementionability"), Alias("crma")]
		[Usage("[Role]")]
		[Summary("Allows the role to be mentioned. Saying the command again removes its ability to be mentioned.")]
		[PermissionRequirement(new[] { GuildPermission.ManageRoles }, null)]
		[DefaultEnabled(true)]
		public sealed class ChangeRoleMentionability : MyModuleBase
		{
			[Command]
			public async Task Command([VerifyObject(false, ObjectVerification.CanBeEdited, ObjectVerification.IsEveryone)] IRole role)
			{
				await CommandRunner(role);
			}

			private async Task CommandRunner(IRole role)
			{
				await role.ModifyAsync(x => x.Mentionable = !role.IsMentionable);
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully made `{0}` {1}.", role.FormatRole(), (role.IsMentionable ? "unmentionable" : "mentionable")));
			}
		}
	}
}
