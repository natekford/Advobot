using Advobot.CommandAssemblies;
using Advobot.Services;
using Advobot.Services.GuildSettings;
using Advobot.Settings.Database;
using Advobot.Settings.Service;
using Advobot.SQLite;

using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Settings;

public sealed class SettingsInstantiator : CommandAssemblyInstantiator
{
	public override Task AddServicesAsync(IServiceCollection services)
	{
		services
			.AddSQLiteDatabase<SettingsDatabase>("GuildSettings")
			.ReplaceAllWithSingleton<IGuildSettingsService, GuildSettingsService>()
			.AddSingleton<ICommandValidator, CommandValidator>();

		return Task.CompletedTask;
	}
}