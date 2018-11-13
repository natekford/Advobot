using System;
using System.Collections.Generic;
using System.Linq;
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
	public delegate VerifiedObjectResult? ValidationRule<T>(SocketGuildUser user, T target);

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
		/// <param name="extra"></param>
		/// <returns></returns>
		public static VerifiedObjectResult ValidateUser(
			this SocketGuildUser invoker,
			SocketGuildUser target,
			params ValidationRule<SocketGuildUser>[] extra)
			=> invoker.Validate(target, CanModify, extra);
		/// <summary>
		/// Verifies that the role can be edited in specific ways.
		/// </summary>
		/// <param name="invoker"></param>
		/// <param name="target"></param>
		/// <param name="extra"></param>
		/// <returns></returns>
		public static VerifiedObjectResult ValidateRole(
			this SocketGuildUser invoker,
			SocketRole target,
			params ValidationRule<SocketRole>[] extra)
			=> invoker.Validate(target, CanModify, extra);
		/// <summary>
		/// Verifies that the channel can be edited in specific ways.
		/// </summary>
		/// <param name="invoker"></param>
		/// <param name="target"></param>
		/// <param name="permissions"></param>
		/// <param name="extra"></param>
		/// <returns></returns>
		public static VerifiedObjectResult ValidateChannel(
			this SocketGuildUser invoker,
			SocketGuildChannel target,
			IEnumerable<ChannelPermission> permissions,
			params ValidationRule<SocketGuildChannel>[] extra)
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
			}, extra);
		}
		/// <summary>
		/// Validates a random Discord object.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="invoker"></param>
		/// <param name="target"></param>
		/// <param name="permissionsCallback"></param>
		/// <param name="extraChecks"></param>
		/// <returns></returns>
		private static VerifiedObjectResult Validate<T>(
			this SocketGuildUser invoker,
			T target,
			ValidatePermissions<T> permissionsCallback,
			params ValidationRule<T>[] extraChecks)
			where T : SocketEntity<ulong>, ISnowflakeEntity
		{
			if (!(invoker.Guild.CurrentUser is SocketGuildUser bot))
			{
				throw new InvalidOperationException($"Invalid bot during {typeof(T).Name} validation.");
			}
			if (target == null)
			{
				return VerifiedObjectResult.FromError(CommandError.ObjectNotFound, $"Unable to find a matching `{typeof(T).Name.ToLower()}`.");
			}

			foreach (var user in new[] { invoker, bot })
			{
				if (!permissionsCallback(user, target))
				{
					return VerifiedObjectResult.FromUnableToModify(user, target);
				}
				foreach (var extra in extraChecks ?? Enumerable.Empty<ValidationRule<T>>())
				{
					if (extra.Invoke(user, target) is VerifiedObjectResult extraResult && !extraResult.IsSuccess)
					{
						return extraResult;
					}
				}
			}
			return VerifiedObjectResult.FromSuccess(target);
		}

		/// <summary>
		/// Validates if <paramref name="user"/> can move <paramref name="target"/> from their current voice channel.
		/// Returns an error if <paramref name="target"/> is not in a voice channel.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="target"></param>
		/// <returns></returns>
		public static VerifiedObjectResult? MovingUserFromVoiceChannel(SocketGuildUser user, SocketGuildUser target)
		{
			if (!(target?.VoiceChannel is SocketVoiceChannel voiceChannel))
			{
				return VerifiedObjectResult.FromError(CommandError.UnmetPrecondition, "The user is not in a voice channel.");
			}
			return user.ValidateChannel(voiceChannel, new[] { ChannelPermission.MoveMembers }, null);
		}
		/// <summary>
		/// Validates if <paramref name="target"/> is not the everyone role.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="target"></param>
		/// <returns></returns>
		public static VerifiedObjectResult? RoleIsNotEveryone(SocketGuildUser user, SocketRole target)
		{
			if (user.Guild.EveryoneRole.Id == target.Id)
			{
				return VerifiedObjectResult.FromError(CommandError.UnmetPrecondition, "The everyone role cannot be used in that way.");
			}
			return VerifiedObjectResult.FromSuccess(target);
		}
		/// <summary>
		/// Validates if <paramref name="target"/> is not managed.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="target"></param>
		/// <returns></returns>
		public static VerifiedObjectResult? RoleIsNotManaged(SocketGuildUser user, SocketRole target)
		{
			if (target.IsManaged)
			{
				return VerifiedObjectResult.FromError(CommandError.UnmetPrecondition, "Managed roles cannot be used in that way.");
			}
			return VerifiedObjectResult.FromSuccess(target);
		}
		/// <summary>
		/// Validates if <paramref name="user"/> has <see cref="GuildPermission.ManageChannels"/>.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="target"></param>
		/// <returns></returns>
		public static VerifiedObjectResult? ChannelCanBeReordered(SocketGuildUser user, SocketGuildChannel target)
		{
			if (!user.GuildPermissions.ManageChannels)
			{
				return VerifiedObjectResult.FromUnableToModify(user, target);
			}
			return VerifiedObjectResult.FromSuccess(target);
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