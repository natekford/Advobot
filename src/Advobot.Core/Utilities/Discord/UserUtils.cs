using System.Collections.Generic;
using System.Threading.Tasks;
using Advobot.Core.Classes;
using Advobot.Core.Classes.Results;
using Advobot.Core.Enums;
using Advobot.Core.Utilities.Formatting;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Advobot.Core.Utilities
{
	/// <summary>
	/// Actions done on an <see cref="IUser"/>.
	/// </summary>
	public static class UserUtils
	{
		/// <summary>
		/// Verifies that the user can be edited in specific ways.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="target"></param>
		/// <param name="checks"></param>
		/// <returns></returns>
		public static VerifiedObjectResult Verify(this IGuildUser target, ICommandContext context, IEnumerable<ObjectVerification> checks)
		{
			if (!(target is SocketGuildUser))
			{
				return new VerifiedObjectResult(target, CommandError.ObjectNotFound, "Unable to find a matching user.");
			}
			if (!(context.User is SocketGuildUser invokingUser && context.Guild.GetBot() is SocketGuildUser bot))
			{
				return new VerifiedObjectResult(target, CommandError.Unsuccessful, "Invalid invoking user or guild or bot.");
			}

			foreach (var check in checks)
			{
				if (!invokingUser.CanModify(target, check))
				{
					return new VerifiedObjectResult(target, CommandError.UnmetPrecondition,
						$"You are unable to make the given changes to the user: `{target.Format()}`.");
				}
				if (!bot.CanModify(target, check))
				{
					return new VerifiedObjectResult(target, CommandError.UnmetPrecondition,
						$"I am unable to make the given changes to the user: `{target.Format()}`.");
				}
			}
			return new VerifiedObjectResult(target, null, null);
		}
		/// <summary>
		/// Returns true if the invoking user's position is greater than the target user's position.
		/// </summary>
		/// <param name="invoker"></param>
		/// <param name="target"></param>
		/// <returns></returns>
		public static bool HasHigherPosition(this IGuildUser invoker, IGuildUser target)
		{
			//User is the bot
			if (target.Id == invoker.Id && target.Id.ToString() == Config.Configuration[Config.ConfigDict.ConfigKey.BotId])
			{
				return true;
			}
			var invokerPosition = invoker is SocketGuildUser socketInvoker ? socketInvoker.Hierarchy : -1;
			var targetPosition = target is SocketGuildUser socketTarget ? socketTarget.Hierarchy : -1;
			return invokerPosition > targetPosition;
		}
		/// <summary>
		/// Returns true if the user can edit the user in the specified way.
		/// </summary>
		/// <param name="invoker"></param>
		/// <param name="target"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public static bool CanModify(this IGuildUser invoker, IGuildUser target, ObjectVerification type)
		{
			switch (type)
			{
				case ObjectVerification.CanBeMovedFromChannel:
					return invoker.CanModify(target?.VoiceChannel, ObjectVerification.CanMoveUsers);
				case ObjectVerification.CanBeEdited:
					return invoker.HasHigherPosition(target);
			}
			return true;
		}
		/// <summary>
		/// Returns true if the user can edit the channel in the specified way.
		/// </summary>
		/// <param name="invoker"></param>
		/// <param name="target"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public static bool CanModify(this IGuildUser invoker, IGuildChannel target, ObjectVerification type)
		{
			var guildPerms = invoker?.GuildPermissions ?? default;
			if (guildPerms.Has(GuildPermission.Administrator))
			{
				return true;
			}

			var channelPerms = invoker?.GetPermissions(target) ?? default;
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
		}
		/// <summary>
		/// Returns true if the user can edit the role in the specified way.
		/// </summary>
		/// <param name="invoker"></param>
		/// <param name="target"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public static bool CanModify(this IGuildUser invoker, IRole target, ObjectVerification type)
		{
			switch (type)
			{
				case ObjectVerification.CanBeEdited:
					return invoker is SocketGuildUser socketInvoker && socketInvoker.Hierarchy > target?.Position;
			}
			return true;
		}
	}
}