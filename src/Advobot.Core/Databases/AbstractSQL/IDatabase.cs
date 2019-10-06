using System.Collections.Generic;
using System.Threading.Tasks;

namespace Advobot.Databases.AbstractSQL
{
	/// <summary>
	/// Abstraction for a database.
	/// </summary>
	public interface IDatabase
	{
		/// <summary>
		/// Makes sure all the tables exist and the database exists.
		/// </summary>
		/// <returns></returns>
		Task<IReadOnlyList<string>> CreateDatabaseAsync();
	}
}