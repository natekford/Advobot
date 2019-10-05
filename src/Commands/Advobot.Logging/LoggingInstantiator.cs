using System;
using System.Threading.Tasks;
using Advobot.CommandAssemblies;
using Advobot.Logging.Service;
using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Logging
{
	public sealed class LoggingInstantiator : ICommandAssemblyInstantiator
	{
		public Task AddServicesAsync(IServiceCollection services)
		{
			services.AddSingleton<ILogService, LogService>();
			return Task.CompletedTask;
		}

		public Task ConfigureServicesAsync(IServiceProvider services)
		{
			services.GetRequiredService<ILogService>();
			return Task.CompletedTask;
		}
	}
}