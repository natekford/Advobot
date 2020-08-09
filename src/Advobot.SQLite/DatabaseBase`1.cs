using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

using AdvorangesUtils;

using Dapper;

namespace Advobot.SQLite
{
	/// <summary>
	/// Base class for a SQL database.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public abstract class DatabaseBase<T> where T : DbConnection, new()
	{
		/// <summary>
		/// Starts the database.
		/// </summary>
		protected IConnectionString Connection { get; }

		/// <summary>
		/// Creates an instance of <see cref="DatabaseBase{T}"/>.
		/// </summary>
		/// <param name="conn"></param>
		protected DatabaseBase(IConnectionString conn)
		{
			Connection = conn;
			ConsoleUtils.DebugWrite($"Created database with the connection \"{conn.ConnectionString}\".", nameof(DatabaseBase<T>));
		}

		/// <summary>
		/// Executes a query to modify many values.
		/// </summary>
		/// <typeparam name="TParam"></typeparam>
		/// <param name="sql"></param>
		/// <param name="params"></param>
		/// <returns></returns>
		protected async Task<int> BulkModifyAsync<TParam>(string sql, IEnumerable<TParam> @params)
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
			=> Connection.GetConnectionAsync<T>();

		/// <summary>
		/// Executes a query to retrieve multiple values.
		/// </summary>
		/// <typeparam name="TRet"></typeparam>
		/// <param name="sql"></param>
		/// <param name="param"></param>
		/// <returns></returns>
		protected async Task<TRet[]> GetManyAsync<TRet>(string sql, object? param)
		{
			using var connection = await GetConnectionAsync().CAF();
			var result = await connection.QueryAsync<TRet>(sql, param).CAF();
			return result.ToArray();
		}

		/// <summary>
		/// Executes a query to retrieve one value.
		/// </summary>
		/// <typeparam name="TRet"></typeparam>
		/// <param name="sql"></param>
		/// <param name="param"></param>
		/// <returns></returns>
		[return: MaybeNull]
		protected async Task<TRet> GetOneAsync<TRet>(string sql, object param)
		{
			using var connection = await GetConnectionAsync().CAF();
			return await connection.QuerySingleOrDefaultAsync<TRet>(sql, param).CAF();
		}

		/// <summary>
		/// Executes a query to modify one value.
		/// </summary>
		/// <typeparam name="TParam"></typeparam>
		/// <param name="sql"></param>
		/// <param name="param"></param>
		/// <returns></returns>
		protected async Task<int> ModifyAsync<TParam>(string sql, TParam param)
		{
			using var connection = await GetConnectionAsync().CAF();
			return await connection.ExecuteAsync(sql, param).CAF();
		}
	}
}