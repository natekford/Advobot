using Advobot.Interfaces;
using Discord;
using System;

namespace Advobot.Classes.UserInformation
{
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
