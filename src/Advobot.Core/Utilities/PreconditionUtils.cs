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
		/// Returns true if the invoking user's position is greater than the target user's position or if both users are the bot.
		/// </summary>
		/// <param name="invoker"></param>
		/// <param name="bot"></param>
		/// <param name="t"></param>
		/// <returns></returns>
		public static bool CanModify(this IGuildUser invoker, ulong bot, IGuildUser t)
			=> (invoker.Id == t.Id && t.Id == bot) || invoker.GetHierarchy() > t.GetHierarchy();

		/// <summary>
		/// Returns true if the invoking user's position is greater than the target user's position.
		/// </summary>
		/// <param name="invoker"></param>
		/// <param name="t"></param>
		/// <returns></returns>
		public static bool CanModify(this IGuildUser invoker, IRole t)
			=> invoker.GetHierarchy() > t.Position;

		/// <summary>
		/// Creates <see cref="PreconditionResult.FromError(string)"/>.
		/// </summary>
		/// <param name="error"></param>
		/// <returns></returns>
		public static PreconditionResult FromError(string error)
			=> PreconditionResult.FromError(error);

		/// <summary>
		/// Acts as <see cref="FromError(string)"/> but async.
		/// </summary>
		/// <param name="error"></param>
		/// <returns></returns>
		public static Task<PreconditionResult> FromErrorAsync(string error)
			=> Task.FromResult(FromError(error));

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
			=> FromError("Invalid invoker.");

		/// <summary>
		/// Acts as <see cref="FromInvalidInvoker"/> but async.
		/// </summary>
		/// <returns></returns>
		public static Task<PreconditionResult> FromInvalidInvokerAsync()
			=> Task.FromResult(FromInvalidInvoker());

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
			return FromError($"{t} only supports {s}.");
		}

		/// <summary>
		/// Acts as <see cref="FromOnlySupports(Attribute, Type[])"/> but async.
		/// </summary>
		/// <param name="attr"></param>
		/// <param name="supported"></param>
		/// <returns></returns>
		public static Task<PreconditionResult> FromOnlySupportsAsync(
			this Attribute attr,
			params Type[] supported)
			=> Task.FromResult(FromOnlySupports(attr, supported));

		/// <summary>
		/// Creates <see cref="PreconditionResult.FromSuccess"/>.
		/// </summary>
		/// <returns></returns>
		public static PreconditionResult FromSuccess()
			=> PreconditionResult.FromSuccess();

		/// <summary>
		/// Acts as <see cref="FromSuccess()"/> but async.
		/// </summary>
		/// <returns></returns>
		public static Task<PreconditionResult> FromSuccessAsync()
			=> Task.FromResult(FromSuccess());

		/// <summary>
		/// Creates a <see cref="PreconditionResult"/> for someone being unable to modify something.
		/// </summary>
		/// <param name="bot"></param>
		/// <param name="invoker"></param>
		/// <param name="target"></param>
		/// <returns></returns>
		public static PreconditionResult FromUnableToModify(
			IGuildUser bot,
			IGuildUser invoker,
			ISnowflakeEntity target)
		{
			var start = invoker.Id == bot.Id ? "I am" : "You are";
			var reason = $"{start} unable to make the given changes to `{target.Format()}`.";
			return PreconditionResult.FromError(reason);
		}

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
			return invoker.ValidateAsync(target, (i, _, t) =>
			{
				if (i.GuildPermissions.Administrator)
				{
					return true;
				}

				var channelPerms = i.GetPermissions(t);
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
			=> invoker.ValidateAsync(target, (i, _, t) => CanModify(i, t));

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
			Func<IGuildUser, ulong, T, bool> permissionsCallback)
			where T : ISnowflakeEntity
		{
			if (target == null)
			{
				return FromError($"Unable to find a matching `{typeof(T).Name}`.");
			}

			var bot = await invoker.Guild.GetCurrentUserAsync().CAF();
			if (bot == null)
			{
				throw new InvalidOperationException($"Invalid bot during {typeof(T).Name} validation.");
			}

			foreach (var user in new[] { invoker, bot })
			{
				if (!permissionsCallback(user, bot.Id, target))
				{
					return FromUnableToModify(bot, user, target);
				}
			}
			return FromSuccess();
		}
	}
}