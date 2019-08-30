using System;

namespace Advobot.Attributes
{
	/// <summary>
	/// Indicates that the class, method, or parameter should not be included in help entries.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public sealed class HiddenAttribute : Attribute { }
}