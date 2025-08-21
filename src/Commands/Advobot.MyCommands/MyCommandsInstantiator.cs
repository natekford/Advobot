using Advobot.CommandAssemblies;
using Advobot.MyCommands.Database;
using Advobot.MyCommands.Service;
using Advobot.Serilog;
using Advobot.SQLite;

using Microsoft.Extensions.DependencyInjection;

namespace Advobot.MyCommands;

public sealed class MyCommandsInstantiator : CommandAssemblyInstantiator
{
	public override Task AddServicesAsync(IServiceCollection services)
	{
		services
			.AddSQLiteDatabase<MyCommandsDatabase>("MyCommands")
			.AddSingletonWithLogger<SpammerHandler>("SpammerHandler");

		return Task.CompletedTask;
	}
}