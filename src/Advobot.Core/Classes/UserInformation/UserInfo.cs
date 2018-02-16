using Advobot.Core.Interfaces;
using Discord.WebSocket;
using System;

namespace Advobot.Core.Classes.UserInformation
{
	/// <summary>
	/// Holds a user and a time.
	/// </summary>
	public abstract class UserInfo : ITime
	{
		public DateTime Time { get; set; }
		public ulong GuildId { get; set; }
		public ulong UserId { get; set; }

		protected UserInfo() { }
		public UserInfo(SocketGuildUser user)
		{
			GuildId = user.Guild.Id;
			UserId = user.Id;
			Time = DateTime.UtcNow;
		}
		public UserInfo(TimeSpan timeToAdd, SocketGuildUser user)
		{
			GuildId = user.Guild.Id;
			UserId = user.Id;
			Time = DateTime.UtcNow.Add(timeToAdd);
		}
	}
}
