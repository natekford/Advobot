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

using static Advobot.AutoMod.Responses.SelfRoles;

namespace Advobot.AutoMod.Commands;

[LocalizedCategory(nameof(SelfRoles))]
public sealed class SelfRoles : AdvobotModuleBase
{
	[LocalizedCommand(nameof(Groups.AssignSelfRole), nameof(Aliases.AssignSelfRole))]
	[LocalizedSummary(nameof(Summaries.AssignSelfRole))]
	[Id("6c574af7-31a7-4733-9f10-badfe1e72f4c")]
	public sealed class AssignSelfRole : AutoModModuleBase
	{
		[Command]
		public async Task<AdvobotResult> Command(SelfRoleState role)
		{
			if (Context.User.RoleIds.Any(x => x == role.Role.Id))
			{
				await Context.User.RemoveRoleAsync(role.Role, GetOptions("self role removal")).ConfigureAwait(false);
				return RemovedRole(role.Role);
			}

			// Remove roles the user already has from the group if they're targeting an exclusive group
			var conflicting = role.ConflictingRoles
				.Where(x => Context.User.RoleIds.Any(y => y == x.Id));
			if (!conflicting.Any())
			{
				await Context.User.AddRoleAsync(role.Role, GetOptions("self role giving")).ConfigureAwait(false);
				return AddedRole(role.Role);
			}

			await Context.User.ModifyRolesAsync(
				rolesToAdd: [role.Role],
				rolesToRemove: conflicting,
				GetOptions("self role giving and removal of conflicts")
			).ConfigureAwait(false);

			return AddedRoleAndRemovedOthers(role.Role);
		}
	}

	[LocalizedCommand(nameof(Groups.DisplaySelfRoles), nameof(Aliases.DisplaySelfRoles))]
	[LocalizedSummary(nameof(Summaries.DisplaySelfRoles))]
	[Id("3e3487e0-691a-45fa-9974-9d345b5337b7")]
	public sealed class DisplaySelfRoles : AutoModModuleBase
	{
		[Command]
		public async Task<AdvobotResult> Command()
			=> Display(await Db.GetSelfRolesAsync(Context.Guild.Id).ConfigureAwait(false));

		[Command]
		public async Task<AdvobotResult> Command([NotNegative] int group)
			=> Display(await Db.GetSelfRolesAsync(Context.Guild.Id, group).ConfigureAwait(false));

		private AdvobotResult Display(IEnumerable<SelfRole> roles)
		{
			var grouped = roles
				.Select(x => (x.GroupId, Role: Context.Guild.GetRole(x.RoleId)))
				.Where(x => x.Role != null)
				.GroupBy(x => x.GroupId, x => x.Role);
			return DisplayGroups(grouped);
		}
	}

	[LocalizedCommand(nameof(Groups.ModifySelfRoles), nameof(Aliases.ModifySelfRoles))]
	[LocalizedSummary(nameof(Summaries.ModifySelfRoles))]
	[Id("2cb8f177-dc52-404c-a7f4-a63c84d976ba")]
	[RequireGuildPermissions]
	public sealed class ModifySelfRoles : AutoModModuleBase
	{
		[LocalizedCommand(nameof(Groups.Add), nameof(Aliases.Add))]
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
			return AddedSelfRoles(group, roles.Length);
		}

		[LocalizedCommand(nameof(Groups.ClearGroup), nameof(Aliases.ClearGroup))]
		public async Task<AdvobotResult> ClearGroup([NotNegative] int group)
		{
			var count = await Db.DeleteSelfRolesGroupAsync(Context.Guild.Id, group).ConfigureAwait(false);
			return ClearedGroup(group, count);
		}

		[LocalizedCommand(nameof(Groups.Remove), nameof(Aliases.Remove))]
		public async Task<AdvobotResult> Remove(
			[CanModifyRole]
			[NotEveryone]
			[NotManaged]
			params IRole[] roles)
		{
			var count = await Db.DeleteSelfRolesAsync(roles.Select(x => x.Id)).ConfigureAwait(false);
			return RemovedSelfRoles(count);
		}
	}
}