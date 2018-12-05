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
		private readonly IBotSettings _Settings;

		/// <summary>
		/// Creates an instance of <see cref="LiteDBWrapperFactory"/>.
		/// </summary>
		/// <param name="provider"></param>
		public LiteDBWrapperFactory(IServiceProvider provider)
		{
			_Settings = provider.GetRequiredService<IBotSettings>();
		}

		/// <inheritdoc />
		public IDatabaseWrapper CreateWrapper(string databaseName)
		{
			if (!Path.HasExtension(databaseName))
			{
				databaseName = databaseName + ".db";
			}
			return new LiteDBWrapper(GetDatabase(_Settings, databaseName));
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
				using (var fs = file.Open(System.IO.FileMode.Open, FileAccess.Read, FileShare.None)) { }
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
		private sealed class LiteDBWrapper : DatabaseWrapper<LiteDatabase>
		{
			/// <summary>
			/// Creates an instance of <see cref="LiteDBWrapper"/>.
			/// </summary>
			/// <param name="db"></param>
			public LiteDBWrapper(LiteDatabase db) : base(db) { }

			/// <inheritdoc />
			public override IEnumerable<T> ExecuteQuery<T>(DBQuery<T> options)
			{
				var collection = Database.GetCollection<T>(options.CollectionName);
				switch (options.Action)
				{
					case DBQuery<T>.DBAction.Update:
						collection.Update(options.Values);
						return options.Values ?? Enumerable.Empty<T>();
					case DBQuery<T>.DBAction.Upsert:
						collection.Upsert(options.Values);
						return options.Values ?? Enumerable.Empty<T>();
					case DBQuery<T>.DBAction.Insert:
						collection.Insert(options.Values);
						return options.Values ?? Enumerable.Empty<T>();
					case DBQuery<T>.DBAction.Get:
						return collection.Find(options.Selector, limit: options.Limit);
					case DBQuery<T>.DBAction.GetAll:
						return collection.Find(Query.All());
					case DBQuery<T>.DBAction.DeleteFromExpression:
						var values = new List<T>(collection.Find(options.Selector));
						collection.Delete(options.Selector);
						return values;
					case DBQuery<T>.DBAction.DeleteFromValues:
						foreach (var value in options.Values ?? Enumerable.Empty<T>())
						{
							collection.Delete(value.Id);
						}
						return options.Values ?? Enumerable.Empty<T>();
					default:
						throw new InvalidOperationException("Invalid database action supplied.");
				}
			}
		}
	}
}