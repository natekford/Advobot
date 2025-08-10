using Advobot.Services;

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
	/// Adds a default options setter.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="services"></param>
	/// <returns></returns>
	public static IServiceCollection AddDefaultOptionsSetter<T>(
		this IServiceCollection services)
		where T : class, IResetter
	{
		return services
			.AddSingleton<T>()
			.AddSingleton<IResetter>(x => x.GetRequiredService<T>());
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
			var config = x.GetRequiredService<IConfig>();
			var path = config.ValidateDbPath("SQLite", fileName).FullName;
			var conn = new SQLiteSystemFileDatabaseConnectionString(path);
			return (IConnectionString<T>)(IConnectionString<object>)conn;
		});
	}

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
		await conn.OpenAsync().ConfigureAwait(false);
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
		").ConfigureAwait(false);
		return [.. result];
	}

	/// <summary>
	/// Downgrades to the specified version.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="connection"></param>
	/// <param name="version"></param>
	public static void MigrateDown<T>(this IConnectionString<T> connection, long version)
		=> connection.CreateMigrationRunner().MigrateDown(version);

	/// <summary>
	/// Runs all migrations which have not been run yet.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="connection"></param>
	public static void MigrateUp<T>(this IConnectionString<T> connection)
		=> connection.CreateMigrationRunner().MigrateUp();

	/// <summary>
	/// Ensures the extension of the file is '.db' and that the directory exists.
	/// </summary>
	/// <param name="config"></param>
	/// <param name="fileNameParts"></param>
	/// <returns></returns>
	public static FileInfo ValidateDbPath(this IConfig config, params string[] fileNameParts)
	{
		static void ExtensionValidation(ref string fileName)
		{
			const string EXT = ".db";
			if (!Path.HasExtension(fileName))
			{
				fileName += EXT;
			}
			else if (Path.GetExtension(fileName) != EXT)
			{
				fileName = Path.GetFileNameWithoutExtension(fileName) + EXT;
			}
		}
		ExtensionValidation(ref fileNameParts[^1]);

		var relativePath = Path.Combine(fileNameParts);
		var absolutePath = config.GetFile(relativePath).FullName;
		//Make sure the directory the db will be created in exists
		Directory.CreateDirectory(Path.GetDirectoryName(absolutePath)!);
		return new(absolutePath);
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