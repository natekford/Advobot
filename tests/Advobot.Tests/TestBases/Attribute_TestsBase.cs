using System;

namespace Advobot.Tests.TestBases
{
	public abstract class Attribute_TestsBase<T> where T : Attribute
	{
		protected abstract T Instance { get; }
	}
}