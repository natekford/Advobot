using Advobot.AutoMod.Database;
using Advobot.Levels.Database;
using Advobot.Logging.Database;
using Advobot.MyCommands.Database;
using Advobot.Services.BotConfig;
using Advobot.Services.Events;
using Advobot.Services.Help;
using Advobot.Services.Time;
using Advobot.Settings.Database;
using Advobot.Tests.Fakes.Discord;
using Advobot.Tests.Fakes.Services.BotConfig;
using Advobot.Tests.Fakes.Services.Events;
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
	protected virtual FakeEventProvider EventProvider { get; set; } = new();
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
			.AddSingleton<EventProvider>(EventProvider)

			.AddFakeDatabase<AutoModDatabase>()
			.AddFakeDatabase<TimedPunishmentDatabase>()
			.AddFakeDatabase<LoggingDatabase>()
			.AddFakeDatabase<LevelDatabase>()
			.AddFakeDatabase<NotificationDatabase>()
			.AddFakeDatabase<MyCommandsDatabase>()
			.AddFakeDatabase<SettingsDatabase>();

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