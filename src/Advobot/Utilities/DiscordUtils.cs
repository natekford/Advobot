using AdvorangesUtils;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Advobot.Utilities;

/// <summary>
/// Actions done on discord objects.
/// </summary>
public static class DiscordUtils
{
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
	/// Generates a default request options explaining who invoked the command for the audit log.
	/// </summary>
	/// <param name="context"></param>
	/// <param name="reason"></param>
	/// <returns></returns>
	public static RequestOptions GenerateRequestOptions(
		this ICommandContext context,
		string? reason = null)
		=> context.User.GenerateRequestOptions(reason);

	/// <summary>
	/// Generates a default request options explaining who invoked the command for the audit log.
	/// </summary>
	/// <param name="user"></param>
	/// <param name="reason"></param>
	/// <returns></returns>
	public static RequestOptions GenerateRequestOptions(
		this IUser user,
		string? reason = null)
	{
		var r = user.Format();
		if (reason != null)
		{
			r += $": {reason.TrimEnd()}.";
		}
		return GenerateRequestOptions(r);
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
			.OrderBy(x => x.Position)];
	}

	/// <summary>
	/// Gets a user from the cache if available.
	/// </summary>
	/// <param name="client"></param>
	/// <param name="id"></param>
	/// <returns></returns>
	public static async Task<IUser?> GetUserAsync(this BaseSocketClient client, ulong id)
		=> (IUser)client.GetUser(id) ?? await client.Rest.GetUserAsync(id).CAF();

	/// <summary>
	/// Changes the role's position and says the supplied reason in the audit log.
	/// </summary>
	/// <param name="role"></param>
	/// <param name="position"></param>
	/// <param name="options"></param>
	/// <returns></returns>
	public static async Task<int> ModifyRolePositionAsync(
		this IRole role,
		int position,
		RequestOptions options)
	{
		// Make sure it's put at the highest a bot can edit, so no permission exception
		var bot = await role.Guild.GetCurrentUserAsync().CAF();
		var roles = role.Guild.Roles
			.Where(x => x.Id != role.Id && bot.CanModify(x))
			.OrderBy(x => x.Position)
			.ToArray();
		position = Math.Max(1, Math.Min(position, roles.Length));

		var reorderProperties = new ReorderRoleProperties[roles.Length + 1];
		var newPosition = -1;
		for (var i = 0; i < reorderProperties.Length; ++i)
		{
			if (i > position)
			{
				reorderProperties[i] = new(roles[i - 1].Id, i);
			}
			else if (i < position)
			{
				reorderProperties[i] = new(roles[i].Id, i);
			}
			else
			{
				reorderProperties[i] = new(role.Id, i);
				newPosition = i;
			}
		}

		await role.Guild.ReorderRolesAsync(reorderProperties, options).CAF();
		return newPosition;
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
			var set = new HashSet<ulong>();
			set.AddRange(user.RoleIds);
			set.Remove(user.Guild.EveryoneRole.Id);

			set.AddRange(rolesToAdd.Select(x => x.Id));
			foreach (var role in rolesToRemove)
			{
				set.Remove(role.Id);
			}

			x.RoleIds = new(set);
		}, options);
	}

	/// <summary>
	/// Changes the guild's system channel flags.
	/// </summary>
	/// <param name="guild"></param>
	/// <param name="flags"></param>
	/// <param name="enable"></param>
	/// <param name="options"></param>
	/// <returns></returns>
	public static Task ModifySystemChannelFlags(
		this IGuild guild,
		SystemChannelMessageDeny flags,
		bool enable,
		RequestOptions options)
	{
		var current = guild.SystemChannelFlags;

		//None are disabled and we're trying to enable, so we don't have to do anything.
		if (current == SystemChannelMessageDeny.None && enable)
		{
			return Task.CompletedTask;
		}

		var toggle = enable ? ~flags : flags;
		var newValue = current & toggle;
		//No change so no need to modify
		if (current == newValue)
		{
			return Task.CompletedTask;
		}

		return guild.ModifyAsync(x => x.SystemChannelFlags = newValue, options);
	}

	/// <summary>
	/// Returns every user that has a non null join time in order from least to greatest.
	/// </summary>
	/// <param name="users"></param>
	/// <returns></returns>
	public static IReadOnlyList<T> OrderByJoinDate<T>(this IEnumerable<T> users) where T : IGuildUser
	{
		return [.. users
			.Where(x => x.JoinedAt.HasValue)
			.OrderBy(x => x.JoinedAt.GetValueOrDefault().Ticks)];
	}
}