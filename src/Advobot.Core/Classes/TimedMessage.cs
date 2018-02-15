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
		public ObjectId Id { get; set; }
		public DateTime Time { get; private set; }
		public ulong UserId { get; private set; }
		public string Text { get; private set; }

		public TimedMessage() { }
		public TimedMessage(TimeSpan time, IGuildUser author, string text)
		{
			Time = DateTime.UtcNow.Add(time);
			UserId = author.Id;
			Text = text;
		}
	}
}
