﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Advobot.Databases.Abstract;
using Advobot.Services.GuildSettings;
using Advobot.Settings;
using Advobot.Utilities;
using LiteDB;
using Microsoft.Extensions.DependencyInjection;
using FileMode = LiteDB.FileMode;

namespace Advobot.Databases.LiteDB
{
	/// <summary>
	/// Generates wrappers for <see cref="LiteDatabase"/>.
	/// </summary>
	internal sealed class LiteDBWrapperFactory : IDatabaseWrapperFactory
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
			var file = AdvobotUtils.ValidateDbPath(_DirectoryAccessor, "LiteDB", databaseName);
			return new LiteDBWrapper(GetDatabase(file));
		}
		private static LiteDatabase GetDatabase(FileInfo file)
		{
			//Make sure the file is not currently being used if it exists
			if (file.Exists)
			{
				using var _ = file.Open(System.IO.FileMode.Open, FileAccess.Read, FileShare.None);
			}
			return new LiteDatabase(new ConnectionString
			{
				Filename = file.FullName,
				Mode = FileMode.Exclusive, //One of my computers will throw exceptions if this is shared
			}, new BsonMapper());
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

				db.Mapper.Entity<GuildSettings>()
					.Id(x => x.GuildId);
				db.Mapper.Entity<DatabaseMetadata>()
					.Id(x => x.ProgramVersion);
			}

			/// <inheritdoc />
			public IEnumerable<T> ExecuteQuery<T>(DatabaseQuery<T> options) where T : class, IDatabaseEntry
			{
				var collection = _Database.GetCollection<T>(options.CollectionName);
				switch (options.Action)
				{
					case DatabaseQuery<T>.DBAction.Update:
						collection.Update(options.Values);
						return options.Values;
					case DatabaseQuery<T>.DBAction.Upsert:
						collection.Upsert(options.Values);
						return options.Values;
					case DatabaseQuery<T>.DBAction.Insert:
						collection.Insert(options.Values);
						return options.Values;
					case DatabaseQuery<T>.DBAction.Get:
						return collection.Find(options.Selector, limit: options.Limit);
					case DatabaseQuery<T>.DBAction.GetAll:
						return collection.Find(Query.All());
					case DatabaseQuery<T>.DBAction.DeleteFromExpression:
						var values = new List<T>(collection.Find(options.Selector));
						collection.Delete(options.Selector);
						return values;
					case DatabaseQuery<T>.DBAction.DeleteFromValues:
						var ids = options.Values.Select(x => x.Id);
						collection.Delete(x => ids.Contains(x.Id));
						return options.Values;
					default:
						throw new ArgumentException(nameof(options.Action));
				}
			}
			/// <inheritdoc />
			public void Dispose()
				=> _Database?.Dispose();

			object IDatabaseWrapper.UnderlyingDatabase => _Database;
		}
	}
}