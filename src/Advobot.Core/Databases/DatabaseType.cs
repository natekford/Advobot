namespace Advobot.Databases
{
	/// <summary>
	/// The possible types of databases.
	/// </summary>
	public enum DatabaseType
	{
		/// <summary>
		/// Specifies to use a LiteDB database.
		/// </summary>
		LiteDB = 0,
		/// <summary>
		/// Specifies to use a MongoDB database.
		/// </summary>
		MongoDB = 1,
	}
}