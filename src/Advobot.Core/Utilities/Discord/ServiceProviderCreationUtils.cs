using Advobot.Core.Classes.Attributes;
using Advobot.Core.Classes.GuildSettings;
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
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

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
		public static async Task<IServiceProvider> CreateServiceProvider()
		{
			//I have no idea if I am providing services correctly, but it works.
			var commandService = await CreateCommandService().CAF();
			var botSettings = CreateBotSettings();
			var client = CreateDiscordClient(botSettings);
			return new DefaultServiceProviderFactory().CreateServiceProvider(new ServiceCollection()
				.AddSingleton<CommandService>(commandService)
				.AddSingleton<IBotSettings>(botSettings)
				.AddSingleton<IDiscordClient>(client)
				.AddSingleton<IGuildSettingsService>(x => new GuildSettingsService(x))
				.AddSingleton<ITimersService>(x => new TimersSservice(x))
				.AddSingleton<ILogService>(x => new LogService(x))
				.AddSingleton<IInviteListService>(x => new InviteListService(x)));
		}
		/// <summary>
		/// Creates the <see cref="CommandService"/> for the bot. Add in typereaders and modules.
		/// </summary>
		/// <returns></returns>
		internal static async Task<CommandService> CreateCommandService()
		{
			var cmds = new CommandService(new CommandServiceConfig { CaseSensitiveCommands = false, ThrowOnError = false, });

			cmds.AddTypeReader<IInvite>(new InviteTypeReader());
			cmds.AddTypeReader<IBan>(new BanTypeReader());
			cmds.AddTypeReader<Emote>(new EmoteTypeReader());
			cmds.AddTypeReader<Color>(new ColorTypeReader());
			cmds.AddTypeReader<RuleCategory>(new RuleCategoryTypeReader());

			//Add in generic custom argument type readers
			var customArgumentsClasses = Assembly.GetAssembly(typeof(NamedArguments<>)).GetTypes()
				.Where(t => t.GetConstructors(BindingFlags.Public | BindingFlags.Instance)
				.Any(c => c.GetCustomAttribute<NamedArgumentConstructorAttribute>() != null));
			foreach (var c in customArgumentsClasses)
			{
				var t = typeof(NamedArguments<>).MakeGenericType(c);
				var tr = (TypeReader)Activator.CreateInstance(typeof(NamedArgumentsTypeReader<>).MakeGenericType(c));
				cmds.AddTypeReader(t, tr);
			}

			//Add in commands
			foreach (var assembly in Constants.COMMAND_ASSEMBLIES)
			{
				await cmds.AddModulesAsync(assembly).CAF();
			}

			return cmds;
		}
		/// <summary>
		/// Returns <see cref="DiscordSocketClient"/> if shard count in <paramref name="botSettings"/> is 1. Else returns <see cref="DiscordShardedClient"/>.
		/// </summary>
		/// <param name="botSettings">The settings to initialize the client with.</param>
		/// <returns>A discord client.</returns>
		internal static IDiscordClient CreateDiscordClient(IBotSettings botSettings)
		{
			var config = new DiscordSocketConfig
			{
				AlwaysDownloadUsers = botSettings.AlwaysDownloadUsers,
				MessageCacheSize = botSettings.MessageCacheCount,
				LogLevel = botSettings.LogLevel,
				TotalShards = botSettings.ShardCount,
			};
			return botSettings.ShardCount > 1 ? new DiscordShardedClient(config) : (IDiscordClient)new DiscordSocketClient(config);
		}
		/// <summary>
		/// Creates settings that the bot uses.
		/// </summary>
		/// <param name="botSettingsType"></param>
		/// <returns></returns>
		internal static IBotSettings CreateBotSettings()
		{
			var path = IOUtils.GetBaseBotDirectoryFile(Constants.BOT_SETTINGS_LOC);
			var botSettings = IOUtils.DeserializeFromFile<IBotSettings>(path, Config.BotSettingsType, create: true);
			botSettings.SaveSettings();
			return botSettings;
		}
		/// <summary>
		/// Creates settings that guilds on the bot use.
		/// </summary>
		/// <param name="guildSettingsType"></param>
		/// <param name="guild"></param>
		/// <returns></returns>
		internal static IGuildSettings CreateGuildSettings(SocketGuild guild)
		{
			var path = IOUtils.GetServerDirectoryFile(guild.Id, Constants.GUILD_SETTINGS_LOC);
			var guildSettings = IOUtils.DeserializeFromFile<IGuildSettings>(path, Config.GuildSettingsType, create: true);
			guildSettings.SaveSettings();
			guildSettings.PostDeserialize(guild);
			return guildSettings;
		}
	}
}
