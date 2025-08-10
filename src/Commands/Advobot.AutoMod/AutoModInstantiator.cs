using Advobot.AutoMod.Database;
using Advobot.AutoMod.Service;
using Advobot.CommandAssemblies;
using Advobot.Punishments;
using Advobot.Serilog;
using Advobot.Services;
using Advobot.SQLite;

using Microsoft.Extensions.DependencyInjection;

namespace Advobot.AutoMod;

public sealed class AutoModInstantiator : CommandAssemblyInstantiator
{
	public override Task AddServicesAsync(IServiceCollection services)
	{
		services
			.AddSingleton<IAutoModDatabase, AutoModDatabase>()
			.AddSQLiteFileDatabaseConnectionString<AutoModDatabase>("AutoMod.db")
			.AddSingleton<AutoModService>()
			.AddLogger<AutoModService>("Automod")
			.AddSingleton<ITimedPunishmentDatabase, TimedPunishmentDatabase>()
			.AddSQLiteFileDatabaseConnectionString<TimedPunishmentDatabase>("TimedPunishments.db")
			.ReplaceAllWithSingleton<IPunishmentService, TimedPunishmentService>()
			.AddLogger<TimedPunishmentService>("TimedPunishments");

		return Task.CompletedTask;
	}
}