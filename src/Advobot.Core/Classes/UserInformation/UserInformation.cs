using Advobot.Core.Interfaces;
using Discord;
using System;

namespace Advobot.Core.Classes.UserInformation
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
			this.User = user;
		}

		public DateTime GetTime() => this._Time;
	}
}
