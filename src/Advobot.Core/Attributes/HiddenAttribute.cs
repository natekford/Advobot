using System;

namespace Advobot.Attributes
{
	/// <summary>
	/// Indicates that the method or parameter should not be included in help entries.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public sealed class HiddenAttribute : Attribute { }
}
