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
		public ulong GuildId { get; }
		public ulong UserId { get; }
		public DateTime Time { get; protected set; }

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
