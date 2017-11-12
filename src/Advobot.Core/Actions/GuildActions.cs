﻿using Advobot.Core.Classes;
using Discord;
using Discord.WebSocket;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Advobot.Core.Actions
{
	public static class GuildActions
	{
		/// <summary>
		/// Attempts to get a guild from a message.
		/// </summary>
		/// <param name="message"></param>
		/// <returns></returns>
		public static IGuild GetGuild(this IMessage message) => (message?.Channel as IGuildChannel)?.Guild;
		/// <summary>
		/// Attempts to get a guild from a user.
		/// </summary>
		/// <param name="user"></param>
		/// <returns></returns>
		public static IGuild GetGuild(this IUser user) => (user as IGuildUser)?.Guild;
		/// <summary>
		/// Attempts to get a guild from a channel.
		/// </summary>
		/// <param name="channel"></param>
		/// <returns></returns>
		public static IGuild GetGuild(this IChannel channel) => (channel as IGuildChannel)?.Guild;
		/// <summary>
		/// Attempts to get a guild from a role.
		/// </summary>
		/// <param name="role"></param>
		/// <returns></returns>
		public static IGuild GetGuild(this IRole role) => role?.Guild;
		/// <summary>
		/// Returns the bot's guild user.
		/// </summary>
		/// <param name="guild"></param>
		/// <returns></returns>
		public static IGuildUser GetBot(this IGuild guild) => (guild as SocketGuild).CurrentUser;
		/// <summary>
		/// Returns every user that has a non null join time in order from least to greatest.
		/// </summary>
		/// <param name="guild"></param>
		/// <returns></returns>
		public static async Task<IReadOnlyList<IGuildUser>> GetUsersAndOrderByJoinAsync(this IGuild guild)
		{
			var users = (await guild.GetUsersAsync().CAF()).Where(x => x.JoinedAt != null).OrderBy(x => x.JoinedAt.Value.Ticks);
			return users.ToList().AsReadOnly();
		}
		/// <summary>
		/// Returns every user that can be modified by both <paramref name="invokingUser"/> and the bot.
		/// </summary>
		/// <param name="guild"></param>
		/// <param name="invokingUser"></param>
		/// <returns></returns>
		public static async Task<IReadOnlyList<IGuildUser>> GetUsersTheBotAndUserCanEditAsync(this IGuild guild, IUser invokingUser)
		{
			var bot = guild.GetBot();
			var users = (await guild.GetUsersAsync().CAF()).Where(x => invokingUser.GetIfCanModifyUser(x) && bot.GetIfCanModifyUser(x));
			return users.ToList().AsReadOnly();
		}
		/// <summary>
		/// Returns true if the guild has any global emotes.
		/// </summary>
		/// <param name="guild"></param>
		/// <returns></returns>
		public static bool HasGlobalEmotes(this IGuild guild) => guild.Emotes.Any(x => x.IsManaged && x.RequireColons);

		/// <summary>
		/// Prunes users who haven't been active in a certain amount of days and says the supplied reason in the audit log.
		/// </summary>
		/// <param name="guild"></param>
		/// <param name="days"></param>
		/// <param name="simulate"></param>
		/// <param name="reason"></param>
		/// <returns></returns>
		public static async Task<int> PruneUsersAsync(IGuild guild, int days, bool simulate, ModerationReason reason)
			=> await guild.PruneUsersAsync(days, simulate, reason.CreateRequestOptions()).CAF();
		/// <summary>
		/// Changes the guild's name and says the supplied reason in the audit log.
		/// </summary>
		/// <param name="guild"></param>
		/// <param name="name"></param>
		/// <param name="reason"></param>
		/// <returns></returns>
		public static async Task ModifyGuildNameAsync(IGuild guild, string name, ModerationReason reason)
			=> await guild.ModifyAsync(x => x.Name = name, reason.CreateRequestOptions()).CAF();
		/// <summary>
		/// Changes the guild's region and says the supplied reason in the audit log.
		/// </summary>
		/// <param name="guild"></param>
		/// <param name="region"></param>
		/// <param name="reason"></param>
		/// <returns></returns>
		public static async Task ModifyGuildRegionAsync(IGuild guild, string region, ModerationReason reason)
			=> await guild.ModifyAsync(x => x.RegionId = region, reason.CreateRequestOptions()).CAF();
		/// <summary>
		/// Changes the guild's afk time and says the supplied reason in the audit log.
		/// </summary>
		/// <param name="guild"></param>
		/// <param name="time"></param>
		/// <param name="reason"></param>
		/// <returns></returns>
		public static async Task ModifyGuildAFKTimeAsync(IGuild guild, int time, ModerationReason reason)
			=> await guild.ModifyAsync(x => x.AfkTimeout = time, reason.CreateRequestOptions()).CAF();
		/// <summary>
		/// Changes the guild's afk channel and says the supplied reason in the audit log.
		/// </summary>
		/// <param name="guild"></param>
		/// <param name="channel"></param>
		/// <param name="reason"></param>
		/// <returns></returns>
		public static async Task ModifyGuildAFKChannelAsync(IGuild guild, IVoiceChannel channel, ModerationReason reason)
			=> await guild.ModifyAsync(x => x.AfkChannel = Optional.Create(channel), reason.CreateRequestOptions()).CAF();
		/// <summary>
		/// Changes the guild's default message notification value and says the supplied reason in the audit log.
		/// </summary>
		/// <param name="guild"></param>
		/// <param name="msgNotifs"></param>
		/// <param name="reason"></param>
		/// <returns></returns>
		public static async Task ModifyGuildDefaultMsgNotificationsAsync(IGuild guild, DefaultMessageNotifications msgNotifs, ModerationReason reason)
			=> await guild.ModifyAsync(x => x.DefaultMessageNotifications = msgNotifs, reason.CreateRequestOptions()).CAF();
		/// <summary>
		/// Changes the guild's verification level and says the supplied reason in the audit log.
		/// </summary>
		/// <param name="guild"></param>
		/// <param name="verifLevel"></param>
		/// <param name="reason"></param>
		/// <returns></returns>
		public static async Task ModifyGuildVerificationLevelAsync(IGuild guild, VerificationLevel verifLevel, ModerationReason reason)
			=> await guild.ModifyAsync(x => x.VerificationLevel = verifLevel, reason.CreateRequestOptions()).CAF();
		/// <summary>
		/// Changes the guild's icon and says the supplied reason in the audit log.
		/// </summary>
		/// <param name="guild"></param>
		/// <param name="fileInfo"></param>
		/// <param name="reason"></param>
		/// <returns></returns>
		public static async Task ModifyGuildIconAsync(IGuild guild, FileInfo fileInfo, ModerationReason reason)
		{
			using (var stream = new StreamReader(fileInfo.FullName))
			{
				await guild.ModifyAsync(x => x.Icon = new Image(stream.BaseStream), reason.CreateRequestOptions()).CAF();
			}
		}
	}
}