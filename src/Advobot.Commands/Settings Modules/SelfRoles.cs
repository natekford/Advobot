using System.Linq;
using System.Threading.Tasks;
using Advobot.Classes.Attributes;
using Advobot.Classes.Attributes.ParameterPreconditions.DiscordObjectValidation.Roles;
using Advobot.Classes.Attributes.ParameterPreconditions.NumberValidation;
using Advobot.Classes.Attributes.ParameterPreconditions.SettingValidation;
using Advobot.Classes.Attributes.Preconditions.Permissions;
using Advobot.Classes.Attributes.Preconditions.QuantityLimitations;
using Advobot.Classes.Modules;
using Advobot.Classes.Settings;
using Advobot.Interfaces;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Advobot.Commands
{
	public sealed class SelfRoles : ModuleBase
	{
		[Group(nameof(ModifySelfRoles)), ModuleInitialismAlias(typeof(ModifySelfRoles))]
		[Summary("Adds a role to the self assignable list. " +
			"Roles can be grouped together which means only one role in the group can be self assigned at a time. " +
			"Create and Delete modify the entire group. " +
			"Add and Remove modify a single role in a group.")]
		[UserPermissionRequirement(GuildPermission.Administrator)]
		[EnabledByDefault(false)]
		public sealed class ModifySelfRoles : AdvobotSettingsModuleBase<IGuildSettings>
		{
			protected override IGuildSettings Settings => Context.GuildSettings;

			[SelfRoleGroupsLimit(QuantityLimitAction.Add)]
			[ImplicitCommand, ImplicitAlias]
			public Task<RuntimeResult> Create([ValidatePositiveNumber, NotAlreadySelfAssignableRoleGroup] int group)
			{
				Settings.SelfAssignableGroups.Add(new SelfAssignableRoles(group));
				return Responses.SelfRoles.CreatedGroup(group);
			}
			[SelfRoleGroupsLimit(QuantityLimitAction.Remove)]
			[ImplicitCommand, ImplicitAlias]
			public Task<RuntimeResult> Delete(SelfAssignableRoles group)
			{
				Settings.SelfAssignableGroups.RemoveAll(x => x.Group == group.Group);
				return Responses.SelfRoles.DeletedGroup(group);
			}
			[ImplicitCommand, ImplicitAlias]
			public Task<RuntimeResult> Add(SelfAssignableRoles group, [ValidateRole] params SocketRole[] roles)
			{
				group.AddRoles(roles);
				return Responses.SelfRoles.ModifiedGroup(group, roles, true);
			}
			[ImplicitCommand, ImplicitAlias]
			public Task<RuntimeResult> Remove(SelfAssignableRoles group, [ValidateRole] params SocketRole[] roles)
			{
				group.RemoveRoles(roles);
				return Responses.SelfRoles.ModifiedGroup(group, roles, false);
			}
		}

		[Group(nameof(AssignSelfRole)), ModuleInitialismAlias(typeof(AssignSelfRole))]
		[Summary("Gives or takes a role depending on if the user has it already. " +
			"Removes all other roles in the same group unless the group is `0`.")]
		[EnabledByDefault(false)]
		public sealed class AssignSelfRole : AdvobotModuleBase
		{
			//Inherit from this to make this accept a self role instead of regular role?
			//https://github.com/discord-net/Discord.Net/blob/ff0fea98a65d907fbce07856f1a9ef4aebb9108b/src/Discord.Net.Commands/Readers/RoleTypeReader.cs
			[Command]
			public async Task<RuntimeResult> Command(SocketRole role)
			{
				if (!Context.GuildSettings.SelfAssignableGroups.TryGetSingle(x => x.Roles.Contains(role.Id), out var group))
				{
					return Responses.SelfRoles.NotSelfAssignable(role);
				}
				if (Context.User.Roles.Any(x => x.Id == role.Id))
				{
					await Context.User.AddRoleAsync(role, GenerateRequestOptions("self role removal")).CAF();
					return Responses.SelfRoles.RemovedRole(role);
				}

				await Context.User.AddRoleAsync(role, GenerateRequestOptions("self role giving")).CAF();

				//Remove roles the user already has from the group if they're targetting an exclusivity group
				var otherRoles = Context.User.Roles.Where(x => group.Roles.Contains(x.Id));
				if (group.Group != 0 && otherRoles.Any())
				{
					await Context.User.RemoveRolesAsync(otherRoles, GenerateRequestOptions("self role removal")).CAF();
					return Responses.SelfRoles.AddedRoleAndRemovedOthers(role, otherRoles);
				}

				return Responses.SelfRoles.AddedRole(role);
			}
		}

		[Group(nameof(DisplaySelfRoles)), ModuleInitialismAlias(typeof(DisplaySelfRoles))]
		[Summary("Shows the current group numbers that exists on the guild. " +
			"If a number is input then it shows the roles in that group.")]
		[EnabledByDefault(false)]
		public sealed class DisplaySelfRoles : AdvobotModuleBase
		{
			[Command]
			public Task<RuntimeResult> Command()
				=> Responses.SelfRoles.DisplayGroups(Context.GuildSettings.SelfAssignableGroups);
			[Command]
			public Task<RuntimeResult> Command(SelfAssignableRoles group)
				=> Responses.SelfRoles.DisplayGroup(group);
		}
	}
}
