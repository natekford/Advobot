using Advobot.Core.Interfaces;
using Discord;
using System;

namespace Advobot.Core.Classes.UserInformation
{
	/// <summary>
	/// Holds a user and a time.
	/// </summary>
	public abstract class UserInfo : ITime
	{
		public IGuildUser User { get; }
		public DateTime Time { get; protected set; }

		public UserInfo(IGuildUser user)
		{
			User = user;
			Time = DateTime.UtcNow;
		}
		public UserInfo(IGuildUser user, TimeSpan timeToAdd)
		{
			User = user;
			Time = DateTime.UtcNow.Add(timeToAdd);
		}
	}
}
