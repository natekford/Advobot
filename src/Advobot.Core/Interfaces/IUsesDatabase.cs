using System;

namespace Advobot.Interfaces
{
	/// <summary>
	/// Indicates that the class uses a database.
	/// </summary>
	internal interface IUsesDatabase : IDisposable
	{
		/// <summary>
		/// Starts the database connection.
		/// </summary>
		void Start();
	}
}