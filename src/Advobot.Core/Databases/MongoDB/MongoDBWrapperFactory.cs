using System;
using System.Collections.Generic;
using System.Linq;

using Advobot.Databases.Abstract;

using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Options;
using MongoDB.Driver;

namespace Advobot.Databases.MongoDB
{
	/// <summary>
	/// Generates wrappers for <see cref="IMongoDatabase"/>.
	/// </summary>
	internal sealed class MongoDBWrapperFactory : IDatabaseWrapperFactory
	{
		private readonly IMongoClient _Client;

		static MongoDBWrapperFactory()
		{
			ConventionRegistry.Register(
				nameof(DictionaryRepresentationConvention),
				new ConventionPack { new DictionaryRepresentationConvention(DictionaryRepresentation.ArrayOfArrays) },
				_ => true);
		}

		/// <summary>
		/// Creates an instance of <see cref="MongoDBWrapperFactory"/>.
		/// </summary>
		/// <param name="client"></param>
		public MongoDBWrapperFactory(IMongoClient client)
		{
			_Client = client;
		}

		/// <inheritdoc />
		public IDatabaseWrapper CreateWrapper(string databaseName)
			=> new MongoDBWrapper(_Client.GetDatabase(databaseName));

		/// <summary>
		/// Acts as a wrapper for <see cref="IMongoDatabase"/>.
		/// </summary>
		private sealed class MongoDBWrapper : IDatabaseWrapper
		{
			private readonly IMongoDatabase _Database;

			/// <summary>
			/// Creates an instance of <see cref="MongoDBWrapper"/>.
			/// </summary>
			/// <param name="db"></param>
			public MongoDBWrapper(IMongoDatabase db)
			{
				_Database = db;
			}

			object IDatabaseWrapper.UnderlyingDatabase => _Database;

			/// <inheritdoc />
			public void Dispose() { }

			/// <inheritdoc />
			public IEnumerable<T> ExecuteQuery<T>(DatabaseQuery<T> options) where T : class, IDatabaseEntry
			{
				var collection = _Database.GetCollection<T>(options.CollectionName);
				switch (options.Action)
				{
					case DatabaseQuery<T>.DBAction.Update:
						foreach (var value in options.Values)
						{
							collection.ReplaceOne(x => x.Id == value.Id, value);
						}
						return options.Values;

					case DatabaseQuery<T>.DBAction.Upsert:
						foreach (var value in options.Values)
						{
							collection.ReplaceOne(x => x.Id == value.Id, value, options: new UpdateOptions { IsUpsert = true });
						}
						return options.Values;

					case DatabaseQuery<T>.DBAction.Insert:
						collection.InsertMany(options.Values);
						return options.Values;

					case DatabaseQuery<T>.DBAction.Get:
						return collection.Find(options.Selector).Limit(options.Limit).ToEnumerable();

					case DatabaseQuery<T>.DBAction.GetAll:
						return collection.Find(_ => true).ToEnumerable();

					case DatabaseQuery<T>.DBAction.DeleteFromExpression:
						var values = collection.Find(options.Selector).ToEnumerable().ToArray();
						collection.DeleteMany(options.Selector);
						return values;

					case DatabaseQuery<T>.DBAction.DeleteFromValues:
						var ids = options.Values.Select(x => x.Id);
						collection.DeleteMany(Builders<T>.Filter.In(x => x.Id, ids));
						return options.Values;

					default:
						throw new InvalidOperationException("Invalid database action supplied.");
				}
			}
		}
	}
}