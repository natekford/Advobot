namespace Advobot.SQLite;

/// <summary>
/// Used for starting a SQLite database from a system file.
/// </summary>
public sealed class SQLiteSystemFileDatabaseConnectionString : IConnectionString<object>
{
	/// <inheritdoc />
	public string ConnectionString { get; }
	/// <summary>
	/// The path of the SQLite database.
	/// </summary>
	public string Path { get; }

	/// <summary>
	/// Creates an instance of <see cref="SQLiteSystemFileDatabaseConnectionString"/>.
	/// </summary>
	/// <param name="path"></param>
	public SQLiteSystemFileDatabaseConnectionString(string path)
	{
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
}