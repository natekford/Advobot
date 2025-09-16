using Advobot.AutoMod.Service;
using Advobot.Levels.Service;
using Advobot.Logging.Resetters;
using Advobot.Logging.Service;
using Advobot.Services.Commands;
using Advobot.Services.GuildSettings;
using Advobot.Services.Punishments;
using Advobot.Settings.Service;
using Advobot.SQLite;
using Advobot.Tests.Fakes.Discord;
using Advobot.Tests.Fakes.Discord.Channels;
using Advobot.Tests.Utilities;

using Discord;

using Microsoft.Extensions.DependencyInjection;

using System.Reflection;
using System.Threading.Channels;

using YACCS.Commands;
using YACCS.Results;

namespace Advobot.Tests.TestBases;

public abstract class Command_Tests : TestsBase
{
	protected virtual FakeRole AdminRole { get; set; }
	protected virtual List<Assembly> CommandAssemblies { get; set; } = [
		typeof(AutoMod.AutoModInstantiator).Assembly,
		typeof(Levels.LevelInstantiator).Assembly,
		typeof(Logging.LoggingInstantiator).Assembly,
		typeof(MyCommands.MyCommandsInstantiator).Assembly,
		typeof(Settings.SettingsInstantiator).Assembly,
		typeof(Standard.StandardInstantiator).Assembly,
		typeof(AdvobotLauncher).Assembly,
	];
	protected virtual NaiveCommandService CommandService { get; set; }
	protected virtual Channel<CommandExecutedResult> ExecutedCommands { get; set; }
		= Channel.CreateUnbounded<CommandExecutedResult>();
	protected virtual bool HasBeenShutdown { get; set; }
	protected virtual FakeTextChannel OtherTextChannel { get; set; }
	protected virtual FakeVoiceChannel VoiceChannel { get; set; }

	protected virtual async Task ExecuteAsync(string input)
	{
		Context.Message.Content = input;

		await CommandService.ExecuteAsync(Context, Context.Message.Content).ConfigureAwait(false);
	}

	protected virtual async Task<IResult> ExecuteWithResultAsync(string input)
	{
		await ExecuteAsync(input).ConfigureAwait(false);
		return await ExecutedCommands.Reader.ReadAsync(CancellationToken.None).ConfigureAwait(false);
	}

	protected override void ModifyServices(IServiceCollection services)
	{
		services
			.AddSingleton<IEnumerable<Assembly>>(CommandAssemblies)
			.AddSingleton<NaiveCommandService>()
			.AddSingleton<CommandService>(x => x.GetRequiredService<NaiveCommandService>())
			.AddSingleton<ICommandService>(x => x.GetRequiredService<NaiveCommandService>())

			.AddSingleton<ShutdownApplication>(_ => HasBeenShutdown = true)
			.AddSingleton<IGuildSettingsService, NaiveGuildSettingsService>()
			.AddSingleton<IPunishmentService>(x => x.GetRequiredService<TimedPunishmentService>())
			.AddSingleton<LevelServiceConfig>()

			.AddSingletonWithFakeLogger<AutoModService>()
			.AddSingletonWithFakeLogger<TimedPunishmentService>()
			.AddSingletonWithFakeLogger<LoggingService>()
			.AddSingletonWithFakeLogger<LevelService>()
			.AddSingletonWithFakeLogger<NotificationService>()
			.AddSingletonWithFakeLogger<GuildSettingsService>()

			.AddDefaultOptionsSetter<LogActionsResetter>()
			.AddDefaultOptionsSetter<WelcomeNotificationResetter>()
			.AddDefaultOptionsSetter<GoodbyeNotificationResetter>();
	}

	protected override async Task SetupAsync()
	{
		CommandService = Services.GetRequiredService<NaiveCommandService>();

		await Context.Client.StartAsync().ConfigureAwait(false);
		await CommandService.InitializeAsync().ConfigureAwait(false);

		EventProvider.CommandExecuted.Add(async x => await ExecutedCommands.Writer.WriteAsync(x).ConfigureAwait(false));

		OtherTextChannel = new(Context.Guild)
		{
			Name = "Other",
		};
		VoiceChannel = new(Context.Guild)
		{
			Name = "VC",
		};
		Context.Channel.Name = "General";

		AdminRole = (FakeRole)await Context.Guild.CreateRoleAsync(
			name: "Admin",
			permissions: GuildPermissions.All,
			color: null,
			isHoisted: false,
			options: null
		).ConfigureAwait(false);

		await Context.User.AddRoleAsync(AdminRole).ConfigureAwait(false);
		await Context.Bot.AddRoleAsync(AdminRole).ConfigureAwait(false);

		await base.SetupAsync().ConfigureAwait(false);
	}
}