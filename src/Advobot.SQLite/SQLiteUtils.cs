using Advobot.Settings;
using Advobot.Utilities;

using AdvorangesUtils;

using Dapper;

using FluentMigrator.Runner;

using Microsoft.Extensions.DependencyInjection;

using System.Data.Common;
using System.Data.SQLite;

namespace Advobot.SQLite;

/// <summary>
/// Utilities for SQLite.
/// </summary>
public static class SQLiteUtils
{
	/// <summary>
	/// Runs all migrations which have not been run yet.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="connection"></param>
	public static void MigrateUp<T>(this IConnectionString<T> connection)
		=> connection.CreateMigrationRunner().MigrateUp();

	/// <summary>
	/// Downgrades to the specified version.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="connection"></param>
	/// <param name="version"></param>
	public static void MigrateDown<T>(this IConnectionString<T> connection, long version)
		=> connection.CreateMigrationRunner().MigrateDown(version);

	/// <summary>
	/// Creates a new <see cref="DbConnection"/>.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="connection"></param>
	/// <returns></returns>
	public static async Task<T> GetConnectionAsync<T>(this IConnectionString connection)
		where T : DbConnection, new()
	{
		var conn = new T
		{
			ConnectionString = connection.ConnectionString
		};
		await conn.OpenAsync().CAF();
		return conn;
	}

	/// <summary>
	/// Gets the table names of a SQLite database.
	/// </summary>
	/// <param name="connection"></param>
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

	/// <summary>
	/// Adds a SQLite connection string for the specified database.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="services"></param>
	/// <param name="fileName"></param>
	/// <returns></returns>
	public static IServiceCollection AddSQLiteFileDatabaseConnectionString<T>(
		this IServiceCollection services,
		string fileName)
	{
		return services.AddSingleton(x =>
		{
			var accessor = x.GetRequiredService<IBotDirectoryAccessor>();
			var path = accessor.ValidateDbPath("SQLite", fileName).FullName;
			var conn = new SQLiteSystemFileDatabaseConnectionString(path);
			return (IConnectionString<T>)(IConnectionString<object>)conn;
		});
	}

	private static IMigrationRunner CreateMigrationRunner<T>(
		this IConnectionString<T> connection)
	{
		return new ServiceCollection()
			.AddFluentMigratorCore()
			.ConfigureRunner(x =>
			{
				x
				.AddSQLite()
				.WithGlobalConnectionString(connection.ConnectionString)
				.ScanIn(typeof(T).Assembly).For.Migrations();
			})
#if DEBUG
			.AddLogging(x => x.AddFluentMigratorConsole())
#endif
			.BuildServiceProvider(false)
			.GetRequiredService<IMigrationRunner>();
	}
}