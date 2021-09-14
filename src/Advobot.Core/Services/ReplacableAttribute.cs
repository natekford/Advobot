
using AdvorangesUtils;

using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Services
{
	/// <summary>
	/// Utilities for replacing services that are designed to be replaced.
	/// </summary>
	public static class ReplacableUtils
	{
		/// <summary>
		/// Removes all services of the specified type that have <see cref="ReplacableAttribute"/>.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="services"></param>
		public static void Remove<T>(this IServiceCollection services)
			where T : class
		{
			services.RemoveAll(x =>
			{
				return x.ServiceType == typeof(T)
					&& x.ImplementationType != null
					&& x.ImplementationType
						.GetCustomAttributes(typeof(ReplacableAttribute), false).Length != 0;
			});
		}

		/// <summary>
		/// Removes all services of the specified type that have <see cref="ReplacableAttribute"/> and then adds a singleton of the other specified type.
		/// </summary>
		/// <typeparam name="T">The type to remove.</typeparam>
		/// <typeparam name="TImpl">The type to add.</typeparam>
		/// <param name="services"></param>
		public static void ReplaceWithSingleton<T, TImpl>(this IServiceCollection services)
			where T : class
			where TImpl : class, T
		{
			services.Remove<T>();
			services.AddSingleton<T, TImpl>();
		}
	}

	/// <summary>
	/// Attribute indicating that the service is allowed to be removed and replaced with a different implementation.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	public sealed class ReplacableAttribute : Attribute
	{
	}
}