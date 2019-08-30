using System;

using Advobot.Databases.Abstract;

using Discord;

namespace Advobot.Classes
{
	/// <summary>
	/// Sends a message to the author after the time has passed.
	/// </summary>
	public class TimedMessage : TimedDatabaseEntry<ulong>
	{
		/// <summary>
		/// The text to send the user.
		/// </summary>
		public string Text { get; set; } = "";

		/// <summary>
		/// Creates an instance of <see cref="TimedMessage"/>. Parameterless constructor is used for the database.
		/// </summary>
		public TimedMessage() : base(default, TimeSpan.Zero) { }

		/// <summary>
		/// Creates an instance of <see cref="TimedMessage"/>.
		/// </summary>
		/// <param name="time"></param>
		/// <param name="author"></param>
		/// <param name="text"></param>
		public TimedMessage(TimeSpan time, IUser author, string text) : base(author.Id, time)
		{
			Text = text;
		}
	}
}