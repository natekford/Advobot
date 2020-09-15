using System;
using System.Threading.Tasks;

using Advobot.CommandAssemblies;
using Advobot.Gacha.ActionLimits;
using Advobot.Gacha.Counters;
using Advobot.Gacha.Database;
using Advobot.Gacha.Displays;
using Advobot.Gacha.Interaction;
using Advobot.Gacha.Trading;
using Advobot.SQLite;

using AdvorangesUtils;

using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Gacha
{
	public sealed class GachaInstantiation : ICommandAssemblyInstantiator
	{
		public Task AddServicesAsync(IServiceCollection services)
		{
			services
				.AddSingleton<IGachaDatabase, GachaDatabase>()
				.AddSQLiteFileDatabaseConnectionStringFor<GachaDatabase>("Gacha.db")
				.AddSingleton<DisplayManager>()
				.AddSingleton<ExchangeManager>()
				.AddSingleton<IInteractionManager, InteractionManager>()
				.AddSingleton<ICounterService, CounterService>()
				.AddSingleton<ITokenHolderService, TokenHolderService>();

			return Task.CompletedTask;
		}

		public async Task ConfigureServicesAsync(IServiceProvider services)
		{
			services.GetRequiredService<IConnectionStringFor<GachaDatabase>>().MigrateUp();

			var db = services.GetRequiredService<IGachaDatabase>();
			//TODO: move to interface
			if (db is GachaDatabase gdb)
			{
				await gdb.CacheNamesAsync().CAF();
			}
		}
	}
}