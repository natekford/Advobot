using System;
using System.Collections.Generic;
using System.Reflection;
using Advobot.Classes;
using Advobot.Interfaces;
using Advobot.Services.Commands;
using Advobot.Services.GuildSettings;
using Advobot.Services.HelpEntries;
using Advobot.Services.InviteList;
using Advobot.Services.Levels;
using Advobot.Services.Logging;
using Advobot.Services.Timers;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Utilities
{
	/// <summary>
	/// Actions creating the services for the main <see cref="IServiceProvider"/>.
	/// </summary>
	public static class CreationUtils
	{
		/// <summary>
		/// Creates services the bot uses. The explicit implementations will always be the same; if wanting to customize
		/// them then remove them from the collection and put in your own implementations.
		/// </summary>
		/// <param name="config">Low level config which is crucial to the bot.</param>
		/// <param name="commands">The assemblies holding commands.</param>
		/// <returns>The service provider which holds all the services.</returns>
		public static ServiceCollection CreateDefaultServices(ILowLevelConfig config, IEnumerable<Assembly> commands = null)
		{
			commands = commands ?? DiscordUtils.GetCommandAssemblies();
			//I have no idea if I am providing services correctly, but it works.
			var s = new ServiceCollection();
			s.AddSingleton<DiscordShardedClient>(p => CreateDiscordClient(p));
			s.AddSingleton<IHelpEntryService>(p => new HelpEntryService());
			s.AddSingleton<IBotSettings>(p => BotSettings.Load(config));
			s.AddSingleton<ILevelService>(p => new LevelService(s.Create(p), new LevelServiceArguments()));
			s.AddSingleton<ICommandHandlerService>(p => new CommandHandlerService(s.Create(p), commands));
			s.AddSingleton<IGuildSettingsFactory>(p => new GuildSettingsFactory<GuildSettings>(s.Create(p)));
			s.AddSingleton<ITimerService>(p => new TimerService(s.Create(p)));
			s.AddSingleton<ILogService>(p => new LogService(s.Create(p)));
			s.AddSingleton<IInviteListService>(p => new InviteListService(s.Create(p)));
			return s;
		}
		/// <summary>
		/// Creates an <see cref="IIterableServiceProvider"/> from already existing services.
		/// </summary>
		/// <param name="services"></param>
		/// <param name="provider"></param>
		/// <returns></returns>
		private static IIterableServiceProvider Create(this ServiceCollection services, IServiceProvider provider)
		{
			return IterableServiceProvider.CreateFromExisting((ServiceProvider)provider, services);
		}
		/// <summary>
		/// Creates a sharded client with the supplied settings from the <see cref="IBotSettings"/> in the provider.
		/// </summary>
		/// <param name="provider"></param>
		/// <returns>A discord client.</returns>
		private static DiscordShardedClient CreateDiscordClient(IServiceProvider provider)
		{
			var settings = provider.GetRequiredService<IBotSettings>();
			return new DiscordShardedClient(new DiscordSocketConfig
			{
				AlwaysDownloadUsers = settings.AlwaysDownloadUsers,
				MessageCacheSize = settings.MessageCacheSize,
				LogLevel = settings.LogLevel,
			});
		}
	}
}
