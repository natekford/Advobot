using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Advobot.Attributes.ParameterPreconditions;

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
		/// <param name="t"></param>
		/// <returns></returns>
		public static bool CanModify(this IGuildUser invoker, IGuildUser t)
			=> invoker.GetHierarchy() > t.GetHierarchy();

		/// <summary>
		/// Returns true if the invoking user's position is greater than the target user's position.
		/// </summary>
		/// <param name="invoker"></param>
		/// <param name="t"></param>
		/// <returns></returns>
		public static bool CanModify(this IGuildUser invoker, IRole t)
			=> invoker.GetHierarchy() > t.Position;

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
			return PreconditionResult.FromSuccess();
		}

		/// <summary>
		/// Creates <see cref="PreconditionResult.FromError(string)"/> but with a message saying the invoker was invalid.
		/// </summary>
		/// <returns></returns>
		public static PreconditionResult FromInvalidInvoker()
			=> PreconditionResult.FromError("Invalid invoking user.");

		/// <summary>
		/// Creates an <see cref="PreconditionResult.FromError(string)"/> saying <paramref name="attr"/> only supports specific types.
		/// </summary>
		/// <param name="attr"></param>
		/// <param name="supported"></param>
		/// <returns></returns>
		public static PreconditionResult FromOnlySupports(
			this Attribute attr,
			params Type[] supported)
		{
			var t = attr.GetType().Name;
			var s = supported.Select(x => x.Name).Join(", ");
			return PreconditionResult.FromError($"{t} only supports {s}.");
		}

		/// <summary>
		/// Gets the subject of a sentence
		/// </summary>
		/// <param name="invoker"></param>
		/// <param name="botId"></param>
		/// <returns></returns>
		public static string GetSubject(this IGuildUser invoker, ulong botId)
			=> invoker.Id == botId ? "I" : "You";

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
				if (i.GuildPermissions.Administrator)
				{
					return true;
				}

				var channelPerms = i.GetPermissions(t);
				foreach (var permission in permissions)
				{
					//Can't do anything if the channel can't be seen
					var temp = permission | ChannelPermission.ViewChannel;
					if (!channelPerms.Has(temp))
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

		private static int GetHierarchy(this IGuildUser u)
		{
			if (u.Guild.OwnerId == u.Id)
			{
				return int.MaxValue;
			}
			return u.RoleIds.Max(x => u.Guild.GetRole(x).Position);
		}

		private static async Task<PreconditionResult> ValidateAsync<T>(
			this IGuildUser invoker,
			T target,
			Func<IGuildUser, T, bool> permissionsCallback)
			where T : ISnowflakeEntity
		{
			if (target == null)
			{
				return PreconditionResult.FromError($"Unable to find a matching `{typeof(T).Name}`.");
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
					var subject = user.GetSubject(bot.Id);
					var error = $"{subject} do not have the ability to modify `{target.Format()}`.";
					return PreconditionResult.FromError(error);
				}
			}
			return PreconditionResult.FromSuccess();
		}
	}
}