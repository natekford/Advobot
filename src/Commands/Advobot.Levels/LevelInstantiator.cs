using Advobot.CommandAssemblies;
using Advobot.Levels.Database;
using Advobot.Levels.Service;
using Advobot.Serilog;
using Advobot.SQLite;

using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Levels;

public sealed class LevelInstantiator : CommandAssemblyInstantiator
{
	public override Task AddServicesAsync(IServiceCollection services)
	{
		services
			.AddSQLiteDatabase<LevelDatabase>("Levels")
			.AddSingletonWithLogger<LevelService>("Levels")
			.AddSingleton<LevelServiceConfig>();

		return Task.CompletedTask;
	}
}