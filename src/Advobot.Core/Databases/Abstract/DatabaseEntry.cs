using System;

namespace Advobot.Databases.Abstract
{
	/// <summary>
	/// Stores a value in a database for later usage.
	/// </summary>
	public abstract class TimedDatabaseEntry<T> : IDatabaseEntry
	{
		/// <summary>
		/// The id of the object.
		/// </summary>
		public T Id { get; set; }
		/// <summary>
		/// The UTC time to do an action at.
		/// </summary>
		public DateTime Time { get; set; }

		/// <summary>
		/// Creates a database entry with the specified timespan added to <see cref="DateTime.UtcNow"/>.
		/// </summary>
		/// <param name="id"></param>
		/// <param name="time"></param>
		public TimedDatabaseEntry(T id, TimeSpan time)
		{
			Id = id;
			Time = DateTime.UtcNow.Add(time);
		}

		//IDatabaseEntry
#pragma warning disable CS8603 // Possible null reference return.
		object IDatabaseEntry.Id { get => Id; set => Id = (T)value; }
#pragma warning restore CS8603 // Possible null reference return.
	}
}
