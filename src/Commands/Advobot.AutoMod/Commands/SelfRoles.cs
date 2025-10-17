using Advobot.Attributes;
using Advobot.AutoMod.Database.Models;
using Advobot.Modules;
using Advobot.ParameterPreconditions.Discord.Roles;
using Advobot.ParameterPreconditions.Numbers;
using Advobot.Preconditions.Permissions;
using Advobot.Resources;
using Advobot.Utilities;

using Discord;

using YACCS.Commands.Attributes;
using YACCS.Localization;

namespace Advobot.AutoMod.Commands;

[LocalizedCategory(nameof(Names.SelfRolesCategory))]
public sealed class SelfRoles
{
	[Command(nameof(Names.AssignSelfRole), nameof(Names.AssignSelfRoleAlias))]
	[LocalizedSummary(nameof(Summaries.AssignSelfRoleSummary))]
	[Meta("6c574af7-31a7-4733-9f10-badfe1e72f4c", IsEnabled = true)]
	public sealed class AssignSelfRole : AutoModModuleBase
	{
		[Command]
		public async Task<AdvobotResult> Give(SelfRoleState role)
		{
			if (Context.User.RoleIds.Any(x => x == role.Role.Id))
			{
				await Context.User.RemoveRoleAsync(role.Role, GetOptions("self role removal")).ConfigureAwait(false);
				return Responses.SelfRoles.RemovedRole(role.Role);
			}

			// Remove roles the user already has from the group if they're targeting an exclusive group
			var conflicting = role.ConflictingRoles
				.Where(x => Context.User.RoleIds.Any(y => y == x.Id));
			if (!conflicting.Any())
			{
				await Context.User.AddRoleAsync(role.Role, GetOptions("self role giving")).ConfigureAwait(false);
				return Responses.SelfRoles.AddedRole(role.Role);
			}

			await Context.User.ModifyRolesAsync(
				rolesToAdd: [role.Role],
				rolesToRemove: conflicting,
				GetOptions("self role giving and removal of conflicts")
			).ConfigureAwait(false);

			return Responses.SelfRoles.AddedRoleAndRemovedOthers(role.Role);
		}
	}

	[Command(nameof(Names.DisplaySelfRoles), nameof(Names.DisplaySelfRolesAlias))]
	[LocalizedSummary(nameof(Summaries.DisplaySelfRolesSummary))]
	[Meta("3e3487e0-691a-45fa-9974-9d345b5337b7", IsEnabled = true)]
	public sealed class DisplaySelfRoles : AutoModModuleBase
	{
		[Command]
		public async Task<AdvobotResult> All()
			=> Display(await Db.GetSelfRolesAsync(Context.Guild.Id).ConfigureAwait(false));

		[Command]
		public async Task<AdvobotResult> Group([NotNegative] int group)
			=> Display(await Db.GetSelfRolesAsync(Context.Guild.Id, group).ConfigureAwait(false));

		private AdvobotResult Display(IEnumerable<SelfRole> roles)
		{
			var grouped = roles
				.Select(x => (x.GroupId, Role: Context.Guild.GetRole(x.RoleId)))
				.Where(x => x.Role != null)
				.GroupBy(x => x.GroupId, x => x.Role);
			return Responses.SelfRoles.DisplayGroups(grouped);
		}
	}

	[Command(nameof(Names.ModifySelfRoles), nameof(Names.ModifySelfRolesAlias))]
	[LocalizedSummary(nameof(Summaries.ModifySelfRolesSummary))]
	[Meta("2cb8f177-dc52-404c-a7f4-a63c84d976ba", IsEnabled = true)]
	[RequireGuildPermissions(GuildPermission.ManageRoles)]
	public sealed class ModifySelfRoles : AutoModModuleBase
	{
		[Command(nameof(Names.Add), nameof(Names.AddAlias))]
		public async Task<AdvobotResult> Add(
			[NotNegative]
			int group,
			[CanModifyRole]
			[NotEveryone]
			[NotManaged]
			params IRole[] roles)
		{
			var selfRoles = roles.Select(x => new SelfRole
			{
				GuildId = x.Guild.Id,
				RoleId = x.Id,
				GroupId = group,
			});
			var count = await Db.UpsertSelfRolesAsync(selfRoles).ConfigureAwait(false);
			return Responses.SelfRoles.AddedSelfRoles(group, roles.Length);
		}

		[Command(nameof(Names.ClearGroup), nameof(Names.ClearGroupAlias))]
		public async Task<AdvobotResult> ClearGroup([NotNegative] int group)
		{
			var count = await Db.DeleteSelfRolesGroupAsync(Context.Guild.Id, group).ConfigureAwait(false);
			return Responses.SelfRoles.ClearedGroup(group, count);
		}

		[Command(nameof(Names.Remove), nameof(Names.RemoveAlias))]
		public async Task<AdvobotResult> Remove(
			[CanModifyRole]
			[NotEveryone]
			[NotManaged]
			params IRole[] roles)
		{
			var count = await Db.DeleteSelfRolesAsync(roles.Select(x => x.Id)).ConfigureAwait(false);
			return Responses.SelfRoles.RemovedSelfRoles(count);
		}
	}
}