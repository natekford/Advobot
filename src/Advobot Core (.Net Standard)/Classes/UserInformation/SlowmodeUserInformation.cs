using Discord;
using System;

namespace Advobot.Classes.UserInformation
{
	public class SlowmodeUserInformation : UserInfo
	{
		public int CurrentMessagesLeft { get; private set; }

		public SlowmodeUserInformation(IUser user, int baseMessages, int interval) : base(user)
		{
			CurrentMessagesLeft = baseMessages;
			_Time = DateTime.UtcNow.AddSeconds(interval);
		}

		public void DecrementMessages()
		{
			--CurrentMessagesLeft;
		}
		public void UpdateTime(int interval)
		{
			_Time = DateTime.UtcNow.AddSeconds(interval);
		}
	}
}
