using Discord;
using System;
using System.Threading;

namespace Advobot.Core.Classes.UserInformation
{
	/// <summary>
	/// Holds how many messages a user has left and when to reset them.
	/// </summary>
	public class SlowmodeUserInfo : UserInfo
	{
		private int _MessagesLeft;
		public int MessagesLeft => _MessagesLeft;

		public SlowmodeUserInfo(IGuildUser user, int baseMessages, int interval) : base(user, TimeSpan.FromSeconds(interval))
		{
			_MessagesLeft = baseMessages;
		}

		public int DecrementMessages()
		{
			return Interlocked.Decrement(ref _MessagesLeft);
		}
	}
}
