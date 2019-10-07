using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace Advobot.Databases.AbstractSQL
{
	/// <summary>
	/// Base class for a SQL database.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public abstract class DatabaseBase<T> : IDatabase
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
		/// Creates the database's tables.
		/// </summary>
		/// <returns></returns>
		public abstract Task<IReadOnlyList<string>> CreateDatabaseAsync();

		/// <summary>
		/// Executes a query in bulk.
		/// </summary>
		/// <typeparam name="TParams"></typeparam>
		/// <param name="sql"></param>
		/// <param name="params"></param>
		/// <returns></returns>
		protected Task<int> BulkModify<TParams>(string sql, IEnumerable<TParams> @params)
			=> Starter.BulkModify<T>((cnn, tr) => BulkModify(cnn, sql, @params, tr));

		/// <summary>
		/// The actual implementation of executing a query in bulk.
		/// </summary>
		/// <typeparam name="TParams"></typeparam>
		/// <param name="connection"></param>
		/// <param name="sql"></param>
		/// <param name="params"></param>
		/// <param name="transaction"></param>
		/// <returns></returns>
		protected abstract Task<int> BulkModify<TParams>(
			IDbConnection connection,
			string sql,
			IEnumerable<TParams> @params,
			IDbTransaction transaction);

		/// <summary>
		/// Gets a database connection.
		/// </summary>
		/// <returns></returns>
		protected Task<T> GetConnectionAsync()
			=> Starter.GetConnectionAsync<T>();
	}
}