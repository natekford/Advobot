using Advobot.Attributes;
using Advobot.AutoMod.Database.Models;
using Advobot.Modules;
using Advobot.ParameterPreconditions.Discord.Roles;
using Advobot.Preconditions.Permissions;
using Advobot.Resources;
using Advobot.Utilities;

using Discord;

using YACCS.Commands.Attributes;
using YACCS.Localization;

namespace Advobot.AutoMod.Commands;

[LocalizedCategory(nameof(Names.PersistentRolesCategory))]
public sealed class PersistentRoles : AdvobotModuleBase
{
	[LocalizedCommand(nameof(Names.DisplayPersistentRoles), nameof(Names.DisplayPersistentRolesAlias))]
	[LocalizedSummary(nameof(Summaries.DisplayPersistentRolesSummary))]
	[Meta("c4ab3f40-c245-4cd1-963b-7cd55a55d494", IsEnabled = false)]
	[RequireGuildPermissions(GuildPermission.ManageRoles)]
	public sealed class DisplayPersistentRoles : AutoModModuleBase
	{
		[Command]
		public async Task<AdvobotResult> Guild()
		{
			var roles = await Db.GetPersistentRolesAsync(Context.Guild.Id).ConfigureAwait(false);
			return await DisplayAsync(roles).ConfigureAwait(false);
		}

		[Command]
		public async Task<AdvobotResult> User(IGuildUser user)
		{
			var roles = await Db.GetPersistentRolesAsync(Context.Guild.Id, user.Id).ConfigureAwait(false);
			return await DisplayAsync(roles).ConfigureAwait(false);
		}

		private async Task<AdvobotResult> DisplayAsync(IEnumerable<PersistentRole> persistentRoles)
		{
			var retrieved = new List<(string User, IRole Role)>();
			foreach (var persistentRole in persistentRoles)
			{
				var user = (await GetUserAsync(persistentRole.UserId).ConfigureAwait(false))
					?.Format() ?? persistentRole.UserId.ToString();
				var role = Context.Guild.GetRole(persistentRole.RoleId);
				retrieved.Add((user, role));
			}

			var grouped = retrieved
				.Where(x => x.Role != null)
				.GroupBy(x => x.User, x => x.Role);
			return Responses.PersistentRoles.DisplayPersistentRoles(grouped);
		}
	}

	[LocalizedCommand(nameof(Names.ModifyPersistentRoles), nameof(Names.ModifyPersistentRolesAlias))]
	[LocalizedSummary(nameof(Summaries.ModifyPersistentRolesSummary))]
	[Meta("b4a8b2d4-c6cc-4d6c-9671-91943e9079fc", IsEnabled = false)]
	[RequireGuildPermissions(GuildPermission.ManageRoles)]
	public sealed class ModifyPersistentRoles : AutoModModuleBase
	{
		[LocalizedCommand(nameof(Names.Give), nameof(Names.GiveAlias))]
		public async Task<AdvobotResult> Give(
			IGuildUser user,
			[Remainder, CanModifyRole, NotEveryone, NotManaged]
			IRole role)
		{
			await user.AddRoleAsync(role, GetOptions()).ConfigureAwait(false);

			var persistentRole = new PersistentRole
			{
				GuildId = Context.Guild.Id,
				UserId = user.Id,
				RoleId = role.Id
			};
			await Db.AddPersistentRoleAsync(persistentRole).ConfigureAwait(false);

			return Responses.PersistentRoles.GavePersistentRole(user, role);
		}

		[LocalizedCommand(nameof(Names.Remove), nameof(Names.RemoveAlias))]
		public async Task<AdvobotResult> Remove(
			IGuildUser user,
			[Remainder, CanModifyRole, NotEveryone, NotManaged]
			IRole role)
		{
			if (user.RoleIds.Contains(role.Id))
			{
				await user.RemoveRoleAsync(role, GetOptions()).ConfigureAwait(false);
			}

			var persistentRole = new PersistentRole
			{
				GuildId = Context.Guild.Id,
				UserId = user.Id,
				RoleId = role.Id
			};
			await Db.DeletePersistentRoleAsync(persistentRole).ConfigureAwait(false);

			return Responses.PersistentRoles.RemovedPersistentRole(user, role);
		}
	}
}