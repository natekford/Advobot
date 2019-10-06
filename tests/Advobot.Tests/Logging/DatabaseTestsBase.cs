using System;
using System.Threading.Tasks;

using Advobot.Logging.Database;
using Advobot.Services.Time;
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
				.AddSingleton<ILoggingDatabaseStarter, FakeLoggingDatabaseStarter>()
				.BuildServiceProvider();
		}

		protected async Task<LoggingDatabase> GetDatabaseAsync()
		{
			var db = Services.GetRequiredService<LoggingDatabase>();
			await db.CreateDatabaseAsync().CAF();
			return db;
		}

		private sealed class FakeLoggingDatabaseStarter : FakeSQLiteDatabaseStarter, ILoggingDatabaseStarter
		{
			public override string GetDbFileName() => "Logging.db";
		}
	}
}