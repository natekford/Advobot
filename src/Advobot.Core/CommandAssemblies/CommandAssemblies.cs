using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Advobot.CommandAssemblies
{
	/// <summary>
	/// A collection of <see cref="CommandAssembly"/>.
	/// </summary>
	public sealed class CommandAssemblyCollection
	{
		/// <summary>
		/// The assemblies to be used as command assemblies.
		/// </summary>
		public IReadOnlyCollection<CommandAssembly> Assemblies => _Assemblies.Values;

		private readonly Dictionary<string, CommandAssembly> _Assemblies = new Dictionary<string, CommandAssembly>();

		private CommandAssemblyCollection() { }

		private void Add(Assembly assembly)
		{
			var attr = assembly.GetCustomAttribute<CommandAssemblyAttribute>();
			if (attr != null)
			{
				var name = assembly.FullName;
				if (_Assemblies.TryGetValue(name, out _))
				{
					throw new InvalidOperationException($"Duplicate assembly name: {name}");
				}
				_Assemblies[name] = new CommandAssembly(assembly, attr);
			}
		}

		/// <summary>
		/// Returns all the assemblies in the base directory which have the <see cref="CommandAssemblyAttribute"/>.
		/// This loads assemblies with a matching name so this can be a risk to use if bad files are in the folder.
		/// </summary>
		/// <returns></returns>
		public static CommandAssemblyCollection Find()
		{
			var assemblies = new CommandAssemblyCollection();
			foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				assemblies.Add(assembly);
			}
#warning fix this security risk
			//This is probably a huge security risk. Like mega huge.
			//Probably should pass in a json file with the command assembly locations tbh
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
	}
}