using System;

namespace Advobot.Classes.Attributes
{
	/// <summary>
	/// Use on the constructor intended to be used by custom arguments.
	/// </summary>
	[AttributeUsage(AttributeTargets.Constructor)]
	public class CustomArgumentConstructorAttribute : Attribute
	{
		public CustomArgumentConstructorAttribute() { }
	}
}
