using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;

using AdvorangesUtils;

using Dapper;

namespace Advobot.SQLite
{
	/// <summary>
	/// Base class for a SQL database.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public abstract class DatabaseBase<T>
		where T : DbConnection, new()
	{
		/// <summary>
		/// Starts the database.
		/// </summary>
		protected IDatabaseStarter Starter { get; }

		/// <summary>
		/// Creates an instance of <see cref="DatabaseBase{T}"/>.
		/// </summary>
		/// <param name="starter"></param>
		protected DatabaseBase(IDatabaseStarter starter)
		{
			Starter = starter;
		}

		/// <summary>
		/// Executes a query in bulk.
		/// </summary>
		/// <typeparam name="TParams"></typeparam>
		/// <param name="sql"></param>
		/// <param name="params"></param>
		/// <returns></returns>
		protected async Task<int> BulkModifyAsync<TParams>(string sql, IEnumerable<TParams> @params)
		{
			//Use a transaction to make bulk modifying way faster in SQLite
			using var connection = await GetConnectionAsync().CAF();
			using var transaction = await connection.BeginTransactionAsync().CAF();

			var affectedRowCount = await connection.ExecuteAsync(sql, @params, transaction).CAF();
			transaction.Commit();
			return affectedRowCount;
		}

		/// <summary>
		/// Gets a database connection.
		/// </summary>
		/// <returns></returns>
		protected Task<T> GetConnectionAsync()
			=> Starter.GetConnectionAsync<T>();
	}
}