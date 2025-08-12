using Advobot.AutoMod.Database;
using Advobot.AutoMod.Database.Models;
using Advobot.TypeReaders;
using Advobot.Utilities;

using Discord;
using Discord.Commands;

using Microsoft.Extensions.DependencyInjection;

namespace Advobot.AutoMod.TypeReaders;

[TypeReaderTargetType(typeof(SelfRoleState))]
public sealed class SelfRoleStateTypeReader : RoleTypeReader<IRole>
{
	public override async Task<TypeReaderResult> ReadAsync(
		ICommandContext context,
		string input,
		IServiceProvider services)
	{
		var result = await base.ReadAsync(context, input, services).ConfigureAwait(false);
		if (!result.IsSuccess)
		{
			return result;
		}
		var role = (IRole)result.BestMatch;

		var db = services.GetRequiredService<IAutoModDatabase>();
		var selfRole = await db.GetSelfRoleAsync(role.Id).ConfigureAwait(false);
		if (selfRole is null)
		{
			return TypeReaderResult.FromError(CommandError.ObjectNotFound,
				$"`{role.Format()}` is not a self assignable role.");
		}

		IReadOnlyList<IRole> conflicting = [];
		if (selfRole.GroupId != SelfRole.NO_GROUP)
		{
			conflicting = await GetConflictingRoles(db, context, selfRole).ConfigureAwait(false);
		}

		var state = new SelfRoleState(selfRole.GroupId, role, conflicting);
		return TypeReaderResult.FromSuccess(state);
	}

	private async Task<List<IRole>> GetConflictingRoles(
		IAutoModDatabase db,
		ICommandContext context,
		SelfRole item)
	{
		var selfRoles = await db.GetSelfRolesAsync(context.Guild.Id, item.GroupId).ConfigureAwait(false);
		var conflicting = new List<IRole>(selfRoles.Count);
		var deletable = new List<ulong>();

		foreach (var selfRole in selfRoles)
		{
			var role = context.Guild.GetRole(selfRole.RoleId);
			// Role doesn't exist anymore, so go remove it from the db
			if (role is null)
			{
				deletable.Add(item.RoleId);
			}
			else if (role.Id != item.RoleId)
			{
				conflicting.Add(role);
			}
		}

		if (deletable.Count != 0)
		{
			await db.DeleteSelfRolesAsync(deletable).ConfigureAwait(false);
		}
		return conflicting;
	}
}