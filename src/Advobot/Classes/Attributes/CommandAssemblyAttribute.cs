using System;

namespace Advobot.Classes.Attributes
{
	/// <summary>
	/// Specifies the assembly is one that holds commands.
	/// </summary>
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = true)]
	public sealed class CommandAssemblyAttribute : Attribute { }
}
