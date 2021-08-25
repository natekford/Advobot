
using Advobot.Services.Time;
using Advobot.SQLite;
using Advobot.Tests.Fakes.Database;

using AdvorangesUtils;

using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Tests.TestBases
{
	public abstract class DatabaseTestsBase<TDb, TConn> : TestsBase where TDb : class
	{
		protected async Task<TDb> GetDatabaseAsync()
		{
			var starter = Services.GetRequiredService<IConnectionStringFor<TDb>>();
			await starter.EnsureCreatedAsync().CAF();
			starter.MigrateUp();

			return Services.GetRequiredService<TDb>();
		}

		protected override void ModifyServices(IServiceCollection services)
		{
			services
				.AddSingleton<TDb>()
				.AddSingleton<IConnectionStringFor<TDb>, FakeSQLiteConnectionString>()
				.AddSingleton<ITime, DefaultTime>();
		}
	}
}