using System;
using System.Collections.Generic;
using Advobot.Interfaces;

namespace Advobot.Classes.DatabaseWrappers
{
	/// <summary>
	/// Wraps a database so various functions can be done on it semi generically.
	/// </summary>
	/// <typeparam name="TDatabase"></typeparam>
	public abstract class DatabaseWrapper<TDatabase> : IDatabaseWrapper
	{
		/// <summary>
		/// The database being wrapped.
		/// </summary>
		public TDatabase Database { get; }

		/// <summary>
		/// Creates an instance of <see cref="DatabaseWrapper{TDatabase}"/>.
		/// </summary>
		/// <param name="database"></param>
		public DatabaseWrapper(TDatabase database)
		{
			Database = database;
		}

		/// <inheritdoc />
		public abstract IEnumerable<T> ExecuteQuery<T>(DBQuery<T> options) where T : DatabaseEntry;
		/// <summary>
		/// Attempts to dispose <see cref="Database"/> if it is disposable.
		/// </summary>
		public void Dispose()
		{
			if (Database is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}
	}
}