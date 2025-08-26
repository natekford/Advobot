using Advobot.AutoMod.Service;
using Advobot.Levels.Service;
using Advobot.Logging.Resetters;
using Advobot.Logging.Service;
using Advobot.Services.GuildSettings;
using Advobot.Services.Punishments;
using Advobot.Settings.Service;
using Advobot.SQLite;
using Advobot.Tests.Fakes.Discord;
using Advobot.Tests.Fakes.Discord.Channels;
using Advobot.Tests.Utilities;
using Advobot.TypeReaders;

using Discord;
using Discord.Commands;

using Microsoft.Extensions.DependencyInjection;

using System.Reflection;
using System.Threading.Channels;

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
		typeof(Standard.Commands.Channels).Assembly,
		typeof(AdvobotLauncher).Assembly,
	];
	protected virtual CommandService CommandService { get; set; } = new(new()
	{
		CaseSensitiveCommands = false,
		ThrowOnError = false,
		LogLevel = LogSeverity.Info,
	});
	protected virtual Channel<IResult> ExecutedCommands { get; set; }
		= Channel.CreateUnbounded<IResult>();
	protected virtual FakeTextChannel OtherTextChannel { get; set; }
	protected virtual FakeVoiceChannel VoiceChannel { get; set; }

	protected virtual async Task ExecuteAsync(string input)
	{
		Context.Message.Content = input;

		await CommandService.ExecuteAsync(Context, 0, Services).ConfigureAwait(false);
	}

	protected virtual async Task<IResult> ExecuteWithResultAsync(string input)
	{
		await ExecuteAsync(input).ConfigureAwait(false);
		return await ExecutedCommands.Reader.ReadAsync(CancellationToken.None).ConfigureAwait(false);
	}

	protected override void ModifyServices(IServiceCollection services)
	{
		services
			.AddSingleton(CommandService)
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
		CommandService.CommandExecuted += async (_, __, result)
			=> await ExecutedCommands.Writer.WriteAsync(result).ConfigureAwait(false);
		foreach (var type in CommandAssemblies.SelectMany(x => x.GetTypes()))
		{
			var attr = type.GetCustomAttribute<TypeReaderTargetTypeAttribute>();
			if (attr is not null && Activator.CreateInstance(type) is TypeReader instance)
			{
				foreach (var targetType in attr.TargetTypes)
				{
					CommandService.AddTypeReader(targetType, instance, true);
				}
			}
		}
		foreach (var assembly in CommandAssemblies)
		{
			await CommandService.AddModulesAsync(assembly, Services).ConfigureAwait(false);
		}

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