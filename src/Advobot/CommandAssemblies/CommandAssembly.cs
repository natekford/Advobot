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

	/// <summary>
	/// Loads command assemblies in the supplied directory.
	/// </summary>
	/// <param name="directory">The directory to search in, this is not recursive.</param>
	/// <returns></returns>
	/// <exception cref="DllNotFoundException">No command assemblies were found in the directory.</exception>
	public static IReadOnlyCollection<CommandAssembly> Load(string directory)
	{
		var assemblies = new Dictionary<string, CommandAssembly>();
		foreach (var file in Directory.EnumerateFiles(directory, "*.dll", SearchOption.TopDirectoryOnly))
		{
			try
			{
				var assembly = Assembly.LoadFrom(file);
				var attr = assembly.GetCustomAttribute<CommandAssemblyAttribute>();
				if (attr is null)
				{
					continue;
				}

				assemblies.Add(assembly.FullName!, new(assembly, attr));
			}
			catch (BadImageFormatException)
			{
			}
		}
		if (assemblies.Count > 0)
		{
			return assemblies.Values;
		}
		throw new DllNotFoundException("Unable to find any command assemblies.");
	}
}