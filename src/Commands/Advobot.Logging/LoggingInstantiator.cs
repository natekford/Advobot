using Advobot.CommandAssemblies;
using Advobot.Logging.Database;
using Advobot.Logging.Resetters;
using Advobot.Logging.Service;
using Advobot.Serilog;
using Advobot.SQLite;

using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Logging;

public sealed class LoggingInstantiator : ICommandAssemblyInstantiator
{
	public Task AddServicesAsync(IServiceCollection services)
	{
		services
			.AddSingleton<ILoggingDatabase, LoggingDatabase>()
			.AddSQLiteFileDatabaseConnectionString<LoggingDatabase>("Logging.db")
			.AddSingleton<LoggingService>()
			.AddLogger<LoggingService>("Logging")
			.AddSingleton<INotificationDatabase, NotificationDatabase>()
			.AddSQLiteFileDatabaseConnectionString<NotificationDatabase>("Notification.db")
			.AddSingleton<NotificationService>()
			.AddLogger<NotificationService>("Notification")
			.AddSingleton<MessageQueue>()
			.AddDefaultOptionsSetter<LogActionsResetter>()
			.AddDefaultOptionsSetter<WelcomeNotificationResetter>()
			.AddDefaultOptionsSetter<GoodbyeNotificationResetter>();

		return Task.CompletedTask;
	}

	public Task ConfigureServicesAsync(IServiceProvider services)
	{
		services.GetRequiredService<IConnectionString<LoggingDatabase>>().MigrateUp();
		services.GetRequiredService<IConnectionString<NotificationDatabase>>().MigrateUp();
		services.GetRequiredService<MessageQueue>().Start();

		return Task.CompletedTask;
	}
}