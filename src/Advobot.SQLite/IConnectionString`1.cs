namespace Advobot.SQLite;

/// <summary>
/// Provides a connection string for the type param.
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IConnectionString<in T> : IConnectionString
{
}