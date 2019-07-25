using System;
using System.Linq;

namespace Advobot.CommandMarking
{
	/// <summary>
	/// Specifies the assembly is one that holds commands.
	/// </summary>
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = true)]
	public sealed class CommandAssemblyAttribute : Attribute
	{
		/// <summary>
		/// An instance of <see cref="InstantiatorType"/>.
		/// </summary>
		public ICommandAssemblyInstantiator? Instantiator { get; private set; }
		/// <summary>
		/// Specifies things to do before these commands can start being used.
		/// </summary>
		public Type? InstantiatorType
		{
			get => _InstatiatorType;
			set
			{
				if (value != null && !value.GetInterfaces().Contains(typeof(ICommandAssemblyInstantiator)))
				{
					throw new ArgumentException($"{nameof(InstantiatorType)} must implement {nameof(ICommandAssemblyInstantiator)}");
				}
				_InstatiatorType = value;

				var instance = Activator.CreateInstance(InstantiatorType);
				Instantiator = (ICommandAssemblyInstantiator)instance;
			}
		}
		private Type? _InstatiatorType;
	}
}