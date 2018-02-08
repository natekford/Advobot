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
		public SocketGuildUser User { get; }
		public DateTime Time { get; protected set; }

		public UserInfo(SocketGuildUser user)
		{
			User = user;
			Time = DateTime.UtcNow;
		}
		public UserInfo(TimeSpan timeToAdd, SocketGuildUser user)
		{
			User = user;
			Time = DateTime.UtcNow.Add(timeToAdd);
		}
	}
}
