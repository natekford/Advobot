using System;
using System.Collections.Generic;
using Advobot.Classes;
using Advobot.Classes.Results;
using Advobot.Enums;
using Advobot.Interfaces;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Advobot.Utilities
{
	/// <summary>
	/// Utilities intended to be only used internally.
	/// </summary>
	internal static class InternalUtils
	{
		/// <summary>
		/// Generic verify restricted to IGuildUser, IGuildChannel, and IRole.
		/// </summary>
		/// <param name="target"></param>
		/// <param name="context"></param>
		/// <param name="checks"></param>
		/// <param name="type"></param>
		/// <param name="extraChecks"></param>
		/// <returns></returns>
		internal static VerifiedObjectResult InternalVerify(
			SocketEntity<ulong> target,
			SocketCommandContext context,
			IEnumerable<Verif> checks,
			string type,
			Func<Verif, VerifiedObjectResult?> extraChecks = null)
		{
			if (target == null)
			{
				return new VerifiedObjectResult(null, CommandError.ObjectNotFound, $"Unable to find a matching {type.ToLower()}.");
			}
			if (!(context.User is SocketGuildUser invokingUser && invokingUser.Guild.CurrentUser is SocketGuildUser bot))
			{
				return new VerifiedObjectResult(target, CommandError.Unsuccessful, "Invalid invoking user or bot.");
			}
			foreach (var check in checks)
			{
				if (!InternalCanModify(invokingUser, target, check))
				{
					return new VerifiedObjectResult(target, CommandError.UnmetPrecondition,
						$"You are unable to make the given changes to the {type.ToLower()}: `{target.Format()}`.");
				}
				if (!InternalCanModify(bot, target, check))
				{
					return new VerifiedObjectResult(target, CommandError.UnmetPrecondition,
						$"I am unable to make the given changes to the {type.ToLower()}: `{target.Format()}`.");
				}
				if (extraChecks?.Invoke(check) is VerifiedObjectResult result)
				{
					return result;
				}
			}
			return new VerifiedObjectResult(target, null, null);
		}
		/// <summary>
		/// Generic can modify restricted to IGuildUser, IGuildChannel, and IRole.
		/// </summary>
		/// <param name="invoker"></param>
		/// <param name="target"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		internal static bool InternalCanModify(SocketGuildUser invoker, SocketEntity<ulong> target, Verif type)
		{
			switch (target)
			{
				case SocketGuildUser user:
					switch (type)
					{
						case Verif.CanBeMovedFromChannel:
							return InternalCanModify(invoker, user?.VoiceChannel, Verif.CanMoveUsers);
						case Verif.CanBeEdited:
							return invoker.HasHigherPosition(user);
					}
					return true;
				case SocketGuildChannel channel:
					var guildPerms = invoker?.GuildPermissions ?? default;
					if (guildPerms.Administrator)
					{
						return true;
					}
#warning rework this into simply supplying the permissions to check
					var channelPerms = invoker?.GetPermissions(channel) ?? default;
					switch (type)
					{
						case Verif.CanBeViewed:
							return channelPerms.ViewChannel;
						case Verif.CanCreateInstantInvite:
							return channelPerms.ViewChannel && channelPerms.CreateInstantInvite;
						case Verif.CanBeManaged:
							return channelPerms.ViewChannel && channelPerms.ManageChannel;
						case Verif.CanModifyPermissions:
							return channelPerms.ViewChannel && channelPerms.ManageChannel && channelPerms.ManageRoles;
						case Verif.CanBeReordered:
							return channelPerms.ViewChannel && guildPerms.ManageChannels;
						case Verif.CanDeleteMessages:
							return channelPerms.ViewChannel && channelPerms.ManageMessages;
						case Verif.CanMoveUsers:
							return channelPerms.MoveMembers;
						case Verif.CanManageWebhooks:
							return channelPerms.ManageWebhooks;
					}
					return true;
				case SocketRole role:
					switch (type)
					{
						case Verif.CanBeEdited:
							return invoker is SocketGuildUser socketInvoker && socketInvoker.Hierarchy > role.Position;
					}
					return true;
				default:
					throw new ArgumentException("Must be either IGuildUser, IGuildChannel, or IRole.", nameof(target));
			}
		}
		/// <summary>
		/// Checks whether to use the bot prefix, or the guild settings prefix.
		/// </summary>
		/// <param name="b"></param>
		/// <param name="g"></param>
		/// <returns></returns>
		internal static string InternalGetPrefix(this IBotSettings b, IGuildSettings g)
			=> string.IsNullOrWhiteSpace(g?.Prefix) ? b.Prefix : g?.Prefix;
		/// <summary>
		/// Makes sure the context can be cast to <see cref="AdvobotCommandContext"/> and the user is a <see cref="SocketGuildUser"/>,
		/// otherwise throws an exception which is clearer than an <see cref="InvalidCastException"/>.
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
		internal static (AdvobotCommandContext Context, SocketGuildUser Invoker) InternalCastContext(this ICommandContext context)
		{
			if (!(context is AdvobotCommandContext aContext))
			{
				throw new ArgumentException($"Invalid context provided, must be {nameof(AdvobotCommandContext)}.");
			}
			if (!(context.User is SocketGuildUser user))
			{
				throw new ArgumentException("Unable to get the invoking user as a guild user.");
			}
			return (aContext, user);
		}
	}
}