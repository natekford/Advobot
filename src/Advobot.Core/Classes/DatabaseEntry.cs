using LiteDB;
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
		/// The id of the object for LiteDB.
		/// </summary>
		[BsonId]
		public ObjectId Id { get; set; }
		/// <summary>
		/// The UTC time to do an action at.
		/// </summary>
		[BsonField("Time")]
		public DateTime Time { get; set; }

		/// <summary>
		/// Creates a database entry with the specified timespan added to <see cref="DateTime.UtcNow"/>.
		/// </summary>
		/// <param name="time"></param>
		public DatabaseEntry(TimeSpan time)
		{
			Id = ObjectId.NewObjectId();
			Time = DateTime.UtcNow.Add(time.Equals(default) ? _Default : time);
		}
	}
}
