using Advobot.Actions.Formatting;
using Advobot.Classes;
using Advobot.Classes.Settings;
using Advobot.Classes.TypeReaders;
using Advobot.Interfaces;
using Advobot.Modules.GuildSettings;
using Advobot.Modules.Log;
using Advobot.Modules.Timers;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Advobot.Actions
{
	public static class CreationActions
	{
		/// <summary>
		/// Creates services the bot uses. Such as <see cref="IBotSettings"/>, <see cref="IGuildSettingsService"/>, <see cref="IDiscordClient"/>,
		/// <see cref="ITimersService"/>, and <see cref="ILogService"/>.
		/// </summary>
		/// <returns>The service provider which holds all the services.</returns>
		public static IServiceProvider CreateServicesAndServiceProvider()
		{
			return new DefaultServiceProviderFactory().CreateServiceProvider(new ServiceCollection()
				.AddSingleton<CommandService>		(CreateCommandService())
				.AddSingleton<IBotSettings>			(CreateBotSettings())
				.AddSingleton<IDiscordClient>		(x => CreateDiscordClient(x.GetRequiredService<IBotSettings>()))
				.AddSingleton<IGuildSettingsService>(x => new GuildSettingsHolder(x))
				.AddSingleton<ITimersService>		(x => new Timers(x))
				.AddSingleton<ILogService>			(x => new Log(x)));
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
				MessageCacheSize = (int)botSettings.MessageCacheCount,
				LogLevel = botSettings.LogLevel,
				TotalShards = (int)botSettings.ShardCount,
			};
			return botSettings.ShardCount > 1 ? new DiscordShardedClient(config) : (IDiscordClient)new DiscordSocketClient(config);
		}
		/// <summary>
		/// Creates the <see cref="CommandService"/> for the bot. Add in typereaders and modules.
		/// </summary>
		/// <returns></returns>
		internal static CommandService CreateCommandService()
		{
			var commandService = new CommandService(new CommandServiceConfig { CaseSensitiveCommands = false, ThrowOnError = false, });

			commandService.AddTypeReader(typeof(IInvite), new InviteTypeReader());
			commandService.AddTypeReader(typeof(IBan), new BanTypeReader());
			commandService.AddTypeReader(typeof(Emote), new EmoteTypeReader());
			commandService.AddTypeReader(typeof(Color), new ColorTypeReader());
			commandService.AddTypeReader(typeof(CommandSwitch), new CommandSwitchTypeReader());

			return commandService;
		}
		/// <summary>
		/// Creates settings that the bot uses.
		/// </summary>
		/// <param name="botSettingsType"></param>
		/// <returns></returns>
		internal static IBotSettings CreateBotSettings()
		{
			IBotSettings botSettings = null;
			var fileInfo = GetActions.GetBaseBotDirectoryFile(Constants.BOT_SETTINGS_LOCATION);
			if (fileInfo.Exists)
			{
				try
				{
					using (var reader = new StreamReader(fileInfo.FullName))
					{
						botSettings = SavingAndLoadingActions.Deserialize<IBotSettings>(reader.ReadToEnd(), Constants.BOT_SETTINGS_TYPE);
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
		internal static async Task<IGuildSettings> CreateGuildSettings(IGuild guild)
		{
			IGuildSettings guildSettings = null;
			var fileInfo = GetActions.GetServerDirectoryFile(guild.Id, Constants.GUILD_SETTINGS_LOCATION);
			if (fileInfo.Exists)
			{
				try
				{
					using (var reader = new StreamReader(fileInfo.FullName))
					{
						guildSettings = SavingAndLoadingActions.Deserialize<IGuildSettings>(reader.ReadToEnd(), Constants.GUILD_SETTINGS_TYPE);
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
