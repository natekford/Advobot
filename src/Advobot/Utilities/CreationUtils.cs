using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Advobot.Classes;
using Advobot.Classes.Attributes;
using Advobot.Classes.TypeReaders;
using Advobot.Interfaces;
using Advobot.Services.Commands;
using Advobot.Services.GuildSettings;
using Advobot.Services.InviteList;
using Advobot.Services.Logging;
using Advobot.Services.Timers;
using Advobot.Services.Levels;
using AdvorangesUtils;
using Discord;
using Discord.Commands;
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
		public static IServiceCollection CreateDefaultServices(ILowLevelConfig config, IEnumerable<Assembly> commands = null)
		{
			commands = commands ?? DiscordUtils.GetCommandAssemblies();
			//Not sure why when put into a func provided as an argument there are lots of enumeration errors
			var helpEntryHolder = new HelpEntryHolder(commands);
			//I have no idea if I am providing services correctly, but it works.
			return new ServiceCollection()
				.AddSingleton<CommandService>(provider => CreateCommandService(provider, commands))
				.AddSingleton<HelpEntryHolder>(helpEntryHolder)
				.AddSingleton<DiscordShardedClient>(provider => CreateDiscordClient(config))
				.AddSingleton<ILowLevelConfig>(config)
				.AddSingleton<IBotSettings>(provider => BotSettings.Load<BotSettings>(config))
				.AddSingleton<ILevelService>(provider => new LevelService(provider, new LevelServiceArguments()))
				.AddSingleton<ICommandHandlerService>(provider => new CommandHandlerService(provider))
				.AddSingleton<IGuildSettingsService>(provider => new GuildSettingsFactory<GuildSettings>(provider))
				.AddSingleton<ITimerService>(provider => new TimerService(provider))
				.AddSingleton<ILogService>(provider => new LogService(provider))
				.AddSingleton<IInviteListService>(provider => new InviteListService(provider));
		}
		/// <summary>
		/// Creates the <see cref="CommandService"/> for the bot. Adds in typereaders and modules.
		/// </summary>
		/// <returns></returns>
		private static CommandService CreateCommandService(IServiceProvider provider, IEnumerable<Assembly> commandAssemblies)
		{
			var cmds = new CommandService(new CommandServiceConfig
			{
				CaseSensitiveCommands = false,
				ThrowOnError = false,
			});

			cmds.AddTypeReader<IInvite>(new InviteTypeReader());
			cmds.AddTypeReader<IBan>(new BanTypeReader());
			cmds.AddTypeReader<IWebhook>(new WebhookTypeReader());
			cmds.AddTypeReader<Emote>(new EmoteTypeReader());
			cmds.AddTypeReader<GuildEmote>(new GuildEmoteTypeReader());
			cmds.AddTypeReader<Color>(new ColorTypeReader());
			cmds.AddTypeReader<Uri>(new UriTypeReader());
			cmds.AddTypeReader<ModerationReason>(new ModerationReasonTypeReader());

			//Add in generic custom argument type readers
			var customArgumentsClasses = Assembly.GetAssembly(typeof(NamedArguments<>)).GetTypes()
				.Where(t => t.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
				.Any(c => c.GetCustomAttribute<NamedArgumentConstructorAttribute>() != null));
			foreach (var c in customArgumentsClasses)
			{
				var t = typeof(NamedArguments<>).MakeGenericType(c);
				var tr = (TypeReader)Activator.CreateInstance(typeof(NamedArgumentsTypeReader<>).MakeGenericType(c));
				cmds.AddTypeReader(t, tr);
			}

			//Add in commands
			Task.Run(async () =>
			{
				foreach (var assembly in commandAssemblies)
				{
					await cmds.AddModulesAsync(assembly, provider).CAF();
				}
				ConsoleUtils.DebugWrite("Successfully added every command assembly.");
			});

			return cmds;
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
