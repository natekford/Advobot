using Advobot.AutoMod.Database;
using Advobot.AutoMod.Service;
using Advobot.CommandAssemblies;
using Advobot.Serilog;
using Advobot.SQLite;

using Microsoft.Extensions.DependencyInjection;

namespace Advobot.AutoMod;

public sealed class AutoModInstantiator : ICommandAssemblyInstantiator
{
	public Task AddServicesAsync(IServiceCollection services)
	{
		services
			.AddSingleton<IAutoModDatabase, AutoModDatabase>()
			.AddSQLiteFileDatabaseConnectionString<AutoModDatabase>("AutoMod.db")
			.AddSingleton<AutoModService>()
			.AddLogger<AutoModService>("Automod")
			.AddSingleton<IRemovablePunishmentDatabase, RemovablePunishmentDatabase>()
			.AddSQLiteFileDatabaseConnectionString<RemovablePunishmentDatabase>("RemovablePunishments.db")
			.AddSingleton<RemovablePunishmentService>()
			.AddLogger<RemovablePunishmentService>("RemovablePunishments");

		return Task.CompletedTask;
	}

	public Task ConfigureServicesAsync(IServiceProvider services)
	{
		services.GetRequiredService<IConnectionString<AutoModDatabase>>().MigrateUp();
		services.GetRequiredService<IConnectionString<RemovablePunishmentDatabase>>().MigrateUp();

		return Task.CompletedTask;
	}
}