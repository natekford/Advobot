using System;
using System.Threading.Tasks;
using Advobot.CommandAssemblies;
using Advobot.Gacha.Counters;
using Advobot.Gacha.Database;
using Advobot.Gacha.Displays;
using Advobot.Gacha.Interaction;
using AdvorangesUtils;
using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Gacha
{
	public sealed class GachaInstantiation : ICommandAssemblyInstantiator
	{
		public Task AddServicesAsync(IServiceCollection services)
		{
			services
				.AddSingleton<GachaDatabase>()
				.AddSingleton<DisplayManager>()
				.AddSingleton<IInteractionManager, InteractionManager>()
				.AddSingleton<IDatabaseStarter, SQLiteFileDatabaseFactory>()
				.AddSingleton<ICounterService, CounterService>();
			return Task.CompletedTask;
		}
		public async Task ConfigureServicesAsync(IServiceProvider services)
		{
			var db = services.GetRequiredService<GachaDatabase>();
			await db.CreateDatabaseAsync().CAF();
		}
	}
}
