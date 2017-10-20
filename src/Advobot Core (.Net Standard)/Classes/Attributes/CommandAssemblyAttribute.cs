using System;

namespace Advobot.Core.Classes.Attributes
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
