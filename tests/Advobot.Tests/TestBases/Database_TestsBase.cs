using System;
using System.Threading.Tasks;

using Advobot.Services.Time;
using Advobot.SQLite;
using Advobot.Tests.Fakes.Database;

using AdvorangesUtils;

using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Tests.TestBases
{
	public abstract class Database_TestsBase<TDatabase, TStarter>
		where TDatabase : class
	{
		protected static readonly Random Rng = new Random();
		protected IServiceProvider Services { get; }
		protected ITime Time => Services.GetRequiredService<ITime>();

		protected Database_TestsBase()
		{
			var services = new ServiceCollection()
				.AddSingleton<TDatabase>()
				.AddSingleton<IConnectionStringFor<TDatabase>, FakeSQLiteConnectionString>()
				.AddSingleton<ITime, DefaultTime>();
			ModifyServices(services);
			Services = services.BuildServiceProvider();
		}

		protected async Task<TDatabase> GetDatabaseAsync()
		{
			var starter = Services.GetRequiredService<IConnectionStringFor<TDatabase>>();
			await starter.EnsureCreatedAsync().CAF();
			starter.MigrateUp();

			return Services.GetRequiredService<TDatabase>();
		}

		protected virtual void ModifyServices(IServiceCollection services)
		{
		}
	}
}