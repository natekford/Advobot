using Advobot.CommandAssemblies;
using Advobot.Levels.Database;
using Advobot.Levels.Service;
using Advobot.Serilog;
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
			.AddSingleton<LevelService>()
			.AddLogger<LevelService>("Levels");

		return Task.CompletedTask;
	}

	public Task ConfigureServicesAsync(IServiceProvider services)
	{
		services.GetRequiredService<IConnectionString<LevelDatabase>>().MigrateUp();

		return Task.CompletedTask;
	}
}