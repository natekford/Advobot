using System;

namespace Advobot.Tests.Core.Attributes
{
	public abstract class AttributeTestsBase<T> where T : Attribute, new()
	{
		public T Instance { get; } = new T();
	}
}