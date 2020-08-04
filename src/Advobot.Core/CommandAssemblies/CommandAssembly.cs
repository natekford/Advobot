using System.Collections.Generic;
using System.Globalization;
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
		public Assembly Assembly { get; }

		/// <summary>
		/// The instantiator to use for this command assembly.
		/// </summary>
		public ICommandAssemblyInstantiator? Instantiator { get; }

		/// <summary>
		/// The cultures this command assembly supports.
		/// </summary>
		public IReadOnlyList<CultureInfo> SupportedCultures { get; }

		/// <summary>
		/// Creates an instance of <see cref="CommandAssembly"/>.
		/// </summary>
		/// <param name="assembly"></param>
		/// <param name="attribute"></param>
		public CommandAssembly(Assembly assembly, CommandAssemblyAttribute attribute)
		{
			Assembly = assembly;
			Instantiator = attribute.Instantiator;
			SupportedCultures = attribute.SupportedCultures;
		}
	}
}