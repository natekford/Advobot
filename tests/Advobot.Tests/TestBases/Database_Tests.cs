using Advobot.Services.Time;
using Advobot.SQLite;
using Advobot.Tests.Fakes.Database;

using AdvorangesUtils;

using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Tests.TestBases;

public abstract class Database_Tests<TDb, TConn> : TestsBase where TDb : class
{
	protected async Task<TDb> GetDatabaseAsync()
	{
		var starter = Services.Value.GetRequiredService<IConnectionString<TDb>>();
		await starter.EnsureCreatedAsync().CAF();
		starter.MigrateUp();

		return Services.Value.GetRequiredService<TDb>();
	}

	protected override void ModifyServices(IServiceCollection services)
	{
		services
			.AddSingleton<TDb>()
			.AddSingleton<IConnectionString<TDb>, FakeSQLiteConnectionString>()
			.AddSingleton<ITime, DefaultTime>();
	}
}