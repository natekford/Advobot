using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
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
			var returnedArgs = Actions.GetArgs(Context, input, new ArgNumbers(2, 3), new[] { "group" });
			if (returnedArgs.Reason != ArgFailureReason.Not_Failure)
			{
				await Actions.HandleArgsGettingErrors(Context, returnedArgs);
				return;
			}
			var actionStr = returnedArgs.Arguments[0];
			var roleStr = returnedArgs.Arguments[1];
			var groupStr = returnedArgs.GetSpecifiedArg("group");

			//Check which action it is
			var returnedActionType = Actions.GetActionType(actionStr, new[] { ActionType.Create, ActionType.Add, ActionType.Remove });
			if (returnedActionType.Reason != TypeFailureReason.Not_Failure)
			{
				await Actions.HandleTypeGettingErrors(Context, returnedActionType);
				return;
			}
			var action = returnedActionType.Type;

			//Check if the guild has too many or no self assignable role lists yet
			if (action != ActionType.Create)
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

			var groupNumber = await Actions.GetIfGroupIsValid(Context, groupStr);
			if (groupNumber == -1)
				return;

			//Necessary to know what group to target
			var successStr = new List<string>();
			var failureStr = new List<string>();
			var deletedStr = new List<string>();
			switch (action)
			{
				case ActionType.Create:
				case ActionType.Add:
				case ActionType.Remove:
				{
					//If create, do not allow a new one made with the same number
					var guildGroups = guildInfo.SelfAssignableGroups;
					if (action == ActionType.Create)
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
					var evaluatedRoles = Actions.GetValidEditRoles(Context, roleStr.Split('/').ToList());
					if (!evaluatedRoles.HasValue)
					{
						await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR(Constants.ROLE_ERROR));
						return;
					}
					var success = evaluatedRoles.Value.Success;
					successStr = success.Select(x => x.FormatRole()).ToList();
					var failure = evaluatedRoles.Value.Failure;

					//Add all the roles to a list of self assignable roles
					var SARoles = success.Select(x => new SelfAssignableRole(Context.Guild.Id, x.Id)).ToList();
					if (action != ActionType.Remove)
					{
						var SAGroups = guildInfo.SelfAssignableGroups;
						var ulongs = SAGroups.SelectMany(x => x.Roles).Select(x => x.RoleID);
						var removed = SARoles.Where(x => ulongs.Contains(x.Role.Id));
						failure.AddRange(removed.Select(x => x.Role.ToString()));
						success.RemoveAll(x => ulongs.Contains(x.Id));
						SARoles.RemoveAll(x => ulongs.Contains(x.RoleID));

						if (action == ActionType.Create)
						{
							//Make a new group and add that to the global list
							guildInfo.SelfAssignableGroups.Add(new SelfAssignableGroup(SARoles, groupNumber));
						}
						else
						{
							//Add the roles to the group
							SAGroups.FirstOrDefault(x => x.Group == groupNumber).AddRoles(SARoles);
						}
						failureStr = failure;
					}
					else
					{
						//Find the one with the correct group number and remove all roles which have an ID on the ulong list
						guildInfo.SelfAssignableGroups.FirstOrDefault(x => x.Group == groupNumber).RemoveRoles(SARoles.Select(x => x.Role.Id).ToList());
					}
					break;
				}
				case ActionType.Delete:
				{
					//Get the groups
					var guildGroups = guildInfo.SelfAssignableGroups;
					//Check if any groups have that position
					if (!guildGroups.Any(x => x.Group == groupNumber))
					{
					    await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("A group needs to exist with that position before it can be deleted."));
						return;
					}

					var group = guildGroups.FirstOrDefault(x => x.Group == groupNumber);
					deletedStr = group.Roles.Select(x => x.Role.Name).ToList();
					guildGroups.Remove(group);
					break;
				}
			}

			//Make the success and failure strings
			var sString = "";
			var fString = "";
			var sBool = successStr.Any();
			var fBool = failureStr.Any();
			switch (action)
			{
				case ActionType.Create:
				{
					sString = sBool ? String.Format("Successfully created the group `{0}` with the following roles: `{1}`", groupNumber.ToString("00"), String.Join("`, `", successStr)) : "";
					fString = fBool ? String.Format("{0}ailed to add the following roles to `{1}`: `{2}`", sBool ? "f" : "F", groupNumber.ToString("00"), String.Join("`, `", failureStr)) : "";
					break;
				}
				case ActionType.Add:
				{
					sString = sBool ? String.Format("Successfully added the following roles to `{0}`: `{1}`", groupNumber.ToString("00"), String.Join("`, `", successStr)) : "";
					fString = fBool ? String.Format("{0}ailed to add the following roles to `{1}`: `{2}`", sBool ? "f" : "F", groupNumber.ToString("00"), String.Join("`, `", failureStr)) : "";
					break;
				}
				case ActionType.Remove:
				{
					sString = sBool ? String.Format("Successfully removed the following roles from `{0}`: `{1}`", groupNumber.ToString("00"), String.Join("`, `", successStr)) : "";
					fString = fBool ? String.Format("{0}ailed to remove the following roles from `{1}`: `{2}`", sBool ? "f" : "F", groupNumber.ToString("00"), String.Join("`, `", failureStr)) : "";
					break;
				}
				case ActionType.Delete:
				{
					sString = String.Format("Successfully deleted the group `{0}` which held the following roles: `{1}`", groupNumber.ToString("00"), String.Join("`, `", deletedStr));
					break;
				}
			}
			var responseMessage = (sBool && fBool) ? (sString + ", and " + fString) : (sString + fString);

			//Save everything and send a success message
			Actions.SaveGuildInfo(guildInfo);
			await Actions.MakeAndDeleteSecondaryMessage(Context, responseMessage + ".");
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
			var returnedRole = Actions.GetRole(Context, new[] { RoleCheck.None }, true, input);
			if (returnedRole.Reason != FailureReason.Not_Failure)
			{
				await Actions.HandleObjectGettingErrors(Context, returnedRole);
				return;
			}
			var role = returnedRole.Object;

			//Check if any groups has it
			var SAGroups = guildInfo.SelfAssignableGroups;
			if (!SAGroups.Any(x => x.Roles.Select(y => y.Role).Contains(role)))
			{
				await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("There is no self assignable role by that name."));
				return;
			}

			var user = Context.User as IGuildUser;
			if (user.RoleIds.Contains(role.Id))
			{
				await user.RemoveRoleAsync(role);
				await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully removed the role `{0}`.", role.FormatRole()));
				return;
			}

			//If a group that has roles conflict, remove all but the wanted role
			var SAGroup = guildInfo.SelfAssignableGroups.FirstOrDefault(x => x.Roles.Select(y => y.Role).Contains(role));
			var otherRoles = new List<IRole>();
			if (SAGroup.Group != 0)
			{
				//Find the intersection of the group's roles and the user's roles
				otherRoles = SAGroup.Roles.Select(x => x.Role.Id).Intersect(user.RoleIds).Select(x => Context.Guild.GetRole(x)).ToList();
				if (otherRoles.Contains(role))
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, "You already have that role.");
					return;
				}
			}

			await Actions.TakeRoles(user, otherRoles);
			await Actions.GiveRole(user, role);

			var removedRoles = "";
			if (otherRoles.Any())
			{
				removedRoles = String.Format(", and removed `{0}`", String.Join("`, `", otherRoles));
			}
			await Actions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully gave you `{0}`{1}.", role.Name, removedRoles));
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
				groupNumber = await Actions.GetIfGroupIsValid(Context, input);
				if (groupNumber == -1)
					return;
			}

			if (groupNumber == -1)
			{
				var groupNumbers = guildInfo.SelfAssignableGroups.Select(x => x.Group).Distinct().ToList();
				if (!groupNumbers.Any())
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("There are currently no self assignable role groups on this guild."));
					return;
				}
				await Actions.SendEmbedMessage(Context.Channel, Actions.MakeNewEmbed("Self Assignable Role Groups", String.Join(", ", groupNumbers.OrderBy(x => x).Distinct())));
			}
			else
			{
				var actualGroup = guildInfo.SelfAssignableGroups.FirstOrDefault(x => x.Group == groupNumber);
				if (actualGroup == null)
				{
					await Actions.MakeAndDeleteSecondaryMessage(Context, Actions.ERROR("There is no group with that number."));
					return;
				}
				var description = String.Format("`{0}`", String.Join("`\n`", actualGroup.Roles.Select(x => x.Role.Name)));
				await Actions.SendEmbedMessage(Context.Channel, Actions.MakeNewEmbed(String.Format("Self Roles Group {0}", groupNumber), description));
			}
		}
	}
}
