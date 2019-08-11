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
	/// Validates something specified on an object.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="user">The user or bot which is currently being checked if they can do this action.</param>
	/// <param name="target">The object to verify this action can be done on.</param>
	/// <returns></returns>
	public delegate Task<PreconditionResult> Precondition<T>(IGuildUser user, T target);

	/// <summary>
	/// Utilities for validating Discord objects (users, roles, channels).
	/// </summary>
	public static class PreconditionUtils
	{
		/// <summary>
		/// Verifies that the user can be edited in specific ways.
		/// </summary>
		/// <param name="invoker"></param>
		/// <param name="target"></param>
		/// <param name="rules"></param>
		/// <returns></returns>
		public static async Task<PreconditionResult> ValidateUser(
			this IGuildUser invoker,
			IGuildUser target,
			IEnumerable<Precondition<IGuildUser>> rules)
		{
			var bot = await invoker.Guild.GetCurrentUserAsync().CAF();
			return await invoker.ValidateAsync(target, (u, t) => CanModify(u, bot.Id, t), rules).CAF();
		}
		/// <summary>
		/// Verifies that the role can be edited in specific ways.
		/// </summary>
		/// <param name="invoker"></param>
		/// <param name="target"></param>
		/// <param name="rules"></param>
		/// <returns></returns>
		public static Task<PreconditionResult> ValidateRole(
			this IGuildUser invoker,
			IRole target,
			IEnumerable<Precondition<IRole>> rules)
			=> invoker.ValidateAsync(target, (u, t) => CanModify(u, t), rules);
		/// <summary>
		/// Verifies that the channel can be edited in specific ways.
		/// </summary>
		/// <param name="invoker"></param>
		/// <param name="target"></param>
		/// <param name="permissions"></param>
		/// <param name="rules"></param>
		/// <returns></returns>
		public static Task<PreconditionResult> ValidateChannel(
			this IGuildUser invoker,
			IGuildChannel target,
			IEnumerable<ChannelPermission> permissions,
			IEnumerable<Precondition<IGuildChannel>> rules)
		{
			return invoker.ValidateAsync(target, (x, y) =>
			{
				if (x.GuildPermissions.Administrator)
				{
					return true;
				}

				var channelPerms = x.GetPermissions(y);
				foreach (var permission in permissions)
				{
					if (!channelPerms.Has(permission))
					{
						return false;
					}
				}
				return true;
			}, rules);
		}
		/// <summary>
		/// Validates a random Discord object.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="invoker"></param>
		/// <param name="target"></param>
		/// <param name="permissionsCallback"></param>
		/// <param name="rules"></param>
		/// <returns></returns>
		private static async Task<PreconditionResult> ValidateAsync<T>(
			this IGuildUser invoker,
			T target,
			Func<IGuildUser, T, bool> permissionsCallback,
			IEnumerable<Precondition<T>> rules)
			where T : ISnowflakeEntity
		{
			if (target == null)
			{
				return FromError(null, $"Unable to find a matching `{typeof(T).Name}`.");
			}
			if (rules == null)
			{
				rules = Array.Empty<Precondition<T>>();
			}
			if (!(await invoker.Guild.GetCurrentUserAsync().CAF() is IGuildUser bot))
			{
				throw new InvalidOperationException($"Invalid bot during {typeof(T).Name} validation.");
			}

			foreach (var user in new[] { invoker, bot })
			{
				if (!permissionsCallback(user, target))
				{
					return FromUnableToModify(bot, user, target);
				}
				foreach (var rule in rules)
				{
					var validationResult = await rule.Invoke(user, target).CAF();
					if (!validationResult.IsSuccess)
					{
						return validationResult;
					}
				}
			}
			return FromSuccess(null);
		}

		/// <summary>
		/// Validates if <paramref name="user"/> can move <paramref name="target"/> from their current voice channel.
		/// Returns an error if <paramref name="target"/> is not in a voice channel.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="target"></param>
		/// <returns></returns>
		public static Task<PreconditionResult> MovingUserFromVoiceChannel(IGuildUser user, IGuildUser target)
		{
			if (!(target?.VoiceChannel is IVoiceChannel voiceChannel))
			{
				return FromErrorAsync(null, "The user is not in a voice channel.");
			}
			var permissions = new[] { ChannelPermission.MoveMembers };
			var rules = Array.Empty<Precondition<IGuildChannel>>();
			return user.ValidateChannel(voiceChannel, permissions, rules);
		}
		/// <summary>
		/// Validates if <paramref name="target"/> is not the everyone role.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="target"></param>
		/// <returns></returns>
		public static Task<PreconditionResult> RoleIsNotEveryone(IGuildUser user, IRole target)
		{
			if (user.Guild.EveryoneRole.Id == target.Id)
			{
				return FromErrorAsync(null, "The everyone role cannot be used in that way.");
			}
			return FromSuccessAsync(null);
		}
		/// <summary>
		/// Validates if <paramref name="target"/> is not managed.
		/// </summary>
		/// <param name="_"></param>
		/// <param name="target"></param>
		/// <returns></returns>
		public static Task<PreconditionResult> RoleIsNotManaged(IGuildUser _, IRole target)
		{
			if (target.IsManaged)
			{
				return FromErrorAsync(null, "Managed roles cannot be used in that way.");
			}
			return FromSuccessAsync(null);
		}
		/// <summary>
		/// Validates if the target is not mentionable.
		/// </summary>
		/// <param name="_"></param>
		/// <param name="target"></param>
		/// <returns></returns>
		public static Task<PreconditionResult> RoleIsNotMentionable(IGuildUser _, IRole target)
		{
			if (target.IsMentionable)
			{
				return FromErrorAsync(null, "The role is already mentionable.");
			}
			return FromSuccessAsync(null);
		}

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
		private static int GetHierarchy(this IGuildUser u)
			=> u.Guild.OwnerId == u.Id ? int.MaxValue : u.RoleIds.Max(x => u.Guild.GetRole(x).Position);

		/// <summary>
		/// Formats the permissions into a precondition string.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="permissions"></param>
		/// <returns></returns>
		public static string FormatPermissions<T>(this IEnumerable<T> permissions)
			where T : Enum
		{
			return permissions.Select(x =>
			{
				var perms = new List<string>();
				foreach (Enum e in Enum.GetValues(x.GetType()))
				{
					if (x.Equals(e))
					{
						return e.ToString();
					}
					else if (x.HasFlag(e))
					{
						perms.Add(e.ToString());
					}
				}
				return perms.Join(" & ");
			}).Join(" | ");
		}

		/// <summary>
		/// Acts as <see cref="FromSuccess(Attribute)"/> but async.
		/// </summary>
		/// <param name="_"></param>
		/// <returns></returns>
		public static Task<PreconditionResult> FromSuccessAsync(
			this Attribute _)
			=> Task.FromResult(FromSuccess(_));
		/// <summary>
		/// Creates <see cref="PreconditionResult.FromSuccess"/>.
		/// </summary>
		/// <param name="_"></param>
		/// <returns></returns>
		public static PreconditionResult FromSuccess(
			this Attribute _)
			=> PreconditionResult.FromSuccess();
		/// <summary>
		/// Acts as <see cref="FromError(Attribute, string)"/> but async.
		/// </summary>
		/// <param name="_"></param>
		/// <param name="error"></param>
		/// <returns></returns>
		public static Task<PreconditionResult> FromErrorAsync(
			this Attribute _,
			string error)
			=> Task.FromResult(FromError(_, error));
		/// <summary>
		/// Creates <see cref="PreconditionResult.FromError(string)"/>.
		/// </summary>
		/// <param name="_"></param>
		/// <param name="error"></param>
		/// <returns></returns>
		public static PreconditionResult FromError(
			this Attribute _,
			string error)
			=> PreconditionResult.FromError(error);
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
				var error = $"`{value}` is not a {type}.";
				return PreconditionResult.FromError(error);
			}
			return PreconditionResult.FromSuccess();
		}
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
		/// Creates an <see cref="ArgumentException"/> saying <paramref name="attr"/> only supports specific types.
		/// </summary>
		/// <param name="attr"></param>
		/// <param name="supported"></param>
		/// <returns></returns>
		public static ArgumentException OnlySupports(
			this Attribute attr,
			params Type[] supported)
		{
			var t = attr.GetType().Name;
			var s = supported.Select(x => x.Name).Join(", ");
			return new ArgumentException($"{t} only supports {s}.");
		}
	}
}