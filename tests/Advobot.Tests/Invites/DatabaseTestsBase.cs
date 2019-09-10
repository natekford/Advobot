using System;
using System.Threading.Tasks;

using Advobot.Invites.Database;
using Advobot.Services.Time;

using AdvorangesUtils;

using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Tests.Invites
{
	public abstract class DatabaseTestsBase
	{
		public static readonly Random Rng = new Random();

		protected IServiceProvider Services { get; }
		protected ITime Time => Services.GetRequiredService<ITime>();

		protected DatabaseTestsBase()
		{
			Services = new ServiceCollection()
				.AddSingleton<InviteDatabase>()
				.AddSingleton<ITime, DefaultTime>()
				.AddSingleton<IDatabaseStarter, SQLiteTestDatabaseFactory>()
				.BuildServiceProvider();
		}

		protected async Task<InviteDatabase> GetDatabaseAsync()
		{
			var db = Services.GetRequiredService<InviteDatabase>();
			await db.CreateDatabaseAsync().CAF();
			return db;
		}
	}
}