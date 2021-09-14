using System.Reflection;

namespace Advobot.CommandAssemblies
{
	/// <summary>
	/// A collection of <see cref="CommandAssembly"/>.
	/// </summary>
	public sealed class CommandAssemblyCollection
	{
		private readonly Dictionary<string, CommandAssembly> _Assemblies = new();

		/// <summary>
		/// The assemblies to be used as command assemblies.
		/// </summary>
		public IReadOnlyCollection<CommandAssembly> Assemblies => _Assemblies.Values;

		private CommandAssemblyCollection()
		{
		}

		/// <summary>
		/// Returns all the assemblies in the base directory which have the <see cref="CommandAssemblyAttribute"/>.
		/// This loads assemblies with a matching name so this can be a risk to use if bad files are in the folder.
		/// </summary>
		/// <returns></returns>
		public static CommandAssemblyCollection Find()
		{
			var assemblies = new CommandAssemblyCollection();
			foreach (var file in Directory.EnumerateFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll", SearchOption.TopDirectoryOnly))
			{
				assemblies.Add(Assembly.LoadFrom(file));
			}
			if (assemblies.Assemblies.Count > 0)
			{
				return assemblies;
			}
			throw new DllNotFoundException("Unable to find any command assemblies.");
		}

		private void Add(Assembly assembly)
		{
			var attr = assembly.GetCustomAttribute<CommandAssemblyAttribute>();
			if (attr is null)
			{
				return;
			}
			var name = assembly.FullName;
			if (_Assemblies.TryGetValue(name, out _))
			{
				throw new InvalidOperationException($"Duplicate assembly name: {name}");
			}
			_Assemblies[name] = new(assembly, attr);
		}
	}
}