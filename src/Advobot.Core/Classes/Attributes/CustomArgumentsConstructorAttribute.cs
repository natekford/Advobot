using System;

namespace Advobot.Core.Classes.Attributes
{
	/// <summary>
	/// Use on the constructor intended to be used by custom arguments.
	/// </summary>
	[AttributeUsage(AttributeTargets.Constructor)]
	public class NamedArgumentConstructorAttribute : Attribute
	{
		public NamedArgumentConstructorAttribute() { }
	}
}
