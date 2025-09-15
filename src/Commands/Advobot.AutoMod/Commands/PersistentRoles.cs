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

using static Advobot.AutoMod.Responses.PersistentRoles;

namespace Advobot.AutoMod.Commands;

[LocalizedCategory(nameof(PersistentRoles))]
public sealed class PersistentRoles : AdvobotModuleBase
{
	[LocalizedCommand(nameof(Groups.DisplayPersistentRoles), nameof(Aliases.DisplayPersistentRoles))]
	[LocalizedSummary(nameof(Summaries.DisplayPersistentRoles))]
	[Id("c4ab3f40-c245-4cd1-963b-7cd55a55d494")]
	[Meta(IsEnabled = false)]
	[RequireGuildPermissions(GuildPermission.ManageRoles)]
	public sealed class DisplayPersistentRoles : AutoModModuleBase
	{
		[LocalizedCommand]
		public async Task<AdvobotResult> Command()
		{
			var roles = await Db.GetPersistentRolesAsync(Context.Guild.Id).ConfigureAwait(false);
			return await DisplayAsync(roles).ConfigureAwait(false);
		}

		[LocalizedCommand]
		public async Task<AdvobotResult> Command(IGuildUser user)
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
			return DisplayPersistentRoles(grouped);
		}
	}

	[LocalizedCommand(nameof(Groups.ModifyPersistentRoles), nameof(Aliases.ModifyPersistentRoles))]
	[LocalizedSummary(nameof(Summaries.ModifyPersistentRoles))]
	[Id("b4a8b2d4-c6cc-4d6c-9671-91943e9079fc")]
	[Meta(IsEnabled = false)]
	[RequireGuildPermissions(GuildPermission.ManageRoles)]
	public sealed class ModifyPersistentRoles : AutoModModuleBase
	{
		[LocalizedCommand(nameof(Groups.Give), nameof(Aliases.Give))]
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

			return GavePersistentRole(user, role);
		}

		[LocalizedCommand(nameof(Groups.Remove), nameof(Aliases.Remove))]
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

			return RemovedPersistentRole(user, role);
		}
	}
}