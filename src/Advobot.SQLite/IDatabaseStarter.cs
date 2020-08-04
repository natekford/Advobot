using System.Threading.Tasks;

namespace Advobot.SQLite
{
	/// <summary>
	/// Starts a SQL database.
	/// </summary>
	public interface IDatabaseStarter
	{
		/// <summary>
		/// Ensures the database is ready to be modified.
		/// </summary>
		/// <returns></returns>
		Task EnsureCreatedAsync();

		/// <summary>
		/// Gets the connection string.
		/// </summary>
		/// <returns></returns>
		string GetConnectionString();
	}
}