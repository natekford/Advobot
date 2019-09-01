using System;

namespace Advobot.Tests.Core.Attributes
{
	public abstract class AttributeTestsBase<T> where T : Attribute
	{
		public abstract T Instance { get; }
	}
}