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

		/// <summary>
		/// Initializes the object. Parameterless constructor is used for the database.
		/// </summary>
		public TimedMessage() : base(default) { }
		/// <summary>
		/// Creates an instance of timedmessage.
		/// </summary>
		/// <param name="time"></param>
		/// <param name="author"></param>
		/// <param name="text"></param>
		public TimedMessage(TimeSpan time, IUser author, string text) : base(time)
		{
			UserId = author.Id;
			Text = text;
		}
	}
}
