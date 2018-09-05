using System;

namespace Advobot.Classes.Attributes
{
	/// <summary>
	/// Use on the constructor intended to be used by custom arguments.
	/// </summary>
	[AttributeUsage(AttributeTargets.Constructor, AllowMultiple = false, Inherited = true)]
	public sealed class NamedArgumentConstructorAttribute : Attribute { }
}
