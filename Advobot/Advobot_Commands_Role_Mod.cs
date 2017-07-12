using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Advobot
{
	namespace RoleModeration
	{
		[Group("giverole")]
		[Alias("gr")]
		[Usage("[User] [Role] <Role> ...")]
		[Summary("Gives the user the role (assuming the person using the command and bot both have the ability to give that role).")]
		[PermissionRequirement(new[] { GuildPermission.ManageRoles }, null)]
		[DefaultEnabled(true)]
		public class GiveRole : ModuleBase<MyCommandContext>
		{
			[Command]
			public async Task Command(IGuildUser user, [VerifyObject(ObjectVerification.CanBeEdited)] params IRole[] roles)
			{
				await CommandRunner(user, roles);
			}

			private async Task CommandRunner(IGuildUser user, IRole[] roles)
			{
				await Actions.GiveRoles(user, roles);
				await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully gave the following roles to `{0}`: `{1}`.", user.FormatUser(), String.Join("`, `", roles.Select(x => x.FormatRole()))));
			}
		}

		[Group("takerole")]
		[Alias("tr")]
		[Usage("[User] [Role] <Role> ...")]
		[Summary("Take the role from the user (assuming the person using the command and bot both have the ability to take that role).")]
		[PermissionRequirement(new[] { GuildPermission.ManageRoles }, null)]
		[DefaultEnabled(true)]
		public class TakeRole : ModuleBase<MyCommandContext>
		{
			[Command]
			public async Task Command(IGuildUser user, [VerifyObject(ObjectVerification.CanBeEdited)] params IRole[] roles)
			{
				await CommandRunner(user, roles);
			}

			private async Task CommandRunner(IGuildUser user, IRole[] roles)
			{
				await Actions.TakeRoles(user, roles);
				await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully took the following roles from `{0}`: `{1}`.", user.FormatUser(), String.Join("`, `", roles.Select(x => x.FormatRole()))));
			}
		}

		[Group("createrole")]
		[Alias("cr")]
		[Usage("[Name]")]
		[Summary("Adds a role to the guild with the chosen name.")]
		[PermissionRequirement(new[] { GuildPermission.ManageRoles }, null)]
		[DefaultEnabled(true)]
		public class CreateRole : ModuleBase<MyCommandContext>
		{
			[Command]
			public async Task Command([Remainder] string name)
			{
				await CommandRunner(name);
			}

			private async Task CommandRunner(string name)
			{
				if (name.Length > Constants.MAX_ROLE_NAME_LENGTH)
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(String.Format("Role names cannot be more than `{0}` characters.", Constants.MAX_ROLE_NAME_LENGTH)));
					return;
				}
				else if (name.Length < Constants.MIN_ROLE_NAME_LENGTH)
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(String.Format("Role names cannot be less than `{0}` characters.", Constants.MIN_ROLE_NAME_LENGTH)));
					return;
				}

				await Context.Guild.CreateRoleAsync(name, new GuildPermissions(0));
				await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully created the role `{0}`.", name));
			}
		}

		[Group("softdeleterole")]
		[Alias("sdr")]
		[Usage("[Role]")]
		[Summary("Removes all permissions from a role (and all channels the role had permissions on) and removes the role from everyone. Leaves the name and color behind.")]
		[PermissionRequirement(new[] { GuildPermission.ManageRoles }, null)]
		[DefaultEnabled(true)]
		public class SoftDeleteRole : ModuleBase<MyCommandContext>
		{
			[Command]
			public async Task Command([VerifyObject(ObjectVerification.CanBeEdited, ObjectVerification.IsEveryone, ObjectVerification.IsManaged)] IRole role)
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

				await Actions.ModifyRolePosition(newRole, position);
				await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully removed all permissions from the role `{0}` and removed the role from all users on the guild.", role.Name));
			}
		}

		[Group("deleterole")]
		[Alias("dr")]
		[Usage("[Role]")]
		[Summary("Deletes the role.")]
		[PermissionRequirement(new[] { GuildPermission.ManageRoles }, null)]
		[DefaultEnabled(true)]
		public class DeleteRole : ModuleBase<MyCommandContext>
		{
			[Command]
			public async Task Command([VerifyObject(ObjectVerification.CanBeEdited, ObjectVerification.IsEveryone, ObjectVerification.IsManaged)] IRole role)
			{
				await CommandRunner(role);
			}

			private async Task CommandRunner(IRole role)
			{
				await role.DeleteAsync();
				await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully deleted `{0}`.", role.FormatRole()));
			}
		}

		[Group("changeroleposition")]
		[Alias("crpo")]
		[Usage("[Role] <Position>")]
		[Summary("If only a role is input its position will be listed, else moves the role to the given position. " + Constants.FAKE_EVERYONE + " is the first position and starts at zero.")]
		[PermissionRequirement(new[] { GuildPermission.ManageRoles }, null)]
		[DefaultEnabled(true)]
		public class ChangeRolePosition : ModuleBase<MyCommandContext>
		{
			[Command]
			public async Task Command([VerifyObject(ObjectVerification.CanBeEdited, ObjectVerification.IsEveryone)] IRole role, uint position)
			{
				await CommandRunner(role, position);
			}

			private async Task CommandRunner(IRole role, uint position)
			{
				var newPos = await Actions.ModifyRolePosition(role, (int)position);
				if (newPos != -1)
				{
					await Actions.SendChannelMessage(Context, String.Format("Successfully gave `{0}` the position `{1}`.", role.FormatRole(), newPos));
				}
				else
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Failed to give `{0}` the position `{1}`.", role.FormatRole(), position));
				}
			}
		}

		[Group("displayrolepositions")]
		[Alias("drp")]
		[Usage("")]
		[Summary("Lists the positions of each role on the guild.")]
		[PermissionRequirement(new[] { GuildPermission.ManageRoles }, null)]
		[DefaultEnabled(true)]
		public class DisplayRolePositions : ModuleBase<MyCommandContext>
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
				await Actions.SendEmbedMessage(Context.Channel, Actions.MakeNewEmbed("Role Positions", desc));
			}
		}

		[Group("changeroleperms")]
		[Alias("crpe")]
		[Usage("[Show|Allow|Deny] <Role> <Permission/...>")]
		[Summary("Permissions must be separated by a `/`. Type `" + Constants.BOT_PREFIX + "rp [Show]` to see the available permissions. " +
			"Type `" + Constants.BOT_PREFIX + "rp [Show] [Role]` to see the permissions of that role.")]
		[PermissionRequirement(new[] { GuildPermission.ManageRoles }, null)]
		[DefaultEnabled(true)]
		public class ChangeRolePerms : ModuleBase<MyCommandContext>
		{
			[Command]
			public async Task Command([VerifyEnum((uint)(ActionType.Allow | ActionType.Deny))] ActionType actionType,
									  [VerifyObject(ObjectVerification.CanBeEdited)] IRole role,
									  [Remainder] string uncutPermissions)
			{
				await CommandRunner(actionType, role, uncutPermissions);
			}
			[Command]
			public async Task Command([VerifyEnum((uint)ActionType.Show)] ActionType actionType,
									  [Optional, VerifyObject(ObjectVerification.CanBeEdited)] IRole role)
			{
				await CommandRunner(role);
			}

			private async Task CommandRunner(ActionType actionType, IRole role, string uncutPermissions)
			{
				var permissions = uncutPermissions.Split('/').SelectMany(x => x.Split(' ').Select(y => y.Trim(','))).ToList();
				var validPerms = permissions.Where(x => Variables.GuildPermissions.Select(y => y.Name).CaseInsContains(x));
				var invalidPerms = permissions.Where(x => !Variables.GuildPermissions.Select(y => y.Name).CaseInsContains(x));
				if (invalidPerms.Any())
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(String.Format("Invalid permission{0} supplied: `{1}`.",
						Actions.GetPlural(invalidPerms.Count()),
						String.Join("`, `", invalidPerms))));
					return;
				}

				ulong changeValue = 0;
				foreach (var permission in permissions)
				{
					changeValue = Actions.AddGuildPermissionBit(permission, changeValue);
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

				var changedPerms = Actions.GetPermissionNames(changeValue);
				await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully {0} `{1}` for `{2}`.",
					actionStr,
					changedPerms.Any() ? String.Join("`, `", changedPerms) : "Nothing",
					role.FormatRole()));
			}
			private async Task CommandRunner(IRole role)
			{
				if (role == null)
				{
					await Actions.SendEmbedMessage(Context.Channel, Actions.MakeNewEmbed("Guild Permission Types", String.Format("`{0}`", String.Join("`, `", Variables.GuildPermissions.Select(x => x.Name)))));
					return;
				}

				var currentRolePerms = Variables.GuildPermissions.Where(x => (role.Permissions.RawValue & (1U << x.Position)) != 0).Select(x => x.Name);
				var permissions = currentRolePerms.Any() ? String.Join("`, `", currentRolePerms) : "No permission";
				await Actions.SendEmbedMessage(Context.Channel, Actions.MakeNewEmbed(role.Name, String.Format("`{0}`", permissions)));
			}
		}

		[Group("copyroleperms")]
		[Alias("corp")]
		[Usage("[Role] [Role]")]
		[Summary("Copies the permissions from the first role to the second role. Will not copy roles that the user does not have access to." +
			"Will not overwrite roles that are above the user's top role.")]
		[PermissionRequirement(new[] { GuildPermission.ManageRoles }, null)]
		[DefaultEnabled(true)]
		public class CopyRolePerms : ModuleBase<MyCommandContext>
		{
			[Command]
			public async Task Command([VerifyObject(ObjectVerification.CanBeEdited)] IRole inputRole,
									  [VerifyObject(ObjectVerification.CanBeEdited)] IRole outputRole)
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

				var immovablePerms = Actions.GetPermissionNames(immovableBits);
				var failedToCopy = Actions.GetPermissionNames(inputRoleBits & ~copyBits);
				var newPerms = Actions.GetPermissionNames(newRoleBits);

				await outputRole.ModifyAsync(x => x.Permissions = new GuildPermissions(newRoleBits));

				var immovablePermsStr = immovablePerms.Any() ? "Output role had some permissions unable to be removed." : null;
				var failedToCopyStr = failedToCopy.Any() ? "Input role had some permission unable to be copied." : null;
				var newPermsStr = String.Format("`{0}` now has the following permissions: `{1}`.", outputRole.FormatRole(), newPerms.Any() ? String.Join("`, `", newPerms) : "Nothing");

				var response = Actions.JoinNonNullStrings(" ", immovablePermsStr, failedToCopyStr, newPermsStr);
				await Actions.SendChannelMessage(Context, response);
			}
		}
	}
	//Role Moderation commands are commands that affect the roles in a guild
	[Name("RoleModeration")]
	public class Advobot_Commands_Role_Mod : ModuleBase
	{

		[Command("clearroleperms")]
		[Alias("clrp")]
		[Usage("[Role]")]
		[Summary("Removes all permissions from a role.")]
		[PermissionRequirement(new[] { GuildPermission.ManageRoles }, null)]
		[DefaultEnabled(true)]
		public async Task ClearRolePermissions([Remainder] string input)
		{
			//Determine if the role exists and if it is able to be edited by both the bot and the user
			var returnedRole = Actions.GetRole(Context, new[] { ObjectVerification.CanBeEdited }, true, input);
			if (returnedRole.Reason != FailureReason.NotFailure)
			{
				await Actions.HandleObjectGettingErrors(Context, returnedRole);
				return;
			}
			var role = returnedRole.Object;

			//Clear the role's perms
			await role.ModifyAsync(x => x.Permissions = new GuildPermissions(0));
			await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully removed all permissions from `{0}`.", input));
		}

		[Command("changerolename")]
		[Alias("crn")]
		[Usage("[Role|Position:Number] [\"New Name\"]")]
		[Summary("Changes the name of the role. This is *extremely* useful for when multiple roles have the same name but you want to edit things.")]
		[PermissionRequirement(new[] { GuildPermission.ManageRoles }, null)]
		[DefaultEnabled(true)]
		public async Task ChangeRoleName([Remainder] string input)
		{
			var returnedArgs = Actions.GetArgs(Context, input, new ArgNumbers(1, 2), new[] { "position" });
			if (returnedArgs.Reason != FailureReason.NotFailure)
			{
				await Actions.HandleArgsGettingErrors(Context, returnedArgs);
				return;
			}
			var roleStr = returnedArgs.Arguments[0];
			var nameStr = returnedArgs.Arguments[1];
			var posStr = returnedArgs.GetSpecifiedArg("position");

			//Check length
			if (nameStr.Length > Constants.MAX_ROLE_NAME_LENGTH)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(String.Format("Roles can only have a name length of up to `{0}` characters.", Constants.MAX_ROLE_NAME_LENGTH)));
				return;
			}
			else if (nameStr.Length < Constants.MIN_ROLE_NAME_LENGTH)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(String.Format("Roles need to have a name equal to or greater than `{0}` characters.", Constants.MIN_ROLE_NAME_LENGTH)));
				return;
			}

			IRole role = null;
			if (!String.IsNullOrWhiteSpace(posStr))
			{
				if (!int.TryParse(posStr, out int position))
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid position supplied."));
					return;
				}

				//Grab the roles with the position
				var roles = Context.Guild.Roles.Where(x => x.Position == position).ToList();
				if (!roles.Any())
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(String.Format("No role has a position of `{0}`", position)));
					return;
				}
				else if (roles.Count == 1)
				{
					var returnedRole = Actions.GetRole(Context, new[] { ObjectVerification.CanBeEdited, ObjectVerification.IsEveryone }, roles.First());
					if (returnedRole.Reason != FailureReason.NotFailure)
					{
						await Actions.HandleObjectGettingErrors(Context, returnedRole);
						return;
					}
					role = returnedRole.Object;
				}
				else
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("`{0}` roles have the position `{1}`.", roles.Count, position));
					return;
				}
			}
			else
			{
				var returnedRole = Actions.GetRole(Context, new[] { ObjectVerification.CanBeEdited, ObjectVerification.IsEveryone }, true, nameStr);
				if (returnedRole.Reason != FailureReason.NotFailure)
				{
					await Actions.HandleObjectGettingErrors(Context, returnedRole);
					return;
				}
				role = returnedRole.Object;
			}

			var beforeName = role.Name;
			await Context.Guild.GetRole(role.Id).ModifyAsync(x => x.Name = nameStr);
			await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully changed the name of the role `{0}` to `{1}`.", beforeName, role.Name));
		}

		[Command("changerolecolor")]
		[Alias("crc")]
		[Usage("[Role] [Hex:Hexadecimal|Name:Color Name]")]
		[Summary("Changes the role's color. A color of '0' sets the role back to the default color. " +
			"Colors must either be in hexadecimal format or be a color listed [here](https://msdn.microsoft.com/en-us/library/system.drawing.color).")]
		[PermissionRequirement(new[] { GuildPermission.ManageRoles }, null)]
		[DefaultEnabled(true)]
		public async Task ChangeRoleColor([Remainder] string input)
		{
			var returnedArgs = Actions.GetArgs(Context, input, new ArgNumbers(2, 2), new[] { "hex", "color" });
			if (returnedArgs.Reason != FailureReason.NotFailure)
			{
				await Actions.HandleArgsGettingErrors(Context, returnedArgs);
				return;
			}
			var roleStr = returnedArgs.Arguments[0];
			var hexStr = returnedArgs.GetSpecifiedArg("hex");
			var colorStr = returnedArgs.GetSpecifiedArg("color");

			//Determine if the role exists and if it is able to be edited by both the bot and the user
			var returnedRole = Actions.GetRole(Context, new[] { ObjectVerification.CanBeEdited, ObjectVerification.IsEveryone }, true, roleStr);
			if (returnedRole.Reason != FailureReason.NotFailure)
			{
				await Actions.HandleObjectGettingErrors(Context, returnedRole);
				return;
			}
			var role = returnedRole.Object;

			uint colorID = 0;
			if (!String.IsNullOrWhiteSpace(colorStr))
			{
				colorID = (uint)System.Drawing.Color.FromName(colorStr).ToArgb();
			}
			else if (!String.IsNullOrWhiteSpace(hexStr))
			{
				//Remove 0x if someone put that in there
				if (Actions.CaseInsStartsWith(hexStr, "0x"))
				{
					hexStr = hexStr.Substring(2);
				}
				//If the color ID isn't a hex number
				if (!uint.TryParse(hexStr, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out colorID))
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid hexadecimal provided."));
					return;
				}
			}
			else
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("No color was input."));
				return;
			}

			if (colorID == 0)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Unable to get a color from the given input."));
				return;
			}

			await Context.Guild.GetRole(role.Id).ModifyAsync(x => x.Color = new Color(colorID & 0xffffff));
			await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully changed the color of `{0}` to `{1}`.", role.FormatRole(), hexStr ?? colorStr));
		}

		[Command("changerolehoist")]
		[Alias("crh")]
		[Usage("[Role]")]
		[Summary("Displays a role separately from others on the user list. Saying the command again remove it from being hoisted.")]
		[PermissionRequirement(new[] { GuildPermission.ManageRoles }, null)]
		[DefaultEnabled(true)]
		public async Task HoistRole([Remainder] string input)
		{
			var returnedRole = Actions.GetRole(Context, new[] { ObjectVerification.CanBeEdited, ObjectVerification.IsEveryone }, true, input);
			if (returnedRole.Reason != FailureReason.NotFailure)
			{
				await Actions.HandleObjectGettingErrors(Context, returnedRole);
				return;
			}
			var role = returnedRole.Object;

			if (role.IsHoisted)
			{
				await role.ModifyAsync(x => x.Hoist = false);
				await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully removed `{0}` from being hoisted.", role.FormatRole()));
			}
			else
			{
				await role.ModifyAsync(x => x.Hoist = true);
				await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully hoisted `{0}`.", role.FormatRole()));
			}
		}

		[Command("changerolementionability")]
		[Alias("crma")]
		[Usage("[Role]")]
		[Summary("Allows the role to be mentioned. Saying the command again removes its ability to be mentioned.")]
		[PermissionRequirement(new[] { GuildPermission.ManageRoles }, null)]
		[DefaultEnabled(true)]
		public async Task ChangeMentionRole([Remainder] string input)
		{
			var returnedRole = Actions.GetRole(Context, new[] { ObjectVerification.CanBeEdited, ObjectVerification.IsEveryone }, true, input);
			if (returnedRole.Reason != FailureReason.NotFailure)
			{
				await Actions.HandleObjectGettingErrors(Context, returnedRole);
				return;
			}
			var role = returnedRole.Object;

			if (role.IsMentionable)
			{
				await role.ModifyAsync(x => x.Mentionable = false);
				await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully removed `{0}` from being mentionable.", role.FormatRole()));
			}
			else
			{
				await role.ModifyAsync(x => x.Mentionable = true);
				await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully made `{0}` mentionable.", role.FormatRole()));
			}
		}
	}
}
