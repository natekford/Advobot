using System;
using System.Threading.Tasks;

using Advobot.Logging.Database;
using Advobot.Services.Time;
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
				.AddSingleton<INotificationDatabaseStarter, FakeLoggingDatabaseStarter>()
				.BuildServiceProvider();
		}

		protected async Task<NotificationDatabase> GetDatabaseAsync()
		{
			var db = Services.GetRequiredService<NotificationDatabase>();
			await db.CreateDatabaseAsync().CAF();
			return db;
		}

		private sealed class FakeLoggingDatabaseStarter : FakeSQLiteDatabaseStarter, INotificationDatabaseStarter
		{
			public override string GetDbFileName() => "Notifications.db";
		}
	}
}