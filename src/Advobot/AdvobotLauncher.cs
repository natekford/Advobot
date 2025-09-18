using Advobot.Resources;
using Advobot.Services;
using Advobot.Services.BotConfig;
using Advobot.Services.Commands;
using Advobot.Services.Events;
using Advobot.Services.GuildSettings;
using Advobot.Services.Punishments;
using Advobot.Services.Time;
using Advobot.Utilities;

using Discord;
using Discord.WebSocket;

using Microsoft.Extensions.DependencyInjection;

using System.Collections.Immutable;
using System.Diagnostics;
using System.Net;
using System.Reflection;

using YACCS.Commands;
using YACCS.Help;
using YACCS.Localization;
using YACCS.Parsing;
using YACCS.Plugins;
using YACCS.TypeReaders;

namespace Advobot;

/// <summary>
/// Shuts down the application.
/// </summary>
/// <param name="exitCode"></param>
public delegate void ShutdownApplication(int exitCode);

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
		Localize.Instance.Append(new ResourceManagerLocalizer(Names.ResourceManager));
		Localize.Instance.Append(new ResourceManagerLocalizer(Parameters.ResourceManager));
		Localize.Instance.Append(new ResourceManagerLocalizer(Responses.ResourceManager));
		Localize.Instance.Append(new ResourceManagerLocalizer(Summaries.ResourceManager));

		Localize.Instance.Append(new ResourceManagerLocalizer(BotSettingNames.ResourceManager));
		Localize.Instance.Append(new ResourceManagerLocalizer(GuildSettingNames.ResourceManager));

		var botConfig = NaiveRuntimeConfig.CreateOrLoad(config);
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

		var directory = AppDomain.CurrentDomain.BaseDirectory;
		var files = Directory.EnumerateFiles(directory, "*.dll", SearchOption.TopDirectoryOnly);
		var pluginAssemblies = PluginUtils.LoadPluginAssemblies(files)
			.Values
			.Prepend(typeof(AdvobotLauncher).Assembly)
			.ToImmutableArray();

		var collection = new ServiceCollection()
			.AddSingleton(httpClient)
			.AddSingleton(discordClient)
			.AddSingleton<ShutdownApplication>(Environment.Exit)
			.AddSingleton<BaseSocketClient>(discordClient)
			.AddSingleton<IDiscordClient>(discordClient)
			.AddSingleton<IRuntimeConfig>(botConfig)
			.AddSingleton<IConfig>(botConfig)
			.AddSingleton<EventProvider>(eventProvider)
			.AddSingleton<IEnumerable<Assembly>>(pluginAssemblies)
			.AddSingleton<ILocalizer>(Localize.Instance)
			.AddSingleton(CommandServiceConfig.Default)
			.AddSingleton<NaiveCommandService>()
			.AddSingleton<CommandService>(x => x.GetRequiredService<NaiveCommandService>())
			.AddSingleton<ICommandService>(x => x.GetRequiredService<NaiveCommandService>())
			.AddSingleton<IArgumentHandler>(x =>
			{
				var config = x.GetRequiredService<CommandServiceConfig>();
				return new ArgumentHandler(
					config.Separator,
					config.StartQuotes,
					config.EndQuotes
				);
			})
			.AddSingleton<IReadOnlyDictionary<Type, ITypeReader>, TypeReaderRegistry>()
			.AddSingleton<IReadOnlyDictionary<Type, string>, TypeNameRegistry>()
			.AddSingleton<DiscordCommandService>()
			.AddSingleton<TimeProvider, NaiveTimeProvider>()
			.AddSingleton<IPunishmentService, NaivePunishmentService>()
			.AddSingleton<IGuildSettingsService, NaiveGuildSettingsService>();

		var services = await collection.InstantiatePlugins(
			pluginAssemblies: pluginAssemblies,
			createServiceProvider: x => x.BuildServiceProvider()
		);

		var commandService = services.GetRequiredService<NaiveCommandService>();
		await commandService.InitializeAsync().ConfigureAwait(false);

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

		services
			.GetRequiredService<IReadOnlyDictionary<Type, ITypeReader>>()
			.ThrowIfUnregisteredServices(services);

		Localize.Instance.KeyNotFound += (key, culture) =>
		{
			_ = eventProvider.Log.InvokeAsync(new(
				severity: LogSeverity.Warning,
				source: key,
				message: $"Unable to find the localization for '{key}' in '{culture}'."
			));
		};
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