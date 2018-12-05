namespace Advobot.Interfaces
{
	/// <summary>
	/// Indicates that the class uses a database.
	/// </summary>
	public interface IUsesDatabase
	{
		/// <summary>
		/// Starts the database connection.
		/// </summary>
		void Start();
	}
}