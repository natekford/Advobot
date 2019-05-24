using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Advobot.Interfaces;
using Advobot.Utilities;
using LiteDB;
using Microsoft.Extensions.DependencyInjection;
using FileMode = LiteDB.FileMode;

namespace Advobot.Classes.DatabaseWrappers.LiteDB
{
	/// <summary>
	/// Generates wrappers for <see cref="LiteDatabase"/>.
	/// </summary>
	public sealed class LiteDBWrapperFactory : IDatabaseWrapperFactory
	{
		private readonly IBotDirectoryAccessor _DirectoryAccessor;

		/// <summary>
		/// Creates an instance of <see cref="LiteDBWrapperFactory"/>.
		/// </summary>
		/// <param name="provider"></param>
		public LiteDBWrapperFactory(IServiceProvider provider)
		{
			_DirectoryAccessor = provider.GetRequiredService<IBotDirectoryAccessor>();
		}

		/// <inheritdoc />
		public IDatabaseWrapper CreateWrapper(string databaseName)
		{
			if (!Path.HasExtension(databaseName))
			{
				databaseName += ".db";
			}
			return new LiteDBWrapper(GetDatabase(_DirectoryAccessor, databaseName));
		}
		/// <summary>
		/// Gets the database and makes sure there will be no errors when dealing with it.
		/// </summary>
		/// <param name="accessor"></param>
		/// <param name="fileName"></param>
		/// <param name="mapper"></param>
		/// <returns></returns>
		public static LiteDatabase GetDatabase(IBotDirectoryAccessor accessor, string fileName, BsonMapper? mapper = null)
		{
			var file = accessor.GetBaseBotDirectoryFile(fileName);
			//Make sure the file is not currently being used if it exists
			if (file.Exists)
			{
				using var _ = file.Open(System.IO.FileMode.Open, FileAccess.Read, FileShare.None);
			}
			return new LiteDatabase(new ConnectionString
			{
				Filename = file.FullName,
				Mode = FileMode.Exclusive, //One of my computer's will throw exceptions if this is shared
			}, mapper);
		}

		/// <summary>
		/// Acts as a wrapper for <see cref="LiteDatabase"/>.
		/// </summary>
		private sealed class LiteDBWrapper : IDatabaseWrapper
		{
			private readonly LiteDatabase _Database;

			/// <summary>
			/// Creates an instance of <see cref="LiteDBWrapper"/>.
			/// </summary>
			/// <param name="db"></param>
			public LiteDBWrapper(LiteDatabase db)
			{
				_Database = db;
			}

			/// <inheritdoc />
			public IEnumerable<T> ExecuteQuery<T>(DatabaseQuery<T> options) where T : DatabaseEntry
			{
				var collection = _Database.GetCollection<T>(options.CollectionName);
				switch (options.Action)
				{
					case DatabaseQuery<T>.DBAction.Update:
						collection.Update(options.Values);
						return options.Values ?? Enumerable.Empty<T>();
					case DatabaseQuery<T>.DBAction.Upsert:
						collection.Upsert(options.Values);
						return options.Values ?? Enumerable.Empty<T>();
					case DatabaseQuery<T>.DBAction.Insert:
						collection.Insert(options.Values);
						return options.Values ?? Enumerable.Empty<T>();
					case DatabaseQuery<T>.DBAction.Get:
						return collection.Find(options.Selector, limit: options.Limit);
					case DatabaseQuery<T>.DBAction.GetAll:
						return collection.Find(Query.All());
					case DatabaseQuery<T>.DBAction.DeleteFromExpression:
						var values = new List<T>(collection.Find(options.Selector));
						collection.Delete(options.Selector);
						return values;
					case DatabaseQuery<T>.DBAction.DeleteFromValues:
						foreach (var value in options.Values ?? Enumerable.Empty<T>())
						{
							collection.Delete(value.Id);
						}
						return options.Values ?? Enumerable.Empty<T>();
					default:
						throw new ArgumentException(nameof(options.Action));
				}
			}
			/// <inheritdoc />
			public void Dispose()
				=> _Database?.Dispose();
		}
	}
}