using Advobot.AutoMod.Database;
using Advobot.AutoMod.Service;
using Advobot.Levels.Database;
using Advobot.Levels.Service;
using Advobot.Logging.Database;
using Advobot.Logging.Service;
using Advobot.MyCommands.Database;
using Advobot.Serilog;
using Advobot.Services.BotConfig;
using Advobot.Services.Help;
using Advobot.Services.Punishments;
using Advobot.Services.Time;
using Advobot.Settings.Database;
using Advobot.Settings.Service;
using Advobot.Tests.Fakes.Discord;
using Advobot.Tests.Fakes.Services.BotConfig;
using Advobot.Tests.Fakes.Services.Time;
using Advobot.Tests.Utilities;

using Discord;

using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Tests.TestBases;

public abstract class TestsBase
{
	internal virtual NaiveHelpService Help { get; } = new();
	protected virtual FakeBotConfig Config { get; } = new();
	protected virtual FakeCommandContext Context { get; } = FakeUtils.CreateContext();
	protected virtual Random Rng { get; } = new();
	protected virtual IServiceProvider Services { get; set; }
	protected virtual MutableTime Time { get; } = new();

	[TestInitialize]
	public async Task TestInitializeAsync()
	{
		var services = new ServiceCollection()
			.AddSingleton(Rng)
			.AddSingleton<IDiscordClient>(Context.Client)
			.AddSingleton<ITimeService>(Time)
			.AddSingleton<IConfig>(Config)
			.AddSingleton<IRuntimeConfig>(Config)
			.AddSingleton<IHelpService>(Help)
			.AddFakeDatabase<AutoModDatabase>()
			.AddSingletonWithLogger<AutoModService>("DELETE_ME1")
			.AddFakeDatabase<TimedPunishmentDatabase>()
			.AddSingletonWithLogger<TimedPunishmentService>("DELETE_ME2")
			.AddSingleton<IPunishmentService>(x => x.GetRequiredService<TimedPunishmentService>())
			.AddFakeDatabase<LoggingDatabase>()
			.AddSingletonWithLogger<LoggingService>("DELETE_ME3")
			.AddFakeDatabase<LevelDatabase>()
			.AddSingleton<LevelServiceConfig>()
			.AddSingletonWithLogger<LevelService>("DELETE_ME4")
			.AddFakeDatabase<NotificationDatabase>()
			.AddSingletonWithLogger<NotificationService>("DELETE_ME5")
			.AddFakeDatabase<MyCommandsDatabase>()
			.AddFakeDatabase<SettingsDatabase>()
			.AddSingletonWithLogger<GuildSettingsService>("DELETE_ME6");
		ModifyServices(services);
		Services = services.BuildServiceProvider();

		await SetupAsync().ConfigureAwait(false);
	}

	protected virtual void ModifyServices(IServiceCollection services)
	{
	}

	protected virtual Task SetupAsync()
		=> Task.CompletedTask;
}