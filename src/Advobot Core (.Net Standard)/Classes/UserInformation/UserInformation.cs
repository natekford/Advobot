using Advobot.Interfaces;
using Discord;
using System;

namespace Advobot.Classes.UserInformation
{
	/// <summary>
	/// Holds a user and a time.
	/// </summary>
	public abstract class UserInfo : IHasTime
	{
		public readonly IGuildUser User;
		protected DateTime _Time;

		public UserInfo(IGuildUser user)
		{
			User = user;
		}

		public DateTime GetTime() => _Time;
	}
}
