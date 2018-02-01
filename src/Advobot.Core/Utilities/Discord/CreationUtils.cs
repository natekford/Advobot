using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Advobot.Core.Classes;
using Advobot.Core.Classes.Attributes;
using Advobot.Core.Classes.NamedArguments;
using Advobot.Core.Classes.Rules;
using Advobot.Core.Classes.TypeReaders;
using Advobot.Core.Interfaces;
using Advobot.Core.Services.GuildSettings;
using Advobot.Core.Services.InviteList;
using Advobot.Core.Services.Log;
using Advobot.Core.Services.Timers;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Core.Utilities
{
	/// <summary>
	/// Actions creating the services for the main <see cref="IServiceProvider"/>.
	/// </summary>
	public static class CreationUtils
	{
		/// <summary>
		/// Creates services the bot uses. Such as <see cref="IBotSettings"/>, <see cref="IGuildSettingsService"/>, <see cref="IDiscordClient"/>,
		/// <see cref="ITimersService"/>, and <see cref="ILogService"/>.
		/// </summary>
		/// <returns>The service provider which holds all the services.</returns>
		public static IServiceProvider CreateServiceProvider(Type botSettingsType, Type guildSettingsType)
		{
			if (!typeof(IBotSettings).IsAssignableFrom(botSettingsType))
			{
				throw new ArgumentException($"Must inherit {nameof(IBotSettings)}.", nameof(botSettingsType));
			}
			if (typeof(IBotSettings) == botSettingsType)
			{
				throw new ArgumentException($"Must not be the interface {nameof(IBotSettings)}.", nameof(botSettingsType));
			}
			if (!typeof(IGuildSettings).IsAssignableFrom(guildSettingsType))
			{
				throw new ArgumentException($"Must inherit {nameof(IGuildSettings)}.", nameof(guildSettingsType));
			}
			if (typeof(IGuildSettings) == guildSettingsType)
			{
				throw new ArgumentException($"Must not be the interface {nameof(IGuildSettings)}.", nameof(guildSettingsType));
			}

			//I have no idea if I am providing services correctly, but it works.
			return new DefaultServiceProviderFactory().CreateServiceProvider(new ServiceCollection()
				.AddSingleton<CommandService>(provider => CreateCommandService())
				.AddSingleton<IBotSettings>(provider => CreateBotSettings(botSettingsType))
				.AddSingleton<IDiscordClient>(provider => CreateDiscordClient(provider))
				.AddSingleton<IGuildSettingsService>(provider => new GuildSettingsService(guildSettingsType, provider))
				.AddSingleton<ITimersService>(provider => new TimersService(provider))
				.AddSingleton<ILogService>(provider => new LogService(provider))
				.AddSingleton<IInviteListService>(provider => new InviteListService(provider)));
		}
		/// <summary>
		/// Creates the <see cref="CommandService"/> for the bot. Add in typereaders and modules.
		/// </summary>
		/// <returns></returns>
		internal static CommandService CreateCommandService()
		{
			var cmds = new CommandService(new CommandServiceConfig
			{
				CaseSensitiveCommands = false,
				ThrowOnError = false,
			});

			cmds.AddTypeReader<IInvite>(new InviteTypeReader());
			cmds.AddTypeReader<IBan>(new BanTypeReader());
			cmds.AddTypeReader<Emote>(new EmoteTypeReader());
			cmds.AddTypeReader<Color>(new ColorTypeReader());
			cmds.AddTypeReader<RuleCategory>(new RuleCategoryTypeReader());
			cmds.AddTypeReader<CommandCategory>(new CommandCategoryTypeReader());

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
				foreach (var assembly in Constants.COMMAND_ASSEMBLIES)
				{
					await cmds.AddModulesAsync(assembly).CAF();
				}
				ConsoleUtils.WriteLine("Successfully added every command assembly.");
			});

			return cmds;
		}
		/// <summary>
		/// Returns <see cref="DiscordSocketClient"/> if shard count in <paramref name="botSettings"/> is 1. Else returns <see cref="DiscordShardedClient"/>.
		/// </summary>
		/// <param name="botSettings">The settings to initialize the client with.</param>
		/// <returns>A discord client.</returns>
		internal static IDiscordClient CreateDiscordClient(IServiceProvider provider)
		{
			var botSettings = provider.GetRequiredService<IBotSettings>();
			var config = new DiscordSocketConfig
			{
				AlwaysDownloadUsers = botSettings.AlwaysDownloadUsers,
				MessageCacheSize = botSettings.MessageCacheCount,
				LogLevel = botSettings.LogLevel,
				TotalShards = botSettings.ShardCount
			};
			return botSettings.ShardCount > 1 ? new DiscordShardedClient(config) : (IDiscordClient)new DiscordSocketClient(config);
		}
		/// <summary>
		/// Creates settings that the bot uses.
		/// </summary>
		/// <param name="botSettingsType"></param>
		/// <returns></returns>
		internal static IBotSettings CreateBotSettings(Type botSettingsType)
		{
			var path = IOUtils.GetBaseBotDirectoryFile(Constants.BOT_SETTINGS_LOC);
			return IOUtils.DeserializeFromFile<IBotSettings>(path, botSettingsType, true);
		}
		/// <summary>
		/// Creates settings the guilds use.
		/// </summary>
		/// <param name="guildSettingsType"></param>
		/// <param name="guild"></param>
		/// <returns></returns>
		internal static IGuildSettings CreateGuildSettings(Type guildSettingsType, IGuild guild)
		{
			var path = IOUtils.GetServerDirectoryFile(guild.Id, Constants.GUILD_SETTINGS_LOC);
			var jsonSettings = IOUtils.GenerateDefaultSerializerSettings();
			jsonSettings.Context = new StreamingContext(StreamingContextStates.Other, guild);
			return IOUtils.DeserializeFromFile<IGuildSettings>(path, guildSettingsType, true, jsonSettings);
		}
	}
}
