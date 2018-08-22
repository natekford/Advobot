using System;
using System.Collections.Generic;
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
			ISnowflakeEntity target,
			ICommandContext context,
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
		internal static bool InternalCanModify(IGuildUser invoker, ISnowflakeEntity target, Verif type)
		{
			switch (target)
			{
				case IGuildUser user:
					switch (type)
					{
						case Verif.CanBeMovedFromChannel:
							return InternalCanModify(invoker, user?.VoiceChannel, Verif.CanMoveUsers);
						case Verif.CanBeEdited:
							return invoker.HasHigherPosition(user);
					}
					return true;
				case IGuildChannel channel:
					var guildPerms = invoker?.GuildPermissions ?? default;
					if (guildPerms.Administrator)
					{
						return true;
					}
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
				case IRole role:
					switch (type)
					{
						case Verif.CanBeEdited:
							return invoker is SocketGuildUser socketInvoker && socketInvoker.Hierarchy > role?.Position;
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
		{
			return string.IsNullOrWhiteSpace(g?.Prefix) ? b.Prefix : g?.Prefix;
		}
	}
}