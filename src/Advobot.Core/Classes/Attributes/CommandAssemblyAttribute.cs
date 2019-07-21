using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace Advobot.Classes.Attributes
{
	/// <summary>
	/// Specifies the assembly is one that holds commands.
	/// </summary>
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = true)]
	public sealed class CommandAssemblyAttribute : Attribute
	{
		/// <summary>
		/// Specifies things to do before these commands can start being used.
		/// </summary>
		public Type? InstantiationFactory { get; set; }

		/// <summary>
		/// Instantiates the assembly and calls a start up method.
		/// </summary>
		/// <param name="services"></param>
		/// <returns></returns>
		public Task InstantiateAsync(IServiceCollection services)
		{
			if (InstantiationFactory == null)
			{
				return Task.CompletedTask;
			}

			var instance = Activator.CreateInstance(InstantiationFactory);
			var cast = (ICommandAssemblyInstantiator)instance;
			return cast.Instantiate(services);
		}
	}

	/// <summary>
	/// A collection of <see cref="CommandAssembly"/>.
	/// </summary>
	public sealed class CommandAssemblies
	{
		/// <summary>
		/// The assemblies to be used as command assemblies.
		/// </summary>
		public IReadOnlyCollection<CommandAssembly> Assemblies => _Assemblies.Values;

		private readonly Dictionary<string, CommandAssembly> _Assemblies = new Dictionary<string, CommandAssembly>();

		private CommandAssemblies() { }

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
		public static CommandAssemblies Find()
		{
			var assemblies = new CommandAssemblies();
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

	/// <summary>
	/// Specifies how to instantiate the command assembly.
	/// </summary>
	public interface ICommandAssemblyInstantiator
	{
		/// <summary>
		/// Does some start up work when the assembly in created.
		/// </summary>
		/// <param name="services"></param>
		/// <returns></returns>
		Task Instantiate(IServiceCollection services);
	}
}
