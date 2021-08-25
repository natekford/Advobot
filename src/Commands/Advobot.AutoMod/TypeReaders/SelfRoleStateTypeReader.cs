
using Advobot.Attributes;
using Advobot.AutoMod.Database;
using Advobot.AutoMod.Models;
using Advobot.Utilities;

using AdvorangesUtils;

using Discord;
using Discord.Commands;

using Microsoft.Extensions.DependencyInjection;

namespace Advobot.AutoMod.TypeReaders
{
	[TypeReaderTargetType(typeof(SelfRoleState))]
	public sealed class SelfRoleStateTypeReader : RoleTypeReader<IRole>
	{
		public override async Task<TypeReaderResult> ReadAsync(
			ICommandContext context,
			string input,
			IServiceProvider services)
		{
			var result = await base.ReadAsync(context, input, services).CAF();
			if (!result.IsSuccess)
			{
				return result;
			}
			var role = (IRole)result.BestMatch;

			var db = services.GetRequiredService<IAutoModDatabase>();
			var selfRole = await db.GetSelfRoleAsync(role.Id).CAF();
			if (selfRole == null)
			{
				return TypeReaderResult.FromError(CommandError.ObjectNotFound,
					$"`{role.Format()}` is not a self assignable role.");
			}

			IReadOnlyList<IRole> conflicting = Array.Empty<IRole>();
			if (selfRole.GroupId != 0)
			{
				conflicting = await GetConflictingRoles(db, context, selfRole).CAF();
			}

			var state = new SelfRoleState(selfRole.GroupId, role, conflicting);
			return TypeReaderResult.FromSuccess(state);
		}

		private async Task<List<IRole>> GetConflictingRoles(
			IAutoModDatabase db,
			ICommandContext context,
			SelfRole item)
		{
			var selfRoles = await db.GetSelfRolesAsync(context.Guild.Id, item.GroupId).CAF();
			var conflicating = new List<IRole>(selfRoles.Count);
			var toDelete = new List<ulong>();

			foreach (var selfRole in selfRoles)
			{
				var role = context.Guild.GetRole(selfRole.RoleId);
				// Role doesn't exist anymore, so go remove it from the db
				if (role == null)
				{
					toDelete.Add(item.RoleId);
				}
				else if (role.Id != item.RoleId)
				{
					conflicating.Add(role);
				}
			}

			if (toDelete.Count != 0)
			{
				await db.DeleteSelfRolesAsync(toDelete).CAF();
			}
			return conflicating;
		}
	}
}