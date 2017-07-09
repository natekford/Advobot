using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Advobot
{
	//Role Moderation commands are commands that affect the roles in a guild
	[Name("RoleModeration")]
	public class Advobot_Commands_Role_Mod : ModuleBase
	{
		[Command("giverole")]
		[Alias("gr")]
		[Usage("[User] [Role]/<Role>/...")]
		[Summary("Gives the user the role (assuming the person using the command and bot both have the ability to give that role).")]
		[PermissionRequirement(1U << (int)GuildPermission.ManageRoles)]
		[DefaultEnabled(true)]
		public async Task GiveRole([Remainder] string input)
		{
			//Split input
			var returnedArgs = Actions.GetArgs(Context, input, new ArgNumbers(1, 2));
			if (returnedArgs.Reason != FailureReason.NotFailure)
			{
				await Actions.HandleArgsGettingErrors(Context, returnedArgs);
				return;
			}
			var userStr = returnedArgs.Arguments[0];
			var roleStr = returnedArgs.Arguments[1];

			//Test if valid user mention
			var returnedUser = Actions.GetGuildUser(Context, new[] { ObjectVerification.None }, true, userStr);
			if (returnedUser.Reason != FailureReason.NotFailure)
			{
				await Actions.HandleObjectGettingErrors(Context, returnedUser);
				return;
			}
			var user = returnedUser.Object;

			//Determine if the role exists and if it is able to be edited by both the bot and the user
			var splitRolesStr = roleStr.Split('/').ToList();
			if (splitRolesStr.Count == 1)
			{
				//Check if it actually exists
				var returnedRole = Actions.GetRole(Context, new[] { ObjectVerification.CanBeEdited, ObjectVerification.IsEveryone, ObjectVerification.IsManaged }, true, roleStr);
				if (returnedRole.Reason != FailureReason.NotFailure)
				{
					await Actions.HandleObjectGettingErrors(Context, returnedRole);
					return;
				}
				var role = returnedRole.Object;

				//See if the role is unable to be given due to management or being the everyone role
				if (role.IsManaged)
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Role is managed and unable to be given."));
					return;
				}
				else if (role == Context.Guild.EveryoneRole)
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Unable to give the everyone role."));
					return;
				}

				//Give the role and make a message
				await Actions.GiveRole(user, role);
				await Actions.MakeAndDeleteSecondaryMessage(Context,
					String.Format("Successfully gave the role `{0}` to `{1}`.", role, user.FormatUser()));
			}
			else
			{
				var failedRoles = new List<string>();
				var roles = new List<IRole>();
				splitRolesStr.ForEach(x =>
				{
					var returnedRole = Actions.GetRole(Context, new[] { ObjectVerification.CanBeEdited, ObjectVerification.IsEveryone, ObjectVerification.IsManaged }, false, x);
					if (returnedRole.Reason == FailureReason.NotFailure)
					{
						roles.Add(returnedRole.Object);
					}
					else
					{
						failedRoles.Add(x);
					}
				});

				var succ = roles.Any();
				var fail = failedRoles.Any();

				//Format the response message
				var succOutput = "";
				if (succ)
				{
					succOutput = String.Format("Successfully gave the role{0} `{1}` to `{2}`", Actions.GetPlural(roles.Count), String.Join(", ", roles.Select(x => x.Name)), user.FormatUser());
				}
				var and = "";
				if (succ && fail)
				{
					and = ", and ";
				}
				else if (succ)
				{
					and = ".";
				}
				var failOutput = "";
				if (fail)
				{
					failOutput = String.Format("{0}ailed to give the role{2} `{1}`{3}.",
						succ ? "F" : "f",
						String.Join(", ", failedRoles),
						Actions.GetPlural(failedRoles.Count),
						succ ? String.Format(" from `{0}`", user.FormatUser()) : "");
				}

				await Actions.GiveRoles(user, roles);
				await Actions.MakeAndDeleteSecondaryMessage(Context, succOutput + and + failOutput);
			}
		}

		[Command("takerole")]
		[Alias("tr")]
		[Usage("[@User] [Role]/<Role>/...")]
		[Summary("Take the role from the user (assuming the person using the command and bot both have the ability to take that role).")]
		[PermissionRequirement(1U << (int)GuildPermission.ManageRoles)]
		[DefaultEnabled(true)]
		public async Task TakeRole([Remainder] string input)
		{
			//Split input
			var returnedArgs = Actions.GetArgs(Context, input, new ArgNumbers(1, 2));
			if (returnedArgs.Reason != FailureReason.NotFailure)
			{
				await Actions.HandleArgsGettingErrors(Context, returnedArgs);
				return;
			}
			var userStr = returnedArgs.Arguments[0];
			var roleStr = returnedArgs.Arguments[1];

			//Test if valid user mention
			var returnedUser = Actions.GetGuildUser(Context, new[] { ObjectVerification.None }, true, userStr);
			if (returnedUser.Reason != FailureReason.NotFailure)
			{
				await Actions.HandleObjectGettingErrors(Context, returnedUser);
				return;
			}
			var user = returnedUser.Object;

			//Determine if the role exists and if it is able to be edited by both the bot and the user
			var splitRolesStr = roleStr.Split('/').ToList();
			if (splitRolesStr.Count == 1)
			{
				//Check if it actually exists
				var returnedRole = Actions.GetRole(Context, new[] { ObjectVerification.CanBeEdited, ObjectVerification.IsEveryone, ObjectVerification.IsManaged }, true, roleStr);
				if (returnedRole.Reason != FailureReason.NotFailure)
				{
					await Actions.HandleObjectGettingErrors(Context, returnedRole);
					return;
				}
				var role = returnedRole.Object;

				await Actions.TakeRole(user, role);
				await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully took `{0}` from `{1}`.", role, user.FormatUser()));
			}
			else
			{
				var evaluatedRoles = Actions.GetValidEditRoles(Context, splitRolesStr);
				if (!evaluatedRoles.HasValue)
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ROLE_ERROR));
					return;
				}
				var success = evaluatedRoles.Value.Success;
				var failure = evaluatedRoles.Value.Failure;

				await Actions.TakeRoles(user, success);
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.FormatResponseMessagesForCmdsOnLotsOfObjects(success, failure, "role", "took", "take"));
			}
		}

		[Command("createrole")]
		[Alias("cr")]
		[Usage("[Name]")]
		[Summary("Adds a role to the guild with the chosen name.")]
		[PermissionRequirement(1U << (int)GuildPermission.ManageRoles)]
		[DefaultEnabled(true)]
		public async Task CreateRole([Remainder] string input)
		{
			//Check length
			if (input.Length > Constants.MAX_ROLE_NAME_LENGTH)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(String.Format("Roles can only have a name length of up to `{0}` characters.", Constants.MAX_ROLE_NAME_LENGTH)));
				return;
			}
			else if (input.Length < Constants.MIN_ROLE_NAME_LENGTH)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(String.Format("Roles need to have a name equal to or greater than `{0}` characters.", Constants.MIN_ROLE_NAME_LENGTH)));
				return;
			}

			//Create role
			await Context.Guild.CreateRoleAsync(input, new GuildPermissions(0));
			await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully created the role `{0}`.", input));
		}

		[Command("softdeleterole")]
		[Alias("sdr")]
		[Usage("[Role]")]
		[Summary("Removes all permissions from a role (and all channels the role had permissions on) and removes the role from everyone. Leaves the name and color behind.")]
		[PermissionRequirement(1U << (int)GuildPermission.ManageRoles)]
		[DefaultEnabled(true)]
		public async Task SoftDeleteRole([Remainder] string input)
		{
			//Determine if the role exists and if it is able to be edited by both the bot and the user
			var returnedRole = Actions.GetRole(Context, new[] { ObjectVerification.CanBeEdited, ObjectVerification.IsEveryone, ObjectVerification.IsManaged }, true, input);
			if (returnedRole.Reason != FailureReason.NotFailure)
			{
				await Actions.HandleObjectGettingErrors(Context, returnedRole);
				return;
			}
			var role = returnedRole.Object;

			//Get the properties of the role before it's deleted
			var name = role.Name;
			var color = role.Color;
			var position = role.Position;

			//Change the new role's position
			await role.DeleteAsync();
			await Actions.ModifyRolePosition(await Context.Guild.CreateRoleAsync(name, new GuildPermissions(0), color), position);
			await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully removed all permissions from the role `{0}` and removed the role from all users on the guild.", role.Name));
		}

		[Command("deleterole")]
		[Alias("dr")]
		[Usage("[Role]")]
		[Summary("Deletes the role.")]
		[PermissionRequirement(1U << (int)GuildPermission.ManageRoles)]
		[DefaultEnabled(true)]
		public async Task DeleteRole([Remainder] string input)
		{
			//Determine if the role exists and if it is able to be edited by both the bot and the user
			var returnedRole = Actions.GetRole(Context, new[] { ObjectVerification.CanBeEdited, ObjectVerification.IsEveryone, ObjectVerification.IsManaged }, true, input);
			if (returnedRole.Reason != FailureReason.NotFailure)
			{
				await Actions.HandleObjectGettingErrors(Context, returnedRole);
				return;
			}
			var role = returnedRole.Object;

			await role.DeleteAsync();
			await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully deleted the role `{0}`.", input));
		}

		[Command("changeroleposition")]
		[Alias("crpo")]
		[Usage("[Role] <Position>")]
		[Summary("If only a role is input its position will be listed, else moves the role to the given position. " + Constants.FAKE_EVERYONE + " is the first position and starts at zero.")]
		[PermissionRequirement(1U << (int)GuildPermission.ManageRoles)]
		[DefaultEnabled(true)]
		public async Task RolePosition([Remainder] string input)
		{
			//Split input
			var returnedArgs = Actions.GetArgs(Context, input, new ArgNumbers(1, 2));
			if (returnedArgs.Reason != FailureReason.NotFailure)
			{
				await Actions.HandleArgsGettingErrors(Context, returnedArgs);
				return;
			}
			var roleStr = returnedArgs.Arguments[0];
			var posStr = returnedArgs.Arguments[1];

			//Get the role
			var returnedRole = Actions.GetRole(Context, new[] { ObjectVerification.CanBeEdited, ObjectVerification.IsEveryone }, true, roleStr);
			if (returnedRole.Reason != FailureReason.NotFailure)
			{
				await Actions.HandleObjectGettingErrors(Context, returnedRole);
				return;
			}
			var role = returnedRole.Object;

			//Get the position as an int
			if (!int.TryParse(posStr, out int position))
			{
				await Actions.SendChannelMessage(Context, String.Format("The `{0}` role has a position of `{1}`.", role.Name, role.Position));
				return;
			}
			else if (position <= 0)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Cannot set a role to a position lower than or equal to zero."));
				return;
			}
			else if (position > Context.Guild.Roles.Max(x => x.Position))
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Cannot set a role to a position higher than the highest role."));
				return;
			}

			//Change its position
			await Actions.ModifyRolePosition(role, position);
			await Actions.SendChannelMessage(Context, String.Format("Successfully gave the role `{0}` the position `{1}`.", role.Name, position));
		}

		[Command("displayrolepositions")]
		[Alias("drp")]
		[Usage("")]
		[Summary("Lists the positions of each role on the guild.")]
		[PermissionRequirement(1U << (int)GuildPermission.ManageRoles)]
		[DefaultEnabled(true)]
		public async Task ListRolePositions()
		{
			var description = String.Join("\n", Context.Guild.Roles.OrderByDescending(x => x.Position).Select(x =>
			{
				if (x == Context.Guild.EveryoneRole)
				{
					return String.Format("`{0}.` {1}", x.Position.ToString("00"), Constants.FAKE_EVERYONE);
				}
				else
				{
					return String.Format("`{0}.` {1}", x.Position.ToString("00"), x.Name);
				}
			}));
			await Actions.SendEmbedMessage(Context.Channel, Actions.MakeNewEmbed("Role Positions", description));
		}

		[Command("changeroleperms")]
		[Alias("crpe")]
		[Usage("[Show|Add|Remove] [Role] [Permission/...]")]
		[Summary("Permissions must be separated by a `/`. Type `" + Constants.BOT_PREFIX + "rp [Show]` to see the available permissions. " +
			"Type `" + Constants.BOT_PREFIX + "rp [Show] [Role]` to see the permissions of that role.")]
		[PermissionRequirement(1U << (int)GuildPermission.ManageRoles)]
		[DefaultEnabled(true)]
		public async Task RolePermissions([Remainder] string input)
		{
			var returnedArgs = Actions.GetArgs(Context, input, new ArgNumbers(1, 3));
			if (returnedArgs.Reason != FailureReason.NotFailure)
			{
				await Actions.HandleArgsGettingErrors(Context, returnedArgs);
				return;
			}
			var actionStr = returnedArgs.Arguments[0];
			var roleStr = returnedArgs.Arguments[1];
			var permStr = returnedArgs.Arguments[2];

			var returnedType = Actions.GetEnum(actionStr, new[] { ActionType.Show, ActionType.Add, ActionType.Remove });
			if (returnedType.Reason != FailureReason.NotFailure)
			{
				await Actions.HandleObjectGettingErrors(Context, returnedType);
				return;
			}
			var action = returnedType.Object;

			//If only show, take that as a person wanting to see the permission types
			if (returnedArgs.ArgCount == 1)
			{
				if (action == ActionType.Show)
				{
					//Embed showing the role permission types
					await Actions.SendEmbedMessage(Context.Channel, Actions.MakeNewEmbed("Role Permission Types", String.Format("`{0}`", String.Join("`, `", Variables.GuildPermissions.Select(x => x.Name)))));
					return;
				}
				else
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR));
					return;
				}
			}

			var returnedRole = Actions.GetRole(Context, new[] { ObjectVerification.CanBeEdited }, true, roleStr);
			if (returnedRole.Reason != FailureReason.NotFailure)
			{
				await Actions.HandleObjectGettingErrors(Context, returnedRole);
				return;
			}
			var role = returnedRole.Object;

			var permissions = new List<string>();
			switch (action)
			{
				case ActionType.Show:
				{
					var rolePerms = Context.Guild.GetRole(role.Id).Permissions;
					var currentRolePerms = Variables.GuildPermissions.Where(x => ((int)rolePerms.RawValue & (1 << x.Position)) != 0).Select(x => x.Name);
					var permissionsString = String.Format("`{0}`", currentRolePerms.Any() ? String.Join("`, `", currentRolePerms) : "NO PERMISSIONS");
					await Actions.SendEmbedMessage(Context.Channel, Actions.MakeNewEmbed(role.Name, permissionsString));
					return;
				}
				case ActionType.Add:
				case ActionType.Remove:
				{
					permissions = permStr.Split('/').ToList();
					break;
				}
			}

			//Check if valid permissions
			var validPerms = permissions.Intersect(Variables.GuildPermissions.Select(x => x.Name).ToList(), StringComparer.OrdinalIgnoreCase).ToList();
			if (validPerms.Count != permissions.Count)
			{
				var invalidPerms = permissions.Where(x => !validPerms.Contains(x, StringComparer.OrdinalIgnoreCase)).ToList();
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(String.Format("Invalid permission{0} supplied: `{1}`.",
					Actions.GetPlural(invalidPerms.Count),
					String.Join("`, `", invalidPerms))));
				return;
			}

			//Determine the permissions being added
			uint rolePermissions = 0;
			await permissions.ForEachAsync(async permission =>
			{
				try
				{
					var bit = Variables.GuildPermissions.FirstOrDefault(x => Actions.CaseInsEquals(x.Name, permission)).Position;
					rolePermissions |= (1U << bit);
				}
				catch (Exception)
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(String.Format("Couldn't parse permission '{0}'", permission)));
					return;
				}
			});

			//Determine if the user can give these perms
			if (!Actions.GetIfUserIsOwner(Context.Guild, Context.User))
			{
				var guildUser = Context.User as IGuildUser;
				if (!guildUser.GuildPermissions.Administrator)
				{
					rolePermissions &= (uint)guildUser.GuildPermissions.RawValue;
				}

				//If the user was unable to give any of the perms
				if (rolePermissions == 0)
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(String.Format("You do not have the ability to modify the following perm{0}: `{1}`.",
						Actions.GetPlural(permissions.Count),
						String.Join("`, `", permissions))));
					return;
				}
			}

			var givenPermissions = Actions.GetPermissionNames(rolePermissions);
			var skippedPermissions = permissions.Except(givenPermissions, StringComparer.OrdinalIgnoreCase);

			//New perms
			var responseStr = "";
			var currentBits = (uint)Context.Guild.GetRole(role.Id).Permissions.RawValue;
			switch (action)
			{
				case ActionType.Add:
				{
					currentBits |= rolePermissions;
					responseStr = Actions.FormatResponseMessagesForCmdsOnLotsOfObjects(givenPermissions, skippedPermissions, "permission", "added", "add");
					break;
				}
				case ActionType.Remove:
				{
					currentBits &= ~rolePermissions;
					responseStr = Actions.FormatResponseMessagesForCmdsOnLotsOfObjects(givenPermissions, skippedPermissions, "permission", "removed", "remove");
					break;
				}
			}

			await Context.Guild.GetRole(role.Id).ModifyAsync(x => x.Permissions = new GuildPermissions(currentBits));
			await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("On `{0}`: {1}", role.FormatRole(), responseStr));
		}

		[Command("copyroleperms")]
		[Alias("corp")]
		[Usage("[Role] [Role]")]
		[Summary("Copies the permissions from the first role to the second role. Will not copy roles that the user does not have access to." +
			"Will not overwrite roles that are above the user's top role.")]
		[PermissionRequirement(1U << (int)GuildPermission.ManageRoles)]
		[DefaultEnabled(true)]
		public async Task CopyRolePermissions([Remainder] string input)
		{
			var returnedArgs = Actions.GetArgs(Context, input, new ArgNumbers(2, 2));
			if (returnedArgs.Reason != FailureReason.NotFailure)
			{
				await Actions.HandleArgsGettingErrors(Context, returnedArgs);
				return;
			}
			var inputRoleStr = returnedArgs.Arguments[0];
			var outputRoleStr = returnedArgs.Arguments[1];

			//Determine if the input role exists
			var returnedInputRole = Actions.GetRole(Context, new[] { ObjectVerification.None }, false, inputRoleStr);
			if (returnedInputRole.Reason != FailureReason.NotFailure)
			{
				await Actions.HandleObjectGettingErrors(Context, returnedInputRole);
				return;
			}
			var inputRole = returnedInputRole.Object;

			//Determine if the role exists and if it is able to be edited by both the bot and the user
			var returnedOutputRole = Actions.GetRole(Context, new[] { ObjectVerification.CanBeEdited }, false, outputRoleStr);
			if (returnedOutputRole.Reason != FailureReason.NotFailure)
			{
				await Actions.HandleObjectGettingErrors(Context, returnedOutputRole);
				return;
			}
			var outputRole = returnedOutputRole.Object;

			//Get the permissions
			var rolePermissions = (uint)inputRole.Permissions.RawValue;
			var permissions = Actions.GetPermissionNames(rolePermissions).ToList();
			if (rolePermissions != 0)
			{
				//Determine if the user can give these permissions
				if (!Actions.GetIfUserIsOwner(Context.Guild, Context.User))
				{
					var guildUser = Context.User as IGuildUser;
					if (!guildUser.GuildPermissions.Administrator)
					{
						rolePermissions &= (uint)guildUser.GuildPermissions.RawValue;
					}

					//If the role has something, but the user is not allowed to edit a permissions
					if (rolePermissions == 0)
					{
						await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(String.Format("You do not have the ability to modify the following perm{0}: `{1}`.",
							Actions.GetPlural(permissions.Count),
							String.Join("`, `", permissions))));
						return;
					}
				}
			}

			var givenPermissions = Actions.GetPermissionNames(rolePermissions);
			var skippedPermissions = permissions.Except(givenPermissions);
			var responseStr = Actions.FormatResponseMessagesForCmdsOnLotsOfObjects(givenPermissions, skippedPermissions, "permission", "copied", "copy");

			await Context.Guild.GetRole(outputRole.Id).ModifyAsync(x => x.Permissions = new GuildPermissions(rolePermissions));
			await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("From `{0}` to `{1}`: {2}", inputRole.FormatRole(), outputRole.FormatRole(), responseStr));
		}

		[Command("clearroleperms")]
		[Alias("clrp")]
		[Usage("[Role]")]
		[Summary("Removes all permissions from a role.")]
		[PermissionRequirement(1U << (int)GuildPermission.ManageRoles)]
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
		[PermissionRequirement(1U << (int)GuildPermission.ManageRoles)]
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
		[PermissionRequirement(1U << (int)GuildPermission.ManageRoles)]
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
		[PermissionRequirement(1U << (int)GuildPermission.ManageRoles)]
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
		[PermissionRequirement(1U << (int)GuildPermission.ManageRoles)]
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
