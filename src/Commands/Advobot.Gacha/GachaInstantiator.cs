using Advobot.CommandAssemblies;
using Advobot.Gacha.Checkers;
using Advobot.Gacha.Database;
using AdvorangesUtils;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Advobot.Gacha
{
	public sealed class GachaInstantiation : ICommandAssemblyInstantiator
	{
		public Task AddServicesAsync(IServiceCollection services)
		{
			services
				.AddSingleton<GachaDatabase>()
				.AddSingleton<IDatabaseStarter, SQLiteFileDatabaseFactory>()
				.AddSingleton<ICheckersService, CheckersService>();
			return Task.CompletedTask;
		}
		public async Task ConfigureServicesAsync(IServiceProvider services)
		{
			var db = services.GetRequiredService<GachaDatabase>();
			await db.CreateDatabaseAsync().CAF();
		}
	}
}
