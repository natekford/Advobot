using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Advobot.Classes;
using Advobot.Interfaces;
using Advobot.Services.Commands;
using Advobot.Services.GuildSettings;
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
			s.AddSingleton<HelpEntryHolder>(p => new HelpEntryHolder(commands));
			s.AddSingleton<DiscordShardedClient>(p => CreateDiscordClient(config));
			s.AddSingleton<ILowLevelConfig>(config);
			s.AddSingleton<IBotSettings>(p => BotSettings.Load<BotSettings>(config));
			s.AddSingleton<ILevelService>(p => new LevelService(s.Create(p), new LevelServiceArguments()));
			s.AddSingleton<ICommandHandlerService>(p => new CommandHandlerService(s.Create(p), commands));
			s.AddSingleton<IGuildSettingsService>(p => new GuildSettingsFactory<GuildSettings>(s.Create(p)));
			s.AddSingleton<ITimerService>(p => new TimerService(s.Create(p)));
			s.AddSingleton<ILogService>(p => new LogService(s.Create(p)));
			s.AddSingleton<IInviteListService>(p => new InviteListService(s.Create(p)));
			return s;
		}
		private static IterableServiceProvider Create(this ServiceCollection services, IServiceProvider provider)
		{
			return IterableServiceProvider.CreateFromExisting((ServiceProvider)provider, services);
		}
		/// <summary>
		/// Creates a sharded client with the supplied settings from the <see cref="IBotSettings"/> in the provider.
		/// </summary>
		/// <param name="config">The settings to initialize the client with.</param>
		/// <returns>A discord client.</returns>
		private static DiscordShardedClient CreateDiscordClient(ILowLevelConfig config)
		{
			return new DiscordShardedClient(new DiscordSocketConfig
			{
				AlwaysDownloadUsers = config.AlwaysDownloadUsers,
				MessageCacheSize = config.MessageCacheSize,
				LogLevel = config.LogLevel,
			});
		}
	}
}
