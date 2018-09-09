using System;
using System.Collections.Generic;
using Advobot.Classes;
using Advobot.Classes.DatabaseWrappers;

namespace Advobot.Interfaces
{
	/// <summary>
	/// Acts as a way to interface with different database types in a single object type.
	/// </summary>
	public interface IDatabaseWrapper : IDisposable
	{
		/// <summary>
		/// Executes a query and returns some values.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="options"></param>
		/// <returns></returns>
		IEnumerable<T> ExecuteQuery<T>(DBQuery<T> options) where T : DatabaseEntry;
	}
}