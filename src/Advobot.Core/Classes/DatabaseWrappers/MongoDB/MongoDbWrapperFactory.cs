using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Advobot.Interfaces;
using AdvorangesUtils;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Options;
using MongoDB.Driver;

namespace Advobot.Classes.DatabaseWrappers.MongoDB
{
	/// <summary>
	/// Generates wrappers for <see cref="IMongoDatabase"/>.
	/// </summary>
	public sealed class MongoDBWrapperFactory : IDatabaseWrapperFactory
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
		/// <param name="provider"></param>
		public MongoDBWrapperFactory(IServiceProvider provider)
		{
			_Client = provider.GetRequiredService<IMongoClient>();
		}

		/// <inheritdoc />
		public IDatabaseWrapper CreateWrapper(string databaseName)
			=> new MongoDBWrapper(_Client.GetDatabase(databaseName));

		/// <summary>
		/// Acts as a wrapper for <see cref="IMongoDatabase"/>.
		/// </summary>
		private sealed class MongoDBWrapper : DatabaseWrapper<IMongoDatabase>
		{
			/// <summary>
			/// Creates an instance of <see cref="MongoDBWrapper"/>.
			/// </summary>
			/// <param name="db"></param>
			public MongoDBWrapper(IMongoDatabase db) : base(db) { }

			/// <inheritdoc />
			public override IEnumerable<T> ExecuteQuery<T>(DBQuery<T> options)
			{
				var collection = Database.GetCollection<T>(options.CollectionName);
				switch (options.Action)
				{
					case DBQuery<T>.DBAction.Update:
						foreach (var value in options.Values)
						{
							collection.ReplaceOne(x => x.Id == value.Id, value);
						}
						return options.Values;
					case DBQuery<T>.DBAction.Upsert:
						foreach (var value in options.Values)
						{
							collection.ReplaceOne(x => x.Id == value.Id, value, options: new UpdateOptions { IsUpsert = true });
						}
						return options.Values;
					case DBQuery<T>.DBAction.Insert:
						collection.InsertMany(options.Values);
						return options.Values;
					case DBQuery<T>.DBAction.Get:
						return collection.Find(options.Selector).Limit(options.Limit).ToEnumerable();
					case DBQuery<T>.DBAction.GetAll:
						return collection.Find(_ => true).ToEnumerable();
					case DBQuery<T>.DBAction.DeleteFromExpression:
						var values = new List<T>(collection.Find(options.Selector).ToEnumerable());
						collection.DeleteMany(options.Selector);
						return values;
					case DBQuery<T>.DBAction.DeleteFromValues:
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
