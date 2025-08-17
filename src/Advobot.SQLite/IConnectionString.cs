namespace Advobot.SQLite;

/// <summary>
/// Provides a connection string for a database.
/// </summary>
public interface IConnectionString
{
	/// <summary>
	/// A SQLite connection string.
	/// </summary>
	string ConnectionString { get; }
}