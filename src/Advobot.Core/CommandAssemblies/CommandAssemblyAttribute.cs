using System.Collections.Immutable;
using System.Globalization;

namespace Advobot.CommandAssemblies;

/// <summary>
/// Specifies the assembly is one that holds commands.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = true)]
public sealed class CommandAssemblyAttribute : Attribute
{
	private Type? _InstatiatorType;

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
			if (value == null)
			{
				_InstatiatorType = null;
				Instantiator = null;
			}

			object i;
			try
			{
				i = Activator.CreateInstance(value);
			}
			catch
			{
				throw new ArgumentException("Must have a parameterless constructor.", nameof(InstantiatorType));
			}

			if (i is not ICommandAssemblyInstantiator instantiator)
			{
				throw new ArgumentException($"Must implement {nameof(ICommandAssemblyInstantiator)}.", nameof(InstantiatorType));
			}

			_InstatiatorType = value;
			Instantiator = instantiator;
		}
	}

	/// <summary>
	/// The cultures this command assembly supports.
	/// </summary>
	public IReadOnlyList<CultureInfo> SupportedCultures { get; }

	/// <summary>
	/// Creates an instance of <see cref="CommandAssemblyAttribute"/>.
	/// </summary>
	/// <param name="supportedCultures"></param>
	public CommandAssemblyAttribute(params string[] supportedCultures)
	{
		SupportedCultures = supportedCultures.Select(CultureInfo.GetCultureInfo).ToImmutableArray();
	}
}