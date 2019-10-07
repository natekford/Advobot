using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace Advobot.Databases.AbstractSQL
{
	public abstract class DatabaseBase<T> : IDatabase
		where T : DbConnection, new()
	{
		protected IDatabaseStarter Starter { get; }

		protected DatabaseBase(IDatabaseStarter starter)
		{
			Starter = starter;
		}

		public abstract Task<IReadOnlyList<string>> CreateDatabaseAsync();

		protected Task<int> BulkModify<TParams>(string sql, IEnumerable<TParams> @params)
			=> Starter.BulkModify<T>((cnn, tr) => BulkModify(cnn, sql, @params, tr));

		protected abstract Task<int> BulkModify<TParams>(
			IDbConnection connection,
			string sql,
			IEnumerable<TParams> @params,
			IDbTransaction transaction);

		protected Task<T> GetConnectionAsync()
			=> Starter.GetConnectionAsync<T>();
	}
}