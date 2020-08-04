using System;
using System.Threading.Tasks;

using Advobot.Logging.Database;
using Advobot.Services.Time;
using Advobot.SQLite;
using Advobot.Tests.Fakes.Database;

using AdvorangesUtils;

using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Tests.Logging
{
	public abstract class DatabaseTestsBase
	{
		public static readonly Random Rng = new Random();

		protected IServiceProvider Services { get; }
		protected ITime Time => Services.GetRequiredService<ITime>();

		protected DatabaseTestsBase()
		{
			Services = new ServiceCollection()
				.AddSingleton<LoggingDatabase>()
				.AddSingleton<ILoggingDatabaseStarter, FakeSQLiteDatabaseStarter>()
				.BuildServiceProvider();
		}

		protected async Task<LoggingDatabase> GetDatabaseAsync()
		{
			var starter = Services.GetRequiredService<ILoggingDatabaseStarter>();
			await starter.EnsureCreatedAsync().CAF();
			starter.MigrateUp();

			return Services.GetRequiredService<LoggingDatabase>();
		}
	}
}