using System;

namespace Advobot.Databases
{
	/// <summary>
	/// Indicates that the class uses a database.
	/// </summary>
	public interface IUsesDatabase : IDisposable
	{
		/// <summary>
		/// Starts the database connection.
		/// </summary>
		void Start();
	}
}