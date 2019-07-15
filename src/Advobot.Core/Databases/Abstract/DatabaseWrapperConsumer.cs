using System;
using System.Linq;
using Advobot.Interfaces;
using AdvorangesUtils;
using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Databases.Abstract
{
	/// <summary>
	/// This class is the base of a service which uses a database.
	/// </summary>
	internal abstract class DatabaseWrapperConsumer : IUsesDatabase, IDisposable
	{
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
		protected IDatabaseWrapperFactory DatabaseFactory { get; }

		private IDatabaseWrapper? _DatabaseWrapper;

		/// <summary>
		/// Creates an instance of <see cref="DatabaseWrapperConsumer"/>.
		/// </summary>
		/// <param name="provider"></param>
		public DatabaseWrapperConsumer(IServiceProvider provider)
		{
			DatabaseFactory = provider.GetRequiredService<IDatabaseWrapperFactory>();
		}

		/// <inheritdoc />
		public void Start()
		{
			_DatabaseWrapper = DatabaseFactory.CreateWrapper(DatabaseName);
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
		/// <inheritdoc />
		public void Dispose()
		{
			BeforeDispose();
			DatabaseWrapper.Dispose();
		}
		/// <summary>
		/// Actions to do before the database connection has been disposed.
		/// </summary>
		protected virtual void BeforeDispose()
		{
			return;
		}

		internal sealed class DatabaseMetadata : IDatabaseEntry
		{
			public int SchemaVersion { get; set; } = Constants.SCHEMA_VERSION;
			public string ProgramVersion { get; set; } = Constants.BOT_VERSION;

			//IDatabaseEntry
			object IDatabaseEntry.Id { get => ProgramVersion; set => ProgramVersion = (string)value; }
		}
	}
}