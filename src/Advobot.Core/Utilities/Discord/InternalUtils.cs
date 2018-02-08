using Advobot.Core.Classes.Results;
using Advobot.Core.Enums;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;

namespace Advobot.Core.Utilities
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
		/// <param name="extraChecks"></param>
		/// <returns></returns>
		internal static VerifiedObjectResult InternalVerify(
			ISnowflakeEntity target,
			ICommandContext context,
			IEnumerable<ObjectVerification> checks,
			Func<ObjectVerification, VerifiedObjectResult?> extraChecks = null)
		{
			if (target == null)
			{
				return new VerifiedObjectResult(null, CommandError.ObjectNotFound, "Unable to find a matching channel.");
			}
			if (!(context.User is SocketGuildUser invokingUser && invokingUser.Guild.CurrentUser is SocketGuildUser bot))
			{
				return new VerifiedObjectResult(target, CommandError.Unsuccessful, "Invalid invoking user or guild or bot.");
			}
			foreach (var check in checks)
			{
				if (!InternalCanModify(invokingUser, target, check))
				{
					return new VerifiedObjectResult(target, CommandError.UnmetPrecondition,
						$"You are unable to make the given changes to the channel: `{target.Format()}`.");
				}
				if (!InternalCanModify(bot, target, check))
				{
					return new VerifiedObjectResult(target, CommandError.UnmetPrecondition,
						$"I am unable to make the given changes to the channel: `{target.Format()}`.");
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
		internal static bool InternalCanModify(IGuildUser invoker, ISnowflakeEntity target, ObjectVerification type)
		{
			switch (target)
			{
				case IGuildUser guildUser:
					switch (type)
					{
						case ObjectVerification.CanBeMovedFromChannel:
							return InternalCanModify(invoker, guildUser?.VoiceChannel, ObjectVerification.CanMoveUsers);
						case ObjectVerification.CanBeEdited:
							return invoker.HasHigherPosition(guildUser);
					}
					return true;
				case IGuildChannel guildChannel:
					var guildPerms = invoker?.GuildPermissions ?? default;
					if (guildPerms.Has(GuildPermission.Administrator))
					{
						return true;
					}
					var channelPerms = invoker?.GetPermissions(guildChannel) ?? default;
					switch (type)
					{
						case ObjectVerification.CanBeRead:
							return channelPerms.ViewChannel;
						case ObjectVerification.CanCreateInstantInvite:
							return channelPerms.ViewChannel && channelPerms.CreateInstantInvite;
						case ObjectVerification.CanBeManaged:
							return channelPerms.ViewChannel && channelPerms.ManageChannel;
						case ObjectVerification.CanModifyPermissions:
							return channelPerms.ViewChannel && channelPerms.ManageChannel && channelPerms.ManageRoles;
						case ObjectVerification.CanBeReordered:
							return channelPerms.ViewChannel && guildPerms.ManageChannels;
						case ObjectVerification.CanDeleteMessages:
							return channelPerms.ViewChannel && channelPerms.ManageMessages;
						case ObjectVerification.CanMoveUsers:
							return channelPerms.MoveMembers;
					}
					return true;
				case IRole guildRole:
					switch (type)
					{
						case ObjectVerification.CanBeEdited:
							return invoker is SocketGuildUser socketInvoker && socketInvoker.Hierarchy > guildRole?.Position;
					}
					return true;
				default:
					throw new ArgumentException("Must be either IGuildUser, IGuildChannel, or IRole.", nameof(target));
			}
		}
	}
}