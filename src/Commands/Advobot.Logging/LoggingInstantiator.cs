using System;
using System.Threading.Tasks;

using Advobot.CommandAssemblies;
using Advobot.Logging.Database;
using Advobot.Logging.OptionSetters;
using Advobot.Logging.Service;
using Advobot.Services.GuildSettings;
using Advobot.Utilities;

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
				.AddSingleton<ILoggingDatabaseStarter, LoggingDatabaseStarter>()
				.AddSingleton<NotificationDatabase>()
				.AddSingleton<INotificationService, NotificationService>()
				.AddSingleton<INotificationDatabaseStarter, NotificationDatabaseStarter>()
				.AddDefaultOptionsSetter<DefaultLogActionsSetter>();
			return Task.CompletedTask;
		}

		public async Task ConfigureServicesAsync(IServiceProvider services)
		{
			{
				var db = services.GetRequiredService<LoggingDatabase>();
				await db.CreateDatabaseAsync().CAF();

				//Needed to instasntiate the log service
				services.GetRequiredService<ILoggingService>();
			}

			{
				var db = services.GetRequiredService<NotificationDatabase>();
				await db.CreateDatabaseAsync().CAF();

				//Needed to instasntiate the notification service
				services.GetRequiredService<INotificationService>();
			}
		}
	}
}