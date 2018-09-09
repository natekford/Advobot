namespace Advobot.Interfaces
{
	/// <summary>
	/// Abstraction for creating <see cref="IDatabaseWrapper"/>.
	/// </summary>
	public interface IDatabaseWrapperFactory
	{
		/// <summary>
		/// Creates a database wrapper.
		/// </summary>
		/// <param name="databaseName">The name of the database.</param>
		/// <returns></returns>
		IDatabaseWrapper CreateWrapper(string databaseName);
	}
}