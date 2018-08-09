using System;
using System.Collections;
using System.Collections.Generic;

namespace Advobot.Interfaces
{
	/// <summary>
	/// Abstraction for a <see cref="IServiceProvider"/> which can have all its services iterated.
	/// </summary>
	public interface IIterableServiceProvider : IEnumerable, IEnumerable<object>, IServiceProvider, IDisposable { }
}