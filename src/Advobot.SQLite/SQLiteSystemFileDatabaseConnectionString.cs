using Advobot.Services;

namespace Advobot.SQLite;

/// <summary>
/// Used for starting a SQLite database from a system file.
/// </summary>
public sealed class SQLiteSystemFileDatabaseConnectionString
	: IConfigurableService, IConnectionString<object>
{
	/// <inheritdoc />
	public string ConnectionString { get; }
	/// <inheritdoc />
	public Type Database { get; }
	/// <summary>
	/// The path of the SQLite database.
	/// </summary>
	public string Path { get; }

	/// <summary>
	/// Creates an instance of <see cref="SQLiteSystemFileDatabaseConnectionString"/>.
	/// </summary>
	/// <param name="path"></param>
	/// <param name="db"></param>
	public SQLiteSystemFileDatabaseConnectionString(string path, Type db)
	{
		Database = db;
		Path = path;
		ConnectionString = $"Data Source={Path}";
	}

	/// <inheritdoc />
	public Task EnsureCreatedAsync()
	{
		if (!File.Exists(Path))
		{
			Directory.CreateDirectory(System.IO.Path.GetDirectoryName(Path)!);
			File.Create(Path).Dispose();
		}
		return Task.CompletedTask;
	}

	async Task IConfigurableService.ConfigureAsync()
	{
		await EnsureCreatedAsync().ConfigureAwait(false);
		this.MigrateUp();
	}
}