using Discord;

using YACCS.Results;

namespace Advobot.Utilities;

/// <summary>
/// Utilities for validating Discord objects (users, roles, channels).
/// </summary>
public static class PreconditionUtils
{
	/// <summary>
	/// Returns true if the invoking user's position is greater than the target user's position or if both users are the bot.
	/// </summary>
	/// <param name="invoker"></param>
	/// <param name="target"></param>
	/// <returns></returns>
	public static bool CanModify(this IGuildUser invoker, IGuildUser target)
		=> invoker.GetHierarchy() > target.GetHierarchy();

	/// <summary>
	/// Returns true if the invoking user's position is greater than the target role's position.
	/// </summary>
	/// <param name="invoker"></param>
	/// <param name="target"></param>
	/// <returns></returns>
	public static bool CanModify(this IGuildUser invoker, IRole target)
		=> invoker.GetHierarchy() > target.Position;

	/// <summary>
	/// Verifies that the channel can be edited in specific ways.
	/// </summary>
	/// <param name="invoker"></param>
	/// <param name="target"></param>
	/// <param name="permissions"></param>
	/// <returns></returns>
	public static Task<IResult> ValidateChannel(
		this IGuildUser invoker,
		IGuildChannel? target,
		IEnumerable<ChannelPermission> permissions)
	{
		return invoker.ValidateAsync(target, (i, t) =>
		{
			// Can do everything if admin
			if (i.GuildPermissions.Administrator)
			{
				return true;
			}

			var channelPerms = i.GetPermissions(t);
			// Can't do anything if the channel can't be seen
			if (!channelPerms.Has(ChannelPermission.ViewChannel))
			{
				return false;
			}

			foreach (var permission in permissions)
			{
				if (!channelPerms.Has(permission))
				{
					return false;
				}
			}
			return true;
		});
	}

	/// <summary>
	/// Verifies that the role can be edited in specific ways.
	/// </summary>
	/// <param name="invoker"></param>
	/// <param name="target"></param>
	/// <returns></returns>
	public static Task<IResult> ValidateRole(
		this IGuildUser invoker,
		IRole? target)
		=> invoker.ValidateAsync(target, CanModify);

	/// <summary>
	/// Verifies that the user can be edited in specific ways.
	/// </summary>
	/// <param name="invoker"></param>
	/// <param name="target"></param>
	/// <returns></returns>
	public static Task<IResult> ValidateUser(
		this IGuildUser invoker,
		IGuildUser? target)
		=> invoker.ValidateAsync(target, CanModify);

	private static int GetHierarchy(this IGuildUser user)
	{
		if (user.Guild.OwnerId == user.Id)
		{
			return int.MaxValue;
		}
		if (user.RoleIds.Count == 0)
		{
			return user.Guild.EveryoneRole.Position;
		}
		return user.RoleIds.Max(x => user.Guild.GetRole(x).Position);
	}

	private static async Task<IResult> ValidateAsync<T>(
		this IGuildUser invoker,
		T? target,
		Func<IGuildUser, T, bool> permissionsCallback)
		where T : ISnowflakeEntity
	{
		if (target is null)
		{
			return Result.NullParameter;
		}

		var bot = await invoker.Guild.GetCurrentUserAsync().ConfigureAwait(false)
			?? throw new InvalidOperationException($"Invalid bot during {typeof(T).Name} validation.");

		if (!permissionsCallback(invoker, target))
		{
			return Result.Failure($"`{invoker.Format()}` can't modify `{target.Format()}`.");
		}
		if (!permissionsCallback(bot, target))
		{
			return Result.Failure($"`{bot.Format()}` can't modify `{target.Format()}`.");
		}

		return Result.EmptySuccess;
	}
}