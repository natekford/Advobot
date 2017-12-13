using Advobot.Core.Utilities.Formatting;
using Advobot.Core.Classes;
using Advobot.Core.Classes.Results;
using Advobot.Core.Enums;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Threading.Tasks;

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
		public static VerifiedObjectResult VerifyUserMeetsRequirements(this IGuildUser target, ICommandContext context,
			IEnumerable<ObjectVerification> checks)
		{
			if (target == null)
			{
				return new VerifiedObjectResult(target, CommandError.ObjectNotFound, "Unable to find a matching user.");
			}

			var invokingUser = context.User as IGuildUser;
			var bot = context.Guild.GetBot();
			foreach (var check in checks)
			{
				if (!invokingUser.GetIfCanDoActionOnUser(target, check))
				{
					return new VerifiedObjectResult(target, CommandError.UnmetPrecondition,
						$"You are unable to make the given changes to the user: `{DiscordObjectFormatting.FormatDiscordObject(target)}`.");
				}
				else if (!bot.GetIfCanDoActionOnUser(target, check))
				{
					return new VerifiedObjectResult(target, CommandError.UnmetPrecondition,
						$"I am unable to make the given changes to the user: `{DiscordObjectFormatting.FormatDiscordObject(target)}`.");
				}
			}

			return new VerifiedObjectResult(target, null, null);
		}

		/// <summary>
		/// Returns the position in the guild the user has.
		/// </summary>
		/// <param name="user"></param>
		/// <returns></returns>
		public static int GetPosition(this IUser user)
			=> user is SocketGuildUser socket ? socket.Hierarchy : -1;
		/// <summary>
		/// Returns true if the invoking user's position is greater than the target user's position.
		/// </summary>
		/// <param name="invokingUser"></param>
		/// <param name="target"></param>
		/// <returns></returns>
		public static bool GetIfCanModifyUser(this IUser invokingUser, IUser target)
			=> (target.Id == invokingUser.Id && target.Id.ToString() == Config.Configuration[ConfigKey.BotId])
			|| invokingUser.GetPosition() > target.GetPosition();
		/// <summary>
		/// Returns true if the user can edit the user in the specified way.
		/// </summary>
		/// <param name="invokingUser"></param>
		/// <param name="target"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public static bool GetIfCanDoActionOnUser(this IGuildUser invokingUser, IGuildUser target, ObjectVerification type)
		{
			if (target == null || invokingUser == null)
			{
				return false;
			}

			switch (type)
			{
				case ObjectVerification.CanBeMovedFromChannel:
				{
					return GetIfCanDoActionOnChannel(invokingUser, target.VoiceChannel, ObjectVerification.CanMoveUsers);
				}
				case ObjectVerification.CanBeEdited:
				{
					return GetIfCanModifyUser(invokingUser, target);
				}
				default:
				{
					return true;
				}
			}
		}
		/// <summary>
		/// Returns true if the user can edit the channel in the specified way.
		/// </summary>
		/// <param name="invokingUser"></param>
		/// <param name="target"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public static bool GetIfCanDoActionOnChannel(this IGuildUser invokingUser, IGuildChannel target, ObjectVerification type)
		{
			if (target == null || invokingUser == null)
			{
				return false;
			}

			var channelPerms = invokingUser.GetPermissions(target);
			var guildPerms = invokingUser.GuildPermissions;
			switch (type)
			{
				case ObjectVerification.CanBeRead:
				{
					return channelPerms.ReadMessages;
				}
				case ObjectVerification.CanCreateInstantInvite:
				{
					return channelPerms.ReadMessages && channelPerms.CreateInstantInvite;
				}
				case ObjectVerification.CanBeManaged:
				{
					return channelPerms.ReadMessages && channelPerms.ManageChannel;
				}
				case ObjectVerification.CanModifyPermissions:
				{
					return channelPerms.ReadMessages && channelPerms.ManageChannel && channelPerms.ManageRoles;
				}
				case ObjectVerification.CanBeReordered:
				{
					return channelPerms.ReadMessages && guildPerms.ManageChannels;
				}
				case ObjectVerification.CanDeleteMessages:
				{
					return channelPerms.ReadMessages && channelPerms.ManageMessages;
				}
				case ObjectVerification.CanMoveUsers:
				{
					return channelPerms.MoveMembers;
				}
				default:
				{
					return true;
				}
			}
		}
		/// <summary>
		/// Returns true if the user can edit the role in the specified way.
		/// </summary>
		/// <param name="invokingUser"></param>
		/// <param name="target"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public static bool GetIfUserCanDoActionOnRole(this IGuildUser invokingUser, IRole target, ObjectVerification type)
		{
			if (target == null || invokingUser == null)
			{
				return false;
			}

			switch (type)
			{
				case ObjectVerification.CanBeEdited:
				{
					return target.Position < invokingUser.GetPosition();
				}
				default:
				{
					return true;
				}
			}
		}

		/// <summary>
		/// Changes the user's nickname then says the supplied reason in the audit log.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="newNickname"></param>
		/// <param name="reason"></param>
		/// <returns></returns>
		public static async Task ChangeNicknameAsync(IGuildUser user, string newNickname, ModerationReason reason)
			=> await user.ModifyAsync(x => x.Nickname = newNickname ?? user.Username, reason.CreateRequestOptions()).CAF();
		/// <summary>
		/// Moves the user to the supplied channel then says the supplied reason in the audit log.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="channel"></param>
		/// <param name="reason"></param>
		/// <returns></returns>
		public static async Task MoveUserAsync(IGuildUser user, IVoiceChannel channel, ModerationReason reason)
			=> await user.ModifyAsync(x => x.Channel = Optional.Create(channel), reason.CreateRequestOptions()).CAF();
	}
}