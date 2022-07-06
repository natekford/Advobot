using Advobot.CommandAssemblies;
using Advobot.MyCommands.Database;
using Advobot.MyCommands.Service;
using Advobot.SQLite;

using Microsoft.Extensions.DependencyInjection;

namespace Advobot.MyCommands;

public sealed class MyCommandsInstantiator : ICommandAssemblyInstantiator
{
	public Task AddServicesAsync(IServiceCollection services)
	{
		services
			.AddSingleton<IMyCommandsDatabase, MyCommandsDatabase>()
			.AddSQLiteFileDatabaseConnectionStringFor<MyCommandsDatabase>("MyCommands.db")
			.AddSingleton<TurkHandler>()
			.AddSingleton<Ashman99ReactionHandler>();

		return Task.CompletedTask;
	}

	public Task ConfigureServicesAsync(IServiceProvider services)
	{
		services.GetRequiredService<IConnectionStringFor<MyCommandsDatabase>>().MigrateUp();

		return Task.CompletedTask;
	}
}