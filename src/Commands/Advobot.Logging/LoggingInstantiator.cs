using System;
using System.Threading.Tasks;

using Advobot.CommandAssemblies;
using Advobot.Logging.Database;
using Advobot.Logging.Service;

using AdvorangesUtils;

using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Logging
{
	public sealed class LoggingInstantiator : ICommandAssemblyInstantiator
	{
		public Task AddServicesAsync(IServiceCollection services)
		{
			services
				.AddSingleton<LoggingDatabase>()
				.AddSingleton<ILoggingService, LoggingService>()
				.AddSingleton<ILoggingDatabaseStarter, LoggingDatabaseStarter>();
			return Task.CompletedTask;
		}

		public async Task ConfigureServicesAsync(IServiceProvider services)
		{
			var db = services.GetRequiredService<LoggingDatabase>();
			await db.CreateDatabaseAsync().CAF();

			//Needed to instasntiate the log service
			services.GetRequiredService<ILoggingService>();
		}
	}
}