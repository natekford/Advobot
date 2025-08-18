using Advobot.AutoMod.Database;
using Advobot.AutoMod.Service;
using Advobot.CommandAssemblies;
using Advobot.Serilog;
using Advobot.Services;
using Advobot.Services.Punishments;
using Advobot.SQLite;

using Microsoft.Extensions.DependencyInjection;

namespace Advobot.AutoMod;

public sealed class AutoModInstantiator : CommandAssemblyInstantiator
{
	public override Task AddServicesAsync(IServiceCollection services)
	{
		services
			.AddSQLiteDatabase<AutoModDatabase>("AutoMod")
			.AddSingletonWithLogger<AutoModService>("AutoMod")
			.AddSQLiteDatabase<TimedPunishmentDatabase>("TimedPunishments")
			.ReplaceAllWithSingleton<IPunishmentService, TimedPunishmentService>()
			.AddLogger<TimedPunishmentService>("TimedPunishments");

		return Task.CompletedTask;
	}
}