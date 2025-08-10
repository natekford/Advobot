using Advobot.CommandAssemblies;
using Advobot.Punishments;
using Advobot.Services.BotSettings;
using Advobot.Services.Commands;
using Advobot.Services.GuildSettings;
using Advobot.Services.Help;
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
		await config.StartAsync(client).ConfigureAwait(false);

		return services;
	}

	private static async Task<IServiceProvider> CreateServicesAsync(StartupConfig config)
	{
		var botSettings = NaiveRuntimeConfig.CreateOrLoad(config);
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
				await assembly.Instantiator.ConfigureServicesAsync(services).ConfigureAwait(false);
			}
		}

		// Add in commands
		var commandHandler = services.GetRequiredService<NaiveCommandService>();
		await commandHandler.AddCommandsAsync(commandAssemblies).ConfigureAwait(false);

		return services;
	}
}