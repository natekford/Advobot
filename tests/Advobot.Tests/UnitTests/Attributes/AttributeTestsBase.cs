using System;

namespace Advobot.Tests.UnitTests.Attributes
{
	public abstract class AttributeTestsBase<T> where T : Attribute, new()
	{
		public T Instance { get; } = new T();
	}
}