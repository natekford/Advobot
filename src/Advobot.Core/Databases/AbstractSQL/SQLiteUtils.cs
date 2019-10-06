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
		/// <summary>
		/// Creates a new SQLite connection.
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