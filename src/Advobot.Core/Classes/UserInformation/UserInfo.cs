using Discord.WebSocket;
using System;

namespace Advobot.Core.Classes.UserInformation
{
	/// <summary>
	/// Holds a user and a time.
	/// </summary>
	public abstract class UserInfo
	{
		/// <summary>
		/// The time to reset the user.
		/// </summary>
		public DateTime Time { get; protected set; }
		/// <summary>
		/// The id of the guild the user is on.
		/// </summary>
		public ulong GuildId { get; }
		/// <summary>
		/// The id of the user.
		/// </summary>
		public ulong UserId { get; }

		public UserInfo(SocketGuildUser user)
		{
			Time = DateTime.UtcNow;
			GuildId = user.Guild.Id;
			UserId = user.Id;
		}
		public UserInfo(TimeSpan time, SocketGuildUser user)
		{
			Time = DateTime.UtcNow.Add(time);
			GuildId = user.Guild.Id;
			UserId = user.Id;
		}

		public abstract void Reset();
	}
}
