using Advobot.SQLite;

namespace Advobot.Tests.Fakes.Database;

public sealed class FakeSQLiteConnectionString : IConnectionString<object>
{
	public string ConnectionString { get; }
	public Type Database { get; }
	public string Id { get; }
	public string Location { get; }

	public FakeSQLiteConnectionString(Type database)
	{
		Database = database;
		Id = Guid.NewGuid().ToString();
		Location = Path.Combine(Environment.CurrentDirectory, "TestDatabases", $"DELETE_ME_{Id}.db");
		ConnectionString = $"Data Source={Location}";
	}

	/// <inheritdoc />
	public Task EnsureCreatedAsync()
	{
		Directory.CreateDirectory(Path.GetDirectoryName(Location)!);
		if (!File.Exists(Location))
		{
			File.Create(Location).Dispose();
		}
		return Task.CompletedTask;
	}
}