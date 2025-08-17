using Discord;
using Discord.Commands;
using Discord.WebSocket;

using System.Diagnostics;
using System.Reflection;

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
		var bot = await role.Guild.GetCurrentUserAsync().ConfigureAwait(false);
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

		await role.Guild.ReorderRolesAsync(reorderProperties, options).ConfigureAwait(false);
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
}