using Advobot.CommandAssemblies;
using Advobot.Services;
using Advobot.Services.GuildSettings;
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
			.AddSQLiteFileDatabaseConnectionString<SettingsDatabase>("GuildSettings.db")
			.AddSingleton<ICommandValidator, CommandValidator>()
			.ReplaceWithSingleton<IGuildSettingsService, GuildSettingsProvider>();

		return Task.CompletedTask;
	}

	public Task ConfigureServicesAsync(IServiceProvider services)
	{
		services.GetRequiredService<IConnectionString<SettingsDatabase>>().MigrateUp();

		return Task.CompletedTask;
	}
}