using LiteDB;
using System;

namespace Advobot.Core.Classes
{
	/// <summary>
	/// Stores a value in a database for later usage.
	/// </summary>
	public abstract class DatabaseEntry
	{
		private static TimeSpan _Default = new TimeSpan(3);

		/// <summary>
		/// The id of the object for LiteDB.
		/// </summary>
		public ObjectId Id { get; set; }
		/// <summary>
		/// The UTC time to do an action at.
		/// </summary>
		public DateTime Time { get; set; }

		public DatabaseEntry(TimeSpan time)
		{
			Time = DateTime.UtcNow.Add(time.Equals(default) ? _Default : time);
		}
	}
}
