using Advobot.CommandAssemblies;
using Advobot.Levels.Database;
using Advobot.Levels.Service;
using Advobot.SQLite;

using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Levels;

public sealed class LevelInstantiator : ICommandAssemblyInstantiator
{
	public Task AddServicesAsync(IServiceCollection services)
	{
		services
			.AddSingleton<LevelServiceConfig>()
			.AddSingleton<ILevelDatabase, LevelDatabase>()
			.AddSQLiteFileDatabaseConnectionString<LevelDatabase>("Levels.db")
			.AddSingleton<ILevelService, LevelService>();

		return Task.CompletedTask;
	}

	public Task ConfigureServicesAsync(IServiceProvider services)
	{
		services.GetRequiredService<IConnectionString<LevelDatabase>>().MigrateUp();

		return Task.CompletedTask;
	}
}