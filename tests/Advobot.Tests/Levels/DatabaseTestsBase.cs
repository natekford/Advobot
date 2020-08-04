using System;
using System.Threading.Tasks;

using Advobot.Levels.Database;
using Advobot.Services.Time;
using Advobot.SQLite;
using Advobot.Tests.Fakes.Database;

using AdvorangesUtils;

using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Tests.Levels
{
	public abstract class DatabaseTestsBase
	{
		public static readonly Random Rng = new Random();

		protected IServiceProvider Services { get; }
		protected ITime Time => Services.GetRequiredService<ITime>();

		protected DatabaseTestsBase()
		{
			Services = new ServiceCollection()
				.AddSingleton<LevelDatabase>()
				.AddSingleton<ITime, DefaultTime>()
				.AddSingleton<ILevelDatabaseStarter, FakeSQLiteDatabaseStarter>()
				.BuildServiceProvider();
		}

		protected async Task<LevelDatabase> GetDatabaseAsync()
		{
			var starter = Services.GetRequiredService<ILevelDatabaseStarter>();
			await starter.EnsureCreatedAsync().CAF();
			starter.MigrateUp();

			return Services.GetRequiredService<LevelDatabase>();
		}
	}
}