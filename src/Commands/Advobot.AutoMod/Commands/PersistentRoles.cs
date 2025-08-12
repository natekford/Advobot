using Advobot.Attributes;
using Advobot.AutoMod.Database.Models;
using Advobot.Localization;
using Advobot.Modules;
using Advobot.ParameterPreconditions.Discord.Roles;
using Advobot.Preconditions.Permissions;
using Advobot.Resources;
using Advobot.Utilities;

using Discord;
using Discord.Commands;

using static Advobot.AutoMod.Responses.PersistentRoles;

namespace Advobot.AutoMod.Commands;

[Category(nameof(PersistentRoles))]
public sealed class PersistentRoles : ModuleBase
{
	[LocalizedGroup(nameof(Groups.DisplayPersistentRoles))]
	[LocalizedAlias(nameof(Aliases.DisplayPersistentRoles))]
	[LocalizedSummary(nameof(Summaries.DisplayPersistentRoles))]
	[Meta("c4ab3f40-c245-4cd1-963b-7cd55a55d494", IsEnabled = false)]
	[RequireGuildPermissions(GuildPermission.ManageRoles)]
	public sealed class DisplayPersistentRoles : AutoModModuleBase
	{
		[Command]
		public async Task<RuntimeResult> Command()
		{
			var roles = await Db.GetPersistentRolesAsync(Context.Guild.Id).ConfigureAwait(false);
			return Display(roles);
		}

		[Command]
		public async Task<RuntimeResult> Command(IGuildUser user)
		{
			var roles = await Db.GetPersistentRolesAsync(Context.Guild.Id, user.Id).ConfigureAwait(false);
			return Display(roles);
		}

		private AdvobotResult Display(IEnumerable<PersistentRole> roles)
		{
			var grouped = roles
				.Select(x =>
				{
					var user = Context.Guild.GetUser(x.UserId).Format() ?? x.UserId.ToString();
					var role = Context.Guild.GetRole(x.RoleId);
					return (User: user, Role: role);
				})
				.Where(x => x.Role != null)
				.GroupBy(x => x.User, x => x.Role);
			return DisplayPersistentRoles(grouped);
		}
	}

	[LocalizedGroup(nameof(Groups.ModifyPersistentRoles))]
	[LocalizedAlias(nameof(Aliases.ModifyPersistentRoles))]
	[LocalizedSummary(nameof(Summaries.ModifyPersistentRoles))]
	[Meta("b4a8b2d4-c6cc-4d6c-9671-91943e9079fc", IsEnabled = false)]
	[RequireGuildPermissions(GuildPermission.ManageRoles)]
	public sealed class ModifyPersistentRoles : AutoModModuleBase
	{
		[LocalizedCommand(nameof(Groups.Give))]
		[LocalizedAlias(nameof(Aliases.Give))]
		public async Task<RuntimeResult> Give(
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

		[LocalizedCommand(nameof(Groups.Remove))]
		[LocalizedAlias(nameof(Aliases.Remove))]
		public async Task<RuntimeResult> Remove(
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