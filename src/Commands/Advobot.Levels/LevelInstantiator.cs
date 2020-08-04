using System;
using System.Threading.Tasks;

using Advobot.CommandAssemblies;
using Advobot.Levels.Database;
using Advobot.Levels.Service;
using Advobot.SQLite;

using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Levels
{
	public sealed class LevelInstantiator : ICommandAssemblyInstantiator
	{
		public Task AddServicesAsync(IServiceCollection services)
		{
			services
				.AddSingleton<LevelServiceConfig>()
				.AddSingleton<LevelDatabase>()
				.AddSingleton<ILevelService, LevelService>()
				.AddSingleton<ILevelDatabaseStarter, LevelDatabaseStarter>();

			return Task.CompletedTask;
		}

		public Task ConfigureServicesAsync(IServiceProvider services)
		{
			services.GetRequiredService<ILevelDatabaseStarter>().MigrateUp();

			// Needed to instantiate the level service
			services.GetRequiredService<ILevelService>();

			return Task.CompletedTask;
		}
	}
}