using System.Collections.Generic;
using System.Data.Common;
using System.Data.SQLite;
using System.Linq;
using System.Threading.Tasks;

using AdvorangesUtils;

using Dapper;

using FluentMigrator.Runner;

using Microsoft.Extensions.DependencyInjection;

namespace Advobot.SQLite
{
	/// <summary>
	/// Utilities for SQLite.
	/// </summary>
	public static class SQLiteUtils
	{
		/// <summary>
		/// Runs all migrations which have not been run yet.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="starter"></param>
		public static void MigrateUp<T>(this T starter) where T : IDatabaseStarter
		{
			new ServiceCollection()
				.AddFluentMigratorCore()
				.ConfigureRunner(x =>
				{
					x
					.AddSQLite()
					.WithGlobalConnectionString(starter.GetConnectionString())
					.ScanIn(typeof(T).Assembly).For.Migrations();
				})
#if DEBUG
				.AddLogging(x => x.AddFluentMigratorConsole())
#endif
				.BuildServiceProvider(false)
				.GetRequiredService<IMigrationRunner>()
				.MigrateUp();
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
			this SQLiteConnection connection)
		{
			var result = await connection.QueryAsync<string>(@"
				SELECT name FROM sqlite_master
				WHERE type='table'
				ORDER BY name;
			").CAF();
			return result.ToArray();
		}
	}
}