using Advobot.Core.Classes;
using Discord;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Advobot.Core.Utilities
{
	/// <summary>
	/// Actions done on an <see cref="IGuild"/>.
	/// </summary>
	public static class GuildUtils
	{
		/// <summary>
		/// Attempts to get a guild from a message.
		/// </summary>
		/// <param name="message"></param>
		/// <returns></returns>
		public static IGuild GetGuild(this IMessage message)
		{
			return (message?.Channel as SocketGuildChannel)?.Guild;
		}
		/// <summary>
		/// Attempts to get a guild from a user.
		/// </summary>
		/// <param name="user"></param>
		/// <returns></returns>
		public static IGuild GetGuild(this IUser user)
		{
			return (user as SocketGuildUser)?.Guild;
		}
		/// <summary>
		/// Attempts to get a guild from a channel.
		/// </summary>
		/// <param name="channel"></param>
		/// <returns></returns>
		public static IGuild GetGuild(this IChannel channel)
		{
			return (channel as SocketGuildChannel)?.Guild;
		}
		/// <summary>
		/// Returns the bot's guild user.
		/// </summary>
		/// <param name="guild"></param>
		/// <returns></returns>
		public static IGuildUser GetBot(this IGuild guild)
		{
			return (guild as SocketGuild)?.CurrentUser;
		}
		/// <summary>
		/// Returns every user that has a non null join time in order from least to greatest.
		/// </summary>
		/// <param name="guild"></param>
		/// <returns></returns>
		public static async Task<IEnumerable<IGuildUser>> GetUsersByJoinDateAsync(this IGuild guild)
		{
			return (await guild.GetUsersAsync().CAF()).Where(x => x.JoinedAt != null).OrderBy(x => x.JoinedAt?.Ticks ?? 0);
		}
		/// <summary>
		/// Returns every user that can be modified by both <paramref name="invokingUser"/> and the bot.
		/// </summary>
		/// <param name="guild"></param>
		/// <param name="invokingUser"></param>
		/// <returns></returns>
		public static async Task<IEnumerable<IGuildUser>> GetEditableUsersAsync(this IGuild guild, IGuildUser invokingUser)
		{
			return (await guild.GetUsersAsync().CAF()).Where(x => invokingUser.HasHigherPosition(x) && guild.GetBot().HasHigherPosition(x));
		}
	}
}