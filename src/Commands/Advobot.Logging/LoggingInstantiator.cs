
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
				.AddSingleton<ILoggingDatabase, LoggingDatabase>()
				.AddSQLiteFileDatabaseConnectionStringFor<LoggingDatabase>("Logging.db")
				.AddSingleton<LoggingService>()
				.AddSingleton<INotificationDatabase, NotificationDatabase>()
				.AddSQLiteFileDatabaseConnectionStringFor<NotificationDatabase>("Notification.db")
				.AddSingleton<NotificationService>()
				.AddDefaultOptionsSetter<LogActionsResetter>()
				.AddDefaultOptionsSetter<WelcomeNotificationResetter>()
				.AddDefaultOptionsSetter<GoodbyeNotificationResetter>();

			return Task.CompletedTask;
		}

		public Task ConfigureServicesAsync(IServiceProvider services)
		{
			services.GetRequiredService<IConnectionStringFor<LoggingDatabase>>().MigrateUp();
			services.GetRequiredService<IConnectionStringFor<NotificationDatabase>>().MigrateUp();

			return Task.CompletedTask;
		}
	}
}