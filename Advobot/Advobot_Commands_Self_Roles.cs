using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Advobot
{
	[Name("Self Roles")]
	public class Advobot_Commands_Self_Roles : ModuleBase
	{
		[Command("selfrolesmodify")]
		[Alias("srm")]
		[Usage("selfrolesmodify [Help] | [Create|Add|Remove] [Role/...] [Group:Number] | [Delete] [Group:Num]")]
		[Summary("Adds a role to the self assignable list. Roles can be grouped together which means only one role in the group can be self assigned at a time. There is an extra help command, too.")]
		[PermissionRequirements]
		public async Task ModifySelfAssignableRoles([Remainder] string input)
		{
			//Check if they've enabled preferences
			if (Variables.Guilds[Context.Guild.Id].DefaultPrefs)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("You do not have preferences enabled."));
				return;
			}

			//Check if it's extra help wanted
			if (input.Equals("help", StringComparison.OrdinalIgnoreCase))
			{
				//Make the embed
				var embed = Actions.makeNewEmbed("Self Roles Help", "The general group number is 0; roles added here don't conflict. Roles cannot be added to more than one group.");
				Actions.addField(embed, "[Create] [Role/...] [Group:Number]", "The group number shows which group to create these roles as.");
				Actions.addField(embed, "[Add] [Role/...] [Group:Number]", "Adds the roles to the given group.");
				Actions.addField(embed, "[Remove] [Role/...] [Group:Number]", "Removes the roles from the given group.");
				Actions.addField(embed, "[Delete] [Group:Number]", "Removes the given group entirely.");

				//Send the embed
				await Actions.sendEmbedMessage(Context.Channel, embed);
				return;
			}

			//Break the input into pieces
			var inputArray = input.Split(new char[] { ' ' }, 2);
			var action = inputArray[0].ToLower();
			var rolesString = inputArray[1];

			//Check which action it is
			SAGAction actionType;
			if (action.Equals("create"))
				actionType = SAGAction.Create;
			else if (action.Equals("add"))
				actionType = SAGAction.Add;
			else if (action.Equals("remove"))
				actionType = SAGAction.Remove;
			else if (action.Equals("delete"))
				actionType = SAGAction.Delete;
			else
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ACTION_ERROR));
				return;
			}

			//Check if the guild has too many or no self assignable role lists yet
			if (actionType != SAGAction.Create)
			{
				if (!Variables.SelfAssignableGroups.Any())
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Before you can edit or delete a group, you need to first create one."));
					return;
				}
			}
			else
			{
				if (Variables.SelfAssignableGroups.Count == Constants.MAX_SA_GROUPS)
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, "You have too many groups. " + Constants.MAX_SA_GROUPS + " is the maximum.");
					return;
				}
			}

			//Success and failure lists
			var success = new List<IRole>();
			var failure = new List<string>();
			var deleted = new List<string>();

			//Necessary to know what group to target
			int groupNumber = 0;
			switch (actionType)
			{
				case SAGAction.Create:
				case SAGAction.Add:
				case SAGAction.Remove:
				{
					//Get the position of the last space
					int lastSpace = rolesString.LastIndexOf(' ');

					//Make the group string everything after the last space
					var groupString = rolesString.Substring(lastSpace).Trim();
					//Make the role string everything before the last space
					rolesString = rolesString.Substring(0, lastSpace).Trim();

					groupNumber = await Actions.getGroup(groupString, Context);
					if (groupNumber == -1)
						return;

					//Check if there are any groups already with that number
					var guildGroups = Variables.SelfAssignableGroups.Where(x => x.GuildID == Context.Guild.Id).ToList();
					//If create, do not allow a new one made with the same number
					if (actionType == SAGAction.Create)
					{
						if (guildGroups.Any(x => x.Group == groupNumber))
						{
							await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("A group already exists with that position."));
							return;
						}
					}
					//If add or remove, make sure one exists
					else
					{
						if (!guildGroups.Any(x => x.Group == groupNumber))
						{
							await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("A group needs to exist with that position before you can modify it."));
							return;
						}
					}

					//Check validity of roles
					rolesString.Split('/').ToList().ForEach(async x =>
					{
						IRole role = await Actions.getRoleEditAbility(Context, x, true);
						//If a valid role that the user is able to access for creation/addition/removal
						if (role == null)
						{
							failure.Add(x);
						}
						//If not then just add it to failures as a string
						else
						{
							success.Add(role);
						}
					});

					//Add all the roles to a list of self assignable roles
					var SARoles = success.Select(x => new SelfAssignableRole(x, groupNumber)).ToList();

					if (actionType != SAGAction.Remove)
					{
						//Find the groups in the guild
						var SAGroups = Variables.SelfAssignableGroups.Where(x => x.GuildID == Context.Guild.Id).ToList();
						//Make a new list of role IDs to check from
						var ulongs = new List<ulong>();
						//Add every single role ID to this list
						SAGroups.ForEach(x => ulongs.AddRange(x.Roles.Select(y => y.Role.Id)));
						//The roles on this list
						var removed = SARoles.Where(x => ulongs.Contains(x.Role.Id));
						//Add the roles to the failure list
						failure.AddRange(removed.Select(x => x.Role.ToString()));
						//Remove them from the success list
						success.RemoveAll(x => ulongs.Contains(x.Id));
						//Remove all roles which are already on the SA list
						SARoles.RemoveAll(x => ulongs.Contains(x.Role.Id));

						//Create
						if (actionType == SAGAction.Create)
						{
							//Make a new group and add that to the global list
							Variables.SelfAssignableGroups.Add(new SelfAssignableGroup(SARoles, groupNumber, Context.Guild.Id));
						}
						//Add
						else
						{
							//Add the roles to the group
							SAGroups.FirstOrDefault(x => x.Group == groupNumber).Roles.AddRange(SARoles);
						}
					}
					//Remove
					else
					{
						//Convert the list of SARoles to ulongs
						var ulongs = SARoles.Select(x => x.Role.Id).ToList();
						//Find the one with the correct group number and remove all roles which have an ID on the ulong list
						Variables.SelfAssignableGroups.Where(x => x.GuildID == Context.Guild.Id).FirstOrDefault(x => x.Group == groupNumber).Roles.RemoveAll(x => ulongs.Contains(x.Role.Id));
					}
					break;
				}
				case SAGAction.Delete:
				{
					groupNumber = await Actions.getGroup(inputArray[1], Context);
					if (groupNumber == -1)
						return;

					//Get the groups
					var guildGroups = Variables.SelfAssignableGroups.Where(x => x.GuildID == Context.Guild.Id).ToList();
					//Check if any groups have that position
					if (!guildGroups.Any(x => x.Group == groupNumber))
					{
						await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("A group needs to exist with that position before it can be deleted."));
						return;
					}

					//Get the group
					var group = guildGroups.FirstOrDefault(x => x.Group == groupNumber);
					//Get the roles it contains
					deleted = group.Roles.Select(x => x.Role.Name).ToList();
					//Delete the group
					guildGroups.Remove(group);
					break;
				}
			}

			//Get the file that's supposed to hold everything
			var path = Actions.getServerFilePath(Context.Guild.Id, Constants.SA_ROLES);
			if (path == null)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.PATH_ERROR));
				return;
			}
			File.Create(path).Close();

			//Rewrite it
			using (StreamWriter writer = new StreamWriter(path))
			{
				var savingString = "";
				Variables.SelfAssignableGroups.Where(x => x.GuildID == Context.Guild.Id).ToList().ForEach(x => x.Roles.ForEach(y => savingString += y.Role.Id + " " + y.Group + "\n"));
				writer.WriteLine(savingString);
			}

			//Make the success and failure strings
			var sString = "";
			var fString = "";
			bool sBool = success.Any();
			bool fBool = failure.Any();
			if (actionType == SAGAction.Create)
			{
				sString = sBool ? String.Format("Successfully created the group `{0}` with the following roles: `{1}`", groupNumber.ToString("00"), String.Join("`, `", success)) : "";
				fString = fBool ? String.Format("{0}ailed to add the following roles to `{1}`: `{2}`", sBool ? "f" : "F", groupNumber.ToString("00"), String.Join("`, `", failure)) : "";
			}
			else if (actionType == SAGAction.Add)
			{
				sString = sBool ? String.Format("Successfully added the following roles to `{0}`: `{1}`", groupNumber.ToString("00"), String.Join("`, `", success)) : "";
				fString = fBool ? String.Format("{0}ailed to add the following roles to `{1}`: `{2}`", sBool ? "f" : "F", groupNumber.ToString("00"), String.Join("`, `", failure)) : "";
			}
			else if (actionType == SAGAction.Remove)
			{
				sString = sBool ? String.Format("Successfully removed the following roles from `{0}`: `{1}`", groupNumber.ToString("00"), String.Join("`, `", success)) : "";
				fString = fBool ? String.Format("{0}ailed to remove the following roles from `{1}`: `{2}`", sBool ? "f" : "F", groupNumber.ToString("00"), String.Join("`, `", failure)) : "";
			}
			else
			{
				sString = String.Format("Successfully deleted the group `{0}` which held the following roles: `{1}`", groupNumber.ToString("00"), String.Join("`, `", deleted));
			}

			//Format the response message
			var responseMessage = "";
			if (sBool && fBool)
			{
				responseMessage = sString + ", and " + fString;
			}
			else
			{
				responseMessage = sString + fString;
			}

			//Send a success message
			await Actions.makeAndDeleteSecondaryMessage(Context, responseMessage + ".", 10000);
		}

		[Command("selfrolesassign")]
		[Alias("sra")]
		[Usage("selfrolesassign [Role]")]
		[Summary("Gives a role or takes a role depending on if the user has the role or not. Remove all other roles in the same group unless the group is 0.")]
		public async Task AssignSelfRole([Remainder] string input)
		{
			//Get the role. No edit ability checking in this command due to how that's already been done in the modify command
			IRole role = await Actions.getRole(Context, input);
			if (role == null)
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("There is no role with that name on this guild."));
				return;
			}

			//Check if any groups has it
			var SAGroups = Variables.SelfAssignableGroups.Where(x => x.GuildID == Context.Guild.Id).ToList();
			if (!SAGroups.Any(x => x.Roles.Select(y => y.Role).Contains(role)))
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("There is no self assignable role by that name."));
				return;
			}

			//Get the user as an IGuildUser
			var user = Context.User as IGuildUser;
			//Get their roles
			var roles = new List<IRole>();

			//Check if the user wants to remove their role
			if (user.RoleIds.Contains(role.Id))
			{
				await user.RemoveRolesAsync(new[] { role });
				await Actions.makeAndDeleteSecondaryMessage(Context, "Successfully removed the role `" + role.Name + "`.");
				return;
			}

			//Get the group that contains the role
			var SAGroup = Variables.SelfAssignableGroups.FirstOrDefault(x => x.Roles.Select(y => y.Role).Contains(role));
			//If a group that has stuff conflict, remove all but the wanted role
			if (SAGroup.Group != 0)
			{
				//Find the intersection of the group's roles and the user's roles
				roles = SAGroup.Roles.Select(x => x.Role.Id).Intersect(user.RoleIds).Select(x => Context.Guild.GetRole(x)).ToList();
				//Check if the user already has the role they're wanting
				if (roles.Contains(role))
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, "You already have that role.");
					return;
				}
			}
			//Give the wanted role to the user
			await user.ChangeRolesAsync(new[] { role }, roles);

			//Format a success message
			var removedRoles = "";
			if (roles.Any())
			{
				removedRoles = String.Format(", and removed `{0}`", String.Join("`, `", roles));
			}

			//Send the message
			await Actions.makeAndDeleteSecondaryMessage(Context, "Successfully gave you `" + role.Name + "`" + removedRoles + ".");
		}

		[Command("selfrolesgroups")]
		[Alias("srg")]
		[Usage("selfrolesgroups <File|Actual>")]
		[Summary("Shows the current group numbers that exists on the guild.")]
		public async Task CurrentGroups([Optional, Remainder] string input)
		{
			//Set a bool
			bool fileBool;
			if (String.IsNullOrWhiteSpace(input))
			{
				fileBool = false;
			}
			else if (input.Equals("file", StringComparison.OrdinalIgnoreCase))
			{
				fileBool = true;
			}
			else if (input.Equals("actual", StringComparison.OrdinalIgnoreCase))
			{
				fileBool = false;
			}
			else
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ACTION_ERROR));
				return;
			}

			var groupNumbers = new List<int>();
			if (fileBool)
			{
				//Check if the file exists
				var path = Actions.getServerFilePath(Context.Guild.Id, Constants.SA_ROLES);
				if (path == null)
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.PATH_ERROR));
					return;
				}
				if (!File.Exists(path))
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, "This guild has no self assignable roles file.");
					return;
				}

				//Get all the self roles that have that group
				var validLines = Actions.getValidLines(path, null);
				validLines.ForEach(x =>
				{
					//Split to get the role ID and the group
					var lineArray = x.Split(' ');
					int throwaway;
					if (int.TryParse(lineArray[1], out throwaway))
					{
						groupNumbers.Add(throwaway);
					}
				});

				if (!groupNumbers.Any())
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("There are currently no self assignable role groups on file."));
					return;
				}
			}
			else
			{
				groupNumbers = Variables.SelfAssignableGroups.Where(x => x.GuildID == Context.Guild.Id).Select(x => x.Group).Distinct().ToList();
				if (!groupNumbers.Any())
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("There are currently no self assignable role groups on this guild."));
					return;
				}
			}

			//Send a sucess message
			await Actions.sendEmbedMessage(Context.Channel, Actions.makeNewEmbed("Self Assignable Role Groups", String.Join(", ", groupNumbers.OrderBy(x => x).Distinct())));
		}

		[Command("selfrolescurrent")]
		[Alias("src")]
		[Usage("selfrolescurrent <File|Actual> [Group:Number]")]
		[Summary("Shows the current self assignable roles on the guild by group.")]
		public async Task CurrentSelfRoles([Remainder] string input)
		{
			//Split the input
			var inputArray = input.Split(new char[] { ' ' }, 2);

			bool fileBool;
			if (inputArray[0].Equals("file", StringComparison.OrdinalIgnoreCase))
			{
				fileBool = true;
			}
			else if (inputArray[0].Equals("actual", StringComparison.OrdinalIgnoreCase))
			{
				fileBool = false;
			}
			else if (inputArray[0].StartsWith("group", StringComparison.OrdinalIgnoreCase))
			{
				fileBool = false;
			}
			else
			{
				await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid option, must be either File or Actual or a Group."));
				return;
			}

			var description = "";
			int groupNumber;
			if (fileBool)
			{
				//Get the group number
				groupNumber = await Actions.getGroup(inputArray, Context);
				if (groupNumber == -1)
					return;

				//Check if the file exists
				var path = Actions.getServerFilePath(Context.Guild.Id, Constants.SA_ROLES);
				if (path == null)
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.PATH_ERROR));
					return;
				}
				if (!File.Exists(path))
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, "This guild has no self assignable roles file.");
					return;
				}

				//Get all the self roles that have that group
				var lines = Actions.getValidLines(path, null);
				var roleIDs = lines.Where(x =>
				{
					var lineArray = x.Split(' ');
					if (lineArray.Length > 2 && lineArray[1].Equals(groupNumber.ToString()))
						return true;
					return false;
				}).ToList();

				//Check if any role IDs were gotten
				if (!roleIDs.Any())
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("There is no group with that number."));
					return;
				}

				//Get the roleIDs as roles
				var roles = new List<string>();
				roleIDs.ForEach(x =>
				{
					//Check if it's an actual number
					ulong roleUlong;
					if (ulong.TryParse(x, out roleUlong))
					{
						//Check if valid role
						IRole role = Context.Guild.GetRole(roleUlong);
						if (role != null)
						{
							roles.Add(role.Name);
						}
					}
				});

				//Check if any valid roles gotten
				if (!roles.Any())
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("That group has no valid self roles."));
					return;
				}

				//Add the roles to the list
				description = "`" + String.Join("`\n`", roles) + "`";
			}
			else
			{
				//Get the group number
				groupNumber = await Actions.getGroup(inputArray, Context);
				if (groupNumber == -1)
					return;

				//Get the group which has that number
				var group = Variables.SelfAssignableGroups.Where(x => x.GuildID == Context.Guild.Id).FirstOrDefault(x => x.Group == groupNumber);
				if (group == null)
				{
					await Actions.makeAndDeleteSecondaryMessage(Context, Actions.ERROR("There is no group with that number."));
					return;
				}

				//Add the group's role's names to a list
				description = "`" + String.Join("`\n`", group.Roles.Select(x => x.Role.Name).ToList()) + "`";
			}

			//Send the embed
			await Actions.sendEmbedMessage(Context.Channel, Actions.makeNewEmbed(String.Format("Self Roles Group {0} ({1})", groupNumber, fileBool ? "File" : "Actual"), description));
		}
	}
}
