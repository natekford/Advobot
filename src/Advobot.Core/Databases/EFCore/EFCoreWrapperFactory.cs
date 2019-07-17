#if false
using Advobot.Databases.Abstract;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Advobot.Databases.EFCore
{
	/// <summary>
	/// Generates wrappers for EF Core.
	/// </summary>
	internal abstract class EFCoreWrapperFactory : IDatabaseWrapperFactory
	{
		/// <inheritdoc />
		public IDatabaseWrapper CreateWrapper(string databaseName)
		{
			var wrapper = new EFCoreWrapper(GenerateOptions(databaseName));
			wrapper.Database.OpenConnection();
			wrapper.Database.EnsureCreated();
			return wrapper;
		}
		/// <summary>
		/// Generate the connection string to connect to a database with.
		/// </summary>
		/// <param name="databaseName"></param>
		/// <returns></returns>
		protected abstract DbContextOptionsBuilder GenerateOptions(string databaseName);

		/// <summary>
		/// Acts as a wrapper for EF Core.
		/// </summary>
		private sealed class EFCoreWrapper : DbContext, IDatabaseWrapper
		{
			/// <summary>
			/// Creates an instance of <see cref="EFCoreWrapper"/>.
			/// </summary>
			/// <param name="options"></param>
			public EFCoreWrapper(DbContextOptionsBuilder options) : base(options.Options) { }

			/// <inheritdoc />
			public IEnumerable<T> ExecuteQuery<T>(DatabaseQuery<T> options) where T : class, IDatabaseEntry
			{
				var set = Set<T>();
				switch (options.Action)
				{
					case DatabaseQuery<T>.DBAction.Update:
						Update(set, options.Values, isUpsert: false);
						SaveChanges();
						return options.Values;
					case DatabaseQuery<T>.DBAction.Upsert:
						Update(set, options.Values, isUpsert: true);
						SaveChanges();
						return options.Values;
					case DatabaseQuery<T>.DBAction.Insert:
						set.AddRange(options.Values);
						SaveChanges();
						return options.Values;
					case DatabaseQuery<T>.DBAction.Get:
						return set.Where(options.Selector).Take(options.Limit);
					case DatabaseQuery<T>.DBAction.GetAll:
						return set.ToArray();
					case DatabaseQuery<T>.DBAction.DeleteFromExpression:
						var values = new List<T>(set.Where(options.Selector));
						set.RemoveRange(values);
						SaveChanges();
						return values;
					case DatabaseQuery<T>.DBAction.DeleteFromValues:
						var ids = options.Values.Select(x => x.Id);
						set.RemoveRange(set.Where(x => ids.Contains(x.Id)));
						SaveChanges();
						return options.Values;
					default:
						throw new ArgumentException(nameof(options.Action));
				}
			}
			protected override void OnModelCreating(ModelBuilder modelBuilder)
			{
				throw new NotImplementedException();
			}
			private void Update<T>(DbSet<T> set, IEnumerable<T> items, bool isUpsert) where T : class, IDatabaseEntry
			{
				foreach (var item in items)
				{
					var entity = set.Find(item.Id);
					if (entity != null)
					{
						Entry(entity).CurrentValues.SetValues(item);
					}
					else if (isUpsert)
					{
						set.Add(item);
					}
				}
			}
		}
	}

	internal static class EFCoreWrapperFactoryUtils
	{
		public static void AddList<TEntity, TProperty>(
			this EntityTypeBuilder<TEntity> entityTypeBuilder,
			Expression<Func<TEntity, TProperty>> propertyExpression,
			Expression<Func<string, TProperty>>? convertFromExpression) where TEntity : class
		{
			if (typeof(TProperty).GetType().IsInterface)
			{
				throw new InvalidOperationException("Cannot deserialize with only an interface.");
			}

			convertFromExpression ??= x => JsonConvert.DeserializeObject<TProperty>(x);

			entityTypeBuilder
				.Property(propertyExpression)
				.HasConversion(x => JsonConvert.SerializeObject(x), convertFromExpression);
		}
	}
}
#endif