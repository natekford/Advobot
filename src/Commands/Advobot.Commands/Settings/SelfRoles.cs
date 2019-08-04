using System.Linq;
using System.Threading.Tasks;
using Advobot.Attributes;
using Advobot.Attributes.ParameterPreconditions.DiscordObjectValidation.Roles;
using Advobot.Attributes.ParameterPreconditions.Numbers;
using Advobot.Attributes.Preconditions.Permissions;
using Advobot.Attributes.Preconditions.QuantityLimitations;
using Advobot.Classes;
using Advobot.Commands.Localization;
using Advobot.Commands.Resources;
using Advobot.Modules;
using Advobot.Services.GuildSettings;
using Advobot.Services.GuildSettings.Settings;
using AdvorangesUtils;
using Discord;
using Discord.Commands;

namespace Advobot.Commands.Settings
{
	public sealed class SelfRoles : ModuleBase
	{
		[Group(nameof(ModifySelfRoles)), ModuleInitialismAlias(typeof(ModifySelfRoles))]
		[LocalizedSummary(nameof(Summaries.ModifySelfRoles))]
		[GuildPermissionRequirement(GuildPermission.Administrator)]
		[EnabledByDefault(false)]
		public sealed class ModifySelfRoles : SettingsModule<IGuildSettings>
		{
			protected override IGuildSettings Settings => Context.Settings;

			[SelfRoleGroupsLimit(QuantityLimitAction.Add)]
			[ImplicitCommand, ImplicitAlias]
			public Task<RuntimeResult> Create(
				[SelfRoleGroup] int group)
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
			public Task<RuntimeResult> Add(
				SelfAssignableRoles group,
				[Role] params IRole[] roles)
			{
				group.AddRoles(roles);
				return Responses.SelfRoles.ModifiedGroup(group, roles, true);
			}
			[ImplicitCommand, ImplicitAlias]
			public Task<RuntimeResult> Remove(
				SelfAssignableRoles group,
				[Role] params IRole[] roles)
			{
				group.RemoveRoles(roles);
				return Responses.SelfRoles.ModifiedGroup(group, roles, false);
			}
		}

		[Group(nameof(AssignSelfRole)), ModuleInitialismAlias(typeof(AssignSelfRole))]
		[LocalizedSummary(nameof(Summaries.AssignSelfRole))]
		[EnabledByDefault(false)]
		public sealed class AssignSelfRole : AdvobotModuleBase
		{
			[Command]
			public async Task<RuntimeResult> Command(SelfAssignableRole role)
			{
				if (Context.User.Roles.Any(x => x.Id == role.Role.Id))
				{
					await Context.User.AddRoleAsync(role.Role, GenerateRequestOptions("self role removal")).CAF();
					return Responses.SelfRoles.RemovedRole(role.Role);
				}

				await Context.User.AddRoleAsync(role.Role, GenerateRequestOptions("self role giving")).CAF();

				//Remove roles the user already has from the group if they're targetting an exclusivity group
				var otherRoles = Context.User.Roles.Where(x => role.Group.Roles.Contains(x.Id));
				if (role.Group.Group != 0 && otherRoles.Any())
				{
					await Context.User.RemoveRolesAsync(otherRoles, GenerateRequestOptions("self role removal")).CAF();
					return Responses.SelfRoles.AddedRoleAndRemovedOthers(role.Role, otherRoles);
				}

				return Responses.SelfRoles.AddedRole(role.Role);
			}
		}

		[Group(nameof(DisplaySelfRoles)), ModuleInitialismAlias(typeof(DisplaySelfRoles))]
		[LocalizedSummary(nameof(Summaries.DisplaySelfRoles))]
		[EnabledByDefault(false)]
		public sealed class DisplaySelfRoles : AdvobotModuleBase
		{
			[Command]
			public Task<RuntimeResult> Command()
				=> Responses.SelfRoles.DisplayGroups(Context.Settings.SelfAssignableGroups);
			[Command]
			public Task<RuntimeResult> Command(SelfAssignableRoles group)
				=> Responses.SelfRoles.DisplayGroup(Context.Guild, group);
		}
	}
}
