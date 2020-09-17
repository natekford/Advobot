using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Advobot.Attributes.ParameterPreconditions;
using Advobot.Preconditions;

using AdvorangesUtils;

using Discord;
using Discord.Commands;

namespace Advobot.Utilities
{
	/// <summary>
	/// Utilities for validating Discord objects (users, roles, channels).
	/// </summary>
	public static class PreconditionUtils
	{
		/// <summary>
		/// A successful result.
		/// </summary>
		public static PreconditionResult SuccessInstance { get; } = PreconditionResult.FromSuccess();

		/// <summary>
		/// Creates a <see cref="Task{T}"/> returning <paramref name="result"/>.
		/// </summary>
		/// <param name="result"></param>
		/// <returns></returns>
		public static Task<PreconditionResult> AsTask(this PreconditionResult result)
			=> Task.FromResult(result);

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
		/// Creates a <see cref="PreconditionResult"/> from the exist status of an object.
		/// </summary>
		/// <param name="precondition"></param>
		/// <param name="exists"></param>
		/// <param name="value"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public static PreconditionResult FromExistence(
			this IExistenceParameterPrecondition precondition,
			bool exists,
			object value,
			string type)
		{
			if (precondition.Status == ExistenceStatus.MustNotExist && exists)
			{
				var error = $"`{value}` already exists as a {type}.";
				return PreconditionResult.FromError(error);
			}
			else if (precondition.Status == ExistenceStatus.MustExist && !exists)
			{
				var error = $"`{value}` does not exist as a {type}.";
				return PreconditionResult.FromError(error);
			}
			return SuccessInstance;
		}

		/// <summary>
		/// Creates a <see cref="InvalidInvokingUserPreconditionResult"/>.
		/// </summary>
		/// <param name="_"></param>
		/// <returns></returns>
		public static PreconditionResult FromInvalidInvoker(
			this Attribute _)
			=> InvalidInvokingUserPreconditionResult.Instance;

		/// <summary>
		/// Creates a <see cref="NotSupportedPreconditionResult"/>.
		/// </summary>
		/// <param name="_"></param>
		/// <param name="value"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public static PreconditionResult FromOnlySupports(
			this Attribute _,
			object value,
			Type type)
			=> new NotSupportedPreconditionResult(value, type);

		/// <summary>
		/// Returns <see cref="SuccessInstance"/>.
		/// </summary>
		/// <param name="_"></param>
		/// <returns></returns>
		public static PreconditionResult FromSuccess(
			this Attribute _)
			=> SuccessInstance;

		/// <summary>
		/// Verifies that the channel can be edited in specific ways.
		/// </summary>
		/// <param name="invoker"></param>
		/// <param name="target"></param>
		/// <param name="permissions"></param>
		/// <returns></returns>
		public static Task<PreconditionResult> ValidateChannel(
			this IGuildUser invoker,
			IGuildChannel target,
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
		public static Task<PreconditionResult> ValidateRole(
			this IGuildUser invoker,
			IRole target)
			=> invoker.ValidateAsync(target, CanModify);

		/// <summary>
		/// Verifies that the user can be edited in specific ways.
		/// </summary>
		/// <param name="invoker"></param>
		/// <param name="target"></param>
		/// <returns></returns>
		public static Task<PreconditionResult> ValidateUser(
			this IGuildUser invoker,
			IGuildUser target)
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

		private static async Task<PreconditionResult> ValidateAsync<T>(
			this IGuildUser invoker,
			T target,
			Func<IGuildUser, T, bool> permissionsCallback)
			where T : ISnowflakeEntity
		{
			if (target == null)
			{
				return new UnableToFindPreconditionResult(typeof(T));
			}

			var bot = await invoker.Guild.GetCurrentUserAsync().CAF();
			if (bot == null)
			{
				throw new InvalidOperationException($"Invalid bot during {typeof(T).Name} validation.");
			}

			foreach (var user in new[] { invoker, bot })
			{
				if (!permissionsCallback(user, target))
				{
					return new LackingPermissionsPreconditionResult(user, target);
				}
			}
			return SuccessInstance;
		}
	}
}