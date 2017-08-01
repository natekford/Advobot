using Advobot.Actions;
using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Advobot.Attributes;
using Advobot.Enums;
using Advobot.NonSavedClasses;
using Advobot.SavedClasses;

namespace Advobot
{
	namespace SelfRoles
	{
		[Group("modifyselfroles"), Alias("msr")]
		[Usage("[Create|Delete|Add|Remove] <Group Number> <Role/...>")]
		[Summary("Adds a role to the self assignable list. Roles can be grouped together which means only one role in the group can be self assigned at a time. " + 
			"Create and Delete modify the entire group. Add and Remove modify a single role in a group.")]
		[PermissionRequirement(null, null)]
		[DefaultEnabled(false)]
		public sealed class ModifySelfRoles : MyModuleBase
		{
			[Command("create")]
			public async Task CommandCreate(uint groupNum)
			{
				await CommandRunner(ActionType.Create, groupNum);
			}
			[Command("delete")]
			public async Task CommandDelete(uint groupNum)
			{
				await CommandRunner(ActionType.Delete, groupNum);
			}
			[Command("add")]
			public async Task CommandAdd(uint groupNum, [VerifyObject(false, ObjectVerification.CanBeEdited)] params IRole[] roles)
			{
				await CommandRunner(ActionType.Add, groupNum, roles);
			}
			[Command("remove")]
			public async Task CommandRemove(uint groupNum, [VerifyObject(false, ObjectVerification.CanBeEdited)] params IRole[] roles)
			{
				await CommandRunner(ActionType.Remove, groupNum, roles);
			}

			private async Task CommandRunner(ActionType action, uint groupNum)
			{

			}
			private async Task CommandRunner(ActionType action, uint groupNum, IRole[] roles)
			{
				var selfAssignableGroups = Context.GuildSettings.SelfAssignableGroups;
				if (!selfAssignableGroups.Any())
				{
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, FormattingActions.ERROR("Before you can edit or delete a group, you need to first create one."));
					return;
				}

				var group = selfAssignableGroups.FirstOrDefault(x => x.Group == groupNum);
				if (group == null)
				{
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, FormattingActions.ERROR("A group needs to exist with that position before you can modify it."));
					return;
				}

				var rolesAdded = new List<IRole>();
				var rolesNotAdded = new List<IRole>();
				var alreadyUsedRoles = selfAssignableGroups.SelectMany(x => x.Roles).Select(x => x.RoleId);

				var addedStr = rolesAdded.Any() ? String.Format("Successfully added the following role(s): `{0}`.", String.Join("`, `", rolesAdded.Select(x => x.FormatRole()))) : null;
				var notAddedStr = rolesNotAdded.Any() ? String.Format("Failed to add the following role(s): `{0}`.", String.Join("`, `", rolesNotAdded.Select(x => x.FormatRole()))) : null;

				switch (action)
				{
					case ActionType.Add:
					{
						foreach (var role in roles)
						{
							if (!alreadyUsedRoles.Contains(role.Id))
							{
								rolesAdded.Add(role);
							}
							else
							{
								rolesNotAdded.Add(role);
							}
						}

						group.AddRoles(rolesAdded.Select(x => new SelfAssignableRole(x)));
						break;
					}
					case ActionType.Remove:
					{
						foreach (var role in roles)
						{
							if (alreadyUsedRoles.Contains(role.Id))
							{
								rolesAdded.Add(role);
							}
							else
							{
								rolesNotAdded.Add(role);
							}
						}

						group.RemoveRoles(rolesAdded.Select(x => x.Id));
						break;
					}
					default:
					{
						return;
					}
				}

