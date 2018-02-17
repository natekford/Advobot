using Advobot.Core.Classes;
using Advobot.Core.Classes.Attributes;
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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Advobot.Core.Utilities
{
	/// <summary>
	/// Actions creating the services for the main <see cref="IServiceProvider"/>.
	/// </summary>
	public static class CreationUtils
	{
		/// <summary>
		/// Creates services the bot uses. The explicit implementations will always be the same; if wanting to customize
		/// them do not use this method.
		/// </summary>
		/// <returns>The service provider which holds all the services.</returns>
		public static IServiceProvider CreateDefaultServiceProvider(IEnumerable<Assembly> commandAssembies, Type botSettingsType, Type guildSettingsType)
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
			var helpEntryHolder = new HelpEntryHolder(commandAssembies);
			return new DefaultServiceProviderFactory().CreateServiceProvider(new ServiceCollection()
				.AddSingleton<CommandService>(provider => CreateCommandService(commandAssembies))
				.AddSingleton<HelpEntryHolder>(helpEntryHolder)
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
		internal static CommandService CreateCommandService(IEnumerable<Assembly> commandAssemblies)
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
			cmds.AddTypeReader<RuleCategory>(new RuleCategoryTypeReader());
			cmds.AddTypeReader<CommandCategory>(new CommandCategoryTypeReader());
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
					await cmds.AddModulesAsync(assembly).CAF();
				}
				ConsoleUtils.DebugWrite("Successfully added every command assembly.");
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
			return IOUtils.DeserializeFromFile<IBotSettings>(IOUtils.GetBotSettingsFile(), botSettingsType, true);
		}
		/// <summary>
		/// Creates settings the guilds use.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="guild"></param>
		/// <returns></returns>
		internal static IGuildSettings CreateGuildSettings(Type type, IGuild guild)
		{
			var settings = IOUtils.GenerateDefaultSerializerSettings();
			settings.Context = new StreamingContext(StreamingContextStates.Other, guild);
			return IOUtils.DeserializeFromFile<IGuildSettings>(IOUtils.GetGuildSettingsFile(guild.Id), type, true, settings);
		}
	}
}
