using System;
using System.Collections.Generic;
using Advobot.Classes.Results;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

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
	public delegate bool ValidatePermissions<T>(SocketGuildUser user, T target);
	/// <summary>
	/// Validates something specified on an object.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="user">The user or bot which is currently being checked if they can do this action.</param>
	/// <param name="target">The object to verify this action can be done on.</param>
	/// <returns></returns>
	public delegate ValidatedObjectResult? ValidationRule<T>(SocketGuildUser user, T target);

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
		public static ValidatedObjectResult ValidateUser(this SocketGuildUser invoker,
			SocketGuildUser target,
			params ValidationRule<SocketGuildUser>[] rules)
			=> invoker.Validate(target, CanModify, rules);
		/// <summary>
		/// Verifies that the role can be edited in specific ways.
		/// </summary>
		/// <param name="invoker"></param>
		/// <param name="target"></param>
		/// <param name="rules"></param>
		/// <returns></returns>
		public static ValidatedObjectResult ValidateRole(this SocketGuildUser invoker,
			SocketRole target,
			params ValidationRule<SocketRole>[] rules)
			=> invoker.Validate(target, CanModify, rules);
		/// <summary>
		/// Verifies that the channel can be edited in specific ways.
		/// </summary>
		/// <param name="invoker"></param>
		/// <param name="target"></param>
		/// <param name="permissions"></param>
		/// <param name="rules"></param>
		/// <returns></returns>
		public static ValidatedObjectResult ValidateChannel(this SocketGuildUser invoker,
			SocketGuildChannel target,
			IEnumerable<ChannelPermission> permissions,
			params ValidationRule<SocketGuildChannel>[] rules)
		{
			return invoker.Validate(target, (x, y) =>
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
		private static ValidatedObjectResult Validate<T>(this SocketGuildUser invoker,
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
			if (!(invoker.Guild.CurrentUser is SocketGuildUser bot))
			{
				throw new InvalidOperationException($"Invalid bot during {typeof(T).Name} validation.");
			}

			foreach (var user in new[] { invoker, bot })
			{
				if (!permissionsCallback(user, target))
				{
					return ValidatedObjectResult.FromUnableToModify(user, target);
				}
				foreach (var rule in rules)
				{
					if (rule.Invoke(user, target) is ValidatedObjectResult r && !r.IsSuccess)
					{
						return r;
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
		public static ValidatedObjectResult? MovingUserFromVoiceChannel(SocketGuildUser user, SocketGuildUser target)
		{
			if (!(target?.VoiceChannel is SocketVoiceChannel voiceChannel))
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
		public static ValidatedObjectResult? RoleIsNotEveryone(SocketGuildUser user, SocketRole target)
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
		public static ValidatedObjectResult? RoleIsNotManaged(SocketGuildUser _, SocketRole target)
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
		public static ValidatedObjectResult? RoleIsNotMentionable(SocketGuildUser _, SocketRole target)
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
		public static ValidatedObjectResult? ChannelCanBeReordered(SocketGuildUser user, SocketGuildChannel target)
		{
			if (!user.GuildPermissions.ManageChannels)
			{
				return ValidatedObjectResult.FromUnableToModify(user, target);
			}
			return ValidatedObjectResult.FromSuccess(target);
		}

		/// <summary>
		/// Returns true if the invoking user's position is greater than the target user's position or if both users are the bot.
		/// </summary>
		/// <param name="invoker"></param>
		/// <param name="target"></param>
		/// <returns></returns>
		public static bool CanModify(this SocketGuildUser invoker, SocketGuildUser target)
			=> (target.Id == invoker.Id && target.Id == target.Guild.CurrentUser.Id) || invoker.Hierarchy > target.Hierarchy;
		/// <summary>
		/// Returns true if the invoking user's position is greater than the target user's position.
		/// </summary>
		/// <param name="invoker"></param>
		/// <param name="target"></param>
		/// <returns></returns>
		public static bool CanModify(this SocketGuildUser invoker, SocketRole target)
			=> invoker.Hierarchy > target.Position;
	}
}