using Microsoft.Extensions.DependencyInjection;

namespace Advobot.CommandAssemblies;

/// <summary>
/// Specifies how to instantiate the command assembly.
/// </summary>
public abstract class CommandAssemblyInstantiator
{
	/// <summary>
	/// Adds some services to <paramref name="services"/>.
	/// </summary>
	/// <param name="services"></param>
	/// <returns></returns>
	public virtual Task AddServicesAsync(IServiceCollection services)
		=> Task.CompletedTask;

	/// <summary>
	/// Configures the services and makes sure they are set up correctly.
	/// </summary>
	/// <param name="services"></param>
	/// <returns></returns>
	public virtual Task ConfigureServicesAsync(IServiceProvider services)
		=> Task.CompletedTask;
}