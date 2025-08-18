using Advobot.CommandAssemblies;
using Advobot.Logging.Database;
using Advobot.Logging.Resetters;
using Advobot.Logging.Service;
using Advobot.Serilog;
using Advobot.SQLite;

using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Logging;

public sealed class LoggingInstantiator : CommandAssemblyInstantiator
{
	public override Task AddServicesAsync(IServiceCollection services)
	{
		services
			.AddSQLiteDatabase<LoggingDatabase>("Logging")
			.AddSingletonWithLogger<LoggingService>("Logging")
			.AddSQLiteDatabase<NotificationDatabase>("Notification")
			.AddSingletonWithLogger<NotificationService>("Notification")
			.AddSingleton<MessageQueue>()
			.AddDefaultOptionsSetter<LogActionsResetter>()
			.AddDefaultOptionsSetter<WelcomeNotificationResetter>()
			.AddDefaultOptionsSetter<GoodbyeNotificationResetter>();

		return Task.CompletedTask;
	}
}