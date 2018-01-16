using Discord;
using System;

namespace Advobot.Core.Classes.UserInformation
{
	/// <summary>
	/// Holds how many messages a user has left and when to reset them.
	/// </summary>
	public class SlowmodeUserInfo : UserInfo
	{
		public int CurrentMessagesLeft { get; private set; }

		public SlowmodeUserInfo(IGuildUser user, int baseMessages, int interval) : base(user)
		{
			CurrentMessagesLeft = baseMessages;
			Time = DateTime.UtcNow.AddSeconds(interval);
		}

		public void DecrementMessages()
		{
			--CurrentMessagesLeft;
		}
		public void UpdateTime(int interval)
		{
			Time = DateTime.UtcNow.AddSeconds(interval);
		}
	}
}
