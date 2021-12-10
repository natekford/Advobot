using Advobot.CommandAssemblies;
using Advobot.Punishments;
using Advobot.Services.BotSettings;
using Advobot.Services.Commands;
using Advobot.Services.GuildSettingsProvider;
using Advobot.Services.HelpEntries;
using Advobot.Services.ImageResizing;
using Advobot.Services.LogCounters;
using Advobot.Services.Time;
using Advobot.Settings;
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
	private readonly IConfig _Config;
	private IServiceProvider? _Services;

	/// <summary>
	/// Creates an instance of <see cref="AdvobotLauncher"/>.
	/// </summary>
	/// <param name="config"></param>
	public AdvobotLauncher(IConfig config)
	{
		AppDomain.CurrentDomain.UnhandledException += (sender, e) => IOUtils.LogUncaughtException(e.ExceptionObject);
		ConsoleUtils.PrintingFlags = 0
			| ConsolePrintingFlags.Print
			| ConsolePrintingFlags.LogTime
			| ConsolePrintingFlags.LogCaller
			| ConsolePrintingFlags.RemoveDuplicateNewLines;

		_Config = config;
		ConsoleUtils.DebugWrite($"Args: {_Config.Instance}|{_Config.PreviousProcessId}", "Launcher Arguments");
	}

	/// <summary>
	/// Starts an instance of Advobot with one method call.
	/// </summary>
	/// <param name="args"></param>
	/// <returns></returns>
	public static async Task<IServiceProvider> NoConfigurationStart(string[] args)
	{
		var launcher = new AdvobotLauncher(Config.Load(args));
		await launcher.GetPathAndKeyAsync().CAF();

		var commands = CommandAssemblyCollection.Find();
		var services = await launcher.GetServicesAsync(commands).CAF();

		var commandHandler = services.GetRequiredService<ICommandHandlerService>();
		await commandHandler.AddCommandsAsync(commands.Assemblies).CAF();

		var client = services.GetRequiredService<BaseSocketClient>();
		await launcher.StartAsync(client).CAF();

		return services;
	}

	/// <summary>
	/// Gets the path and bot key from user input if they're not already stored in file.
	/// </summary>
	/// <returns></returns>
	public async Task GetPathAndKeyAsync()
	{
		// Get the save path
		var validPath = _Config.ValidatePath(null, true);
		while (!validPath)
		{
			validPath = _Config.ValidatePath(Console.ReadLine(), false);
		}

		// Get the bot key
		var validKey = await _Config.ValidateBotKey(null, true, ClientUtils.RestartBotAsync).CAF();
		while (!validKey)
		{
			validKey = await _Config.ValidateBotKey(Console.ReadLine(), false, ClientUtils.RestartBotAsync).CAF();
		}
	}

	/// <summary>
	/// Creates the service provider and starts the Discord bot.
	/// </summary>
	/// <param name="client"></param>
	/// <returns></returns>
	public Task StartAsync(BaseSocketClient client)
		=> _Config.StartAsync(client);

	/// <summary>
	/// Waits until the old process is killed. This is blocking.
	/// </summary>
	public void WaitUntilOldProcessKilled()
	{
		//Wait until the old process is killed
		if (_Config.PreviousProcessId != -1)
		{
			try
			{
				while (Process.GetProcessById(_Config.PreviousProcessId) != null)
				{
					Thread.Sleep(25);
				}
			}
			catch (ArgumentException) { }
		}
	}

	private static async Task<IServiceProvider> CreateServicesAsync(
		CommandAssemblyCollection assemblies,
		IConfig config)
	{
		const GatewayIntents INTENTS = GatewayIntents.All;

		var botSettings = NaiveBotSettings.CreateOrLoad(config);
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
			GatewayIntents = INTENTS,
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
			.AddSingleton(discordClient)
			.AddSingleton(httpClient)
			.AddSingleton<BaseSocketClient>(discordClient)
			.AddSingleton<IDiscordClient>(discordClient)
			.AddSingleton<IBotSettings>(botSettings)
			.AddSingleton<IBotDirectoryAccessor>(botSettings)
			.AddSingleton<ITime, DefaultTime>()
			.AddSingleton<IHelpEntryService, HelpEntryService>()
			.AddSingleton<ICommandHandlerService, CommandHandlerService>()
			.AddSingleton<ILogCounterService, LogCounterService>()
			.AddSingleton<IImageResizer, ImageResizer>()
			.AddSingleton<IPunisher, Punisher>()
			.AddSingleton<IGuildSettingsProvider, NaiveGuildSettingsProvider>();

		foreach (var assembly in assemblies.Assemblies)
		{
			if (assembly.Instantiator != null)
			{
				await assembly.Instantiator.AddServicesAsync(collection).CAF();
			}
		}

		var provider = collection.BuildServiceProvider();
		foreach (var service in collection)
		{
			if (service.Lifetime != ServiceLifetime.Singleton)
			{
				continue;
			}

			// Just to instantiate it
			_ = provider.GetRequiredService(service.ServiceType);
		}

		foreach (var assembly in assemblies.Assemblies)
		{
			if (assembly.Instantiator != null)
			{
				await assembly.Instantiator.ConfigureServicesAsync(provider).CAF();
			}
		}

		return provider;
	}

	private async Task<IServiceProvider> GetServicesAsync(CommandAssemblyCollection assemblies)
		=> _Services ??= await CreateServicesAsync(assemblies, _Config).CAF();
}