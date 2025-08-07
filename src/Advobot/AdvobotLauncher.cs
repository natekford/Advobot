using Advobot.CommandAssemblies;
using Advobot.Punishments;
using Advobot.Services.BotSettings;
using Advobot.Services.Commands;
using Advobot.Services.GuildSettingsProvider;
using Advobot.Services.HelpEntries;
using Advobot.Services.LogCounters;
using Advobot.Services.Time;
using Advobot.Utilities;

using AdvorangesUtils;

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
	public static async Task<IServiceProvider> NoConfigurationStart(string[] args)
	{
		AppDomain.CurrentDomain.UnhandledException += (sender, e)
			=> IOUtils.LogUncaughtException(e.ExceptionObject);
		ConsoleUtils.PrintingFlags = 0
			| ConsolePrintingFlags.Print
			| ConsolePrintingFlags.LogTime
			| ConsolePrintingFlags.LogCaller
			| ConsolePrintingFlags.RemoveDuplicateNewLines;

		var config = AdvobotConfig.Load(args);
		ConsoleUtils.DebugWrite($"Args: {config.Instance}|{config.PreviousProcessId}", "Launcher Arguments");

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

		// Get the save path
		var validPath = config.ValidatePath(null, true);
		while (!validPath)
		{
			validPath = config.ValidatePath(Console.ReadLine(), false);
		}

		// Get the bot key
		var validKey = await config.ValidateBotKey(null, true).CAF();
		while (!validKey)
		{
			validKey = await config.ValidateBotKey(Console.ReadLine(), false).CAF();
		}

		var services = await CreateServicesAsync(config).CAF();

		var client = services.GetRequiredService<BaseSocketClient>();
		await config.StartAsync(client).CAF();

		return services;
	}

	private static async Task<IServiceProvider> CreateServicesAsync(AdvobotConfig config)
	{
		var botSettings = RuntimeConfig.CreateOrLoad(config);
		var commandConfig = new CommandServiceConfig
		{
			CaseSensitiveCommands = false,
			ThrowOnError = false,
			LogLevel = botSettings.LogLevel,
		};
		var discordClient = new DiscordShardedClient(new DiscordSocketConfig
		{
			MessageCacheSize = botSettings.MessageCacheSize,
			LogLevel = botSettings.LogLevel,
			AlwaysDownloadUsers = true,
			LogGatewayIntentWarnings = false,
			GatewayIntents = GatewayIntents.All,
		});
		var httpClient = new HttpClient(new HttpClientHandler
		{
			AllowAutoRedirect = true,
			Credentials = CredentialCache.DefaultCredentials,
			Proxy = new WebProxy(),
			AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
		});

		var collection = new ServiceCollection()
			.AddSingleton(commandConfig)
			.AddSingleton(httpClient)
			.AddSingleton(discordClient)
			.AddSingleton<BaseSocketClient>(discordClient)
			.AddSingleton<IDiscordClient>(discordClient)
			.AddSingleton<IRuntimeConfig>(botSettings)
			.AddSingleton<IConfig>(botSettings)
			.AddSingleton<ITime, DefaultTime>()
			.AddSingleton<IHelpEntryService, HelpEntryService>()
			.AddSingleton<ICommandHandlerService, CommandHandlerService>()
			.AddSingleton<ILogCounterService, LogCounterService>()
			.AddSingleton<IPunishmentService, PunishmentService>()
			.AddSingleton<IGuildSettingsService, NaiveGuildSettingsService>();

		var commandAssemblyDirectory = AppDomain.CurrentDomain.BaseDirectory;
		var commandAssemblies = CommandAssembly.Load(commandAssemblyDirectory);

		// Add in services each command assembly uses
		foreach (var assembly in commandAssemblies)
		{
			if (assembly.Instantiator != null)
			{
				await assembly.Instantiator.AddServicesAsync(collection).CAF();
			}
		}

		var services = collection.BuildServiceProvider();
		// Instantiate each service
		foreach (var service in collection)
		{
			if (service.Lifetime != ServiceLifetime.Singleton)
			{
				continue;
			}

			_ = services.GetRequiredService(service.ServiceType);
		}

		// Configure each service
		foreach (var assembly in commandAssemblies)
		{
			if (assembly.Instantiator != null)
			{
				await assembly.Instantiator.ConfigureServicesAsync(services).CAF();
			}
		}

		// Add in commands
		var commandHandler = services.GetRequiredService<ICommandHandlerService>();
		await commandHandler.AddCommandsAsync(commandAssemblies).CAF();

		return services;
	}
}