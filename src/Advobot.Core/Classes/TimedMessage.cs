using Advobot.Core.Interfaces;
using Discord;
using LiteDB;
using System;

namespace Advobot.Core.Classes
{
	/// <summary>
	/// Sends a message to the author after the time has passed.
	/// </summary>
	public class TimedMessage : ITime
	{
		/// <summary>
		/// The id of the object for LiteDB.
		/// </summary>
		public ObjectId Id { get; set; }
		/// <summary>
		/// The time to send the message at.
		/// </summary>
		public DateTime Time { get; set; }
		/// <summary>
		/// The user to send the message to.
		/// </summary>
		public ulong UserId { get; set; }
		/// <summary>
		/// The text to send the user.
		/// </summary>
		public string Text { get; set; }

		public TimedMessage() { }
		public TimedMessage(TimeSpan time, IGuildUser author, string text)
		{
			Time = DateTime.UtcNow.Add(time);
			UserId = author.Id;
			Text = text;
		}
	}
}
