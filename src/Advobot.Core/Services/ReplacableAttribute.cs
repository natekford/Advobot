using System;

namespace Advobot.Services
{
	/// <summary>
	/// Attribute indicating that the service is allowed to be removed and replaced with a different implementation.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	public sealed class ReplacableAttribute : Attribute
	{
	}
}