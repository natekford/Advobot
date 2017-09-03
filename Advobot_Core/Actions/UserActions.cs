using Advobot.Enums;
using Advobot.Interfaces;
using Advobot.Structs;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Advobot.Actions
{
	public static class UserActions
	{
		public static ReturnedObject<IGuildUser> GetGuildUser(ICommandContext context, UserVerification[] checkingTypes, bool mentions, string input)
		{
			IGuildUser user = null;
			if (!String.IsNullOrWhiteSpace(input))
			{
				if (ulong.TryParse(input, out ulong userID))
				{
					user = GetGuildUser(context.Guild, userID);
				}
				else if (MentionUtils.TryParseUser(input, out userID))
				{
					user = GetGuildUser(context.Guild, userID);
				}
				else
				{
					var users = (context.Guild as SocketGuild).Users.Where(x => x.Username.CaseInsEquals(input));
					if (users.Count() == 1)
					{
						user = users.First();
					}
					else if (users.Count() > 1)
					{
						return new ReturnedObject<IGuildUser>(user, FailureReason.TooMany);
					}
				}
			}

			if (user == null && mentions)
			{
				var userMentions = context.Message.MentionedUserIds;
				if (userMentions.Count() == 1)
				{
					user = GetGuildUser(context.Guild, userMentions.First());
				}
				else if (userMentions.Count() > 1)
				{
					return new ReturnedObject<IGuildUser>(user, FailureReason.TooMany);
				}
			}

			return GetGuildUser(context, checkingTypes, user);
		}
		public static ReturnedObject<IGuildUser> GetGuildUser(ICommandContext context, UserVerification[] checkingTypes, ulong inputID)
		{
			return GetGuildUser(context, checkingTypes, GetGuildUser(context.Guild, inputID));
		}
		public static ReturnedObject<IGuildUser> GetGuildUser(ICommandContext context, UserVerification[] checkingTypes, IGuildUser user)
		{
			return GetGuildUser(context.Guild, context.User as IGuildUser, checkingTypes, user);
		}
		public static ReturnedObject<T> GetGuildUser<T>(IGuild guild, IGuildUser currUser, UserVerification[] checkingTypes, T user) where T : IGuildUser
		{
			if (user == null)
			{
				return new ReturnedObject<T>(user, FailureReason.TooFew);
			}

			var bot = GetBot(guild);
			foreach (var type in checkingTypes)
			{
				if (!GetIfUserCanDoActionOnUser(currUser, type, user))
				{
					return new ReturnedObject<T>(user, FailureReason.UserInability);
				}
				else if (!GetIfUserCanDoActionOnUser(bot, type, user))
				{
					return new ReturnedObject<T>(user, FailureReason.BotInability);
				}
			}

			return new ReturnedObject<T>(user, FailureReason.NotFailure);
		}
		public static IGuildUser GetGuildUser(IGuild guild, ulong ID)
		{
			return (guild as SocketGuild).GetUser(ID);
		}
		public static bool GetIfUserCanDoActionOnUser(IGuildUser currUser, UserVerification type, IGuildUser targetUser)
		{
			if (targetUser == null || currUser == null)
				return false;

			switch (type)
			{
				case UserVerification.CanBeMovedFromChannel:
				{
					return ChannelActions.GetIfUserCanDoActionOnChannel(targetUser.VoiceChannel, currUser, ChannelVerification.CanMoveUsers);
				}
				case UserVerification.CanBeEdited:
				{
					return targetUser.CanBeModifiedByUser(currUser);
				}
				default:
				{
					return true;
				}
			}
		}

		public static IGuildUser GetBot(IGuild guild)
		{
			return (guild as SocketGuild).CurrentUser;
		}
		public static async Task<IUser> GetGlobalUser(IDiscordClient client, ulong ID)
		{
			return await client.GetUserAsync(ID);
		}
		public static async Task<IUser> GetBotOwner(IDiscordClient client)
		{
			return (await client.GetApplicationInfoAsync()).Owner;
		}

		public static int GetUserPosition(IUser user)
		{
			//Make sure they're a SocketGuildUser
			var tempUser = user as SocketGuildUser;
			if (user == null)
				return -1;

			return tempUser.Hierarchy;
		}

		public static async Task<IEnumerable<IGuildUser>> GetUsersTheBotAndUserCanEdit(ICommandContext context)
		{
			return (await context.Guild.GetUsersAsync()).Where(x => x.CanBeModifiedByUser(context.User) && x.CanBeModifiedByUser(GetBot(context.Guild)));
		}

		public static async Task ChangeNickname(IGuildUser user, string newNickname, string reason)
		{
			await user.ModifyAsync(x => x.Nickname = newNickname ?? user.Username, new RequestOptions { AuditLogReason = reason });
		}
		public static async Task NicknameManyUsers(IMyCommandContext context, List<IGuildUser> users, string replace, string reason)
		{
			var msg = await MessageActions.SendChannelMessage(context, $"Attempting to rename `{users.Count}` people.");
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
		public static async Task MoveUser(IGuildUser user, IVoiceChannel channel, string reason)
		{
			await user.ModifyAsync(x => x.Channel = Optional.Create(channel), new RequestOptions { AuditLogReason = reason });
		}
		public static async Task MoveManyUsers(IMyCommandContext context, List<IGuildUser> users, IVoiceChannel outputChannel, string reason)
		{
			var msg = await MessageActions.SendChannelMessage(context, $"Attempting to move `{users.Count}` people.");
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
	}
}