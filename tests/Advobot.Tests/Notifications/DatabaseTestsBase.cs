using System;
using System.Threading.Tasks;

using Advobot.Logging.Database;
using Advobot.Services.Time;
using Advobot.SQLite;
using Advobot.Tests.Fakes.Database;

using AdvorangesUtils;

using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Tests.Notifications
{
	public abstract class DatabaseTestsBase
	{
		public static readonly Random Rng = new Random();

		protected IServiceProvider Services { get; }
		protected ITime Time => Services.GetRequiredService<ITime>();

		protected DatabaseTestsBase()
		{
			Services = new ServiceCollection()
				.AddSingleton<NotificationDatabase>()
				.AddSingleton<INotificationDatabaseStarter, FakeSQLiteDatabaseStarter>()
				.BuildServiceProvider();
		}

		protected async Task<NotificationDatabase> GetDatabaseAsync()
		{
			var starter = Services.GetRequiredService<INotificationDatabaseStarter>();
			await starter.EnsureCreatedAsync().CAF();
			starter.MigrateUp();

			return Services.GetRequiredService<NotificationDatabase>();
		}
	}
}