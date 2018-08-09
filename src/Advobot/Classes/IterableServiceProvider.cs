﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Advobot.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Classes
{
	/// <summary>
	/// Allows a way to get all the services from a service provider.
	/// Acts as a wrapper around a service provider.
	/// </summary>
	public class IterableServiceProvider : IIterableServiceProvider
	{
		private readonly ServiceProvider _Provider;
		private readonly ServiceCollection _Services;

		/// <summary>
		/// Creates an instance of <see cref="IterableServiceProvider"/> with the supplied services.
		/// </summary>
		/// <param name="services"></param>
		/// <param name="instantiateInCtor">Whether to instantiate every service in the constructor.</param>
		public IterableServiceProvider(ServiceCollection services, bool instantiateInCtor)
			: this(services.BuildServiceProvider(), services, instantiateInCtor) { }
		private IterableServiceProvider(ServiceProvider provider, ServiceCollection services, bool instantiateInCtor)
		{
			_Services = services;
			_Provider = provider;
			if (instantiateInCtor)
			{
				foreach (var type in _Services.Select(x => x.ServiceType))
				{
					_Provider.GetService(type);
				}
			}
		}

		/// <summary>
		/// Creates a wrapper around an already existing <see cref="ServiceProvider"/>.
		/// <paramref name="provider"/> and <paramref name="services"/> must be from the same services.
		/// </summary>
		/// <param name="provider"></param>
		/// <param name="services"></param>
		/// <returns></returns>
		public static IterableServiceProvider CreateFromExisting(ServiceProvider provider, ServiceCollection services)
		{
			return new IterableServiceProvider(provider, services, false);
		}
		public IEnumerable<T> GetServicesOfType<T>()
		{
			throw new NotImplementedException();
			//foreach (var type in _Services.Select(x => x.ServiceType).Where)
		}
		/// <inheritdoc />
		public void Dispose()
		{
			_Provider.Dispose();
		}
		/// <inheritdoc />
		public object GetService(Type serviceType)
		{
			return _Provider.GetService(serviceType);
		}
		/// <inheritdoc />
		public IEnumerator<object> GetEnumerator()
		{
			//TODO: fix stackoverflow error which occurs here.
			//probs kinda big fix to do so maybe just remove this class?
			foreach (var type in _Services.Select(x => x.ServiceType))
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