using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Advobot.Databases.Abstract;

namespace Advobot.Databases
{
	/// <summary>
	/// Class holding information about how to use <see cref="IDatabaseWrapper"/> methods.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	internal sealed class DatabaseQuery<T> where T : IDatabaseEntry
	{
		/// <summary>
		/// The name of the collection to search.
		/// </summary>
		public string CollectionName { get; set; } = typeof(T).Name;
		/// <summary>
		/// The new value to either insert or update.
		/// </summary>
		public IEnumerable<T>? Values { get; private set; }
		/// <summary>
		/// How to select values.
		/// </summary>
		public Expression<Func<T, bool>>? Selector { get; private set; }
		/// <summary>
		/// How many values to search.
		/// </summary>
		public int Limit { get; private set; }
		/// <summary>
		/// The action to do.
		/// </summary>
		public DBAction Action { get; private set; }

		/// <summary>
		/// Creates an instance of <see cref="DatabaseQuery{T}"/>.
		/// </summary>
		/// <param name="action"></param>
		private DatabaseQuery(DBAction action)
		{
			Action = action;
		}

		/// <summary>
		/// Update the specified values.
		/// </summary>
		public static DatabaseQuery<T> Update(IEnumerable<T> values)
			=> new DatabaseQuery<T>(DBAction.Update) { Values = values, };
		/// <summary>
		/// Updates or inserts the specified values.
		/// </summary>
		public static DatabaseQuery<T> Upsert(IEnumerable<T> values)
			=> new DatabaseQuery<T>(DBAction.Upsert) { Values = values, };
		/// <summary>
		/// Insert the specified values.
		/// </summary>
		public static DatabaseQuery<T> Insert(IEnumerable<T> values)
			=> new DatabaseQuery<T>(DBAction.Insert) { Values = values, };
		/// <summary>
		/// Get the values which match the passed in predicate.
		/// </summary>
		public static DatabaseQuery<T> Get(Expression<Func<T, bool>> selector, int limit = int.MaxValue)
			=> new DatabaseQuery<T>(DBAction.Get) { Selector = selector, Limit = limit, };
		/// <summary>
		/// Gets every value.
		/// </summary>
		public static DatabaseQuery<T> GetAll()
			=> new DatabaseQuery<T>(DBAction.GetAll);
		/// <summary>
		/// Delete the values which match the passed in predicate.
		/// </summary>
		public static DatabaseQuery<T> Delete(Expression<Func<T, bool>> selector)
			=> new DatabaseQuery<T>(DBAction.DeleteFromExpression) { Selector = selector, };
		/// <summary>
		/// Deletes the specified values from the supplied list.
		/// </summary>
		public static DatabaseQuery<T> Delete(IEnumerable<T> values)
			=> new DatabaseQuery<T>(DBAction.DeleteFromValues) { Values = values, };

		/// <summary>
		/// Actions to do with a database.
		/// </summary>
		public enum DBAction
		{
			/// <summary>
			/// Update the specified values.
			/// </summary>
			Update,
			/// <summary>
			/// Updates or inserts the specified values.
			/// </summary>
			Upsert,
			/// <summary>
			/// Insert the specified values.
			/// </summary>
			Insert,
			/// <summary>
			/// Get the specified values.
			/// </summary>
			Get,
			/// <summary>
			/// Gets every value.
			/// </summary>
			GetAll,
			/// <summary>
			/// Delete the values which match the passed in predicate.
			/// </summary>
			DeleteFromExpression,
			/// <summary>
			/// Deletes the specified values from the supplied list.
			/// </summary>
			DeleteFromValues,
		}
	}
}