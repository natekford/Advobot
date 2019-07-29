using System.Reflection;

namespace Advobot.CommandAssemblies
{
	/// <summary>
	/// Holds an assembly and the attribute marking it as a command assembly.
	/// </summary>
	public readonly struct CommandAssembly
	{
		/// <summary>
		/// The assembly marked as a command assembly.
		/// </summary>
		public readonly Assembly Assembly;
		/// <summary>
		/// The attribute marking it as a command assembly.
		/// </summary>
		public readonly CommandAssemblyAttribute Attribute;

		/// <summary>
		/// Creates an instance of <see cref="CommandAssembly"/>.
		/// </summary>
		/// <param name="assembly"></param>
		/// <param name="attribute"></param>
		public CommandAssembly(Assembly assembly, CommandAssemblyAttribute attribute)
		{
			Assembly = assembly;
			Attribute = attribute;
		}
	}
}