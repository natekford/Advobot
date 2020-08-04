using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Advobot.CommandAssemblies;
using Advobot.Databases;
using Advobot.Databases.Abstract;
using Advobot.Databases.LiteDB;
using Advobot.Databases.MongoDB;
using Advobot.Services.BotSettings;
using Advobot.Services.Commands;
using Advobot.Services.GuildSettings;
using Advobot.Services.HelpEntries;
using Advobot.Services.ImageResizing;
using Advobot.Services.LogCounters;
using Advobot.Services.Temp;
using Advobot.Services.Time;
using Advobot.Services.Timers;
using Advobot.Settings;
using Advobot.Utilities;

using AdvorangesUtils;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

using Microsoft.Extensions.DependencyInjection;

using MongoDB.Driver;

namespace Advobot
{
	/// <summary>
	/// Puts the similarities from launching the console application and the .Net Core UI application into one.
	/// </summary>
	public sealed class AdvobotLauncher
	{
		private readonly ILowLevelConfig _Config;
		private IServiceProvider? _Services;

		/// <summary>
		/// Creates an instance of <see cref="AdvobotLauncher"/>.
		/// </summary>
		/// <param name="config"></param>
		public AdvobotLauncher(ILowLevelConfig config)
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
			var launcher = new AdvobotLauncher(LowLevelConfig.Load(args));
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
			//Get the save path
			_Config.ValidatePath(null, true);
			while (!_Config.ValidatedPath)
			{
				_Config.ValidatePath(Console.ReadLine(), false);
			}

			//Get the bot key
			await _Config.ValidateBotKey(null, true, ClientUtils.RestartBotAsync).CAF();
			while (!_Config.ValidatedKey)
			{
				await _Config.ValidateBotKey(Console.ReadLine(), false, ClientUtils.RestartBotAsync).CAF();
			}
		}

		/// <summary>
		/// Creates the service provider and starts the Discord bot.
		/// </summary>
		/// <param name="client"></param>
		/// <returns></returns>
		public Task StartAsync(BaseSocketClient client)
		{
			if (!(_Config.ValidatedPath && _Config.ValidatedKey))
			{
				throw new InvalidOperationException("Attempted to start the bot before the path and key have been set.");
			}
			return _Config.StartAsync(client);
		}

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
			ILowLevelConfig config)
		{
			var botSettings = BotSettings.CreateOrLoad(config);
			var commandConfig = new CommandServiceConfig
			{
				CaseSensitiveCommands = false,
				ThrowOnError = false,
				LogLevel = botSettings.LogLevel,
			};
			var discordClient = new DiscordShardedClient(new DiscordSocketConfig
			{
				AlwaysDownloadUsers = botSettings.AlwaysDownloadUsers,
				MessageCacheSize = botSettings.MessageCacheSize,
				LogLevel = botSettings.LogLevel,
				ExclusiveBulkDelete = false,
			});
			var httpClient = new HttpClient(new HttpClientHandler
			{
				AllowAutoRedirect = true,
				Credentials = CredentialCache.DefaultCredentials,
				Proxy = new WebProxy(),
				AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
			});

			var s = new ServiceCollection()
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
				.AddSingleton<IGuildSettingsFactory, GuildSettingsFactory>()
				.AddSingleton<ILogCounterService, LogCounterService>()
				.AddSingleton<ITimerService, TimerService>()
				.AddSingleton<IImageResizer, ImageResizer>()
				//TODO: remove eventually
				.AddSingleton<TempService>();

			switch (config.DatabaseType)
			{
				//-DatabaseType LiteDB (or no arguments supplied at all)
				case DatabaseType.LiteDB:
					s.AddSingleton<IDatabaseWrapperFactory, LiteDBWrapperFactory>();
					break;
				//-DatabaseType MongoDB -DatabaseConnectionString "mongodb://localhost:27017"
				case DatabaseType.MongoDB:
					s.AddSingleton<IDatabaseWrapperFactory, MongoDBWrapperFactory>();
					s.AddSingleton<IMongoClient>(_ => new MongoClient(config.DatabaseConnectionString));
					break;
			}

			foreach (var assembly in assemblies.Assemblies)
			{
				if (assembly.Attribute.Instantiator != null)
				{
					await assembly.Attribute.Instantiator.AddServicesAsync(s).CAF();
				}
			}

			var services = s.BuildServiceProvider();
			foreach (var service in s)
			{
				if (service.Lifetime != ServiceLifetime.Singleton)
				{
					continue;
				}

				var instance = services.GetRequiredService(service.ServiceType);
				if (instance is IUsesDatabase usesDb)
				{
					usesDb.Start();
				}
			}

			foreach (var assembly in assemblies.Assemblies)
			{
				if (assembly.Attribute.Instantiator != null)
				{
					await assembly.Attribute.Instantiator.ConfigureServicesAsync(services).CAF();
				}
			}

			return services;
		}

		private async Task<IServiceProvider> GetServicesAsync(CommandAssemblyCollection assemblies)
			=> _Services ??= await CreateServicesAsync(assemblies, _Config).CAF();
	}
}