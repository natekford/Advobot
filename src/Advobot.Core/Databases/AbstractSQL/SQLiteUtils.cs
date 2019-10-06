using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

using AdvorangesUtils;

namespace Advobot.Databases.AbstractSQL
{
	/// <summary>
	/// Utilities for SQLite.
	/// </summary>
	public static class SQLiteUtils
	{
		public static async Task<int> BulkModify<T>(
			this IDatabaseStarter starter,
			Func<IDbConnection, IDbTransaction, Task<int>> query)
			where T : DbConnection, new()
		{
			//Use a transaction to make bulk modifying way faster in SQLite
			using var connection = await starter.GetConnectionAsync<T>().CAF();
			using var transaction = await connection.BeginTransactionAsync().CAF();

			var affectedRowCount = await query.Invoke(connection, transaction).CAF();
			transaction.Commit();
			return affectedRowCount;
		}

		/// <summary>
		/// Creates a new <see cref="DbConnection"/>.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="starter"></param>
		/// <returns></returns>
		public static async Task<T> GetConnectionAsync<T>(this IDatabaseStarter starter)
			where T : DbConnection, new()
		{
			var connection = new T
			{
				ConnectionString = starter.GetConnectionString()
			};
			await connection.OpenAsync().CAF();
			return connection;
		}

		/// <summary>
		/// Gets the table names of a SQLite database.
		/// </summary>
		/// <param name="connection"></param>
		/// <param name="query"></param>
		/// <returns></returns>
		public static async Task<IReadOnlyList<string>> GetTableNames(
			this IDbConnection connection,
			Func<IDbConnection, string, Task<IEnumerable<string>>> query)
		{
			const string SQL = @"
				SELECT name FROM sqlite_master
				WHERE type='table'
				ORDER BY name;
			";
			var result = await query.Invoke(connection, SQL).CAF();
			return result.ToArray();
		}
	}
}