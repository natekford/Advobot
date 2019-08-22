using System;

namespace Advobot.Attributes
{
	/// <summary>
	/// Indicates not to add the module as a help entry automatically.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	public class DontAddHelpEntryAttribute : Attribute { }
}
