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
						$"You are unable to make the given changes to the user: `{DiscordObjectFormatting.FormatDiscordObject(target)}`.");
				}
				if (!bot.CanModify(target, check))
				{
					return new VerifiedObjectResult(target, CommandError.UnmetPrecondition,
						$"I am unable to make the given changes to the user: `{DiscordObjectFormatting.FormatDiscordObject(target)}`.");
				}
			}
			return new VerifiedObjectResult(target, null, null);
		}
		/// <summary>
		/// Returns the position the user has in the guild hierarchy.
		/// </summary>
		/// <param name="user"></param>
		/// <returns></returns>
		public static int GetPosition(this IGuildUser user)
		{
			return user is SocketGuildUser socket ? socket.Hierarchy : -1;
		}
		/// <summary>
		/// Returns true if the invoking user's position is greater than the target user's position.
		/// </summary>
		/// <param name="invokingUser"></param>
		/// <param name="target"></param>
		/// <returns></returns>
		public static bool CanModify(this IGuildUser invokingUser, IGuildUser target)
		{
			return (target.Id == invokingUser.Id && target.Id.ToString() == Config.Configuration[Config.ConfigDict.ConfigKey.BotId])
				|| invokingUser.GetPosition() > target.GetPosition();
		}
		/// <summary>
		/// Returns true if the user can edit the user in the specified way.
		/// </summary>
		/// <param name="invokingUser"></param>
		/// <param name="target"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public static bool CanModify(this IGuildUser invokingUser, IGuildUser target, ObjectVerification type)
		{
			switch (type)
			{
				case ObjectVerification.CanBeMovedFromChannel:
					return invokingUser.CanModify(target?.VoiceChannel, ObjectVerification.CanMoveUsers);
				case ObjectVerification.CanBeEdited:
					return invokingUser.CanModify(target);
			}
			return true;
		}
		/// <summary>
		/// Returns true if the user can edit the channel in the specified way.
		/// </summary>
		/// <param name="invokingUser"></param>
		/// <param name="target"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public static bool CanModify(this IGuildUser invokingUser, IGuildChannel target, ObjectVerification type)
		{
			var guildPerms = invokingUser?.GuildPermissions ?? default;
			if (guildPerms.Has(GuildPermission.Administrator))
			{
				return true;
			}

			var channelPerms = invokingUser?.GetPermissions(target) ?? default;
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
		/// <param name="invokingUser"></param>
		/// <param name="target"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public static bool CanModify(this IGuildUser invokingUser, IRole target, ObjectVerification type)
		{
			switch (type)
			{
				case ObjectVerification.CanBeEdited:
					return target?.Position < invokingUser.GetPosition();
			}
			return true;
		}
		/// <summary>
		/// Changes the user's nickname then says the supplied reason in the audit log.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="newNickname"></param>
		/// <param name="reason"></param>
		/// <returns></returns>
		public static async Task ChangeNicknameAsync(IGuildUser user, string newNickname, ModerationReason reason)
		{
			await user.ModifyAsync(x => x.Nickname = newNickname ?? user.Username, reason.CreateRequestOptions()).CAF();
		}
		/// <summary>
		/// Moves the user to the supplied channel then says the supplied reason in the audit log.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="channel"></param>
		/// <param name="reason"></param>
		/// <returns></returns>
		public static async Task MoveUserAsync(IGuildUser user, IVoiceChannel channel, ModerationReason reason)
		{
			await user.ModifyAsync(x => x.Channel = Optional.Create(channel), reason.CreateRequestOptions()).CAF();
		}
	}
}