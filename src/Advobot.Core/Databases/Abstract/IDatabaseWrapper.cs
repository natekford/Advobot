using System;
using System.Collections.Generic;

namespace Advobot.Databases.Abstract
{
	/// <summary>
	/// Acts as a way to interface with different database types in a single object type.
	/// </summary>
	internal interface IDatabaseWrapper : IDisposable
	{
		/// <summary>
		/// Executes a query and returns some values.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="options"></param>
		/// <returns></returns>
		IEnumerable<T> ExecuteQuery<T>(DatabaseQuery<T> options) where T : class, IDatabaseEntry;
	}
}