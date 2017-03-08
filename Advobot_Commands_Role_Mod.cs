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
		[Usage("[@User] [Role]/<Role>/...")]
		[Summary("Gives the user the role (assuming the person using the command and bot both have the ability to give that role).")]
		[PermissionRequirement(1U << (int)GuildPermission.ManageRoles)]
		public async Task GiveRole([Remainder] string input)
		{
			//Test number of arguments
			var inputArray = input.Split(new char[] { ' ' }, 2);
			if (inputArray.Length != 2)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR));
				return;
			}

			//Test if valid user mention
			var inputUser = await Actions.GetUser(Context.Guild, inputArray[0]);
			if (inputUser == null)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.USER_ERROR));
				return;
			}

			//Determine if the role exists and if it is able to be edited by both the bot and the user
			var inputRoles = inputArray[1].Split('/').ToList();
			if (inputRoles.Count == 1)
			{
				//Check if it actually exists
				var role = await Actions.GetRoleEditAbility(Context, inputRoles[0]);
				if (role == null)
					return;

				//See if the role is unable to be given due to management
				if (role.IsManaged)
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Role is managed and unable to be given."));
					return;
				}

				//Check if trying to delete the everyone role
				if (role == Context.Guild.EveryoneRole)
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Unable to give the everyone role."));
					return;
				}

				//Give the role and make a message
				await Actions.GiveRole(inputUser, role);
				await Actions.MakeAndDeleteSecondaryMessage(Context,
					String.Format("Successfully gave `{0}#{1}` the `{2}` role.", inputUser.Username, inputUser.Discriminator, role));
			}
			else
			{
				var failedRoles = new List<string>();
				var succeededRoles = new List<string>();
				var roles = new List<IRole>();
				await inputRoles.ForEachAsync(async roleName =>
				{
					var role = await Actions.GetRoleEditAbility(Context, roleName, true);
					if (role == null || role.IsManaged || role == Context.Guild.EveryoneRole)
					{
						failedRoles.Add(roleName);
					}
					else
					{
						roles.Add(role);
						succeededRoles.Add(roleName);
					}
				});

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

				await Actions.GiveRole(inputUser, roles.ToArray());
				await Actions.MakeAndDeleteSecondaryMessage(Context, succeed + and + failed);
			}
		}

		[Command("roletake")]
		[Alias("rta")]
		[Usage("[@User] [Role]")]
		[Summary("Take the role from the user (assuming the person using the command and bot both have the ability to take that role).")]
		[PermissionRequirement(1U << (int)GuildPermission.ManageRoles)]
		public async Task TakeRole([Remainder] string input)
		{
			//Test number of arguments
			var inputArray = input.Split(new char[] { ' ' }, 2);
			if (inputArray.Length != 2)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR));
				return;
			}

			//Test if valid user mention
			var inputUser = await Actions.GetUser(Context.Guild, inputArray[0]);
			if (inputUser == null)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.USER_ERROR));
				return;
			}

			//Determine if the role exists and if it is able to be edited by both the bot and the user
			var inputRoles = inputArray[1].Split('/').ToList();
			if (inputRoles.Count == 1)
			{
				//Check if it actually exists
				var role = await Actions.GetRoleEditAbility(Context, inputRoles[0]);
				if (role == null)
					return;

				//See if the role is unable to be taken due to management
				if (role.IsManaged)
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Role is managed and unable to be taken."));
					return;
				}

				//Check if trying to delete the everyone role
				if (role == Context.Guild.EveryoneRole)
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Unable to take the everyone role."));
					return;
				}

				//Take the role and make a message
				await Actions.TakeRole(inputUser, role);
				await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully took `{0}` from `{1}#{2}`.", role, inputUser.Username, inputUser.Discriminator));
			}
			else
			{
				var failedRoles = new List<string>();
				var succeededRoles = new List<string>();
				var roles = new List<IRole>();
				await inputRoles.ForEachAsync(async roleName =>
				{
					var role = await Actions.GetRoleEditAbility(Context, roleName, true);
					if (role == null || role.IsManaged || role == Context.Guild.EveryoneRole)
					{
						failedRoles.Add(roleName);
					}
					else
					{
						roles.Add(role);
						succeededRoles.Add(roleName);
					}
				});

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

				await Actions.TakeRole(inputUser, roles.ToArray());
				await Actions.MakeAndDeleteSecondaryMessage(Context, succeed + and + failed);
			}
		}

		[Command("rolecreate")]
		[Alias("rcr")]
		[Usage("[Role]")]
		[Summary("Adds a role to the guild with the chosen name.")]
		[PermissionRequirement(1U << (int)GuildPermission.ManageRoles)]
		public async Task CreateRole([Remainder] string input)
		{
			//Check length
			if (input.Length > Constants.ROLE_NAME_MAX_LENGTH)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(String.Format("Roles can only have a name length of up to {0} characters.", Constants.ROLE_NAME_MAX_LENGTH)));
				return;
			}
			else if (input.Length < Constants.ROLE_NAME_MIN_LENGTH)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(String.Format("Roles need to have a name equal to or greater than {0} characters.", Constants.ROLE_NAME_MIN_LENGTH)));
				return;
			}

			//Create role
			await Context.Guild.CreateRoleAsync(input, new GuildPermissions(0));
			await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully created the `{0}` role.", input));
		}

		[Command("rolesoftdelete")]
		[Alias("rsd")]
		[Usage("[Role]")]
		[Summary("Removes all permissions from a role (and all channels the role had permissions on) and removes the role from everyone. Leaves the name and color behind.")]
		[PermissionRequirement(1U << (int)GuildPermission.ManageRoles)]
		public async Task SoftDeleteRole([Remainder] string input)
		{
			//Determine if the role exists and if it is able to be edited by both the bot and the user
			var inputRole = await Actions.GetRoleEditAbility(Context, input);
			if (inputRole == null)
				return;

			//Check if even removable
			if (inputRole.IsManaged)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Role is managed and unable to be softdeleted."));
				return;
			}

			//Check if trying to delete the everyone role
			if (inputRole == Context.Guild.EveryoneRole)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Unable to softdelete the everyone role."));
				return;
			}

			//Create a new role with the same attributes (including space) and no perms
			var newRole = await Context.Guild.CreateRoleAsync(inputRole.Name, new GuildPermissions(0), inputRole.Color);

			//Make a new list of IRole
			var roles = Context.Guild.Roles.Where(x => x != newRole).OrderBy(x => x.Position).ToList();
			//Add in the targetted role with the given position
			roles.Insert(Math.Min(roles.Count(), inputRole.Position), newRole);

			//Make a new list of BulkRoleProperties
			var listOfBulk = roles.Select(x => new BulkRoleProperties(x.Id)).ToList();
			//Readd the positions to it
			listOfBulk.ForEach(x => x.Position = listOfBulk.IndexOf(x));
			//Mass modify the roles with the list having the correct positions
			await Context.Guild.ModifyRolesAsync(listOfBulk);

			//Delete the old role
			await inputRole.DeleteAsync();
			await Actions.MakeAndDeleteSecondaryMessage(Context,
				String.Format("Successfully removed all permissions from `{0}` and removed the role from all users on the guild.", inputRole.Name));
		}

		[Command("roledelete")]
		[Alias("rd")]
		[Usage("[Role]")]
		[Summary("Deletes the role..")]
		[PermissionRequirement(1U << (int)GuildPermission.ManageRoles)]
		public async Task DeleteRole([Remainder] string input)
		{
			//Determine if the role exists and if it is able to be edited by both the bot and the user
			var role = await Actions.GetRoleEditAbility(Context, input);
			if (role == null)
				return;

			//Check if even removable
			if (role.IsManaged)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Role is managed and unable to be deleted."));
				return;
			}

			//Check if trying to delete the everyone role
			if (role == Context.Guild.EveryoneRole)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Unable to delete the everyone role."));
				return;
			}

			await role.DeleteAsync();
			await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully deleted the `{0}` role.", input));
		}

		[Command("roleposition")]
		[Alias("rpos")]
		[Usage("[Role] [New Position]")]
		[Summary("Moves the role to the given position. @ev" + Constants.ZERO_LENGTH_CHAR + "eryone is the first position and starts at zero.")]
		[PermissionRequirement(1U << (int)GuildPermission.ManageRoles)]
		public async Task RolePosition([Remainder] string input)
		{
			//Get the role
			var role = await Actions.GetRoleEditAbility(Context, input.Substring(0, input.LastIndexOf(' ')), true);
			if (role == null)
				return;

			//Check if it's the everyone role
			if (role == Context.Guild.EveryoneRole)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Unable to move the everyone role."));
				return;
			}

			//Get the position as an int
			var position = 0;
			if (!int.TryParse(input.Substring(input.LastIndexOf(' ')), out position))
			{
				await Actions.SendChannelMessage(Context, String.Format("The `{0}` role has a position of `{1}`.", role.Name, role.Position));
				return;
			}

			//Checking if valid positions
			var maxPos = 0;
			Context.Guild.Roles.ToList().ForEach(x => maxPos = Math.Max(maxPos, x.Position));
			if (position <= 0)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Cannot set a role to a position lower than or equal to one."));
				return;
			}
			else if (position > maxPos)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Cannot set a role to a position higher than the highest role."));
				return;
			}

			//See if the user can access that position
			if (position > Actions.GetPosition(Context.Guild, Context.User as IGuildUser))
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Position is higher than you can access."));
				return;
			}
			//See if the bot can access that position
			if (position > Actions.GetPosition(Context.Guild, await Context.Guild.GetUserAsync(Variables.Bot_ID)))
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Position is higher than the bot can access."));
				return;
			}

			//Grab all roles that aren't the targeted one
			var roles = Context.Guild.Roles.Where(x => x != role).ToList().OrderBy(x => x.Position).ToList();
			//Add in the targetted role with the given position
			roles.Insert(Math.Min(roles.Count(), position), role);
			//Make a new list of BulkRoleProperties
			var listOfBulk = roles.Select(x => new BulkRoleProperties(x.Id)).ToList();
			//Readd the positions to it
			listOfBulk.ForEach(x => x.Position = listOfBulk.IndexOf(x));
			//Mass modify the roles with the list having the correct positions
			await Context.Guild.ModifyRolesAsync(listOfBulk);

			//Send a message stating what position the channel was sent to
			await Actions.SendChannelMessage(Context, String.Format("Successfully gave the `{0}` role the position `{1}`.", role.Name, role.Position));
		}

		[Command("rolepositions")]
		[Alias("rposs")]
		[Usage("")]
		[Summary("Lists the positions of each role on the guild.")]
		[PermissionRequirement(1U << (int)GuildPermission.ManageRoles)]
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
					description += "`" + role.Position.ToString("00") + ".` @ev" + Constants.ZERO_LENGTH_CHAR + "eryone";
				}
				else
				{
					description += "`" + role.Position.ToString("00") + ".` " + role.Name + "\n";
				}
			});

			//Check the length to see if the message can be sent
			if (description.Length > Constants.SHORT_LENGTH_CHECK)
			{
				description = Actions.UploadToHastebin(Actions.ReplaceMarkdownChars(description));
			}

			//Send the embed
			await Actions.SendEmbedMessage(Context.Channel, Actions.MakeNewEmbed("Role Positions", description));
		}

		[Command("roleperms")]
		[Alias("rp")]
		[Usage("[Show|Add|Remove] [Role] [Permission/...]")]
		[Summary("Add/remove the selected permissions to/from the role. Permissions must be separated by a `/`! Type `" + Constants.BOT_PREFIX + "rolepermissions [Show]` to see the available permissions. " +
			"Type `" + Constants.BOT_PREFIX + "rolepermissions [Show] [Role]` to see the permissions of that role.")]
		[PermissionRequirement(1U << (int)GuildPermission.ManageRoles)]
		public async Task RolePermissions([Remainder] string input)
		{
			//Set the permission types into a list to later check against
			var permissionTypeStrings = Variables.GuildPermissions.Select(x => x.Name).ToList();

			//Separate the role and whether to add or remove from the permissions
			var inputArray = input.Split(new char[] { ' ' }, 2);
			var permsString = "";
			var roleString = "";
			var show = false;

			//If the user wants to see the permission types, print them out
			if (input.Equals("show", StringComparison.OrdinalIgnoreCase))
			{
				await Actions.SendEmbedMessage(Context.Channel, Actions.MakeNewEmbed("Role Permissions", String.Join("\n", permissionTypeStrings)));
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
				if (inputArray.Length == 1)
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR));
					return;
				}
				var lastSpace = inputArray[1].LastIndexOf(' ');
				if (lastSpace <= 0)
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR));
					return;
				}
				//Separate out the permissions
				permsString = inputArray[1].Substring(lastSpace).Trim();
				//Separate out the role
				roleString = inputArray[1].Substring(0, lastSpace).Trim();
			}

			//Determine if the role exists and if it is able to be edited by both the bot and the user
			var role = await Actions.GetRoleEditAbility(Context, roleString);
			if (role == null)
				return;

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
			var addOrRemove = inputArray[0];
			bool add;
			if (addOrRemove.Equals("add", StringComparison.OrdinalIgnoreCase))
			{
				add = true;
			}
			else if (addOrRemove.Equals("remove", StringComparison.OrdinalIgnoreCase))
			{
				add = false;
			}
			else
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Add or remove not specified."));
				return;
			}

			//Get the permissions
			var permissions = permsString.Split('/').ToList();
			//Check if valid permissions
			var validPerms = permissions.Intersect(permissionTypeStrings, StringComparer.OrdinalIgnoreCase).ToList();
			if (validPerms.Count != permissions.Count)
			{
				var invalidPermissions = new List<string>();
				permissions.ForEach(permission =>
				{
					if (!validPerms.Contains(permission, StringComparer.OrdinalIgnoreCase))
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
					var bit = Variables.GuildPermissions.FirstOrDefault(x => x.Name.Equals(permission, StringComparison.OrdinalIgnoreCase)).Position;
					rolePermissions |= (1U << bit);
				}
				catch (Exception)
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(String.Format("Couldn't parse permission '{0}'", permission)));
					return;
				}
			});

			//Determine if the user can give these perms
			if (!await Actions.GetIfUserIsOwner(Context.Guild, Context.User as IGuildUser))
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
				(add ? "to" : "from"), role.Name),
				7500);
		}

		[Command("rolepermscopy")]
		[Alias("rpc")]
		[Usage("[Role]/[Role]")]
		[Summary("Copies the permissions from the first role to the second role. Will not copy roles that the user does not have access to." +
			"Will not overwrite roles that are above the user's top role.")]
		[PermissionRequirement(1U << (int)GuildPermission.ManageRoles)]
		public async Task CopyRolePermissions([Remainder] string input)
		{
			//Put the input into a string
			var inputArray = input.Split(new char[] { '/' }, 2);

			//Test if two roles were input
			if (inputArray.Length != 2)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR));
				return;
			}

			//Determine if the input role exists
			var inputRole = await Actions.GetRole(Context, inputArray[0]);
			if (inputRole == null)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ROLE_ERROR));
				return;
			}

			//Determine if the role exists and if it is able to be edited by both the bot and the user
			var outputRole = await Actions.GetRoleEditAbility(Context, inputArray[1]);
			if (outputRole == null)
				return;

			//Get the permissions
			var rolePermissions = (uint)inputRole.Permissions.RawValue;
			var permissions = Actions.GetPermissionNames(rolePermissions).ToList();
			if (rolePermissions != 0)
			{
				//Determine if the user can give these permissions
				if (!await Actions.GetIfUserIsOwner(Context.Guild, Context.User as IGuildUser))
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

			//Get a list of the permissions that were given
			var givenPermissions = Actions.GetPermissionNames(rolePermissions).ToList();
			//Get a list of the permissions that were not given
			var skippedPermissions = permissions.Except(givenPermissions).ToList();

			//Actually change the permissions
			await Context.Guild.GetRole(outputRole.Id).ModifyAsync(x => x.Permissions = new GuildPermissions(rolePermissions));
			//Send the long ass message detailing what happened with the command
			await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully copied `{0}` {1} from `{2}` to `{3}`.",
				(givenPermissions.Count == 0 ? "NOTHING" : givenPermissions.Count == permissions.Count ? "ALL" : String.Join("`, `", givenPermissions)),
				(skippedPermissions.Any() ? "and failed to copy `" + String.Join("`, `", skippedPermissions) + "`" : ""),
				inputRole, outputRole),
				7500);
		}

		[Command("rolepermsclear")]
		[Alias("rpcl")]
		[Usage("[Role]")]
		[Summary("Removes all permissions from a role.")]
		[PermissionRequirement(1U << (int)GuildPermission.ManageRoles)]
		public async Task ClearRolePermissions([Remainder] string input)
		{
			//Determine if the role exists and if it is able to be edited by both the bot and the user
			var role = await Actions.GetRoleEditAbility(Context, input);
			if (role == null)
				return;

			//Clear the role's perms
			await role.ModifyAsync(x => x.Permissions = new GuildPermissions(0));
			await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully removed all permissions from `{0}`.", input));
		}

		[Command("rolename")]
		[Alias("rn")]
		[Usage("[Role|Position{x}]/[New Name]")]
		[Summary("Changes the name of the role. This is *extremely* useful for when multiple roles have the same name but you want to edit things.")]
		[PermissionRequirement(1U << (int)GuildPermission.ManageRoles)]
		public async Task ChangeRoleName([Remainder] string input)
		{
			//Split at the current role name and the new role name
			var inputArray = input.Split(new char[] { '/' }, 2);

			//Check if correct number of arguments
			if (inputArray.Length != 2)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ARGUMENTS_ERROR));
				return;
			}

			//Check length
			var newName = inputArray[1];
			if (newName.Length > Constants.ROLE_NAME_MAX_LENGTH)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(String.Format("Roles can only have a name length of up to {0} characters.", Constants.ROLE_NAME_MAX_LENGTH)));
				return;
			}
			else if (newName.Length < Constants.ROLE_NAME_MIN_LENGTH)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(String.Format("Roles need to have a name equal to or greater than {0} characters.", Constants.ROLE_NAME_MIN_LENGTH)));
				return;
			}

			//Initialize the role
			IRole role = null;

			//See if it's a position trying to be gotten instead
			var roleInput = inputArray[0];
			if (roleInput.IndexOf("position{", StringComparison.OrdinalIgnoreCase) >= 0)
			{
				//Get the position
				int position;
				var leftBracePos = roleInput.IndexOf('{');
				var rightBracePos = roleInput.IndexOf('}');
				if (!int.TryParse(roleInput.Substring(leftBracePos, rightBracePos), out position))
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid position supplied."));
					return;
				}

				//Grab the roles with the position
				var roles = Context.Guild.Roles.Where(x => x.Position == position).ToList();
				if (roles.Count == 0)
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(String.Format("No role has a position of `{0}`", position)));
					return;
				}
				else if (roles.Count == 1)
				{
					//Get the role
					role = await Actions.GetRoleEditAbility(Context, role: roles.First());
				}
				else
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("`{0}` roles have the position `{1}`.", roles.Count, position));
					return;
				}
			}

			//Determine if the role exists and if it is able to be edited by both the bot and the user
			role = role ?? await Actions.GetRoleEditAbility(Context, roleInput);
			if (role == null)
				return;

			//Get a before name
			var beforeName = role.Name;

			//Check if it's the everyone role
			if (role == Context.Guild.EveryoneRole)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Unable to rename the everyone role."));
				return;
			}

			//Change the name
			await Context.Guild.GetRole(role.Id).ModifyAsync(x => x.Name = newName);
			await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully changed the name of the role `{0}` to `{1}`.", beforeName, role.Name));
		}

		[Command("rolecolor")]
		[Alias("rc")]
		[Usage("[Role]/[Hexadecimal|Color Name]")]
		[Summary("Changes the role's color. A color of '0' sets the role back to the default color. " +
			"Colors must either be in hexadecimal format or be a color listed [here](https://msdn.microsoft.com/en-us/library/system.drawing.color).")]
		[PermissionRequirement(1U << (int)GuildPermission.ManageRoles)]
		public async Task ChangeRoleColor([Remainder] string input)
		{
			var inputArray = input.Split(new char[] { '/' }, 2);

			//Determine if the role exists and if it is able to be edited by both the bot and the user
			var role = await Actions.GetRoleEditAbility(Context, inputArray[0]);
			if (role == null)
				return;

			UInt32 colorID = (UInt32)System.Drawing.Color.FromName(inputArray[1]).ToArgb();
			if (colorID == 0)
			{
				//Couldn't get name
				var hexString = inputArray[1];
				//Remove 0x if someone put that in there
				if (hexString.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
				{
					hexString = hexString.Substring(2);
				}
				//If the color ID isn't a hex number
				if (!UInt32.TryParse(hexString, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out colorID))
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Color is unable to be added."));
					return;
				}
			}

			//Change the color
			await Context.Guild.GetRole(role.Id).ModifyAsync(x => x.Color = new Color(colorID & 0xffffff));
			await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully changed the color of the role `{0}` to `{1}`.", inputArray[0], inputArray[1]));
		}

		[Command("rolehoist")]
		[Alias("rh")]
		[Usage("[Role]")]
		[Summary("Displays a role separately from others on the user list. Saying the command again remove it from being hoisted.")]
		[PermissionRequirement(1U << (int)GuildPermission.ManageRoles)]
		public async Task HoistRole([Remainder] string input)
		{
			var role = await Actions.GetRoleEditAbility(Context, input);
			if (role == null)
				return;

			if (role.IsHoisted)
			{
				await role.ModifyAsync(x => x.Hoist = false);
				await Actions.MakeAndDeleteSecondaryMessage(Context, "Successfully removed `" + role.Name + "` from being hoisted.");
			}
			else
			{
				await role.ModifyAsync(x => x.Hoist = true);
				await Actions.MakeAndDeleteSecondaryMessage(Context, "Successfully hoisted `" + role.Name + "`.");
			}
		}

		[Command("rolementionability")]
		[Alias("rma")]
		[Usage("[Role]")]
		[Summary("Allows the role to be mentioned. Saying the command again removes its ability to be mentioned.")]
		[PermissionRequirement(1U << (int)GuildPermission.ManageRoles)]
		public async Task ChangeMentionRole([Remainder] string input)
		{
			var role = await Actions.GetRoleEditAbility(Context, input);
			if (role == null)
				return;

			if (role.IsMentionable)
			{
				await role.ModifyAsync(x => x.Mentionable = false);
				await Actions.MakeAndDeleteSecondaryMessage(Context, "Successfully removed `" + role.Name + "` from being mentionable.");
			}
			else
			{
				await role.ModifyAsync(x => x.Mentionable = true);
				await Actions.MakeAndDeleteSecondaryMessage(Context, "Successfully made `" + role.Name + "` mentionable.");
			}
		}
	}
}
