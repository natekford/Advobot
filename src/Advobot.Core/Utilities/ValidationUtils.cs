using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdvorangesUtils;
using Discord;
using Discord.Commands;

namespace Advobot.Utilities
{
	/// <summary>
	/// Validates the permissions of something.
	/// For users and roles this checks to make sure the user has a higher position.
	/// For channels this checks the user's channel permissions.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="user">The user or bot which is currently being checked if they can do this action.</param>
	/// <param name="target">The object to verify this action can be done on.</param>
	/// <returns></returns>
	public delegate bool ValidatePermissions<T>(IGuildUser user, T target);
	/// <summary>
	/// Validates something specified on an object.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="user">The user or bot which is currently being checked if they can do this action.</param>
	/// <param name="target">The object to verify this action can be done on.</param>
	/// <returns></returns>
	public delegate Task<PreconditionResult> ValidationRule<T>(IGuildUser user, T target);

	/// <summary>
	/// Utilities for validating Discord objects (users, roles, channels).
	/// </summary>
	public static class ValidationUtils
	{
		/// <summary>
		/// Verifies that the user can be edited in specific ways.
		/// </summary>
		/// <param name="invoker"></param>
		/// <param name="target"></param>
		/// <param name="rules"></param>
		/// <returns></returns>
		public static async Task<PreconditionResult> ValidateUser(this IGuildUser invoker,
			IGuildUser target,
			params ValidationRule<IGuildUser>[] rules)
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
		public static Task<PreconditionResult> ValidateRole(this IGuildUser invoker,
			IRole target,
			params ValidationRule<IRole>[] rules)
			=> invoker.ValidateAsync(target, (u, t) => CanModify(u, t), rules);
		/// <summary>
		/// Verifies that the channel can be edited in specific ways.
		/// </summary>
		/// <param name="invoker"></param>
		/// <param name="target"></param>
		/// <param name="permissions"></param>
		/// <param name="rules"></param>
		/// <returns></returns>
		public static Task<PreconditionResult> ValidateChannel(this IGuildUser invoker,
			IGuildChannel target,
			IEnumerable<ChannelPermission> permissions,
			params ValidationRule<IGuildChannel>[] rules)
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
		private static async Task<PreconditionResult> ValidateAsync<T>(this IGuildUser invoker,
			T target,
			ValidatePermissions<T> permissionsCallback,
			params ValidationRule<T>[] rules)
			where T : ISnowflakeEntity
		{
			if (target == null)
			{
				return FromError($"Unable to find a matching `{typeof(T).Name}`.");
			}
			if (rules == null)
			{
				rules = Array.Empty<ValidationRule<T>>();
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
			return FromSuccess();
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
				return FromErrorTask("The user is not in a voice channel.");
			}
			return user.ValidateChannel(voiceChannel, new[] { ChannelPermission.MoveMembers });
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
				return FromErrorTask("The everyone role cannot be used in that way.");
			}
			return FromSuccessTask();
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
				return FromErrorTask("Managed roles cannot be used in that way.");
			}
			return FromSuccessTask();
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
				return FromErrorTask("The role is already mentionable.");
			}
			return FromSuccessTask();
		}
		/// <summary>
		/// Validates if <paramref name="user"/> has <see cref="GuildPermission.ManageChannels"/>.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="target"></param>
		/// <returns></returns>
		public static async Task<PreconditionResult> ChannelCanBeReordered(IGuildUser user, IGuildChannel target)
		{
			if (!user.GuildPermissions.ManageChannels)
			{
				var bot = await user.Guild.GetCurrentUserAsync().CAF();
				return FromUnableToModify(bot, user, target);
			}
			return FromSuccess();
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

		private static PreconditionResult FromError(string reason)
			=> PreconditionResult.FromError(reason);
		private static Task<PreconditionResult> FromErrorTask(string reason)
			=> Task.FromResult(PreconditionResult.FromError(reason));
		private static PreconditionResult FromSuccess()
			=> PreconditionResult.FromSuccess();
		private static Task<PreconditionResult> FromSuccessTask()
			=> Task.FromResult(PreconditionResult.FromSuccess());
		private static PreconditionResult FromUnableToModify(IGuildUser bot, IGuildUser invoker, ISnowflakeEntity target)
		{
			var start = invoker.Id == bot.Id ? "I am" : "You are";
			var reason = $"{start} unable to make the given changes to `{target.Format()}`.";
			return PreconditionResult.FromError(reason);
		}
	}
}