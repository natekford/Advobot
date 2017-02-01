using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Advobot
{
	//Role Moderation commands are commands that affect the roles in a guild
	[Name("Role Moderation")]
	public class Advobot_Commands_Role_Mod : ModuleBase
	{
		[Command("rolegive")]
		[Alias("rgi")]
		[Usage("rolegive [@User] [Role]/<Role>/...")]
		[Summary("Gives the user the role (assuming the person using the command and bot both have the ability to give that role).")]
		[PermissionRequirements(1U << (int)GuildPermission.ManageRoles)]
		public async Task GiveRole([Remainder] string input)
		{
			//Test number of arguments
			var values = input.Split(new char[] { ' ' }, 2);
			if (values.Length != 2)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR));
				return;
			}

			//Test if valid user mention
			var inputUser = await Actions.getUser(Context.Guild, values[0]);
			if (inputUser == null)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.USER_ERROR));
				return;
			}

			//Determine if the role exists and if it is able to be edited by both the bot and the user
			var inputRoles = values[1].Split('/').ToList();
			if (inputRoles.Count == 1)
			{
				//Check if it actually exists
				var role = await Actions.getRoleEditAbility(Context, inputRoles[0]);
				if (role == null)
					return;

				//See if the role is unable to be given due to management
				if (role.IsManaged)
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Role is managed and unable to be given."));
					return;
				}

				//Check if trying to delete the everyone role
				if (role == Context.Guild.EveryoneRole)
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Unable to give the everyone role."));
					return;
				}

				//Give the role and make a message
				await Actions.giveRole(inputUser, role);
				await Actions.makeAndDeleteSecondaryMessage(Context,
					String.Format("Successfully gave `{0}#{1}` the `{2}` role.", inputUser.Username, inputUser.Discriminator, role));
			}
			else
			{
				var failedRoles = new List<string>();
				var succeededRoles = new List<string>();
				var roles = new List<IRole>();
				foreach (string roleName in inputRoles)
				{
					var role = await Actions.getRoleEditAbility(Context, roleName, true);
					if (role == null || role.IsManaged || role == Context.Guild.EveryoneRole)
					{
						failedRoles.Add(roleName);
					}
					else
					{
						roles.Add(role);
						succeededRoles.Add(roleName);
					}
				}

				//Format the success message
				var succeed = "";
				if (succeededRoles.Any())
				{
					succeed = String.Format("Successfully gave `{0}#{1}` the `{2}` role{3}",
						inputUser.Username,
						inputUser.Discriminator,
						String.Join(", ", succeededRoles),
						succeededRoles.Count != 1 ? "s" : "");
				}
				//Check if an and is needed
				var and = ".";
				if (succeededRoles.Any() && failedRoles.Any())
				{
					and = " and ";
				}
				//Format the fail message
				var failed = "";
				if (failedRoles.Any())
				{
					failed = String.Format("{0}ailed to give{1} the `{2}` role{3}.",
						String.IsNullOrEmpty(succeed) ? "F" : "f",
						String.IsNullOrEmpty(succeed) ? String.Format(" `{0}#{1}`", inputUser.Username, inputUser.Discriminator) : "",
						String.Join(", ", failedRoles),
						failedRoles.Count != 1 ? "s" : "");
				}

				await Actions.giveRole(inputUser, roles.ToArray());
				await Actions.makeAndDeleteSecondaryMessage(Context, succeed + and + failed);
			}
		}

		[Command("roletake")]
		[Alias("rta")]
		[Usage("roletake [@User] [Role]")]
		[Summary("Take the role from the user (assuming the person using the command and bot both have the ability to take that role).")]
		[PermissionRequirements(1U << (int)GuildPermission.ManageRoles)]
		public async Task TakeRole([Remainder] string input)
		{
			//Test number of arguments
			var values = input.Split(new char[] { ' ' }, 2);
			if (values.Length != 2)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR));
				return;
			}

			//Test if valid user mention
			var inputUser = await Actions.getUser(Context.Guild, values[0]);
			if (inputUser == null)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.USER_ERROR));
				return;
			}

			//Determine if the role exists and if it is able to be edited by both the bot and the user
			var inputRoles = values[1].Split('/').ToList();
			if (inputRoles.Count == 1)
			{
				//Check if it actually exists
				var role = await Actions.getRoleEditAbility(Context, inputRoles[0]);
				if (role == null)
					return;

				//See if the role is unable to be taken due to management
				if (role.IsManaged)
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Role is managed and unable to be taken."));
					return;
				}

				//Check if trying to delete the everyone role
				if (role == Context.Guild.EveryoneRole)
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Unable to take the everyone role."));
					return;
				}

				//Take the role and make a message
				await Actions.takeRole(inputUser, role);
				await Actions.makeAndDeleteSecondaryMessage(Context,
					String.Format("Successfully took `{0}` from `{1}#{2}`.", role, inputUser.Username, inputUser.Discriminator));
			}
			else
			{
				var failedRoles = new List<string>();
				var succeededRoles = new List<string>();
				var roles = new List<IRole>();
				foreach (string roleName in inputRoles)
				{
					var role = await Actions.getRoleEditAbility(Context, roleName, true);
					if (role == null || role.IsManaged || role == Context.Guild.EveryoneRole)
					{
						failedRoles.Add(roleName);
					}
					else
					{
						roles.Add(role);
						succeededRoles.Add(roleName);
					}
				}

				//Format the success message
				var succeed = "";
				if (succeededRoles.Any())
				{
					succeed = String.Format("Successfully took the `{0}` role{1} from `{2}#{3}`",
						String.Join(", ", succeededRoles), succeededRoles.Count != 1 ? "s" : "", inputUser.Username, inputUser.Discriminator);
				}
				//Check if an and is needed
				var and = ".";
				if (succeededRoles.Any() && failedRoles.Any())
				{
					and = " and ";
				}
				//Format the fail message
				var failed = "";
				if (failedRoles.Any())
				{
					failed = String.Format("{0}ailed to take the `{1}` role{2}{3}.",
						String.IsNullOrEmpty(succeed) ? "F" : "f", String.Join(", ", failedRoles), failedRoles.Count != 1 ? "s" : "",
						String.IsNullOrEmpty(succeed) ? String.Format(" from `{0}#{1}`", inputUser.Username, inputUser.Discriminator) : "");
				}

				await Actions.takeRole(inputUser, roles.ToArray());
				await Actions.makeAndDeleteSecondaryMessage(Context, succeed + and + failed);
			}
		}

		[Command("rolecreate")]
		[Alias("rcr")]
		[Usage("rolecreate [Role]")]
		[Summary("Adds a role to the guild with the chosen name.")]
		[PermissionRequirements(1U << (int)GuildPermission.ManageRoles)]
		public async Task CreateRole([Remainder] string input)
		{
			//Check length
			if (input.Length > Constants.ROLE_NAME_LENGTH)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Roles can only have a name length of up to 32 characters."));
				return;
			}

			//Create role
			await Context.Guild.CreateRoleAsync(input, new GuildPermissions(0));
			await Actions.makeAndDeleteSecondaryMessage(Context, String.Format("Successfully created the `{0}` role.", input));
		}

		[Command("rolesoftdelete")]
		[Alias("rsd")]
		[Usage("rolesoftdelete [Role]")]
		[Summary("Removes all permissions from a role (and all channels the role had permissions on) and removes the role from everyone. Leaves the name and color behind.")]
		[PermissionRequirements(1U << (int)GuildPermission.ManageRoles)]
		public async Task SoftDeleteRole([Remainder] string input)
		{
			//Determine if the role exists and if it is able to be edited by both the bot and the user
			var inputRole = await Actions.getRoleEditAbility(Context, input);
			if (inputRole == null)
				return;

			//Check if even removable
			if (inputRole.IsManaged)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Role is managed and unable to be softdeleted."));
				return;
			}

			//Check if trying to delete the everyone role
			if (inputRole == Context.Guild.EveryoneRole)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Unable to softdelete the everyone role."));
				return;
			}

			//Create a new role with the same attributes (including space) and no perms
			var newRole = await Context.Guild.CreateRoleAsync(inputRole.Name, new GuildPermissions(0), inputRole.Color);

			//Make a new list of IRole
			var roles = new List<IRole>();
			//Grab all roles that aren't the targeted one
			Context.Guild.Roles.Where(x => x != newRole).ToList().ForEach(x => roles.Add(x));
			//Sort the list by position
			roles = roles.OrderBy(x => x.Position).ToList();
			//Add in the targetted role with the given position
			roles.Insert(Math.Min(roles.Count(), inputRole.Position), newRole);

			//Make a new list of BulkRoleProperties
			var listOfBulk = new List<BulkRoleProperties>();
			//Add the role's IDs and positions into it
			roles.ForEach(x => listOfBulk.Add(new BulkRoleProperties(x.Id)));
			//Readd the positions to it
			listOfBulk.ForEach(x => x.Position = listOfBulk.IndexOf(x));
			//Mass modify the roles with the list having the correct positions
			await Context.Guild.ModifyRolesAsync(listOfBulk);

			//Delete the old role
			await inputRole.DeleteAsync();
			await Actions.makeAndDeleteSecondaryMessage(Context,
				String.Format("Successfully removed all permissions from `{0}` and removed the role from all users on the guild.", inputRole.Name));
		}

		[Command("roledelete")]
		[Alias("rd")]
		[Usage("roledelete [Role]")]
		[Summary("Deletes the role..")]
		[PermissionRequirements(1U << (int)GuildPermission.ManageRoles)]
		public async Task DeleteRole([Remainder] string input)
		{
			//Determine if the role exists and if it is able to be edited by both the bot and the user
			var role = await Actions.getRoleEditAbility(Context, input);
			if (role == null)
				return;

			//Check if even removable
			if (role.IsManaged)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Role is managed and unable to be deleted."));
				return;
			}

			//Check if trying to delete the everyone role
			if (role == Context.Guild.EveryoneRole)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Unable to delete the everyone role."));
				return;
			}

			await role.DeleteAsync();
			await Actions.makeAndDeleteSecondaryMessage(Context, String.Format("Successfully deleted the `{0}` role.", input));
		}

		[Command("roleposition")]
		[Alias("rpos")]
		[Usage("roleposition [Role] [New Position]")]
		[Summary("Moves the role to the given position. @ev" + Constants.ZERO_LENGTH_CHAR + "eryone is the first position and starts at zero.")]
		[PermissionRequirements(1U << (int)GuildPermission.ManageRoles)]
		public async Task RolePosition([Remainder] string input)
		{
			//Get the role
			var role = await Actions.getRoleEditAbility(Context, input.Substring(0, input.LastIndexOf(' ')), true);
			if (role == null)
				return;

			//Check if it's the everyone role
			if (role == Context.Guild.EveryoneRole)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Unable to move the everyone role."));
				return;
			}

			//Get the position as an int
			var position = 0;
			if (!int.TryParse(input.Substring(input.LastIndexOf(' ')), out position))
			{
				await Actions.sendChannelMessage(Context, String.Format("The `{0}` role has a position of `{1}`.", role.Name, role.Position));
				return;
			}

			//Checking if valid positions
			var maxPos = 0;
			Context.Guild.Roles.ToList().ForEach(x => maxPos = Math.Max(maxPos, x.Position));
			if (position <= 0)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Cannot set a role to a position lower than or equal to one."));
				return;
			}
			else if (position > maxPos)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Cannot set a role to a position higher than the highest role."));
				return;
			}

			//See if the user can access that position
			if (position > Actions.getPosition(Context.Guild, Context.User as IGuildUser))
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Position is higher than you can access."));
				return;
			}
			//See if the bot can access that position
			if (position > Actions.getPosition(Context.Guild, await Context.Guild.GetUserAsync(Variables.Bot_ID)))
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Position is higher than the bot can access."));
				return;
			}

			//Make a new list of IRole
			var roles = new List<IRole>();
			//Grab all roles that aren't the targeted one
			Context.Guild.Roles.Where(x => x != role).ToList().ForEach(x => roles.Add(x));
			//Sort the list by position
			roles = roles.OrderBy(x => x.Position).ToList();
			//Add in the targetted role with the given position
			roles.Insert(Math.Min(roles.Count(), position), role);

			//Make a new list of BulkRoleProperties
			var listOfBulk = new List<BulkRoleProperties>();
			//Add the role's IDs and positions into it
			roles.ForEach(x => listOfBulk.Add(new BulkRoleProperties(x.Id)));
			//Readd the positions to it
			listOfBulk.ForEach(x => x.Position = listOfBulk.IndexOf(x));
			//Mass modify the roles with the list having the correct positions
			await Context.Guild.ModifyRolesAsync(listOfBulk);

			//Send a message stating what position the channel was sent to
			await Actions.sendChannelMessage(Context, String.Format("Successfully gave the `{0}` role the position `{1}`.", role.Name, roles.IndexOf(role)));
		}

		[Command("rolepositions")]
		[Alias("rposs")]
		[Usage("rolepositions")]
		[Summary("Lists the positions of each role on the guild.")]
		[PermissionRequirements(1U << (int)GuildPermission.ManageRoles)]
		public async Task ListRolePositions()
		{
			//List of the roles
			var roles = Context.Guild.Roles.OrderBy(x => x.Position).Reverse().ToList();

			//Put them into strings now
			var description = "";
			foreach (var role in roles)
			{
				if (role == Context.Guild.EveryoneRole)
				{
					description += "`" + role.Position.ToString("00") + ".` @ev" + Constants.ZERO_LENGTH_CHAR + "eryone";
					continue;
				}
				description += "`" + role.Position.ToString("00") + ".` " + role.Name + "\n";
			}

			//Check the length to see if the message can be sent
			if (description.Length > Constants.SHORT_LENGTH_CHECK)
			{
				description = Actions.uploadToHastebin(Actions.replaceMessageCharacters(description));
			}

			//Send the embed
			await Actions.sendEmbedMessage(Context.Channel, Actions.makeNewEmbed("Role Positions", description));
		}

		[Command("roleperms")]
		[Alias("rp")]
		[Usage("roleperms [Show|Add|Remove] [Role] [Permission/...]")]
		[Summary("Add/remove the selected permissions to/from the role. Permissions must be separated by a `/`! " +
			"Type `" + Constants.BOT_PREFIX + "rolepermissions [Show]` to see the available permissions. " +
			"Type `" + Constants.BOT_PREFIX + "rolepermissions [Show] [Role]` to see the permissions of that role.")]
		[PermissionRequirements(1U << (int)GuildPermission.ManageRoles)]
		public async Task RolePermissions([Remainder] string input)
		{
			//Set the permission types into a list to later check against
			var permissionTypeStrings = Variables.GuildPermissions.Select(x => x.Name).ToList();

			var actionRolePerms = input.ToLower().Split(new char[] { ' ' }, 2); //Separate the role and whether to add or remove from the permissions
			var permsString = ""; //Set placeholder perms variable
			var roleString = ""; //Set placeholder role variable
			var show = false; //Set show bool

			//If the user wants to see the permission types, print them out
			if (input.Equals("show", StringComparison.OrdinalIgnoreCase))
			{
				await Actions.sendEmbedMessage(Context.Channel, Actions.makeNewEmbed("Role Permissions", String.Join("\n", permissionTypeStrings)));
				return;
			}
			//If something is said after show, take that as a role.
			else if (input.StartsWith("show", StringComparison.OrdinalIgnoreCase))
			{
				roleString = input.Substring("show".Length).Trim();
				show = true;
			}
			//If show is not input, take the stuff being said as a role and perms
			else
			{
				if (actionRolePerms.Length == 1)
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR));
					return;
				}
				var lastSpace = actionRolePerms[1].LastIndexOf(' ');
				if (lastSpace <= 0)
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR));
					return;
				}
				//Separate out the permissions
				permsString = actionRolePerms[1].Substring(lastSpace).Trim();
				//Separate out the role
				roleString = actionRolePerms[1].Substring(0, lastSpace).Trim();
			}

			//Determine if the role exists and if it is able to be edited by both the bot and the user
			var role = await Actions.getRoleEditAbility(Context, roleString);
			if (role == null)
				return;

			//Send a message of the permissions the targetted role has
			if (show)
			{
				var rolePerms = new GuildPermissions(Context.Guild.GetRole(role.Id).Permissions.RawValue);
				var currentRolePerms = new List<string>();
				foreach (var permissionValue in Variables.GuildPermissions.Select(x => x.Position))
				{
					var bit = permissionValue;
					if (((int)rolePerms.RawValue & (1 << bit)) != 0)
					{
						currentRolePerms.Add(Variables.GuildPermissions.FirstOrDefault(x => x.Position == bit).Name);
					}
				}
				var permissionsString = String.IsNullOrEmpty(String.Join("\n", currentRolePerms)) ? "No permissions" : String.Join("\n", currentRolePerms);
				await Actions.sendEmbedMessage(Context.Channel, Actions.makeNewEmbed(role.Name, permissionsString));
				return;
			}

			//See if it's add or remove
			var addOrRemove = actionRolePerms[0];
			bool add;
			if (addOrRemove.Equals("add"))
			{
				add = true;
			}
			else if (addOrRemove.Equals("remove"))
			{
				add = false;
			}
			else
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Add or remove not specified."));
				return;
			}

			//Get the permissions
			var permissions = permsString.Split('/').ToList();
			//Check if valid permissions
			var validPerms = permissions.Intersect(permissionTypeStrings, StringComparer.OrdinalIgnoreCase).ToList();
			if (validPerms.Count != permissions.Count)
			{
				var invalidPermissions = new List<string>();
				foreach (string permission in permissions)
				{
					if (!validPerms.Contains(permission, StringComparer.OrdinalIgnoreCase))
					{
						invalidPermissions.Add(permission);
					}
				}
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(String.Format("Invalid {0} supplied: `{1}`.",
					permissions.Count - permissions.Intersect(permissionTypeStrings).Count() == 1 ? "permission" : "permissions",
					String.Join("`, `", invalidPermissions))), 7500);
				return;
			}

			//Determine the permissions being added
			uint rolePermissions = 0;
			foreach (string permission in permissions)
			{
				var perms = Variables.GuildPermissions.Select(x => x.Name).ToList();
				try
				{
					var bit = Variables.GuildPermissions.FirstOrDefault(x => x.Name.Equals(permission, StringComparison.OrdinalIgnoreCase)).Position;
					rolePermissions |= (1U << bit);
				}
				catch (Exception)
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(String.Format("Couldn't parse permission '{0}'", permission)));
					return;
				}
			}

			//Determine if the user can give these perms
			if (!await Actions.userHasOwner(Context.Guild, Context.User as IGuildUser))
			{
				if (!(Context.User as IGuildUser).GuildPermissions.Administrator)
				{
					rolePermissions &= (uint)(Context.User as IGuildUser).GuildPermissions.RawValue;
				}
				//If the role has something, but the user is not allowed to edit a permissions
				if (rolePermissions == 0)
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(String.Format("You do not have the ability to modify {0}.",
						permissions.Count == 1 ? "that permission" : "those permissions")));
					return;
				}
			}

			//Get a list of the permissions that were given
			var givenPermissions = Actions.getPermissionNames(rolePermissions).ToList();
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
			await Actions.makeAndDeleteSecondaryMessage(Context, String.Format("Successfully {0} `{1}` {2} {3} `{4}`.",
				(add ? "added" : "removed"),
				String.Join("`, `", givenPermissions),
				(skippedPermissions.Any() ? " and failed to " + (add ? "add `" : "remove `") + String.Join("`, `", skippedPermissions) + "`" : ""),
				(add ? "to" : "from"), role.Name),
				7500);
		}

		[Command("rolepermscopy")]
		[Alias("rpc")]
		[Usage("rolepermscopy [Role]/[Role]")]
		[Summary("Copies the permissions from the first role to the second role. Will not copy roles that the user does not have access to." +
			"Will not overwrite roles that are above the user's top role.")]
		[PermissionRequirements(1U << (int)GuildPermission.ManageRoles)]
		public async Task CopyRolePermissions([Remainder] string input)
		{
			//Put the input into a string
			input = input.ToLower();
			var roles = input.Split(new char[] { '/' }, 2);

			//Test if two roles were input
			if (roles.Length != 2)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR));
				return;
			}

			//Determine if the input role exists
			var inputRole = await Actions.getRole(Context, roles[0]);
			if (inputRole == null)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ROLE_ERROR));
				return;
			}

			//Determine if the role exists and if it is able to be edited by both the bot and the user
			var outputRole = await Actions.getRoleEditAbility(Context, roles[1]);
			if (outputRole == null)
				return;

			//Get the permissions
			var rolePermissions = (uint)inputRole.Permissions.RawValue;
			var permissions = Actions.getPermissionNames(rolePermissions).ToList();
			if (rolePermissions != 0)
			{
				//Determine if the user can give these permissions
				if (!await Actions.userHasOwner(Context.Guild, Context.User as IGuildUser))
				{
					if (!(Context.User as IGuildUser).GuildPermissions.Administrator)
					{
						rolePermissions &= (uint)(Context.User as IGuildUser).GuildPermissions.RawValue;
					}
					//If the role has something, but the user is not allowed to edit a permissions
					if (rolePermissions == 0)
					{
						await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(String.Format("You do not have the ability to modify {0}.",
							permissions.Count == 1 ? "that permission" : "those permissions")));
						return;
					}
				}
			}

			//Get a list of the permissions that were given
			var givenPermissions = Actions.getPermissionNames(rolePermissions).ToList();
			//Get a list of the permissions that were not given
			var skippedPermissions = permissions.Except(givenPermissions).ToList();

			//Actually change the permissions
			await Context.Guild.GetRole(outputRole.Id).ModifyAsync(x => x.Permissions = new GuildPermissions(rolePermissions));
			//Send the long ass message detailing what happened with the command
			await Actions.makeAndDeleteSecondaryMessage(Context, String.Format("Successfully copied `{0}` {1} from `{2}` to `{3}`.",
				(givenPermissions.Count == 0 ? "NOTHING" : givenPermissions.Count == permissions.Count ? "ALL" : String.Join("`, `", givenPermissions)),
				(skippedPermissions.Any() ? "and failed to copy `" + String.Join("`, `", skippedPermissions) + "`" : ""),
				inputRole, outputRole),
				7500);
		}

		[Command("rolepermsclear")]
		[Alias("rpcl")]
		[Usage("rolepermsclear [Role]")]
		[Summary("Removes all permissions from a role.")]
		[PermissionRequirements(1U << (int)GuildPermission.ManageRoles)]
		public async Task ClearRolePermissions([Remainder] string input)
		{
			//Determine if the role exists and if it is able to be edited by both the bot and the user
			var role = await Actions.getRoleEditAbility(Context, input);
			if (role == null)
				return;

			//Clear the role's perms
			await role.ModifyAsync(x => x.Permissions = new GuildPermissions(0));
			await Actions.makeAndDeleteSecondaryMessage(Context, String.Format("Successfully removed all permissions from `{0}`.", input));
		}

		[Command("rolename")]
		[Alias("rn")]
		[Usage("rolename [Role|Position{x}]/[New Name]")]
		[Summary("Changes the name of the role. This is *extremely* useful for when multiple roles have the same name but you want to edit things.")]
		[PermissionRequirements(1U << (int)GuildPermission.ManageRoles)]
		public async Task ChangeRoleName([Remainder] string input)
		{
			//Split at the current role name and the new role name
			var values = input.Split(new char[] { '/' }, 2);

			//Check if correct number of arguments
			if (values.Length != 2)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR));
				return;
			}

			//See if the new name is a valid length
			if (values[1].Length > Constants.ROLE_NAME_LENGTH)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Roles can only have a name length of up to 32 characters."));
				return;
			}

			//Initialize the role
			IRole role = null;

			//See if it's a position trying to be gotten instead
			int position;
			if (values[0].ToLower().Contains("position{") && int.TryParse(values[0].Substring(9, 1), out position))
			{
				//Grab the roles with the position
				var roles = Context.Guild.Roles.Where(x => x.Position == position).ToList();
				if (roles.Count == 0)
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(String.Format("No role has a position of `{0}`", position)));
					return;
				}
				else if (roles.Count == 1)
				{
					//Get the role
					role = await Actions.getRoleEditAbility(Context, role: roles.First());
				}
				else
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, String.Format("`{0}` roles have the position `{1}`.", roles.Count, position));
					return;
				}
			}

			//Determine if the role exists and if it is able to be edited by both the bot and the user
			role = role ?? await Actions.getRoleEditAbility(Context, values[0]);
			if (role == null)
				return;

			//Get a before name
			var beforeName = role.Name;

			//Check if it's the everyone role
			if (role == Context.Guild.EveryoneRole)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Unable to rename the everyone role."));
				return;
			}

			//Change the name
			await Context.Guild.GetRole(role.Id).ModifyAsync(x => x.Name = values[1]);
			await Actions.makeAndDeleteSecondaryMessage(Context, String.Format("Successfully changed the name of the role `{0}` to `{1}`.", beforeName, values[1]));
		}

		[Command("rolecolor")]
		[Alias("rc")]
		[Usage("rolecolor Role/[Hexadecimal|Color Name]")]
		[Summary("Changes the role's color. A color of '0' sets the role back to the default color. " +
			"Colors must either be in hexadecimal format or be a color listed [here](https://msdn.microsoft.com/en-us/library/system.drawing.color).")]
		[PermissionRequirements(1U << (int)GuildPermission.ManageRoles)]
		public async Task ChangeRoleColor([Remainder] string input)
		{
			var values = input.Split(new char[] { '/' }, 2);

			//Determine if the role exists and if it is able to be edited by both the bot and the user
			var role = await Actions.getRoleEditAbility(Context, values[0]);
			if (role == null)
				return;

			UInt32 colorID = (UInt32)System.Drawing.Color.FromName(values[1]).ToArgb();
			if (colorID == 0)
			{
				//Couldn't get name
				var hexString = values[1];
				//Remove 0x if someone put that in there
				if (hexString.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
				{
					hexString = hexString.Substring(2);
				}
				//If the color ID isn't a hex number
				if (!UInt32.TryParse(hexString, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out colorID))
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Color is unable to be added."));
					return;
				}
			}

			//Change the color
			await Context.Guild.GetRole(role.Id).ModifyAsync(x => x.Color = new Color(colorID & 0xffffff));
			await Actions.makeAndDeleteSecondaryMessage(Context, String.Format("Successfully changed the color of the role `{0}` to `{1}`.",
				values[0], values[1]));
		}

		[Command("rolehoist")]
		[Alias("rh")]
		[Usage("rolehoist [Role]")]
		[Summary("Displays a role separately from others on the user list. Saying the command again remove it from being hoisted.")]
		[PermissionRequirements(1U << (int)GuildPermission.ManageRoles)]
		public async Task HoistRole([Remainder] string input)
		{
			var role = await Actions.getRoleEditAbility(Context, input);
			if (role == null)
				return;

			if (role.IsHoisted)
			{
				await role.ModifyAsync(x => x.Hoist = false);
				await Actions.makeAndDeleteSecondaryMessage(Context, "Successfully removed `" + role.Name + "` from being hoisted.");
			}
			else
			{
				await role.ModifyAsync(x => x.Hoist = true);
				await Actions.makeAndDeleteSecondaryMessage(Context, "Successfully hoisted `" + role.Name + "`.");
			}
		}

		[Command("rolementionability")]
		[Alias("rma")]
		[Usage("rolementionability [Role]")]
		[Summary("Allows the role to be mentioned. Saying the command again removes its ability to be mentioned.")]
		[PermissionRequirements(1U << (int)GuildPermission.ManageRoles)]
		public async Task ChangeMentionRole([Remainder] string input)
		{
			var role = await Actions.getRoleEditAbility(Context, input);
			if (role == null)
				return;

			if (role.IsMentionable)
			{
				await role.ModifyAsync(x => x.Mentionable = false);
				await Actions.makeAndDeleteSecondaryMessage(Context, "Successfully removed `" + role.Name + "` from being mentionable.");
			}
			else
			{
				await role.ModifyAsync(x => x.Mentionable = true);
				await Actions.makeAndDeleteSecondaryMessage(Context, "Successfully made `" + role.Name + "` mentionable.");
			}
		}
	}
}
