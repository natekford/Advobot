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
				.AddSingleton<ILevelDatabase, LevelDatabase>()
				.AddSQLiteFileDatabaseConnectionStringFor<LevelDatabase>("Levels.db")
				.AddSingleton<ILevelService, LevelService>();

			return Task.CompletedTask;
		}

		public Task ConfigureServicesAsync(IServiceProvider services)
		{
			services.GetRequiredService<IConnectionStringFor<LevelDatabase>>().MigrateUp();

			return Task.CompletedTask;
		}
	}
}