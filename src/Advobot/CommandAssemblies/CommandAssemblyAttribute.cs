using System.Globalization;

namespace Advobot.CommandAssemblies;

/// <summary>
/// Specifies the assembly is one that holds commands.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = true)]
public sealed class CommandAssemblyAttribute(params string[] supportedCultures) : Attribute
{
	/// <summary>
	/// An instance of <see cref="InstantiatorType"/>.
	/// </summary>
	public CommandAssemblyInstantiator? Instantiator { get; private set; }
	/// <summary>
	/// Specifies things to do before these commands can start being used.
	/// </summary>
	/// <exception cref="ArgumentException"></exception>
	public Type? InstantiatorType
	{
		get;
		set
		{
			if (value is null)
			{
				field = null;
				Instantiator = null;
				return;
			}

			object i;
			try
			{
				i = Activator.CreateInstance(value)!;
			}
			catch
			{
				throw new ArgumentException("Must have a parameterless constructor.", nameof(InstantiatorType));
			}

			if (i is not CommandAssemblyInstantiator instantiator)
			{
				throw new ArgumentException($"Must implement {nameof(CommandAssemblyInstantiator)}.", nameof(InstantiatorType));
			}

			field = value;
			Instantiator = instantiator;
		}
	}
	/// <summary>
	/// The cultures this command assembly supports.
	/// </summary>
	public IReadOnlyList<CultureInfo> SupportedCultures { get; } = [.. supportedCultures.Select(CultureInfo.GetCultureInfo)];
}