using System;
using System.Threading.Tasks;

using Advobot.CommandAssemblies;
using Advobot.Logging.Database;
using Advobot.Logging.OptionSetters;
using Advobot.Logging.Service;
using Advobot.SQLite;
using Advobot.Utilities;

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
				.AddDefaultOptionsSetter<LogActionsResetter>()
				.AddDefaultOptionsSetter<WelcomeNotificationResetter>()
				.AddDefaultOptionsSetter<GoodbyeNotificationResetter>();

			return Task.CompletedTask;
		}

		public Task ConfigureServicesAsync(IServiceProvider services)
		{
			{
				services.GetRequiredService<ILoggingDatabaseStarter>().MigrateUp();

				// Needed to instantiate the log service
				services.GetRequiredService<ILoggingService>();
			}

			{
				services.GetRequiredService<INotificationDatabaseStarter>().MigrateUp();

				// Needed to instasntiate the notification service
				services.GetRequiredService<INotificationService>();
			}

			return Task.CompletedTask;
		}
	}
}