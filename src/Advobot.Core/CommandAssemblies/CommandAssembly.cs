using System.Diagnostics;
using System.Globalization;
using System.Reflection;

namespace Advobot.CommandAssemblies;

/// <summary>
/// Holds an assembly and the attribute marking it as a command assembly.
/// </summary>
[DebuggerDisplay("{DebuggerDisplay,nq}")]
public readonly struct CommandAssembly(Assembly assembly, CommandAssemblyAttribute attribute)
{
	/// <summary>
	/// The assembly marked as a command assembly.
	/// </summary>
	public Assembly Assembly { get; } = assembly;
	/// <summary>
	/// The instantiator to use for this command assembly.
	/// </summary>
	public ICommandAssemblyInstantiator? Instantiator { get; } = attribute.Instantiator;
	/// <summary>
	/// The cultures this command assembly supports.
	/// </summary>
	public IReadOnlyList<CultureInfo> SupportedCultures { get; } = attribute.SupportedCultures;
	private string DebuggerDisplay => Assembly.GetName()?.Name ?? "null";
}