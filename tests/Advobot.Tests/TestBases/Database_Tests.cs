using Advobot.Services.Time;
using Advobot.SQLite;
using Advobot.Tests.Fakes.Database;

using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Tests.TestBases;

public abstract class Database_Tests<TDb, TConn> : TestsBase where TDb : class
{
	protected async Task<TDb> GetDatabaseAsync()
	{
		var starter = Services.Value.GetRequiredService<IConnectionString<TDb>>();
		await starter.EnsureCreatedAsync().ConfigureAwait(false);
		starter.MigrateUp();

		return Services.Value.GetRequiredService<TDb>();
	}

	protected override void ModifyServices(IServiceCollection services)
	{
		var connectionString = new FakeSQLiteConnectionString(typeof(TDb));
		services
			.AddSingleton<TDb>()
			.AddSingleton<IConnectionString<TDb>>(connectionString)
			.AddSingleton<ITimeService, NaiveTimeService>();
	}
}