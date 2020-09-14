using System.Linq;
using System.Threading.Tasks;

using Advobot.Attributes;
using Advobot.Attributes.ParameterPreconditions.DiscordObjectValidation.Roles;
using Advobot.Attributes.ParameterPreconditions.Numbers;
using Advobot.Attributes.Preconditions.Permissions;
using Advobot.Classes;
using Advobot.Localization;
using Advobot.Modules;
using Advobot.Resources;
using Advobot.Services.GuildSettings;
using Advobot.Services.GuildSettings.Settings;

using AdvorangesUtils;

using Discord;
using Discord.Commands;

namespace Advobot.Settings.Commands
{
	[Category(nameof(SelfRoles))]
	public sealed class SelfRoles : ModuleBase
	{
		[LocalizedGroup(nameof(Groups.AssignSelfRole))]
		[LocalizedAlias(nameof(Aliases.AssignSelfRole))]
		[LocalizedSummary(nameof(Summaries.AssignSelfRole))]
		[Meta("6c574af7-31a7-4733-9f10-badfe1e72f4c")]
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

		[LocalizedGroup(nameof(Groups.DisplaySelfRoles))]
		[LocalizedAlias(nameof(Aliases.DisplaySelfRoles))]
		[LocalizedSummary(nameof(Summaries.DisplaySelfRoles))]
		[Meta("3e3487e0-691a-45fa-9974-9d345b5337b7")]
		public sealed class DisplaySelfRoles : AdvobotModuleBase
		{
			[Command]
			public Task<RuntimeResult> Command()
				=> Responses.SelfRoles.DisplayGroups(Context.Settings.SelfAssignableGroups);

			[Command]
			public Task<RuntimeResult> Command(SelfAssignableRoles group)
				=> Responses.SelfRoles.DisplayGroup(Context.Guild, group);
		}

		[LocalizedGroup(nameof(Groups.ModifySelfRoles))]
		[LocalizedAlias(nameof(Aliases.ModifySelfRoles))]
		[LocalizedSummary(nameof(Summaries.ModifySelfRoles))]
		[Meta("2cb8f177-dc52-404c-a7f4-a63c84d976ba")]
		[RequireGuildPermissions]
		public sealed class ModifySelfRoles : SettingsModule<IGuildSettings>
		{
			protected override IGuildSettings Settings => Context.Settings;

			[LocalizedCommand(nameof(Groups.Add))]
			[LocalizedAlias(nameof(Aliases.Add))]
			public Task<RuntimeResult> Add(
				SelfAssignableRoles group,
				[CanModifyRole, NotEveryone, NotManaged] params IRole[] roles)
			{
				group.AddRoles(roles);
				return Responses.SelfRoles.ModifiedGroup(group, roles, true);
			}

			[LocalizedCommand(nameof(Groups.Create))]
			[LocalizedAlias(nameof(Aliases.Create))]
			public Task<RuntimeResult> Create(
				[SelfRoleGroup] int group)
			{
				Settings.SelfAssignableGroups.Add(new SelfAssignableRoles(group));
				return Responses.SelfRoles.CreatedGroup(group);
			}

			[LocalizedCommand(nameof(Groups.Delete))]
			[LocalizedAlias(nameof(Aliases.Delete))]
			public Task<RuntimeResult> Delete(SelfAssignableRoles group)
			{
				Settings.SelfAssignableGroups.RemoveAll(x => x.Group == group.Group);
				return Responses.SelfRoles.DeletedGroup(group);
			}

			[LocalizedCommand(nameof(Groups.Remove))]
			[LocalizedAlias(nameof(Aliases.Remove))]
			public Task<RuntimeResult> Remove(
				SelfAssignableRoles group,
				[CanModifyRole, NotEveryone, NotManaged] params IRole[] roles)
			{
				group.RemoveRoles(roles);
				return Responses.SelfRoles.ModifiedGroup(group, roles, false);
			}
		}
	}
}