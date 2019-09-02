using System;
using System.Linq;

using AdvorangesUtils;

namespace Advobot.Databases.Abstract
{
	/// <summary>
	/// This class is the base of a service which uses a database.
	/// </summary>
	internal abstract class DatabaseWrapperConsumer : IUsesDatabase
	{
		private IDatabaseWrapper? _DatabaseWrapper;

		/// <summary>
		/// The name of the database.
		/// </summary>
		public abstract string DatabaseName { get; }

		/// <summary>
		/// The database being used. This can be any database type, or even just a simple dictionary.
		/// </summary>
		protected IDatabaseWrapper DatabaseWrapper => _DatabaseWrapper
			?? throw new InvalidOperationException("Database connection has not been started yet.");

		/// <summary>
		/// The factory for creating <see cref="DatabaseWrapper"/>.
		/// </summary>
		protected IDatabaseWrapperFactory DbFactory { get; }

		/// <summary>
		/// Creates an instance of <see cref="DatabaseWrapperConsumer"/>.
		/// </summary>
		/// <param name="dbFactory"></param>
		protected DatabaseWrapperConsumer(IDatabaseWrapperFactory dbFactory)
		{
			DbFactory = dbFactory;
		}

		/// <inheritdoc />
		public void Dispose()
		{
			BeforeDispose();
			_DatabaseWrapper?.Dispose();
		}

		/// <inheritdoc />
		public void Start()
		{
			_DatabaseWrapper = DbFactory.CreateWrapper(DatabaseName);
			ConsoleUtils.DebugWrite($"Started the database connection {DatabaseName}.");

			const string META_COLLECTION_NAME = "Meta";

			var findQuery = DatabaseQuery<DatabaseMetadata>.GetAll();
			findQuery.CollectionName = META_COLLECTION_NAME;
			var metas = DatabaseWrapper.ExecuteQuery(findQuery);
			var schema = metas.Any() ? metas.Max(x => x.SchemaVersion) : -1;

			AfterStart(schema);

			if (schema != Constants.SCHEMA_VERSION)
			{
				var insertQuery = DatabaseQuery<DatabaseMetadata>.Insert(new[] { new DatabaseMetadata() });
				insertQuery.CollectionName = META_COLLECTION_NAME;
				DatabaseWrapper.ExecuteQuery(insertQuery);
			}
		}

		/// <summary>
		/// Actions to do after the database connection has started.
		/// </summary>
		/// <param name="schema"></param>
		protected virtual void AfterStart(int schema)
		{
			return;
		}

		/// <summary>
		/// Actions to do before the database connection has been disposed.
		/// </summary>
		protected virtual void BeforeDispose()
		{
			return;
		}
	}
}