using Advobot.Services.Punishments;
using Advobot.TypeReaders;

using Discord;
using Discord.Commands;

using Microsoft.Extensions.DependencyInjection;

using System.Reflection;

namespace Advobot.Tests.TestBases;

public abstract class Command_Tests : TestsBase
{
	public const string CHANNEL = "General";

	protected virtual List<Assembly> CommandAssemblies { get; set; } = [
		typeof(AutoMod.AutoModInstantiator).Assembly,
		typeof(Levels.LevelInstantiator).Assembly,
		typeof(Logging.LoggingInstantiator).Assembly,
		typeof(MyCommands.MyCommandsInstantiator).Assembly,
		typeof(Settings.SettingsInstantiator).Assembly,
		typeof(Standard.Commands.Channels).Assembly,
		typeof(AdvobotLauncher).Assembly,
	];

	protected virtual TaskCompletionSource<IResult> CommandExecuted { get; set; }
		= new(TaskCreationOptions.RunContinuationsAsynchronously);
	protected virtual CommandService CommandService { get; set; } = new(new()
	{
		CaseSensitiveCommands = false,
		ThrowOnError = false,
		LogLevel = LogSeverity.Info,
	});

	protected virtual Task ExecuteAsync(string input)
	{
		Context.Message.Content = input;
		return CommandService.ExecuteAsync(Context, 0, Services);
	}

	protected override void ModifyServices(IServiceCollection services)
	{
		services.AddSingleton(CommandService)
			.AddSingleton<IPunishmentService, NaivePunishmentService>();
	}

	protected override async Task SetupAsync()
	{
		CommandService.CommandExecuted += (_, __, result) =>
		{
			CommandExecuted.SetResult(result);
			return Task.CompletedTask;
		};
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

		var adminRole = await Context.Guild.CreateRoleAsync(
			name: "Admin",
			permissions: GuildPermissions.All,
			color: null,
			isHoisted: false,
			options: null
		).ConfigureAwait(false);
		await Context.User.AddRoleAsync(adminRole).ConfigureAwait(false);

		Context.Channel.Name = CHANNEL;

		await base.SetupAsync().ConfigureAwait(false);
	}
}