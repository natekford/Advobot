using System;

namespace Advobot.Tests.PreconditionTestsBases
{
	public abstract class AttributeTestsBase<T> where T : Attribute
	{
		public abstract T Instance { get; }
	}
}