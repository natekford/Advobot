namespace Advobot.Interfaces
{
	/// <summary>
	/// Indicates that the class uses a database.
	/// </summary>
	public interface IUsesDatabase
	{
		/// <summary>
		/// The name of the database.
		/// </summary>
		string DatabaseName { get; }

		/// <summary>
		/// Starts the database connection.
		/// </summary>
		void Start();
	}
}