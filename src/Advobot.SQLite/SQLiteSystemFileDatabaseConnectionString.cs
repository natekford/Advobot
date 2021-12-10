namespace Advobot.SQLite;

/// <summary>
/// Used for starting a SQLite database from a system file.
/// </summary>
public sealed class SQLiteSystemFileDatabaseConnectionString : IConnectionStringFor<object>
{
	public string ConnectionString { get; }
	public string Location { get; }

	/// <summary>
	/// Creates an instance of <see cref="SQLiteSystemFileDatabaseConnectionString"/>.
	/// </summary>
	/// <param name="path"></param>
	public SQLiteSystemFileDatabaseConnectionString(string path)
	{
		Location = path;
		ConnectionString = $"Data Source={Location}";
	}

	/// <inheritdoc />
	public Task EnsureCreatedAsync()
	{
		if (!File.Exists(Location))
		{
			Directory.CreateDirectory(Path.GetDirectoryName(Location));
			File.Create(Location).Dispose();
		}
		return Task.CompletedTask;
	}
}