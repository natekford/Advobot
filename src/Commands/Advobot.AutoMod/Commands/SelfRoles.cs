
using Advobot.Attributes;
using Advobot.Attributes.ParameterPreconditions.DiscordObjectValidation.Roles;
using Advobot.Attributes.ParameterPreconditions.Numbers;
using Advobot.Attributes.Preconditions.Permissions;
using Advobot.AutoMod.Models;
using Advobot.Localization;
using Advobot.Resources;
using Advobot.Utilities;

using AdvorangesUtils;

using Discord;
using Discord.Commands;

using static Advobot.AutoMod.Responses.SelfRoles;

namespace Advobot.AutoMod.Commands
{
	[Category(nameof(SelfRoles))]
	public sealed class SelfRoles : ModuleBase
	{
		[LocalizedGroup(nameof(Groups.AssignSelfRole))]
		[LocalizedAlias(nameof(Aliases.AssignSelfRole))]
		[LocalizedSummary(nameof(Summaries.AssignSelfRole))]
		[Meta("6c574af7-31a7-4733-9f10-badfe1e72f4c")]
		public sealed class AssignSelfRole : AutoModModuleBase
		{
			[Command]
			public async Task<RuntimeResult> Command(SelfRoleState role)
			{
				if (Context.User.Roles.Any(x => x.Id == role.Role.Id))
				{
					await Context.User.RemoveRoleAsync(role.Role, GenerateRequestOptions("self role removal")).CAF();
					return RemovedRole(role.Role);
				}

				// Remove roles the user already has from the group if they're targeting an exclusive group
				await Context.User.AddRoleAsync(role.Role, GenerateRequestOptions("self role giving")).CAF();
				if (role.ConflictingRoles.Count == 0)
				{
					return AddedRole(role.Role);
				}

				var conflicting = role.ConflictingRoles
					.Where(x => Context.User.Roles.Any(y => y.Id == x.Id));
				if (!conflicting.Any())
				{
					return AddedRole(role.Role);
				}

				await Context.User.SmartRemoveRolesAsync(conflicting, GenerateRequestOptions("self role removal")).CAF();
				return AddedRoleAndRemovedOthers(role.Role);
			}
		}

		[LocalizedGroup(nameof(Groups.DisplaySelfRoles))]
		[LocalizedAlias(nameof(Aliases.DisplaySelfRoles))]
		[LocalizedSummary(nameof(Summaries.DisplaySelfRoles))]
		[Meta("3e3487e0-691a-45fa-9974-9d345b5337b7")]
		public sealed class DisplaySelfRoles : AutoModModuleBase
		{
			[Command]
			public async Task<RuntimeResult> Command()
				=> Display(await Db.GetSelfRolesAsync(Context.Guild.Id).CAF());

			[Command]
			public async Task<RuntimeResult> Command([NotNegative] int group)
				=> Display(await Db.GetSelfRolesAsync(Context.Guild.Id, group).CAF());

			private RuntimeResult Display(IEnumerable<SelfRole> roles)
			{
				var grouped = roles
					.Select(x => (x.GroupId, Role: Context.Guild.GetRole(x.RoleId)))
					.Where(x => x.Role != null)
					.GroupBy(x => x.GroupId, x => x.Role);
				return DisplayGroups(grouped);
			}
		}

		[LocalizedGroup(nameof(Groups.ModifySelfRoles))]
		[LocalizedAlias(nameof(Aliases.ModifySelfRoles))]
		[LocalizedSummary(nameof(Summaries.ModifySelfRoles))]
		[Meta("2cb8f177-dc52-404c-a7f4-a63c84d976ba")]
		[RequireGuildPermissions]
		public sealed class ModifySelfRoles : AutoModModuleBase
		{
			[LocalizedCommand(nameof(Groups.Add))]
			[LocalizedAlias(nameof(Aliases.Add))]
			public async Task<RuntimeResult> Add(
				[NotNegative]
				int group,
				[CanModifyRole, NotEveryone, NotManaged]
				params IRole[] roles)
			{
				var selfRoles = roles.Select(x => new SelfRole
				{
					GuildId = x.Guild.Id,
					RoleId = x.Id,
					GroupId = group,
				});
				var count = await Db.UpsertSelfRolesAsync(selfRoles).CAF();
				return AddedSelfRoles(group, roles.Length);
			}

			[LocalizedCommand(nameof(Groups.ClearGroup))]
			[LocalizedAlias(nameof(Aliases.ClearGroup))]
			public async Task<RuntimeResult> ClearGroup([NotNegative] int group)
			{
				var count = await Db.DeleteSelfRolesGroupAsync(Context.Guild.Id, group).CAF();
				return ClearedGroup(group, count);
			}

			[LocalizedCommand(nameof(Groups.Remove))]
			[LocalizedAlias(nameof(Aliases.Remove))]
			public async Task<RuntimeResult> Remove(
				[CanModifyRole, NotEveryone, NotManaged]
				params IRole[] roles)
			{
				var count = await Db.DeleteSelfRolesAsync(roles.Select(x => x.Id)).CAF();
				return RemovedSelfRoles(count);
			}
		}
	}
}