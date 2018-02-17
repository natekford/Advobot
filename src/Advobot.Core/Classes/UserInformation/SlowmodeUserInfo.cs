using Discord.WebSocket;
using LiteDB;
using System;
using System.Threading;

namespace Advobot.Core.Classes.UserInformation
{
	/// <summary>
	/// Holds how many messages a user has left and when to reset them.
	/// </summary>
	public class SlowmodeUserInfo : UserDatabaseEntry
	{
		[BsonIgnore]
		private int _MessagesLeft;

		/// <summary>
		/// The amount of messages left for the user to send before they should start being deleted.
		/// </summary>
		public int MessagesLeft
		{
			get => _MessagesLeft;
			set => _MessagesLeft = value;
		}

		public SlowmodeUserInfo() { }
		public SlowmodeUserInfo(TimeSpan time, SocketGuildUser user, int baseMessages) : base(time, user)
		{
			_MessagesLeft = baseMessages;
		}

		public void DecrementValue()
		{
			Interlocked.Decrement(ref _MessagesLeft);
		}
	}
}
