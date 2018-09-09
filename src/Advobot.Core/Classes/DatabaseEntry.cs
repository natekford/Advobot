using System;

namespace Advobot.Classes
{
	/// <summary>
	/// Stores a value in a database for later usage.
	/// </summary>
	public abstract class DatabaseEntry
	{
		private static TimeSpan _Default = TimeSpan.FromSeconds(3);

		/// <summary>
		/// The id of the object.
		/// This is not necessarily as unique as a regular <see cref="Guid"/> because sometimes it is created from a user's id.
		/// </summary>
		public Guid Id { get; set; }
		/// <summary>
		/// The UTC time to do an action at.
		/// </summary>
		public DateTime Time { get; set; }

		/// <summary>
		/// Creates a database entry with the specified timespan added to <see cref="DateTime.UtcNow"/>.
		/// </summary>
		/// <param name="time"></param>
		public DatabaseEntry(TimeSpan time)
		{
			Id = Guid.NewGuid();
			Time = DateTime.UtcNow.Add(time.Equals(default) ? _Default : time);
		}
	}
}
