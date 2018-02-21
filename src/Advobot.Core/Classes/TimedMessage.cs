using Discord;
using System;

namespace Advobot.Core.Classes
{
	/// <summary>
	/// Sends a message to the author after the time has passed.
	/// </summary>
	public class TimedMessage : DatabaseEntry
	{
		/// <summary>
		/// The user to send the message to.
		/// </summary>
		public ulong UserId { get; set; }
		/// <summary>
		/// The text to send the user.
		/// </summary>
		public string Text { get; set; }

		public TimedMessage() : base(default) { }
		public TimedMessage(TimeSpan time, IUser author, string text) : base(time)
		{
			UserId = author.Id;
			Text = text;
		}
	}
}
