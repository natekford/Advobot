using System;
using System.Threading.Tasks;

using Advobot.CommandAssemblies;
using Advobot.Services;
using Advobot.Services.GuildSettingsProvider;
using Advobot.Settings.Database;
using Advobot.Settings.Service;
using Advobot.SQLite;

using AdvorangesUtils;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Advobot.Settings
{
	public sealed class SettingsInstantiator : ICommandAssemblyInstantiator
	{
		public Task AddServicesAsync(IServiceCollection services)
		{
			services.RemoveAll(x =>
			{
				return x.ServiceType == typeof(IGuildSettingsProvider)
					&& x.ImplementationType != null
					&& x.ImplementationType
						.GetCustomAttributes(typeof(ReplacableAttribute), false).Length != 0;
			});

			services
				.AddSingleton<ISettingsDatabase, SettingsDatabase>()
				.AddSQLiteFileDatabaseConnectionStringFor<SettingsDatabase>("GuildSettings.db")
				.AddSingleton<ICommandValidator, CommandValidator>()
				.AddSingleton<IGuildSettingsProvider, GuildSettingsProvider>();

			return Task.CompletedTask;
		}

		public Task ConfigureServicesAsync(IServiceProvider services)
		{
			services.GetRequiredService<IConnectionStringFor<SettingsDatabase>>().MigrateUp();

			return Task.CompletedTask;
		}
	}
}