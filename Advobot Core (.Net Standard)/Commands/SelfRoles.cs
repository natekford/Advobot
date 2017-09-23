using Advobot.Actions;
using Advobot.Attributes;
using Advobot.Classes;
using Advobot.Enums;
using Advobot.Formatting;
using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Advobot.Commands.SelfRoles
{
	[Group(nameof(ModifySelfRoles)), Alias("msr")]
	[Usage("[Create|Delete|Add|Remove] <Group Number> <Role/...>")]
	[Summary("Adds a role to the self assignable list. Roles can be grouped together which means only one role in the group can be self assigned at a time. " + 
		"Create and Delete modify the entire group. Add and Remove modify a single role in a group.")]
	[PermissionRequirement(null, null)]
	[DefaultEnabled(false)]
	public sealed class ModifySelfRoles : MySavingModuleBase
	{
		[Command(nameof(ActionType.Create)), Alias("c")]
		public async Task CommandCreate(uint groupNum)
		{
			await CommandRunner(ActionType.Create, groupNum);
		}
		[Command(nameof(ActionType.Delete)), Alias("d")]
		public async Task CommandDelete(uint groupNum)
		{
			await CommandRunner(ActionType.Delete, groupNum);
		}
		[Command(nameof(ActionType.Add)), Alias("a")]
		public async Task CommandAdd(uint groupNum, [VerifyRole(false, RoleVerification.CanBeEdited)] params IRole[] roles)
		{
			await CommandRunner(ActionType.Add, groupNum, roles);
		}
		[Command(nameof(ActionType.Remove)), Alias("r")]
		public async Task CommandRemove(uint groupNum, [VerifyRole(false, RoleVerification.CanBeEdited)] params IRole[] roles)
		{
			await CommandRunner(ActionType.Remove, groupNum, roles);
		}

		private async Task CommandRunner(ActionType action, uint groupNum)
		{
			var selfAssignableGroups = Context.GuildSettings.SelfAssignableGroups;
			switch (action)
			{
				case ActionType.Create:
				{
					if (selfAssignableGroups.Count >= Constants.MAX_SA_GROUPS)
					{
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"You have too many groups. {Constants.MAX_SA_GROUPS} is the maximum.");
						return;
					}
					else if (selfAssignableGroups.Any(x => x.Group == groupNum))
					{
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, GeneralFormatting.ERROR("A group already exists with that position."));
						return;
					}

					selfAssignableGroups.Add(new SelfAssignableGroup((int)groupNum));
					break;
				}
				case ActionType.Delete:
				{
					if (selfAssignableGroups.Count <= 0)
					{
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"You have too many groups. {Constants.MAX_SA_GROUPS} is the maximum.");
						return;
					}
					else if (!selfAssignableGroups.Any(x => x.Group == groupNum))
					{
						await MessageActions.MakeAndDeleteSecondaryMessage(Context, GeneralFormatting.ERROR("A group needs to exist with that position before it can be deleted."));
						return;
					}

					selfAssignableGroups.RemoveAll(x => x.Group == groupNum);
					break;
				}
				default:
				{
					return;
				}
			}

			//What could go wrong by simply doing action.EnumName().ToLower() + "d"? The switch returning on default should mean nothing should end up looking dumb, like "Addd"
			await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully {action.EnumName().ToLower() + "d"} group `{groupNum}`.");
		}
		private async Task CommandRunner(ActionType action, uint groupNum, IRole[] roles)
		{
			var selfAssignableGroups = Context.GuildSettings.SelfAssignableGroups;
			if (!selfAssignableGroups.Any())
			{
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, GeneralFormatting.ERROR("Before you can edit or delete a group, you need to first create one."));
				return;
			}

			var group = selfAssignableGroups.FirstOrDefault(x => x.Group == groupNum);
			if (group == null)
			{
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, GeneralFormatting.ERROR("A group needs to exist with that position before you can modify it."));
				return;
			}

			var rolesAdded = new List<IRole>();
			var rolesNotAdded = new List<IRole>();
			var alreadyUsedRoles = selfAssignableGroups.SelectMany(x => x.Roles.Select(y => y.RoleId));
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

			var addedStr = rolesAdded.Any() ? $"Successfully added the following role(s): `{String.Join("`, `", rolesAdded.Select(x => x.FormatRole()))}`." : null;
			var notAddedStr = rolesNotAdded.Any() ? $"Failed to add the following role(s): `{String.Join("`, `", rolesNotAdded.Select(x => x.FormatRole()))}`." : null;
			await MessageActions.MakeAndDeleteSecondaryMessage(Context, GeneralFormatting.JoinNonNullStrings(" ", addedStr, notAddedStr));
		}
	}

	[Group(nameof(AssignSelfRole)), Alias("asr")]
	[Usage("[Role]")]
	[Summary("Gives or takes a role depending on if the user has it already. Removes all other roles in the same group unless the group is `0`.")]
	[DefaultEnabled(false)]
	public sealed class AssignSelfRole : MyModuleBase
	{
		[Command]
		public async Task Command(IRole role)
		{
			var group = Context.GuildSettings.SelfAssignableGroups.FirstOrDefault(x => x.Roles.Select(y => y.RoleId).Contains(role.Id));
			if (group == null)
			{
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, GeneralFormatting.ERROR("There is no self assignable role by that name."));
				return;
			}

			var user = Context.User as IGuildUser;
			if (user.RoleIds.Contains(role.Id))
			{
				await RoleActions.TakeRoles(user, new[] { role }, GeneralFormatting.FormatBotReason("self role removal"));
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully removed `{role.FormatRole()}`.");
				return;
			}

			//Remove roles the user already has from the group if they're targetting an exclusivity group
			var removedRoles = "";
			if (group.Group != 0)
			{
				var otherRoles = group.Roles.Where(x => user.RoleIds.Contains(x?.RoleId ?? 0)).Select(x => x.Role);
				await RoleActions.TakeRoles(user, otherRoles, GeneralFormatting.FormatBotReason("self role removal"));
				if (otherRoles.Any())
				{
					removedRoles = $", and removed `{String.Join("`, `", otherRoles.Select(x => x.FormatRole()))}`";
				}
			}

			await RoleActions.GiveRoles(user, new[] { role }, GeneralFormatting.FormatBotReason("self role giving"));
			await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully gave `{role.Name}`{removedRoles}.");
		}
	}

	[Group(nameof(DisplaySelfRoles)), Alias("dsr")]
	[Usage("<Number>")]
	[Summary("Shows the current group numbers that exists on the guild. If a number is input then it shows the roles in that group.")]
	[DefaultEnabled(false)]
	public sealed class DisplaySelfRoles : MyModuleBase
	{
		[Command]
		public async Task Command()
		{
			var groupNumbers = Context.GuildSettings.SelfAssignableGroups.Select(x => x.Group).OrderBy(x => x);
			if (!groupNumbers.Any())
			{
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, GeneralFormatting.ERROR("There are currently no self assignable role groups on this guild."));
				return;
			}

			await MessageActions.SendEmbedMessage(Context.Channel, EmbedActions.MakeNewEmbed("Self Assignable Role Groups", $"`{String.Join("`, `", groupNumbers)}`"));
		}
		[Command]
		public async Task Command(uint groupNum)
		{
			var group = Context.GuildSettings.SelfAssignableGroups.FirstOrDefault(x => x.Group == groupNum);
			if (group == null)
			{
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, GeneralFormatting.ERROR("There is no group with that number."));
				return;
			}

			var desc = group.Roles.Any() ? $"`{String.Join("`, `", group.Roles.Select(x => x.Role?.Name ?? "null"))}`" : "`Nothing`";
			await MessageActions.SendEmbedMessage(Context.Channel, EmbedActions.MakeNewEmbed($"Self Roles Group {groupNum}", desc));
		}
	}
}
