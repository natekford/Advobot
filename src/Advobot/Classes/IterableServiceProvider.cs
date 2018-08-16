using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Advobot.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Classes
{
	/// <summary>
	/// Allows a way to get all the singleton services from a service provider.
	/// Acts as a wrapper around a service provider.
	/// </summary>
	public class IterableServiceProvider : IIterableServiceProvider
	{
		private readonly IServiceProvider _Provider;
		private readonly IServiceCollection _Services;

		/// <summary>
		/// Creates an instance of <see cref="IterableServiceProvider"/> with the supplied services.
		/// </summary>
		/// <param name="services"></param>
		/// <param name="instantiateInCtor">Whether to instantiate every service in the constructor.</param>
		public IterableServiceProvider(IServiceCollection services, bool instantiateInCtor)
			: this(services.BuildServiceProvider(), services, instantiateInCtor) { }
		private IterableServiceProvider(IServiceProvider provider, IServiceCollection services, bool instantiateInCtor)
		{
			_Services = services;
			_Provider = provider;
			if (instantiateInCtor)
			{
				foreach (var type in GetSingletonTypes())
				{
					_Provider.GetService(type);
				}
			}
		}

		/// <summary>
		/// Creates a wrapper around an already existing <see cref="IServiceProvider"/>.
		/// <paramref name="provider"/> and <paramref name="services"/> should be the same services.
		/// </summary>
		/// <param name="provider"></param>
		/// <param name="services"></param>
		/// <returns></returns>
		public static IterableServiceProvider CreateFromExisting(IServiceProvider provider, IServiceCollection services)
		{
			return new IterableServiceProvider(provider, services, false);
		}
		/// <summary>
		/// Gets all the singletons from the provider.
		/// </summary>
		/// <returns></returns>
		private IEnumerable<Type> GetSingletonTypes()
		{
			return _Services.Where(x => x.Lifetime == ServiceLifetime.Singleton).Select(x => x.ServiceType);
		}
		/// <inheritdoc />
		public IEnumerable<Type> GetAllTypes()
		{
			return _Services.Select(x => x.ServiceType);
		}
		/// <inheritdoc />
		public IEnumerable<object> GetServicesExcept<T>()
		{
			return GetServicesExcept(typeof(T));
		}
		/// <inheritdoc />
		public IEnumerable<object> GetServicesExcept(params Type[] types)
		{
			foreach (var type in GetSingletonTypes())
			{
				if (!types.Contains(type))
				{
					yield return _Provider.GetService(type);
				}
			}
		}
		/// <inheritdoc />
		public void Dispose()
		{
			if (_Provider is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}
		/// <inheritdoc />
		public object GetService(Type serviceType)
		{
			return _Provider.GetService(serviceType);
		}
		/// <inheritdoc />
		public IEnumerator<object> GetEnumerator()
		{
			foreach (var type in GetSingletonTypes())
			{
				yield return _Provider.GetService(type);
			}
		}
		/// <inheritdoc />
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}