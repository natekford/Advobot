using System;

namespace Advobot.Classes.Attributes
{
	/// <summary>
	/// Specifies the assembly is one that holds commands.
	/// </summary>
	[AttributeUsage(AttributeTargets.Assembly)]
	public sealed class CommandAssemblyAttribute : Attribute
	{
		public CommandAssemblyAttribute() { }
	}
}
