using System;

namespace Advobot.Attributes
{
	/// <summary>
	/// Indicates that the settings should not be saved.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public sealed class DontSaveAfterExecutionAttribute : Attribute { }
}