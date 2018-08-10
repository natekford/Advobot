using System;
using System.Collections;
using System.Collections.Generic;

namespace Advobot.Interfaces
{
	/// <summary>
	/// Abstraction for a <see cref="IServiceProvider"/> which can have all its services iterated.
	/// </summary>
	public interface IIterableServiceProvider : IEnumerable, IEnumerable<object>, IServiceProvider, IDisposable
	{
		/// <summary>
		/// Returns all but the specified type so a <see cref="StackOverflowException"/> will not occur.
		/// </summary>
		/// <typeparam name="T">The type of services to not return.</typeparam>
		/// <returns></returns>
		IEnumerable<object> GetServicesExcept<T>();
		/// <summary>
		/// Returns all but the specified types so a <see cref="StackOverflowException"/> will not occur.
		/// </summary>
		/// <param name="types">The types of services to not return.</param>
		/// <returns></returns>
		IEnumerable<object> GetServicesExcept(params Type[] types);
	}
}