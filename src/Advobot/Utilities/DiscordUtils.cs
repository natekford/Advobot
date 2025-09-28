using Discord;

using System.Threading.Channels;

namespace Advobot.Utilities;

/// <summary>
/// Actions done on discord objects.
/// </summary>
public static class DiscordUtils
{
	/// <summary>
	/// Removes every overwrite and returns the amount of removed overwrites.
	/// </summary>
	/// <param name="channel"></param>
	/// <param name="options"></param>
	/// <returns></returns>
	/// <exception cref="ArgumentException"></exception>
	public static async Task<int> ClearOverwritesAsync(
		this IGuildChannel channel,
		RequestOptions? options = null)
	{
		var overwrites = channel.PermissionOverwrites;
		foreach (var overwrite in overwrites)
		{
			var entity = await channel.Guild.GetEntityAsync(overwrite).ConfigureAwait(false);
			await (entity switch
			{
				IRole r => channel.RemovePermissionOverwriteAsync(r, options),
				IUser u => channel.RemovePermissionOverwriteAsync(u, options),
				_ => throw NotRoleOrUser(),
			}).ConfigureAwait(false);
		}
		return overwrites.Count;
	}

	/// <summary>
	/// If <paramref name="id"/> has a value will only copy overwrites targeting that id, otherwise copies every overwrite.
	/// </summary>
	/// <param name="input"></param>
	/// <param name="output"></param>
	/// <param name="id"></param>
	/// <param name="options"></param>
	/// <returns></returns>
	/// <exception cref="ArgumentException"></exception>
	public static async Task<IReadOnlyCollection<Overwrite>> CopyOverwritesAsync(
		this IGuildChannel input,
		IGuildChannel output,
		ulong? id,
		RequestOptions? options = null)
	{
		if (input.GuildId != output.GuildId)
		{
			throw new ArgumentException("Both channels must come from the same guild.");
		}

		var overwrites = id.HasValue
			? [.. input.PermissionOverwrites.Where(x => x.TargetId == id)]
			: input.PermissionOverwrites;
		foreach (var overwrite in overwrites)
		{
			var entity = await input.Guild.GetEntityAsync(overwrite).ConfigureAwait(false);
			await (entity switch
			{
				IRole r => output.AddPermissionOverwriteAsync(r, overwrite.Permissions, options),
				IUser u => output.AddPermissionOverwriteAsync(u, overwrite.Permissions, options),
				_ => throw NotRoleOrUser(),
			}).ConfigureAwait(false);
		}
		return overwrites;
	}

	/// <summary>
	/// Creates a role with a name and no permissions/color.
	/// </summary>
	/// <param name="guild"></param>
	/// <param name="name"></param>
	/// <param name="options"></param>
	/// <returns></returns>
	public static Task<IRole> CreateEmptyRoleAsync(
		this IGuild guild,
		string name,
		RequestOptions? options = null)
	{
		return guild.CreateRoleAsync(
			name: name,
			permissions: GuildPermissions.None,
			color: null,
			isHoisted: false,
			isMentionable: false,
			options: options
		);
	}

	/// <summary>
	/// Returns request options, with <paramref name="reason"/> as the audit log reason.
	/// </summary>
	/// <param name="reason"></param>
	/// <returns></returns>
	public static RequestOptions GenerateRequestOptions(string? reason = null)
	{
		return new()
		{
			AuditLogReason = reason,
			RetryMode = RetryMode.RetryRatelimit,
		};
	}

	/// <summary>
	/// Returns all the roles a user has.
	/// </summary>
	/// <param name="user"></param>
	/// <returns></returns>
	public static IReadOnlyList<IRole> GetRoles(this IGuildUser user)
	{
		return [.. user.RoleIds
			.Select(x => user.Guild.GetRole(x))
			.Where(x => x.Id != user.Guild.EveryoneRole.Id)
			.OrderBy(x => x.Position)
		];
	}

	/// <summary>
	/// Removes multiple roles in one API call.
	/// </summary>
	/// <param name="user"></param>
	/// <param name="rolesToAdd"></param>
	/// <param name="rolesToRemove"></param>
	/// <param name="options"></param>
	/// <returns></returns>
	public static Task ModifyRolesAsync(
		this IGuildUser user,
		IEnumerable<IRole> rolesToAdd,
		IEnumerable<IRole> rolesToRemove,
		RequestOptions? options = null)
	{
		return user.ModifyAsync(x =>
		{
			var set = user.RoleIds
				.Concat(rolesToAdd.Select(x => x.Id))
				.ToHashSet();

			set.Remove(user.Guild.EveryoneRole.Id);
			foreach (var role in rolesToRemove)
			{
				set.Remove(role.Id);
			}

			x.RoleIds = new(set);
		}, options);
	}

	private static async Task<ISnowflakeEntity> GetEntityAsync(
		this IGuild guild,
		Overwrite overwrite
	) => overwrite.TargetType switch
	{
		PermissionTarget.Role => guild.GetRole(overwrite.TargetId),
		PermissionTarget.User => await guild.GetUserAsync(overwrite.TargetId).ConfigureAwait(false),
		_ => throw NotRoleOrUser(),
	};

	private static InvalidOperationException NotRoleOrUser()
		=> new("Not a role or user.");
}