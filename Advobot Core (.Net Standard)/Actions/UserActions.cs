using Advobot.Enums;
using Advobot.Interfaces;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Advobot.Actions
{
	public static class UserActions
	{
		public static FailureReason VerifyUserMeetsRequirements(ICommandContext context, IGuildUser target, UserVerification[] checkingTypes)
		{
			if (target == null)
			{
				return FailureReason.TooFew;
			}

			var invokingUser = context.User as IGuildUser;
			var bot = GetBot(context.Guild);
			foreach (var type in checkingTypes)
			{
				if (!invokingUser.GetIfCanDoActionOnUser(target, type))
				{
					return FailureReason.UserInability;
				}
				else if (!bot.GetIfCanDoActionOnUser(target, type))
				{
					return FailureReason.BotInability;
				}
			}

			return FailureReason.NotFailure;
		}

		public static IGuildUser GetBot(IGuild guild)
		{
			return (guild as SocketGuild).CurrentUser;
		}
		public static async Task<IUser> GetBotOwner(IDiscordClient client)
		{
			return (await client.GetApplicationInfoAsync()).Owner;
		}

		public static async Task<IEnumerable<IGuildUser>> GetUsersTheBotAndUserCanEdit(ICommandContext context)
		{
			return (await context.Guild.GetUsersAsync()).Where(x => x.CanBeModifiedByUser(context.User) && x.CanBeModifiedByUser(GetBot(context.Guild)));
		}

		public static async Task ChangeNickname(this IGuildUser user, string newNickname, string reason)
		{
			await user.ModifyAsync(x => x.Nickname = newNickname ?? user.Username, new RequestOptions { AuditLogReason = reason });
		}
		public static async Task NicknameManyUsers(IMyCommandContext context, List<IGuildUser> users, string replace, string reason)
		{
			var msg = await MessageActions.SendChannelMessage(context.Channel, $"Attempting to rename `{users.Count}` people.");
			for (int i = 0; i < users.Count; ++i)
			{
				if (i % 10 == 0)
				{
					await msg.ModifyAsync(x => x.Content = $"Attempting to rename `{users.Count - i}` people. ETA on completion: `{(int)((users.Count - i) * 1.2)}`.");
				}

				await ChangeNickname(users[i], replace, reason);
			}

			await MessageActions.DeleteMessage(msg);
			await MessageActions.MakeAndDeleteSecondaryMessage(context, $"Successfully renamed `{users.Count}` people.");
		}
		public static async Task MoveUser(this IGuildUser user, IVoiceChannel channel, string reason)
		{
			await user.ModifyAsync(x => x.Channel = Optional.Create(channel), new RequestOptions { AuditLogReason = reason });
		}
		public static async Task MoveManyUsers(IMyCommandContext context, List<IGuildUser> users, IVoiceChannel outputChannel, string reason)
		{
			var msg = await MessageActions.SendChannelMessage(context.Channel, $"Attempting to move `{users.Count}` people.");
			for (int i = 0; i < users.Count; ++i)
			{
				if (i % 10 == 0)
				{
					await msg.ModifyAsync(x => x.Content = $"Attempting to move `{users.Count - i}` people. ETA on completion: `{(int)((users.Count - i) * 1.2)}`.");
				}

				await MoveUser(users[i], outputChannel, reason);
			}

			await MessageActions.DeleteMessage(msg);
			await MessageActions.MakeAndDeleteSecondaryMessage(context, $"Successfully moved `{users.Count}` people.");
		}

		public static int GetPosition(this IUser user)
		{
			if (user is SocketGuildUser socketGuildUser)
			{
				return socketGuildUser.Hierarchy;
			}
			return -1;
		}
		public static bool CanBeModifiedByUser(this IUser targetUser, IUser invokingUser)
		{
			//Allow users to do stuff on themselves.
			if (targetUser.Id == invokingUser.Id && invokingUser.Id.ToString() == Config.Configuration[ConfigKeys.Bot_Id])
			{
				return true;
			}

			var modifierPosition = invokingUser.GetPosition();
			var modifieePosition = targetUser.GetPosition();
			return modifierPosition > modifieePosition;
		}
		public static bool GetIfCanDoActionOnUser(this IGuildUser invokingUser, IGuildUser targetUser, UserVerification type)
		{
			if (targetUser == null || invokingUser == null)
			{
				return false;
			}

			switch (type)
			{
				case UserVerification.CanBeMovedFromChannel:
				{
					return invokingUser.GetIfCanDoActionOnChannel(targetUser.VoiceChannel, ChannelVerification.CanMoveUsers);
				}
				case UserVerification.CanBeEdited:
				{
					return targetUser.CanBeModifiedByUser(invokingUser);
				}
				default:
				{
					return true;
				}
			}
		}
		public static bool GetIfCanDoActionOnChannel(this IGuildUser invokingUser, IGuildChannel target, ChannelVerification type)
		{
			if (target == null || invokingUser == null)
			{
				return false;
			}

			var channelPerms = invokingUser.GetPermissions(target);
			var guildPerms = invokingUser.GuildPermissions;

			//TODO: Make sure this works when the enums are updated.
			switch (type)
			{
				case ChannelVerification.CanBeRead:
				{
					return channelPerms.ReadMessages;
				}
				case ChannelVerification.CanCreateInstantInvite:
				{
					return channelPerms.ReadMessages && channelPerms.CreateInstantInvite;
				}
				case ChannelVerification.CanBeManaged:
				{
					return channelPerms.ReadMessages && channelPerms.ManageChannel;
				}
				case ChannelVerification.CanModifyPermissions:
				{
					return channelPerms.ReadMessages && channelPerms.ManageChannel && channelPerms.ManagePermissions;
				}
				case ChannelVerification.CanBeReordered:
				{
					return channelPerms.ReadMessages && guildPerms.ManageChannels;
				}
				case ChannelVerification.CanDeleteMessages:
				{
					return channelPerms.ReadMessages && channelPerms.ManageMessages;
				}
				case ChannelVerification.CanMoveUsers:
				{
					return channelPerms.MoveMembers;
				}
				default:
				{
					return true;
				}
			}
		}
		public static bool GetIfUserCanDoActionOnRole(this IGuildUser invokingUser, IRole target, RoleVerification type)
		{
			if (target == null || invokingUser == null)
			{
				return false;
			}

			switch (type)
			{
				case RoleVerification.CanBeEdited:
				{
					return target.Position < invokingUser.GetPosition();
				}
				default:
				{
					return true;
				}
			}
		}
	}
}