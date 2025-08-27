using Advobot.CommandAssemblies;
using Advobot.Services;
using Advobot.Services.BotConfig;
using Advobot.Services.Commands;
using Advobot.Services.Events;
using Advobot.Services.GuildSettings;
using Advobot.Services.Help;
using Advobot.Services.Punishments;
using Advobot.Services.Time;
using Advobot.Utilities;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

using Microsoft.Extensions.DependencyInjection;

using System.Diagnostics;
using System.Net;

namespace Advobot;

/// <summary>
/// Puts the similarities from launching the console application and the .Net Core UI application into one.
/// </summary>
public sealed class AdvobotLauncher
{
	/// <summary>
	/// Starts an instance of Advobot with one method call.
	/// </summary>
	/// <param name="args"></param>
	/// <returns></returns>
	public static async Task<IServiceProvider> Start(string[] args)
	{
		AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
		{
			var file = Path.Combine(Directory.GetCurrentDirectory(), "CrashLog.txt");
			var output = $"{DateTime.UtcNow.ToReadable()}: {e.ExceptionObject}\n";
			File.AppendAllText(file, output);

			Console.WriteLine($"!!! Something has gone drastically wrong. Check {file} for more details.");
		};

		var config = StartupConfig.Load(args);
		Console.WriteLine($"Supplied arguments: Instance={config.Instance}, PreviousProcessId={config.PreviousProcessId}");

		// Wait until the old process is killed
		if (config.PreviousProcessId != -1)
		{
			try
			{
				while (Process.GetProcessById(config.PreviousProcessId) != null)
				{
					Thread.Sleep(25);
				}
			}
			catch (ArgumentException) { }
		}
		config.ValidatePath();
		await config.ValidateBotKey().ConfigureAwait(false);

		var services = await CreateServicesAsync(config).ConfigureAwait(false);
		var client = services.GetRequiredService<BaseSocketClient>();

		Console.WriteLine("Connecting to Discord...");
		await client.LoginAsync(TokenType.Bot, config.BotKey).ConfigureAwait(false);
		await client.StartAsync().ConfigureAwait(false);
		Console.WriteLine("Successfully connected to Discord.");

		return services;
	}

	private static async Task<IServiceProvider> CreateServicesAsync(StartupConfig config)
	{
		var botConfig = NaiveRuntimeConfig.CreateOrLoad(config);
		var commandConfig = new CommandServiceConfig
		{
			CaseSensitiveCommands = false,
			ThrowOnError = false,
			LogLevel = botConfig.LogLevel,
		};
		var discordClient = new DiscordShardedClient(new DiscordSocketConfig
		{
			MessageCacheSize = botConfig.MessageCacheSize,
			LogLevel = botConfig.LogLevel,
			AlwaysDownloadUsers = true,
			LogGatewayIntentWarnings = false,
			GatewayIntents = GatewayIntents.All,
		});
		var eventProvider = new ShardedClientEventProvider(discordClient);
		var httpClient = new HttpClient(new HttpClientHandler
		{
			AllowAutoRedirect = true,
			Credentials = CredentialCache.DefaultCredentials,
			Proxy = new WebProxy(),
			AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
		});

		var collection = new ServiceCollection()
			.AddSingleton<ShutdownApplication>(Environment.Exit)
			.AddSingleton(commandConfig)
			.AddSingleton(httpClient)
			.AddSingleton(discordClient)
			.AddSingleton<BaseSocketClient>(discordClient)
			.AddSingleton<IDiscordClient>(discordClient)
			.AddSingleton<IRuntimeConfig>(botConfig)
			.AddSingleton<IConfig>(botConfig)
			.AddSingleton<EventProvider>(eventProvider)
			.AddSingleton<NaiveCommandService>()
			.AddSingleton<ITimeService, NaiveTimeService>()
			.AddSingleton<IHelpService, NaiveHelpService>()
			.AddSingleton<IPunishmentService, NaivePunishmentService>()
			.AddSingleton<IGuildSettingsService, NaiveGuildSettingsService>();

		var commandAssemblyDirectory = AppDomain.CurrentDomain.BaseDirectory;
		var commandAssemblies = CommandAssembly.Load(commandAssemblyDirectory);

		// Add in services each command assembly uses
		foreach (var assembly in commandAssemblies)
		{
			if (assembly.Instantiator != null)
			{
				await assembly.Instantiator.AddServicesAsync(collection).ConfigureAwait(false);
			}
		}

		var services = collection.BuildServiceProvider();
		// Instantiate and configure each service
		foreach (var service in collection)
		{
			if (service.Lifetime != ServiceLifetime.Singleton)
			{
				continue;
			}

			var instance = services.GetRequiredService(service.ServiceType);
			if (instance is IConfigurableService configurable)
			{
				await configurable.ConfigureAsync().ConfigureAwait(false);
			}
		}

		// Allow each command assembly to do configuration with access to all of the services
		foreach (var assembly in commandAssemblies)
		{
			if (assembly.Instantiator != null)
			{
				await assembly.Instantiator.ConfigureServicesAsync(services).ConfigureAwait(false);
			}
		}

		// Add in commands
		var commandHandler = services.GetRequiredService<NaiveCommandService>();
		await commandHandler.AddCommandsAsync(commandAssemblies).ConfigureAwait(false);

		eventProvider.Ready.Add(async _ =>
		{
			var game = botConfig.Game;
			var stream = botConfig.Stream;
			var activityType = ActivityType.Playing;
			if (!string.IsNullOrWhiteSpace(stream))
			{
				stream = $"https://www.twitch.tv/{stream[(stream.LastIndexOf('/') + 1)..]}";
				activityType = ActivityType.Streaming;
			}
			await discordClient.SetGameAsync(game, stream, activityType).ConfigureAwait(false);
		});

		return services;
	}
}

/// <summary>
/// Shuts down the application.
/// </summary>
/// <param name="exitCode"></param>
public delegate void ShutdownApplication(int exitCode);