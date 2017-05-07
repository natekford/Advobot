using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Advobot
{
	//Role Moderation commands are commands that affect the roles in a guild
	[Name("Role_Moderation")]
	public class Advobot_Commands_Role_Mod : ModuleBase
	{
		[Command("rolegive")]
		[Alias("rg")]
		[Usage("[@User] [Role]/<Role>/...")]
		[Summary("Gives the user the role (assuming the person using the command and bot both have the ability to give that role).")]
		[PermissionRequirement(1U << (int)GuildPermission.ManageRoles)]
		[DefaultEnabled(true)]
		public async Task GiveRole([Remainder] string input)
		{
			//Test number of arguments
			var inputArray = input.Split(new[] { ' ' }, 2);
			if (inputArray.Length != 2)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR));
				return;
			}
			var userStr = inputArray[0];
			var roleStr = inputArray[1];

			//Test if valid user mention
			var returnedUser = Actions.GetGuildUser(Context, new[] { UserCheck.None }, true, userStr);
			if (returnedUser.Reason != FailureReason.Not_Failure)
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
				var returnedRole = Actions.GetRole(Context, new[] { RoleCheck.Can_Be_Edited, RoleCheck.Is_Everyone, RoleCheck.Is_Managed }, false, roleStr);
				if (returnedRole.Reason != FailureReason.Not_Failure)
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
					var returnedRole = Actions.GetRole(Context, new[] { RoleCheck.Can_Be_Edited, RoleCheck.Is_Everyone, RoleCheck.Is_Managed }, false, x);
					if (returnedRole.Reason == FailureReason.Not_Failure)
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

		[Command("roletake")]
		[Alias("rt")]
		[Usage("[@User] [Role]/<Role>/...")]
		[Summary("Take the role from the user (assuming the person using the command and bot both have the ability to take that role).")]
		[PermissionRequirement(1U << (int)GuildPermission.ManageRoles)]
		[DefaultEnabled(true)]
		public async Task TakeRole([Remainder] string input)
		{
			//Test number of arguments
			var inputArray = input.Split(new[] { ' ' }, 2);
			if (inputArray.Length != 2)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR));
				return;
			}
			var userStr = inputArray[0];
			var roleStr = inputArray[1];

			//Test if valid user mention
			var returnedUser = Actions.GetGuildUser(Context, new[] { UserCheck.None }, true, userStr);
			if (returnedUser.Reason != FailureReason.Not_Failure)
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
				var returnedRole = Actions.GetRole(Context, new[] { RoleCheck.Can_Be_Edited, RoleCheck.Is_Everyone, RoleCheck.Is_Managed }, false, roleStr);
				if (returnedRole.Reason != FailureReason.Not_Failure)
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

		[Command("rolecreate")]
		[Alias("rcr")]
		[Usage("[Role]")]
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

		[Command("rolesoftdelete")]
		[Alias("rsd")]
		[Usage("[Role]")]
		[Summary("Removes all permissions from a role (and all channels the role had permissions on) and removes the role from everyone. Leaves the name and color behind.")]
		[PermissionRequirement(1U << (int)GuildPermission.ManageRoles)]
		[DefaultEnabled(true)]
		public async Task SoftDeleteRole([Remainder] string input)
		{
			//Determine if the role exists and if it is able to be edited by both the bot and the user
			var returnedRole = Actions.GetRole(Context, new[] { RoleCheck.Can_Be_Edited, RoleCheck.Is_Everyone, RoleCheck.Is_Managed }, true, input);
			if (returnedRole.Reason != FailureReason.Not_Failure)
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

		[Command("roledelete")]
		[Alias("rd")]
		[Usage("[Role]")]
		[Summary("Deletes the role..")]
		[PermissionRequirement(1U << (int)GuildPermission.ManageRoles)]
		[DefaultEnabled(true)]
		public async Task DeleteRole([Remainder] string input)
		{
			//Determine if the role exists and if it is able to be edited by both the bot and the user
			var returnedRole = Actions.GetRole(Context, new[] { RoleCheck.Can_Be_Edited, RoleCheck.Is_Everyone, RoleCheck.Is_Managed }, true, input);
			if (returnedRole.Reason != FailureReason.Not_Failure)
			{
				await Actions.HandleObjectGettingErrors(Context, returnedRole);
				return;
			}
			var role = returnedRole.Object;

			await role.DeleteAsync();
			await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully deleted the role `{0}`.", input));
		}

		[Command("roleposition")]
		[Alias("rpos")]
		[Usage("[\"Role Name\"] [New Position]")]
		[Summary("Moves the role to the given position. " + Constants.FAKE_EVERYONE + " is the first position and starts at zero.")]
		[PermissionRequirement(1U << (int)GuildPermission.ManageRoles)]
		[DefaultEnabled(true)]
		public async Task RolePosition([Remainder] string input)
		{
			var inputArray = Actions.SplitByCharExceptInQuotes(input, ' ');
			if (inputArray.Length != 2)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR));
				return;
			}
			var roleStr = inputArray[0];
			var poseStr = inputArray[1];

			//Get the role
			var returnedRole = Actions.GetRole(Context, new[] { RoleCheck.Can_Be_Edited, RoleCheck.Is_Everyone }, true, roleStr);
			if (returnedRole.Reason != FailureReason.Not_Failure)
			{
				await Actions.HandleObjectGettingErrors(Context, returnedRole);
				return;
			}
			var role = returnedRole.Object;

			//Get the position as an int
			if (!int.TryParse(input.Substring(input.LastIndexOf(' ')), out int position))
			{
				await Actions.SendChannelMessage(Context, String.Format("The `{0}` role has a position of `{1}`.", role.Name, role.Position));
				return;
			}
			else if (position <= 0)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Cannot set a role to a position lower than or equal to one."));
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

		[Command("rolepositions")]
		[Alias("rposs")]
		[Usage("")]
		[Summary("Lists the positions of each role on the guild.")]
		[PermissionRequirement(1U << (int)GuildPermission.ManageRoles)]
		[DefaultEnabled(true)]
		public async Task ListRolePositions()
		{
			//List of the roles
			var roles = Context.Guild.Roles.OrderBy(x => x.Position).Reverse().ToList();

			//Put them into strings now
			var description = "";
			roles.ForEach(role =>
			{
				if (role == Context.Guild.EveryoneRole)
				{
					description += String.Format("`{0}.` {1}", role.Position.ToString("00"), Constants.FAKE_EVERYONE);
				}
				else
				{
					description += String.Format("`{0}.` {1}\n", role.Position.ToString("00"), role.Name);
				}
			});

			//Send the embed
			await Actions.SendEmbedMessage(Context.Channel, Actions.MakeNewEmbed("Role Positions", description));
		}

		[Command("roleperms")]
		[Alias("rp")]
		[Usage("[Show|Add|Remove] <\"Role Name\"> <Permission/...>")]
		[Summary("Add/remove the selected permissions to/from the role. Permissions must be separated by a `/`! Type `" + Constants.BOT_PREFIX + "rolepermissions [Show]` to see the available permissions. " +
			"Type `" + Constants.BOT_PREFIX + "rolepermissions [Show] [Role]` to see the permissions of that role.")]
		[PermissionRequirement(1U << (int)GuildPermission.ManageRoles)]
		[DefaultEnabled(true)]
		public async Task RolePermissions([Remainder] string input)
		{
			//Set the permission types into a list to later check against
			var permissionTypeStrings = Variables.GuildPermissions.Select(x => x.Name).ToList();

			//Separate the role and whether to add or remove from the permissions
			var inputArray = Actions.SplitByCharExceptInQuotes(input, ' ');
			var actionStr = inputArray[0];
			var permStr = inputArray.Length > 1 ? inputArray[1] : null;
			var roleStr = inputArray.Length > 2 ? inputArray[2] : null;

			//If the user wants to see the permission types, print them out
			var show = Actions.CaseInsEquals(actionStr, "show");
			if (inputArray.Length == 1)
			{
				await Actions.SendEmbedMessage(Context.Channel, Actions.MakeNewEmbed("Role Permissions", String.Join("\n", permissionTypeStrings)));
				return;
			}

			//Determine if the role exists and if it is able to be edited by both the bot and the user
			var returnedRole = Actions.GetRole(Context, new[] { RoleCheck.Can_Be_Edited }, true, roleStr);
			if (returnedRole.Reason != FailureReason.Not_Failure)
			{
				await Actions.HandleObjectGettingErrors(Context, returnedRole);
				return;
			}
			var role = returnedRole.Object;

			//Send a message of the permissions the targetted role has
			if (show)
			{
				var rolePerms = new GuildPermissions(Context.Guild.GetRole(role.Id).Permissions.RawValue);
				var currentRolePerms = new List<string>();
				Variables.GuildPermissions.Select(x => x.Position).ToList().ForEach(permissionValue =>
				{
					var bit = permissionValue;
					if (((int)rolePerms.RawValue & (1 << bit)) != 0)
					{
						currentRolePerms.Add(Variables.GuildPermissions.FirstOrDefault(x => x.Position == bit).Name);
					}
				});
				var permissionsString = String.IsNullOrEmpty(String.Join("\n", currentRolePerms)) ? "No permissions" : String.Join("\n", currentRolePerms);
				await Actions.SendEmbedMessage(Context.Channel, Actions.MakeNewEmbed(role.Name, permissionsString));
				return;
			}

			//See if it's add or remove
			var add = Actions.CaseInsEquals(actionStr, "add");
			if(!add && !Actions.CaseInsEquals(actionStr, "remove"))
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ACTION_ERROR));
				return;
			}

			//Get the permissions
			var permissions = permStr.Split('/').ToList();
			var validPerms = permissions.Intersect(permissionTypeStrings, StringComparer.OrdinalIgnoreCase).ToList();
			if (validPerms.Count != permissions.Count)
			{
				var invalidPermissions = new List<string>();
				permissions.ForEach(permission =>
				{
					if (!Actions.CaseInsContains(validPerms, permission))
					{
						invalidPermissions.Add(permission);
					}
				});
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(String.Format("Invalid {0} supplied: `{1}`.",
					permissions.Count - permissions.Intersect(permissionTypeStrings).Count() == 1 ? "permission" : "permissions",
					String.Join("`, `", invalidPermissions))), 7500);
				return;
			}

			//Determine the permissions being added
			uint rolePermissions = 0;
			await permissions.ForEachAsync(async permission =>
			{
				var perms = Variables.GuildPermissions.Select(x => x.Name).ToList();
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
				if (!(Context.User as IGuildUser).GuildPermissions.Administrator)
				{
					rolePermissions &= (uint)(Context.User as IGuildUser).GuildPermissions.RawValue;
				}
				//If the role has something, but the user is not allowed to edit a permissions
				if (rolePermissions == 0)
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(String.Format("You do not have the ability to modify {0}.",
						permissions.Count == 1 ? "that permission" : "those permissions")));
					return;
				}
			}

			//Get a list of the permissions that were given
			var givenPermissions = Actions.GetPermissionNames(rolePermissions).ToList();
			//Get a list of the permissions that were not given
			var skippedPermissions = permissions.Except(givenPermissions, StringComparer.OrdinalIgnoreCase).ToList();

			//New perms
			var currentBits = (uint)Context.Guild.GetRole(role.Id).Permissions.RawValue;
			if (add)
			{
				currentBits |= rolePermissions;
			}
			else
			{
				currentBits &= ~rolePermissions;
			}

			await Context.Guild.GetRole(role.Id).ModifyAsync(x => x.Permissions = new GuildPermissions(currentBits));
			await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully {0} `{1}` {2} {3} `{4}`.",
				(add ? "added" : "removed"),
				String.Join("`, `", givenPermissions),
				(skippedPermissions.Any() ? " and failed to " + (add ? "add `" : "remove `") + String.Join("`, `", skippedPermissions) + "`" : ""),
				(add ? "to" : "from"), role.Name));
		}

		[Command("rolepermscopy")]
		[Alias("rpc")]
		[Usage("[\"Role\"]/[\"Role\"]")]
		[Summary("Copies the permissions from the first role to the second role. Will not copy roles that the user does not have access to." +
			"Will not overwrite roles that are above the user's top role.")]
		[PermissionRequirement(1U << (int)GuildPermission.ManageRoles)]
		[DefaultEnabled(true)]
		public async Task CopyRolePermissions([Remainder] string input)
		{
			var inputArray = Actions.SplitByCharExceptInQuotes(input, ' ');
			if (inputArray.Length != 2)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR));
				return;
			}
			var inputStr = inputArray[0];
			var outputStr = inputArray[1];

			//Determine if the input role exists
			var returnedInputRole = Actions.GetRole(Context, new[] { RoleCheck.None }, false, inputStr);
			if (returnedInputRole.Reason != FailureReason.Not_Failure)
			{
				await Actions.HandleObjectGettingErrors(Context, returnedInputRole);
				return;
			}
			var inputRole = returnedInputRole.Object;

			//Determine if the role exists and if it is able to be edited by both the bot and the user
			var returnedOutputRole = Actions.GetRole(Context, new[] { RoleCheck.Can_Be_Edited }, false, outputStr);
			if (returnedOutputRole.Reason != FailureReason.Not_Failure)
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
					if (!(Context.User as IGuildUser).GuildPermissions.Administrator)
					{
						rolePermissions &= (uint)(Context.User as IGuildUser).GuildPermissions.RawValue;
					}
					//If the role has something, but the user is not allowed to edit a permissions
					if (rolePermissions == 0)
					{
						await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(String.Format("You do not have the ability to modify {0}.",
							permissions.Count == 1 ? "that permission" : "those permissions")));
						return;
					}
				}
			}

			var givenPermissions = Actions.GetPermissionNames(rolePermissions).ToList();
			var skippedPermissions = permissions.Except(givenPermissions).ToList();
			await Context.Guild.GetRole(outputRole.Id).ModifyAsync(x => x.Permissions = new GuildPermissions(rolePermissions));

			var succOutput = "ALL";
			if (!givenPermissions.Any())
			{
				succOutput = "NOTHING";
			}
			else
			{
				succOutput = String.Join("`, `", givenPermissions);
			}
			var failOutput = "";
			if (skippedPermissions.Any())
			{
				failOutput = String.Format("and failed to copy `{0}`", String.Join("`, `", skippedPermissions));
			}
			await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully copied `{0}` {1} from `{2}` to `{3}`.",
				succOutput,
				failOutput,
				Actions.FormatRole(inputRole),
				Actions.FormatRole(outputRole)));
		}

		[Command("rolepermsclear")]
		[Alias("rpcl")]
		[Usage("[Role]")]
		[Summary("Removes all permissions from a role.")]
		[PermissionRequirement(1U << (int)GuildPermission.ManageRoles)]
		[DefaultEnabled(true)]
		public async Task ClearRolePermissions([Remainder] string input)
		{
			//Determine if the role exists and if it is able to be edited by both the bot and the user
			var returnedRole = Actions.GetRole(Context, new[] { RoleCheck.Can_Be_Edited }, true, input);
			if (returnedRole.Reason != FailureReason.Not_Failure)
			{
				await Actions.HandleObjectGettingErrors(Context, returnedRole);
				return;
			}
			var role = returnedRole.Object;

			//Clear the role's perms
			await role.ModifyAsync(x => x.Permissions = new GuildPermissions(0));
			await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully removed all permissions from `{0}`.", input));
		}

		[Command("rolename")]
		[Alias("rn")]
		[Usage("[\"Role Name\"|Position:Number] [\"New Name\"]")]
		[Summary("Changes the name of the role. This is *extremely* useful for when multiple roles have the same name but you want to edit things.")]
		[PermissionRequirement(1U << (int)GuildPermission.ManageRoles)]
		[DefaultEnabled(true)]
		public async Task ChangeRoleName([Remainder] string input)
		{
			var inputArray = Actions.SplitByCharExceptInQuotes(input, ' ');
			if (inputArray.Length != 2)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR));
				return;
			}
			var nameStr = inputArray[0];
			var posStr = Actions.GetVariable(inputArray, "position");
			var newName = inputArray[1];

			//Check length
			if (newName.Length > Constants.MAX_ROLE_NAME_LENGTH)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(String.Format("Roles can only have a name length of up to `{0}` characters.", Constants.MAX_ROLE_NAME_LENGTH)));
				return;
			}
			else if (newName.Length < Constants.MIN_ROLE_NAME_LENGTH)
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
					var returnedRole = Actions.GetRole(Context, new[] { RoleCheck.Can_Be_Edited, RoleCheck.Is_Everyone }, roles.First());
					if (returnedRole.Reason != FailureReason.Not_Failure)
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
				var returnedRole = Actions.GetRole(Context, new[] { RoleCheck.Can_Be_Edited, RoleCheck.Is_Everyone }, true, nameStr);
				if (returnedRole.Reason != FailureReason.Not_Failure)
				{
					await Actions.HandleObjectGettingErrors(Context, returnedRole);
					return;
				}
				role = returnedRole.Object;
			}

			var beforeName = role.Name;
			await Context.Guild.GetRole(role.Id).ModifyAsync(x => x.Name = newName);
			await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully changed the name of the role `{0}` to `{1}`.", beforeName, role.Name));
		}

		[Command("rolecolor")]
		[Alias("rc")]
		[Usage("[\"Role Name\"] [Hex:Hexadecimal|Name:Color Name]")]
		[Summary("Changes the role's color. A color of '0' sets the role back to the default color. " +
			"Colors must either be in hexadecimal format or be a color listed [here](https://msdn.microsoft.com/en-us/library/system.drawing.color).")]
		[PermissionRequirement(1U << (int)GuildPermission.ManageRoles)]
		[DefaultEnabled(true)]
		public async Task ChangeRoleColor([Remainder] string input)
		{
			var inputArray = Actions.SplitByCharExceptInQuotes(input, ' ');
			var roleStr = inputArray[0];
			var hexStr = Actions.GetVariable(inputArray, "hex");
			var colStr = Actions.GetVariable(inputArray, "color");

			//Determine if the role exists and if it is able to be edited by both the bot and the user
			var returnedRole = Actions.GetRole(Context, new[] { RoleCheck.Can_Be_Edited, RoleCheck.Is_Everyone }, true, roleStr);
			if (returnedRole.Reason != FailureReason.Not_Failure)
			{
				await Actions.HandleObjectGettingErrors(Context, returnedRole);
				return;
			}
			var role = returnedRole.Object;

			uint colorID = 0;
			if (!String.IsNullOrWhiteSpace(colStr))
			{
				colorID = (uint)System.Drawing.Color.FromName(colStr).ToArgb();
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
			await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully changed the color of the role `{0}` to `{1}`.", inputArray[0], inputArray[1]));
		}

		[Command("rolehoist")]
		[Alias("rh")]
		[Usage("[Role]")]
		[Summary("Displays a role separately from others on the user list. Saying the command again remove it from being hoisted.")]
		[PermissionRequirement(1U << (int)GuildPermission.ManageRoles)]
		[DefaultEnabled(true)]
		public async Task HoistRole([Remainder] string input)
		{
			var returnedRole = Actions.GetRole(Context, new[] { RoleCheck.Can_Be_Edited, RoleCheck.Is_Everyone }, true, input);
			if (returnedRole.Reason != FailureReason.Not_Failure)
			{
				await Actions.HandleObjectGettingErrors(Context, returnedRole);
				return;
			}
			var role = returnedRole.Object;

			if (role.IsHoisted)
			{
				await role.ModifyAsync(x => x.Hoist = false);
				await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully removed `{0}` from being hoisted.", Actions.FormatRole(role)));
			}
			else
			{
				await role.ModifyAsync(x => x.Hoist = true);
				await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully hoisted `{0}`.", Actions.FormatRole(role)));
			}
		}

		[Command("rolementionability")]
		[Alias("rma")]
		[Usage("[Role]")]
		[Summary("Allows the role to be mentioned. Saying the command again removes its ability to be mentioned.")]
		[PermissionRequirement(1U << (int)GuildPermission.ManageRoles)]
		[DefaultEnabled(true)]
		public async Task ChangeMentionRole([Remainder] string input)
		{
			var returnedRole = Actions.GetRole(Context, new[] { RoleCheck.Can_Be_Edited, RoleCheck.Is_Everyone }, true, input);
			if (returnedRole.Reason != FailureReason.Not_Failure)
			{
				await Actions.HandleObjectGettingErrors(Context, returnedRole);
				return;
			}
			var role = returnedRole.Object;

			if (role.IsMentionable)
			{
				await role.ModifyAsync(x => x.Mentionable = false);
				await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully removed `{0}` from being mentionable.", Actions.FormatRole(role)));
			}
			else
			{
				await role.ModifyAsync(x => x.Mentionable = true);
				await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully made `{0}` mentionable.", Actions.FormatRole(role)));
			}
		}
	}
}
