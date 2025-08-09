using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Services;

/// <summary>
/// Utilities for replacing services that are designed to be replaced.
/// </summary>
public static class ReplacableUtils
{
	/// <summary>
	/// Removes all services of the specified type that have <see cref="ReplacableAttribute"/> and then adds a singleton of the other specified type.
	/// </summary>
	/// <typeparam name="T">The type to remove.</typeparam>
	/// <typeparam name="TImpl">The type to add.</typeparam>
	/// <param name="services"></param>
	public static void ReplaceAllWithSingleton<T, TImpl>(this IServiceCollection services)
		where T : class
		where TImpl : class, T
	{
		for (var i = services.Count - 1; i >= 0; --i)
		{
			var service = services[i];
			if (service.ServiceType == typeof(T)
				&& service.ImplementationType != null
				&& service.ImplementationType
					.GetCustomAttributes(typeof(ReplacableAttribute), false).Length != 0)
			{
				services.RemoveAt(i);
			}
		}
		services.AddSingleton<T, TImpl>();
	}
}

/// <summary>
/// Attribute indicating that the service is allowed to be removed and replaced with a different implementation.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class ReplacableAttribute : Attribute;