using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Advobot.Classes.Results;
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
	public delegate Task<ValidatedObjectResult> ValidationRule<T>(IGuildUser user, T target);

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
		public static async Task<ValidatedObjectResult> ValidateUser(this IGuildUser invoker,
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
		public static Task<ValidatedObjectResult> ValidateRole(this IGuildUser invoker,
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
		public static Task<ValidatedObjectResult> ValidateChannel(this IGuildUser invoker,
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
		private static async Task<ValidatedObjectResult> ValidateAsync<T>(this IGuildUser invoker,
			T target,
			ValidatePermissions<T> permissionsCallback,
			params ValidationRule<T>[] rules)
			where T : ISnowflakeEntity
		{
			if (target == null)
			{
				return ValidatedObjectResult.FromError(CommandError.ObjectNotFound, $"Unable to find a matching `{typeof(T).Name}`.");
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
					return ValidatedObjectResult.FromUnableToModify(bot, user, target);
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
			return ValidatedObjectResult.FromSuccess(target);
		}

		/// <summary>
		/// Validates if <paramref name="user"/> can move <paramref name="target"/> from their current voice channel.
		/// Returns an error if <paramref name="target"/> is not in a voice channel.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="target"></param>
		/// <returns></returns>
		public static Task<ValidatedObjectResult> MovingUserFromVoiceChannel(IGuildUser user, IGuildUser target)
		{
			if (!(target?.VoiceChannel is IVoiceChannel voiceChannel))
			{
				return ValidatedObjectResult.FromError(CommandError.UnmetPrecondition, "The user is not in a voice channel.");
			}
			return user.ValidateChannel(voiceChannel, new[] { ChannelPermission.MoveMembers });
		}
		/// <summary>
		/// Validates if <paramref name="target"/> is not the everyone role.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="target"></param>
		/// <returns></returns>
		public static Task<ValidatedObjectResult> RoleIsNotEveryone(IGuildUser user, IRole target)
		{
			if (user.Guild.EveryoneRole.Id == target.Id)
			{
				return ValidatedObjectResult.FromError(CommandError.UnmetPrecondition, "The everyone role cannot be used in that way.");
			}
			return ValidatedObjectResult.FromSuccess(target);
		}
		/// <summary>
		/// Validates if <paramref name="target"/> is not managed.
		/// </summary>
		/// <param name="_"></param>
		/// <param name="target"></param>
		/// <returns></returns>
		public static Task<ValidatedObjectResult> RoleIsNotManaged(IGuildUser _, IRole target)
		{
			if (target.IsManaged)
			{
				return ValidatedObjectResult.FromError(CommandError.UnmetPrecondition, "Managed roles cannot be used in that way.");
			}
			return ValidatedObjectResult.FromSuccess(target);
		}
		/// <summary>
		/// Validates if the target is not mentionable.
		/// </summary>
		/// <param name="_"></param>
		/// <param name="target"></param>
		/// <returns></returns>
		public static Task<ValidatedObjectResult> RoleIsNotMentionable(IGuildUser _, IRole target)
		{
			if (target.IsMentionable)
			{
				return ValidatedObjectResult.FromError(CommandError.UnmetPrecondition, "The role is already mentionable.");
			}
			return ValidatedObjectResult.FromSuccess(target);
		}
		/// <summary>
		/// Validates if <paramref name="user"/> has <see cref="GuildPermission.ManageChannels"/>.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="target"></param>
		/// <returns></returns>
		public static async Task<ValidatedObjectResult> ChannelCanBeReordered(IGuildUser user, IGuildChannel target)
		{
			if (!user.GuildPermissions.ManageChannels)
			{
				var bot = await user.Guild.GetCurrentUserAsync().CAF();
				return ValidatedObjectResult.FromUnableToModify(bot, user, target);
			}
			return ValidatedObjectResult.FromSuccess(target);
		}

		/// <summary>
		/// Returns true if the invoking user's position is greater than the target user's position or if both users are the bot.
		/// </summary>
		/// <param name="invoker"></param>
		/// <param name="botId"></param>
		/// <param name="target"></param>
		/// <returns></returns>
		public static bool CanModify(this IGuildUser invoker, ulong botId, IGuildUser target)
			=> (invoker.Id == target.Id && target.Id == botId)
				|| invoker.GetHierarchy() > target.GetHierarchy();
		/// <summary>
		/// Returns true if the invoking user's position is greater than the target user's position.
		/// </summary>
		/// <param name="invoker"></param>
		/// <param name="target"></param>
		/// <returns></returns>
		public static bool CanModify(this IGuildUser invoker, IRole target)
			=> invoker.GetHierarchy() > target.Position;
		private static int GetHierarchy(this IGuildUser user)
		{
			if (user.Guild.OwnerId == user.Id)
			{
				return int.MaxValue;
			}
			return user.RoleIds.Max(x => user.Guild.GetRole(x).Position);
		}
	}
}