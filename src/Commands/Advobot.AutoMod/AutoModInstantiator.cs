using Advobot.AutoMod.Database;
using Advobot.AutoMod.Service;
using Advobot.CommandAssemblies;
using Advobot.SQLite;

using Microsoft.Extensions.DependencyInjection;

namespace Advobot.AutoMod;

public sealed class AutoModInstantiator : ICommandAssemblyInstantiator
{
	public Task AddServicesAsync(IServiceCollection services)
	{
		services
			.AddSingleton<IAutoModDatabase, AutoModDatabase>()
			.AddSQLiteFileDatabaseConnectionStringFor<AutoModDatabase>("AutoMod.db")
			.AddSingleton<IRemovablePunishmentDatabase, RemovablePunishmentDatabase>()
			.AddSQLiteFileDatabaseConnectionStringFor<RemovablePunishmentDatabase>("RemovablePunishments.db")
			.AddSingleton<AutoModService>()
			.AddSingleton<MassBanRecentJoinsService>()
			.AddSingleton<RemovablePunishmentService>();

		return Task.CompletedTask;
	}

	public Task ConfigureServicesAsync(IServiceProvider services)
	{
		services.GetRequiredService<IConnectionStringFor<AutoModDatabase>>().MigrateUp();
		services.GetRequiredService<IConnectionStringFor<RemovablePunishmentDatabase>>().MigrateUp();

		return Task.CompletedTask;
	}
}