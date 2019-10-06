using System;
using System.Threading.Tasks;

using Advobot.Levels.Database;
using Advobot.Services.Time;
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
				.AddSingleton<ILevelDatabaseStarter, FakeLevelDatabaseStarter>()
				.BuildServiceProvider();
		}

		protected async Task<LevelDatabase> GetDatabaseAsync()
		{
			var db = Services.GetRequiredService<LevelDatabase>();
			await db.CreateDatabaseAsync().CAF();
			return db;
		}

		private sealed class FakeLevelDatabaseStarter : FakeSQLiteDatabaseStarter, ILevelDatabaseStarter
		{
			public override string GetDbFileName() => "Levels.db";
		}
	}
}