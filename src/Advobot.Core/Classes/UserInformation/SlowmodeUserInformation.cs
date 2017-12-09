using Discord;
using System;

namespace Advobot.Core.Classes.UserInformation
{
	/// <summary>
	/// Holds how many messages a user has left and when to reset them.
	/// </summary>
	public class SlowmodeUserInformation : UserInfo
	{
		public int CurrentMessagesLeft { get; private set; }

		public SlowmodeUserInformation(IGuildUser user, int baseMessages, int interval) : base(user)
		{
			this.CurrentMessagesLeft = baseMessages;
			this._Time = DateTime.UtcNow.AddSeconds(interval);
		}

		public void DecrementMessages() => --this.CurrentMessagesLeft;
		public void UpdateTime(int interval) => this._Time = DateTime.UtcNow.AddSeconds(interval);
	}
}
