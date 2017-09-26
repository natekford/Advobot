using Advobot.Actions.Formatting;
using Advobot.Interfaces;
using Advobot.Modules.GuildSettings;
using Advobot.Modules.Log;
using Advobot.Modules.Timers;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Advobot.Actions
{
	public static class CreationActions
	{
		/// <summary>
		/// Creates services the bot uses. Such as <see cref="IBotSettings"/>, <see cref="IGuildSettingsModule"/>, <see cref="IDiscordClient"/>,
		/// <see cref="ITimersModule"/>, and <see cref="ILogModule"/>.
		/// </summary>
		/// <returns>The service provider which holds all the services.</returns>
		public static IServiceProvider CreateServicesAndServiceProvider()
		{
			return new DefaultServiceProviderFactory().CreateServiceProvider(new ServiceCollection()
				.AddSingleton<IBotSettings>			(CreateBotSettings())
				.AddSingleton<IDiscordClient>		(x => CreateDiscordClient(x.GetRequiredService<IBotSettings>()))
				.AddSingleton<IGuildSettingsModule>	(x => new MyGuildSettingsModule(x))
				.AddSingleton<ITimersModule>		(x => new MyTimersModule(x))
				.AddSingleton<ILogModule>			(x => new MyLogModule(x))
				.AddSingleton<CommandService>		(new CommandService(new CommandServiceConfig { CaseSensitiveCommands = false, ThrowOnError = false, })));
		}
		/// <summary>
		/// Returns <see cref="DiscordSocketClient"/> if shard count in <paramref name="botSettings"/> is 1. Else returns <see cref="DiscordShardedClient"/>.
		/// </summary>
		/// <param name="botSettings">The settings to initialize the client with.</param>
		/// <returns>A discord client.</returns>
		public static IDiscordClient CreateDiscordClient(IBotSettings botSettings)
		{
			if (botSettings.ShardCount > 1)
			{
				return new DiscordShardedClient(new DiscordSocketConfig
				{
					AlwaysDownloadUsers = botSettings.AlwaysDownloadUsers,
					MessageCacheSize = (int)botSettings.MessageCacheCount,
					LogLevel = botSettings.LogLevel,
					TotalShards = (int)botSettings.ShardCount,
				});
			}
			else
			{
				return new DiscordSocketClient(new DiscordSocketConfig
				{
					AlwaysDownloadUsers = botSettings.AlwaysDownloadUsers,
					MessageCacheSize = (int)botSettings.MaxUserGatherCount,
					LogLevel = botSettings.LogLevel,
				});
			}
		}
		/// <summary>
		/// Creates settings that the bot uses.
		/// </summary>
		/// <param name="botSettingsType"></param>
		/// <returns></returns>
		public static IBotSettings CreateBotSettings()
		{
			IBotSettings botSettings = null;
			var fileInfo = GetActions.GetBaseBotDirectoryFile(Constants.BOT_SETTINGS_LOCATION);
			if (fileInfo.Exists)
			{
				try
				{
					using (var reader = new StreamReader(fileInfo.FullName))
					{
						botSettings = (IBotSettings)JsonConvert.DeserializeObject(reader.ReadToEnd(), Constants.BOT_SETTINGS_TYPE);
					}
					ConsoleActions.WriteLine("The bot information has successfully been loaded.");
				}
				catch (Exception e)
				{
					ConsoleActions.ExceptionToConsole(e);
				}
			}
			else
			{
				ConsoleActions.WriteLine("The bot information file could not be found; using default.");
			}
			return (botSettings ?? (IBotSettings)Activator.CreateInstance(Constants.BOT_SETTINGS_TYPE));
		}
		/// <summary>
		/// Creates settings that guilds on the bot use.
		/// </summary>
		/// <param name="guildSettingsType"></param>
		/// <param name="guild"></param>
		/// <returns></returns>
		public static async Task<IGuildSettings> CreateGuildSettings(IGuild guild)
		{
			IGuildSettings guildSettings = null;
			var fileInfo = GetActions.GetServerDirectoryFile(guild.Id, Constants.GUILD_SETTINGS_LOCATION);
			if (fileInfo.Exists)
			{
				try
				{
					using (var reader = new StreamReader(fileInfo.FullName))
					{
						guildSettings = (IGuildSettings)JsonConvert.DeserializeObject(reader.ReadToEnd(), Constants.GUILD_SETTINGS_TYPE);
					}
					ConsoleActions.WriteLine($"The guild information for {guild.FormatGuild()} has successfully been loaded.");
				}
				catch (Exception e)
				{
					ConsoleActions.ExceptionToConsole(e);
				}
			}
			else
			{
				ConsoleActions.WriteLine($"The guild information file for {guild.FormatGuild()} could not be found; using default.");
			}
			return await (guildSettings ?? (IGuildSettings)Activator.CreateInstance(Constants.GUILD_SETTINGS_TYPE)).PostDeserialize(guild);
		}
	}
}
