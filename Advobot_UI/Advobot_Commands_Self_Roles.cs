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
	[Name("Self_Roles")]
	public class Advobot_Commands_Self_Roles : ModuleBase
	{
		[Command("selfrolesmodify")]
		[Alias("srm")]
		[Usage("[Help] | [Create|Add|Remove] [Role/...] [Group:Number] | [Delete] [Group:Num]")]
		[Summary("Adds a role to the self assignable list. Roles can be grouped together which means only one role in the group can be self assigned at a time. There is an extra help command too.")]
		[PermissionRequirement]
		[DefaultEnabled(false)]
		public async Task ModifySelfAssignableRoles([Remainder] string input)
		{
			//Check if using the default preferences
			var guildInfo = Variables.Guilds[Context.Guild.Id];
			if (guildInfo.DefaultPrefs)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.DENY_WITHOUT_PREFERENCES));
				return;
			}

			//Check if it's extra help wanted
			if (Actions.CaseInsEquals(input, "help"))
			{
				//Make the embed
				var embed = Actions.MakeNewEmbed("Self Roles Help", "The general group number is 0; roles added here don't conflict. Roles cannot be added to more than one group.");
				Actions.AddField(embed, "[Create] [Role/...] [Group:Number]", "The group number shows which group to create these roles as.");
				Actions.AddField(embed, "[Add] [Role/...] [Group:Number]", "Adds the roles to the given group.");
				Actions.AddField(embed, "[Remove] [Role/...] [Group:Number]", "Removes the roles from the given group.");
				Actions.AddField(embed, "[Delete] [Group:Number]", "Removes the given group entirely.");

				//Send the embed
				await Actions.SendEmbedMessage(Context.Channel, embed);
				return;
			}

			//Break the input into pieces
			var inputArray = input.Split(new char[] { ' ' }, 2);
			var action = inputArray[0];
			var rolesString = inputArray[1];

			//Check which action it is
			if (!Enum.TryParse(action, out SAGAction actionType))
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ACTION_ERROR));
				return;
			}

			//Check if the guild has too many or no self assignable role lists yet
			if (actionType != SAGAction.Create)
			{
				if (!guildInfo.SelfAssignableGroups.Any())
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Before you can edit or delete a group, you need to first create one."));
					return;
				}
			}
			else
			{
				if (guildInfo.SelfAssignableGroups.Count == Constants.MAX_SA_GROUPS)
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, "You have too many groups. " + Constants.MAX_SA_GROUPS + " is the maximum.");
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

					//Make sure valid last space
					if (lastSpace < 1)
					{
						await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("Invalid group supplied."));
						return;
					}

					//Make the group string everything after the last space
					var groupString = rolesString.Substring(lastSpace).Trim();
					//Make the role string everything before the last space
					rolesString = rolesString.Substring(0, lastSpace).Trim();

					groupNumber = await Actions.GetIfGroupIsValid(Context, Actions.GetVariable(groupString, "group"));
					if (groupNumber == -1)
						return;

					//Check if there are any groups already with that number
					var guildGroups = guildInfo.SelfAssignableGroups;
					//If create, do not allow a new one made with the same number
					if (actionType == SAGAction.Create)
					{
						if (guildGroups.Any(x => x.Group == groupNumber))
						{
							await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("A group already exists with that position."));
							return;
						}
					}
					//If add or remove, make sure one exists
					else
					{
						if (!guildGroups.Any(x => x.Group == groupNumber))
						{
							await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("A group needs to exist with that position before you can modify it."));
							return;
						}
					}

					//Check validity of roles
					await rolesString.Split('/').ToList().ForEachAsync(async x =>
					{
						var role = await Actions.GetRoleEditAbility(Context, x, true);
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
					var SARoles = success.Select(x => new SelfAssignableRole(Context.Guild.Id, x.Id)).ToList();

					if (actionType != SAGAction.Remove)
					{
						var SAGroups = guildInfo.SelfAssignableGroups;
						var ulongs = SAGroups.SelectMany(x => x.Roles).Select(x => x.RoleID);
						var removed = SARoles.Where(x => ulongs.Contains(x.Role.Id));
						failure.AddRange(removed.Select(x => x.Role.ToString()));
						success.RemoveAll(x => ulongs.Contains(x.Id));
						SARoles.RemoveAll(x => ulongs.Contains(x.RoleID));

						if (actionType == SAGAction.Create)
						{
							//Make a new group and add that to the global list
							guildInfo.SelfAssignableGroups.Add(new SelfAssignableGroup(SARoles, groupNumber));
						}
						else
						{
							//Add the roles to the group
							SAGroups.FirstOrDefault(x => x.Group == groupNumber).AddRoles(SARoles);
						}
					}
					//Remove
					else
					{
						//Find the one with the correct group number and remove all roles which have an ID on the ulong list
						guildInfo.SelfAssignableGroups.FirstOrDefault(x => x.Group == groupNumber).RemoveRoles(SARoles.Select(x => x.Role.Id).ToList());
					}
					break;
				}
				case SAGAction.Delete:
				{
                    groupNumber = await Actions.GetIfGroupIsValid(Context, Actions.GetVariable(inputArray[1], "group"));
					if (groupNumber == -1)
						return;

					//Get the groups
					var guildGroups = guildInfo.SelfAssignableGroups;
					//Check if any groups have that position
					if (!guildGroups.Any(x => x.Group == groupNumber))
					{
					    await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("A group needs to exist with that position before it can be deleted."));
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

			//Make the success and failure strings
			var sString = "";
			var fString = "";
			var sBool = success.Any();
			var fBool = failure.Any();
			switch (actionType)
			{
				case SAGAction.Create:
				{
					sString = sBool ? String.Format("Successfully created the group `{0}` with the following roles: `{1}`", groupNumber.ToString("00"), String.Join("`, `", success)) : "";
					fString = fBool ? String.Format("{0}ailed to add the following roles to `{1}`: `{2}`", sBool ? "f" : "F", groupNumber.ToString("00"), String.Join("`, `", failure)) : "";
					break;
				}
				case SAGAction.Add:
				{
					sString = sBool ? String.Format("Successfully added the following roles to `{0}`: `{1}`", groupNumber.ToString("00"), String.Join("`, `", success)) : "";
					fString = fBool ? String.Format("{0}ailed to add the following roles to `{1}`: `{2}`", sBool ? "f" : "F", groupNumber.ToString("00"), String.Join("`, `", failure)) : "";
					break;
				}
				case SAGAction.Remove:
				{
					sString = sBool ? String.Format("Successfully removed the following roles from `{0}`: `{1}`", groupNumber.ToString("00"), String.Join("`, `", success)) : "";
					fString = fBool ? String.Format("{0}ailed to remove the following roles from `{1}`: `{2}`", sBool ? "f" : "F", groupNumber.ToString("00"), String.Join("`, `", failure)) : "";
					break;
				}
				case SAGAction.Delete:
				{
					sString = String.Format("Successfully deleted the group `{0}` which held the following roles: `{1}`", groupNumber.ToString("00"), String.Join("`, `", deleted));
					break;
				}
			}
			var responseMessage = (sBool && fBool) ? (sString + ", and " + fString) : (sString + fString);

			//Save everything and send a success message
			Actions.SaveGuildInfo(guildInfo);
			await Actions.MakeAndDeleteSecondaryMessage(Context, responseMessage + ".", 10000);
		}

		[Command("selfrolesassign")]
		[Alias("sra")]
		[Usage("[Role]")]
		[Summary("Gives or takes a role depending on if the user has it already. Removes all other roles in the same group unless the group is `0`.")]
		[DefaultEnabled(false)]
		public async Task AssignSelfRole([Remainder] string input)
		{
			//Check if using the default preferences
			var guildInfo = Variables.Guilds[Context.Guild.Id];
			if (guildInfo.DefaultPrefs)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.DENY_WITHOUT_PREFERENCES));
				return;
			}

			//Get the role. No edit ability checking in this command due to how that's already been done in the modify command
			var wantedRole = await Actions.GetRole(Context, input);
			if (wantedRole == null)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("There is no role with that name on this guild."));
				return;
			}

			//Check if any groups has it
			var SAGroups = guildInfo.SelfAssignableGroups;
			if (!SAGroups.Any(x => x.Roles.Select(y => y.Role).Contains(wantedRole)))
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("There is no self assignable role by that name."));
				return;
			}

			//Get the user as an IGuildUser
			var user = Context.User as IGuildUser;
			//Check if the user wants to remove their role
			if (user.RoleIds.Contains(wantedRole.Id))
			{
				await user.RemoveRoleAsync(wantedRole);
				await Actions.MakeAndDeleteSecondaryMessage(Context, "Successfully removed the role `" + wantedRole.Name + "`.");
				return;
			}

			//Get the group that contains the role
			var SAGroup = guildInfo.SelfAssignableGroups.FirstOrDefault(x => x.Roles.Select(y => y.Role).Contains(wantedRole));
			//If a group that has roles conflict, remove all but the wanted role
			var otherRoles = new List<IRole>();
			if (SAGroup.Group != 0)
			{
				//Find the intersection of the group's roles and the user's roles
				otherRoles = SAGroup.Roles.Select(x => x.Role.Id).Intersect(user.RoleIds).Select(x => Context.Guild.GetRole(x)).ToList();
				//Check if the user already has the role they're wanting
				if (otherRoles.Contains(wantedRole))
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, "You already have that role.");
					return;
				}
			}

			await user.RemoveRolesAsync(otherRoles);
			await user.AddRoleAsync(wantedRole);

			var removedRoles = otherRoles.Any() ? String.Format(", and removed `{0}`", String.Join("`, `", otherRoles)) : "";
			await Actions.MakeAndDeleteSecondaryMessage(Context, "Successfully gave you `" + wantedRole.Name + "`" + removedRoles + ".");
		}

		[Command("selfroles")]
		[Alias("srs")]
		[Usage("<Number>")]
		[Summary("Shows the current group numbers that exists on the guild. If a number is input then it shows the roles in that group.")]
		[DefaultEnabled(false)]
		public async Task CurrentGroups([Optional, Remainder] string input)
		{
			//Check if using the default preferences
			var guildInfo = Variables.Guilds[Context.Guild.Id];
			if (guildInfo.DefaultPrefs)
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.DENY_WITHOUT_PREFERENCES));
				return;
			}

			//Get the group
			var groupNumber = -1;
			if (!String.IsNullOrWhiteSpace(input))
			{
				//Get the group number
				groupNumber = await Actions.GetIfGroupIsValid(Context, input);
				if (groupNumber == -1)
					return;
			}

			if (groupNumber == -1)
			{
				//Get the groups
				var groupNumbers = guildInfo.SelfAssignableGroups.Select(x => x.Group).Distinct().ToList();
				if (!groupNumbers.Any())
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("There are currently no self assignable role groups on this guild."));
					return;
				}

				//Send a sucess message
				await Actions.SendEmbedMessage(Context.Channel, Actions.MakeNewEmbed("Self Assignable Role Groups", String.Join(", ", groupNumbers.OrderBy(x => x).Distinct())));
			}
			else
			{
				//Get the group which has that number
				var actualGroup = guildInfo.SelfAssignableGroups.FirstOrDefault(x => x.Group == groupNumber);
				if (actualGroup == null)
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("There is no group with that number."));
					return;
				}

				//Add the group's role's names to a list
				var description = "`" + String.Join("`\n`", actualGroup.Roles.Select(x => x.Role.Name).ToList()) + "`";

				//Send the embed
				await Actions.SendEmbedMessage(Context.Channel, Actions.MakeNewEmbed(String.Format("Self Roles Group {0}", groupNumber), description));
			}
		}
	}
}