				await MessageActions.SendChannelMessage(Context, FormattingActions.JoinNonNullStrings(" ", addedStr, notAddedStr));
			}
		}
	}
	/*
	[Name("SelfRoles")]
	public class Advobot_Commands_Self_Roles : ModuleBase
	{

		public async Task ModifySelfAssignableRoles([Remainder] string input)
		{
			var guildInfo = await Actions.CreateOrGetGuildInfo(Context.Guild);

			//Check if it's extra help wanted
			if (Actions.CaseInsEquals(input, "help"))
			{

			}

			//Break the input into pieces
			var returnedArgs = Actions.GetArgs(Context, input, new ArgNumbers(2, 3), new[] { "group" });
			if (returnedArgs.Reason != FailureReason.NotFailure)
			{
				await Actions.HandleArgsGettingErrors(Context, returnedArgs);
				return;
			}
			var actionStr = returnedArgs.Arguments[0];
			var roleStr = returnedArgs.Arguments[1];
			var groupStr = returnedArgs.GetSpecifiedArg("group");

			//Check which action it is
			var returnedType = Actions.GetEnum(actionStr, new[] { ActionType.Create, ActionType.Delete, ActionType.Add, ActionType.Remove });
			if (returnedType.Reason != FailureReason.NotFailure)
			{
				await Actions.HandleObjectGettingErrors(Context, returnedType);
				return;
			}
			var action = returnedType.Object;

			//Check if the guild has too many or no self assignable role lists yet
			if (action != ActionType.Create)
			{
				if (!((List<SelfAssignableGroup>)guildInfo.GetSetting(SettingOnGuild.SelfAssignableGroups)).Any())
				{
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("Before you can edit or delete a group, you need to first create one."));
					return;
				}
			}
			else
			{
				if (((List<SelfAssignableGroup>)guildInfo.GetSetting(SettingOnGuild.SelfAssignableGroups)).Count == Constants.MAX_SA_GROUPS)
				{
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, "You have too many groups. " + Constants.MAX_SA_GROUPS + " is the maximum.");
					return;
				}
			}

			if (!int.TryParse(groupStr, out int groupNumber) || groupNumber < 0)
			{
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("Invalid group number supplied."));
				return;
			}

			//Necessary to know what group to target
			var successStr = new List<string>();
			var failureStr = new List<string>();
			var guildGroups = ((List<SelfAssignableGroup>)guildInfo.GetSetting(SettingOnGuild.SelfAssignableGroups));
			switch (action)
			{
				case ActionType.Create:
				{
					if (guildGroups.Any(x => x.Group == groupNumber))
					{
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("A group already exists with that position."));
						return;
					}

					((List<SelfAssignableGroup>)guildInfo.GetSetting(SettingOnGuild.SelfAssignableGroups)).Add(new SelfAssignableGroup(groupNumber));
					break;
				}
				case ActionType.Delete:
				{
					if (!guildGroups.Any(x => x.Group == groupNumber))
					{
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("A group needs to exist with that position before it can be deleted."));
						return;
					}

					guildGroups.RemoveAll(x => x.Group == groupNumber);
					break;
				}
				case ActionType.Add:
				case ActionType.Remove:
				{
					if (!guildGroups.Any(x => x.Group == groupNumber))
					{
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("A group needs to exist with that position before you can modify it."));
						return;
					}

					if (String.IsNullOrWhiteSpace(roleStr))
					{
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR(""));
						return;
					}

					//Check validity of roles
					var evaluatedRoles = Actions.GetValidEditRoles(Context, roleStr.Split('/'));
					if (!evaluatedRoles.HasValue)
					{
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR(Constants.ROLE_ERROR));
						return;
					}
					var success = evaluatedRoles.Value.Success;
					var failure = evaluatedRoles.Value.Failure;

					var SARoles = success.Select(x => new SelfAssignableRole(Context.Guild.Id, x.Id)).ToList();
					var group = guildGroups.FirstOrDefault(x => x.Group == groupNumber);
					if (action == ActionType.Add)
					{
						var alreadyUsedRoles = guildGroups.SelectMany(x => x.Roles).Select(x => x.RoleID);
						//Don't add roles that are already in a group
						SARoles.RemoveAll(x =>
						{
							if (alreadyUsedRoles.Contains(x.RoleID))
							{
								failure.Add(x.Role.FormatRole());
								return true;
							}
							return false;
						});

						group.AddRoles(SARoles);
					}
					else if (action == ActionType.Remove)
					{
						group.RemoveRoles(SARoles.Select(x => x.Role.Id));
					}

					successStr.AddRange(SARoles.Select(x => x.Role.FormatRole()));
					failureStr.AddRange(failure);
					break;
				}
			}

			var sString = "";
			var fString = "";
			var sBool = successStr.Any();
			var fBool = failureStr.Any();
			switch (action)
			{
				case ActionType.Create:
				{
					sString = String.Format("Successfully created the group `{0}`", groupNumber.ToString("00"));
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
					sString = String.Format("Successfully deleted the group `{0}`", groupNumber.ToString("00"));
					break;
				}
			}
			var responseMessage = (sBool && fBool) ? (sString + ", and " + fString) : (sString + fString);

			guildInfo.SaveInfo();
			await MessageActions.MakeAndDeleteSecondaryMessage(Context, responseMessage + ".");
		}

		[Command("assignselfrole")]
		[Alias("asr")]
		[Usage("[Role]")]
		[Summary("Gives or takes a role depending on if the user has it already. Removes all other roles in the same group unless the group is `0`.")]
		[DefaultEnabled(false)]
		public async Task AssignSelfRole([Remainder] string input)
		{
			var guildInfo = await Actions.CreateOrGetGuildInfo(Context.Guild);

			//Get the role. No edit ability checking in this command due to how that's already been done in the modify command
			var role = Actions.GetRole(Context, new[] { ObjectVerification.None }, true, input).Object;
			if (role == null)
			{
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("There is no self assignable role by that name."));
				return;
			}

			//Check if any groups has it
			var SARole = ((List<SelfAssignableGroup>)guildInfo.GetSetting(SettingOnGuild.SelfAssignableGroups)).SelectMany(x => x.Roles).FirstOrDefault(x => x.RoleID == role.Id);
			if (SARole == null)
			{
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("There is no self assignable role by that name."));
				return;
			}

			var user = Context.User as IGuildUser;
			if (user.RoleIds.Contains(role.Id))
			{
				await user.RemoveRoleAsync(role);
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully removed the role `{0}`.", role.FormatRole()));
				return;
			}

			//If a group that has roles conflict, remove all but the wanted role
			var SAGroup = ((List<SelfAssignableGroup>)guildInfo.GetSetting(SettingOnGuild.SelfAssignableGroups)).FirstOrDefault(x => x?.Group == SARole?.Group);
			var removedRoles = "";
			if (SAGroup?.Group != 0)
			{
				var otherRoles = SAGroup.Roles.Where(x =>
				{
					return true
					&& x != null
					&& user.RoleIds.Contains(x.RoleID);
				}).Select(x => x.Role);

				await Actions.TakeRoles(user, otherRoles);

				if (otherRoles.Any())
				{
					removedRoles = String.Format(", and removed `{0}`", String.Join("`, `", otherRoles.Select(x => x.FormatRole())));
				}
			}

			await Actions.GiveRole(user, role);
			await MessageActions.MakeAndDeleteSecondaryMessage(Context, String.Format("Successfully gave you `{0}`{1}.", role.Name, removedRoles));
		}

		[Command("displayselfroles")]
		[Alias("dsr")]
		[Usage("<Number>")]
		[Summary("Shows the current group numbers that exists on the guild. If a number is input then it shows the roles in that group.")]
		[DefaultEnabled(false)]
		public async Task CurrentGroups([Optional, Remainder] string input)
		{
			var guildInfo = await Actions.CreateOrGetGuildInfo(Context.Guild);

			var groupNumber = -1;
			if (!String.IsNullOrWhiteSpace(input))
			{
				if (!int.TryParse(input, out groupNumber) || groupNumber < 0)
				{
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("Invalid group number supplied."));
					return;
				}
			}

			var SAGroups = (List<SelfAssignableGroup>)guildInfo.GetSetting(SettingOnGuild.SelfAssignableGroups);
			if (groupNumber == -1)
			{
				var groupNumbers = SAGroups.Select(x => x.Group).Distinct().OrderBy(x => x).ToList();
				if (!groupNumbers.Any())
				{
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("There are currently no self assignable role groups on this guild."));
					return;
				}
				await MessageActions.SendEmbedMessage(Context.Channel, Messages.MakeNewEmbed("Self Assignable Role Groups", String.Format("`{0}`", String.Join("`, `", groupNumbers))));
			}
			else
			{
				var group = SAGroups.FirstOrDefault(x => x.Group == groupNumber);
				if (group == null)
				{
					await MessageActions.MakeAndDeleteSecondaryMessage(Context, Formatting.ERROR("There is no group with that number."));
					return;
				}

				var embed = Messages.MakeNewEmbed(String.Format("Self Roles Group {0}", groupNumber));
				if (group.Roles.Any())
				{
					embed.Description = String.Format("`{0}`", String.Join("`, `", group.Roles.Select(x => x.Role?.Name ?? "null")));
				}
				else
				{
					embed.Description = "`NOTHING`";
				}
				await MessageActions.SendEmbedMessage(Context.Channel, embed);
			}
		}
	}
	*/
}
