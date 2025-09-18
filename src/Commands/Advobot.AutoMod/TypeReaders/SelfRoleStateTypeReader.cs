using Advobot.AutoMod.Database;
using Advobot.AutoMod.Database.Models;
using Advobot.Modules;
using Advobot.TypeReaders.Discord;

using Discord;
using Discord.Commands;

using Microsoft.Extensions.DependencyInjection;

using MorseCode.ITask;

using YACCS.Results;
using YACCS.TypeReaders;

namespace Advobot.AutoMod.TypeReaders;

[TypeReaderTargetTypes(typeof(SelfRoleState))]
public sealed class SelfRoleStateTypeReader : DiscordTypeReader<SelfRoleState>
{
	private readonly RoleTypeReader _RoleTypeReader = new();

	public override async ITask<ITypeReaderResult<SelfRoleState>> ReadAsync(
		IGuildContext context,
		ReadOnlyMemory<string> input)
	{
		var result = await _RoleTypeReader.ReadAsync(context, input).ConfigureAwait(false);
		if (!result.InnerResult.IsSuccess)
		{
			return Error(result.InnerResult);
		}
		if (result.Value is not IRole role)
		{
			return TypeReaderResult<SelfRoleState>.ParseFailed.Result;
		}

		var db = GetDatabase(context.Services);
		var selfRole = await db.GetSelfRoleAsync(role.Id).ConfigureAwait(false);
		if (selfRole is null)
		{
			return TypeReaderResult<SelfRoleState>.NotFound.Result;
		}

		IReadOnlyList<IRole> conflicting = [];
		if (selfRole.GroupId != SelfRole.NO_GROUP)
		{
			conflicting = await GetConflictingRoles(db, selfRole, context.Guild).ConfigureAwait(false);
		}

		return Success(new(selfRole.GroupId, role, conflicting));
	}

	[GetServiceMethod]
	private static AutoModDatabase GetDatabase(IServiceProvider services)
		=> services.GetRequiredService<AutoModDatabase>();

	private async Task<List<IRole>> GetConflictingRoles(
		AutoModDatabase db,
		SelfRole item,
		IGuild guild)
	{
		var selfRoles = await db.GetSelfRolesAsync(guild.Id, item.GroupId).ConfigureAwait(false);
		var conflicting = new List<IRole>(selfRoles.Count);
		var deletable = new List<ulong>();

		foreach (var selfRole in selfRoles)
		{
			var role = guild.GetRole(selfRole.RoleId);
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