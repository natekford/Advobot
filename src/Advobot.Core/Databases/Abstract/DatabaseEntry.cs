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
		public DateTimeOffset Time { get; set; }

		//IDatabaseEntry
		object IDatabaseEntry.Id
		{
			get => Id ?? throw new InvalidOperationException($"{nameof(Id)} is null.");
			set => Id = (T)value;
		}

		/// <summary>
		/// Creates a database entry with the specified timespan added to <see cref="DateTimeOffset.UtcNow"/>.
		/// </summary>
		/// <param name="id"></param>
		/// <param name="time"></param>
		protected TimedDatabaseEntry(T id, TimeSpan time)
		{
			Id = id;
			Time = DateTimeOffset.UtcNow.Add(time);
		}
	}
}