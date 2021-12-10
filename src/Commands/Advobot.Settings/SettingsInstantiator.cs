using Advobot.CommandAssemblies;
using Advobot.Services;
using Advobot.Services.GuildSettingsProvider;
using Advobot.Settings.Database;
using Advobot.Settings.Service;
using Advobot.SQLite;

using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Settings;

public sealed class SettingsInstantiator : ICommandAssemblyInstantiator
{
	public Task AddServicesAsync(IServiceCollection services)
	{
		services
			.AddSingleton<ISettingsDatabase, SettingsDatabase>()
			.AddSQLiteFileDatabaseConnectionStringFor<SettingsDatabase>("GuildSettings.db")
			.AddSingleton<ICommandValidator, CommandValidator>()
			.ReplaceWithSingleton<IGuildSettingsProvider, GuildSettingsProvider>();

		return Task.CompletedTask;
	}

	public Task ConfigureServicesAsync(IServiceProvider services)
	{
		services.GetRequiredService<IConnectionStringFor<SettingsDatabase>>().MigrateUp();

		return Task.CompletedTask;
	}
}