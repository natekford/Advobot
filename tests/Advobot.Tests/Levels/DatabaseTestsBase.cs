using System;
using System.Threading.Tasks;

using Advobot.Levels.Database;
using Advobot.Services.Time;

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
				.AddSingleton<IDatabaseStarter, SQLiteTestDatabaseFactory>()
				.BuildServiceProvider();
		}

		protected async Task<LevelDatabase> GetDatabaseAsync()
		{
			var db = Services.GetRequiredService<LevelDatabase>();
			await db.CreateDatabaseAsync().CAF();
			return db;
		}
	}
}