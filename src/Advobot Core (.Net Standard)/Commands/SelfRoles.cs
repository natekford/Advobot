using Advobot.Actions;
using Advobot.Actions.Formatting;
using Advobot.Classes;
using Advobot.Classes.Attributes;
using Advobot.Enums;
using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Advobot.Commands.SelfRoles
{
	[Group(nameof(ModifySelfRoles)), TopLevelShortAlias(typeof(ModifySelfRoles))]
	[Summary("Adds a role to the self assignable list. Roles can be grouped together which means only one role in the group can be self assigned at a time. " + 
		"Create and Delete modify the entire group. Add and Remove modify a single role in a group.")]
	[PermissionRequirement(null, null)]
	[DefaultEnabled(false)]
	public sealed class ModifySelfRoles : SavingModuleBase
	{
		[Command(nameof(Create)), ShortAlias(nameof(Create))]
		public async Task Create(uint groupNumber)
		{
			await CommandRunner(groupNumber);
		}
		[Command(nameof(Delete)), ShortAlias(nameof(Delete))]
		public async Task Delete(uint groupNumber)
		{
			await CommandRunner(groupNumber);
		}
		[Command(nameof(Add)), ShortAlias(nameof(Add))]
		public async Task Add(uint groupNumber, [VerifyObject(false, ObjectVerification.CanBeEdited)] params IRole[] roles)
		{
			await CommandRunner(groupNumber, roles);
		}
		[Command(nameof(Remove)), ShortAlias(nameof(Remove))]
		public async Task Remove(uint groupNumber, [VerifyObject(false, ObjectVerification.CanBeEdited)] params IRole[] roles)
		{
			await CommandRunner(groupNumber, roles);
		}

		private async Task CommandRunner(uint groupNum, [CallerMemberName] string caller = "")
		{
			var selfAssignableGroups = Context.GuildSettings.SelfAssignableGroups;
			switch (caller)
			{
				case nameof(Create):
				{
					if (selfAssignableGroups.Count >= Constants.MAX_SA_GROUPS)
					{
						await MessageActions.SendErrorMessage(Context, new ErrorReason($"You have too many groups. {Constants.MAX_SA_GROUPS} is the maximum."));
						return;
					}
					else if (selfAssignableGroups.Any(x => x.Group == groupNum))
					{
						await MessageActions.SendErrorMessage(Context, new ErrorReason("A group already exists with that position."));
						return;
					}

					selfAssignableGroups.Add(new SelfAssignableGroup((int)groupNum));
					break;
				}
				case nameof(Delete):
				{
					if (selfAssignableGroups.Count <= 0)
					{
						await MessageActions.SendErrorMessage(Context, new ErrorReason("There are no groups to delete."));
						return;
					}
					else if (!selfAssignableGroups.Any(x => x.Group == groupNum))
					{
						await MessageActions.SendErrorMessage(Context, new ErrorReason("A group needs to exist with that position before it can be deleted."));
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

			var actionName = caller.ToLower() + "d";
			await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully {actionName} group `{groupNum}`.");
		}
		private async Task CommandRunner(uint groupNum, IRole[] roles, [CallerMemberName] string caller = "")
		{
			var selfAssignableGroups = Context.GuildSettings.SelfAssignableGroups;
			if (!selfAssignableGroups.Any())
			{
				await MessageActions.SendErrorMessage(Context, new ErrorReason("There are no groups to edit."));
				return;
			}

			var group = selfAssignableGroups.FirstOrDefault(x => x.Group == groupNum);
			if (group == null)
			{
				await MessageActions.SendErrorMessage(Context, new ErrorReason("A group needs to exist with that position before you can modify it."));
				return;
			}

			var rolesModified = new List<IRole>();
			var rolesNotModified = new List<IRole>();
			var alreadyUsedRoles = selfAssignableGroups.SelectMany(x => x.Roles.Select(y => y.RoleId));
			switch (caller)
			{
				case nameof(Add):
				{
					foreach (var role in roles)
					{
						if (!alreadyUsedRoles.Contains(role.Id))
						{
							rolesModified.Add(role);
						}
						else
						{
							rolesNotModified.Add(role);
						}
					}
					group.AddRoles(rolesModified.Select(x => new SelfAssignableRole(x)));
					break;
				}
				case nameof(Remove):
				{
					foreach (var role in roles)
					{
						if (alreadyUsedRoles.Contains(role.Id))
						{
							rolesModified.Add(role);
						}
						else
						{
							rolesNotModified.Add(role);
						}
					}
					group.RemoveRoles(rolesModified.Select(x => x.Id));
					break;
				}
				default:
				{
					return;
				}
			}

			var actionName = caller.ToLower() + "d";
			var modified = rolesModified.Any() ? $"Successfully {actionName} the following role(s): `{String.Join("`, `", rolesModified.Select(x => x.FormatRole()))}`." : null;
			var notModified = rolesNotModified.Any() ? $"Failed to {actionName} the following role(s): `{String.Join("`, `", rolesNotModified.Select(x => x.FormatRole()))}`." : null;
			await MessageActions.MakeAndDeleteSecondaryMessage(Context, GeneralFormatting.JoinNonNullStrings(" ", modified, notModified));
		}
	}

	[Group(nameof(AssignSelfRole)), TopLevelShortAlias(typeof(AssignSelfRole))]
	[Summary("Gives or takes a role depending on if the user has it already. Removes all other roles in the same group unless the group is `0`.")]
	[DefaultEnabled(false)]
	public sealed class AssignSelfRole : AdvobotModuleBase
	{
		[Command]
		public async Task Command(IRole role)
		{
			var group = Context.GuildSettings.SelfAssignableGroups.FirstOrDefault(x => x.Roles.Select(y => y.RoleId).Contains(role.Id));
			if (group == null)
			{
				await MessageActions.SendErrorMessage(Context, new ErrorReason("There is no self assignable role by that name."));
				return;
			}

			var user = Context.User as IGuildUser;
			if (user.RoleIds.Contains(role.Id))
			{
				await RoleActions.TakeRoles(user, new[] { role }, new AutomaticModerationReason("self role removal"));
				await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully removed `{role.FormatRole()}`.");
				return;
			}

			//Remove roles the user already has from the group if they're targetting an exclusivity group
			var removedRoles = "";
			if (group.Group != 0)
			{
				var otherRoles = group.Roles.Where(x => user.RoleIds.Contains(x?.RoleId ?? 0)).Select(x => x.Role);
				if (otherRoles.Any())
				{
					await RoleActions.TakeRoles(user, otherRoles, new AutomaticModerationReason("self role removal"));
					removedRoles = $", and removed `{String.Join("`, `", otherRoles.Select(x => x.FormatRole()))}`";
				}
			}

			await RoleActions.GiveRoles(user, new[] { role }, new AutomaticModerationReason("self role giving"));
			await MessageActions.MakeAndDeleteSecondaryMessage(Context, $"Successfully gave `{role.Name}`{removedRoles}.");
		}
	}

	[Group(nameof(DisplaySelfRoles)), TopLevelShortAlias(typeof(DisplaySelfRoles))]
	[Summary("Shows the current group numbers that exists on the guild. If a number is input then it shows the roles in that group.")]
	[DefaultEnabled(false)]
	public sealed class DisplaySelfRoles : AdvobotModuleBase
	{
		[Command]
		public async Task Command()
		{
			var groupNumbers = Context.GuildSettings.SelfAssignableGroups.Select(x => x.Group).OrderBy(x => x);
			if (!groupNumbers.Any())
			{
				await MessageActions.SendErrorMessage(Context, new ErrorReason("There are currently no self assignable role groups on this guild."));
				return;
			}

			await MessageActions.SendEmbedMessage(Context.Channel, new MyEmbed("Self Assignable Role Groups", $"`{String.Join("`, `", groupNumbers)}`"));
		}
		[Command]
		public async Task Command(uint groupNum)
		{
			var group = Context.GuildSettings.SelfAssignableGroups.FirstOrDefault(x => x.Group == groupNum);
			if (group == null)
			{
				await MessageActions.SendErrorMessage(Context, new ErrorReason("There is no group with that number."));
				return;
			}

			var desc = group.Roles.Any() ? $"`{String.Join("`, `", group.Roles.Select(x => x.Role?.Name ?? "null"))}`" : "`Nothing`";
			await MessageActions.SendEmbedMessage(Context.Channel, new MyEmbed($"Self Roles Group {groupNum}", desc));
		}
	}
}
