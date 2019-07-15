namespace Advobot.Databases.Abstract
{
	/// <summary>
	/// Abstraction for creating <see cref="IDatabaseWrapper"/>.
	/// </summary>
	internal interface IDatabaseWrapperFactory
	{
		/// <summary>
		/// Creates a database wrapper.
		/// </summary>
		/// <param name="databaseName">The connection string of the database.</param>
		/// <returns></returns>
		IDatabaseWrapper CreateWrapper(string databaseName);
	}
}