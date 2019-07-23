using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Advobot.CommandMarking
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
		public Type? InstantiationFactory
		{
			get => _InstantiationFactory;
			set
			{
				if (value != null && !value.GetInterfaces().Contains(typeof(ICommandAssemblyInstantiator)))
				{
					throw new ArgumentException($"{nameof(InstantiationFactory)} must implement {nameof(ICommandAssemblyInstantiator)}");
				}
				_InstantiationFactory = value;
			}
		}
		private Type? _InstantiationFactory;

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
}