using Advobot.Classes;
using Advobot.Enums;
using Advobot.Interfaces;
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
		public static ReturnedObject<IGuildUser> VerifyUserMeetsRequirements(ICommandContext context, IGuildUser target, UserVerification[] checkingTypes)
		{
			if (target == null)
			{
				return new ReturnedObject<IGuildUser>(target, FailureReason.TooFew);
			}

			var invokingUser = context.User as IGuildUser;
			var bot = GetBot(context.Guild);
			foreach (var type in checkingTypes)
			{
				if (!invokingUser.GetIfUserCanDoActionOnUser(target, type))
				{
					return new ReturnedObject<IGuildUser>(target, FailureReason.UserInability);
				}
				else if (!bot.GetIfUserCanDoActionOnUser(target, type))
				{
					return new ReturnedObject<IGuildUser>(target, FailureReason.BotInability);
				}
			}

			return new ReturnedObject<IGuildUser>(target, FailureReason.NotFailure);
		}
		public static IGuildUser GetGuildUser(IGuild guild, ulong ID)
		{
			return (guild as SocketGuild).GetUser(ID);
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