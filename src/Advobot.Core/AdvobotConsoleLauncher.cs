using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Advobot.Classes;
using Advobot.Classes.DatabaseWrappers;
using Advobot.Classes.DatabaseWrappers.LiteDB;
using Advobot.Classes.DatabaseWrappers.MongoDB;
using Advobot.Classes.ImageResizing;
using Advobot.Interfaces;
using Advobot.Services.BotSettings;
using Advobot.Services.Commands;
using Advobot.Services.GuildSettings;
using Advobot.Services.HelpEntries;
using Advobot.Services.InviteList;
using Advobot.Services.Levels;
using Advobot.Services.Logging;
using Advobot.Services.Timers;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace Advobot
{
	/// <summary>
	/// Puts the similarities from launching the console application and the .Net Core UI application into one.
	/// </summary>
	public sealed class AdvobotConsoleLauncher
	{
		private ILowLevelConfig _Config { get; }
		private IServiceCollection? _Services { get; set; }

		/// <summary>
		/// Creates an instance of <see cref="AdvobotConsoleLauncher"/>.
		/// </summary>
		/// <param name="args"></param>
		public AdvobotConsoleLauncher(string[] args)
		{
			AppDomain.CurrentDomain.UnhandledException += (sender, e) => IOUtils.LogUncaughtException(e.ExceptionObject);
			Console.Title = "Advobot";
			ConsoleUtils.PrintingFlags = 0
				| ConsolePrintingFlags.Print
				| ConsolePrintingFlags.LogTime
				| ConsolePrintingFlags.LogCaller
				| ConsolePrintingFlags.RemoveDuplicateNewLines;

			_Config = LowLevelConfig.Load(args);
			ConsoleUtils.DebugWrite($"Args: {_Config.CurrentInstance}|{_Config.PreviousProcessId}", "Launcher Arguments");
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
		/// <summary>
		/// Gets the path and bot key from user input if they're not already stored in file.
		/// </summary>
		/// <returns></returns>
		public async Task GetPathAndKeyAsync()
		{
            //Get the save path
            var startup = true;
            while (!_Config.ValidatedPath)
            {
                startup = _Config.ValidatePath(startup ? null : Console.ReadLine(), startup);
            }

            //Get the bot key
            startup = true;
            while (!_Config.ValidatedKey)
            {
                startup = await _Config.ValidateBotKey(startup ? null : Console.ReadLine(), startup, ClientUtils.RestartBotAsync).CAF();
            }
        }
		/// <summary>
		/// Returns the default services for the bot if both the path and key have been set.
		/// </summary>
		/// <returns></returns>
		public IServiceCollection GetDefaultServices(IEnumerable<Assembly> commands)
		{
			if (!(_Config.ValidatedPath && _Config.ValidatedKey))
			{
				throw new InvalidOperationException("Attempted to start the bot before the path and key have been set.");
			}
			return _Services ?? (_Services = CreateDefaultServices(_Config, commands));
		}
		/// <summary>
		/// Creates a provider and initializes all of its singletons.
		/// </summary>
		/// <param name="services"></param>
		/// <returns></returns>
		public IServiceProvider CreateProvider(IServiceCollection services)
		{
			var provider = services.BuildServiceProvider();
			foreach (var service in services.Where(x => x.Lifetime == ServiceLifetime.Singleton))
			{
				provider.GetRequiredService(service.ServiceType);
			}
			return provider;
		}
		private static IServiceCollection CreateDefaultServices(ILowLevelConfig config, IEnumerable<Assembly> commands)
		{
			T StartDatabase<T>(T db) where T : IUsesDatabase
			{
				db.Start();
				return db;
			}

			commands = commands ?? throw new ArgumentException($"{nameof(commands)} cannot be null.");
			//I have no idea if I am providing services correctly, but it works.
			var s = new ServiceCollection();
			s.AddSingleton(p =>
			{
				return new CommandService(new CommandServiceConfig
				{
					CaseSensitiveCommands = false,
					ThrowOnError = false,
				});
			});
			s.AddSingleton(p =>
			{
				var settings = p.GetRequiredService<IBotSettings>();
				return new DiscordShardedClient(new DiscordSocketConfig
				{
					AlwaysDownloadUsers = settings.AlwaysDownloadUsers,
					MessageCacheSize = settings.MessageCacheSize,
					LogLevel = settings.LogLevel,
				});
			});
			s.AddSingleton<IHelpEntryService>(p => new HelpEntryService());
			s.AddSingleton<IBotSettings>(p => BotSettings.Load(config));
			s.AddSingleton<ICommandHandlerService>(p => new CommandHandlerService(p, commands));
			s.AddSingleton<IGuildSettingsFactory>(p => new GuildSettingsFactory<GuildSettings>(p));
			s.AddSingleton<ILogService>(p => new LogService(p));
			s.AddSingleton<ILevelService>(p => StartDatabase(new LevelService(p)));
			s.AddSingleton<ITimerService>(p => StartDatabase(new TimerService(p)));
			s.AddSingleton<IInviteListService>(p => StartDatabase(new InviteListService(p)));
			s.AddSingleton<IImageResizer>(p => new ImageResizer(10));

			switch (config.DatabaseType)
			{
				//-DatabaseType LiteDB (or no arguments supplied at all)
				case DatabaseType.LiteDB:
					s.AddSingleton<IDatabaseWrapperFactory>(p => new LiteDBWrapperFactory(p));
					break;
				//-DatabaseType MongoDB -DatabaseConnectionString "mongodb://localhost:27017"
				case DatabaseType.MongoDB:
					s.AddSingleton<IDatabaseWrapperFactory>(p => new MongoDBWrapperFactory(p));
					s.AddSingleton<IMongoClient>(p => new MongoClient(config.DatabaseConnectionString));
					break;
			}
			return s;
		}
		/// <summary>
		/// Creates the service provider and starts the Discord bot.
		/// </summary>
		/// <returns></returns>
		public Task StartAsync(IServiceProvider provider)
		{
			if (!(_Config.ValidatedPath && _Config.ValidatedKey))
			{
				throw new InvalidOperationException("Attempted to start the bot before the path and key have been set.");
			}
			return _Config.StartAsync(provider.GetRequiredService<DiscordShardedClient>());
		}
	}
}