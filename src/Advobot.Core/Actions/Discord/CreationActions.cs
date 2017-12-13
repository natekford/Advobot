﻿using Advobot.Core.Actions.Formatting;
using Advobot.Core.Classes;
using Advobot.Core.Classes.Attributes;
using Advobot.Core.Classes.Rules;
using Advobot.Core.Classes.Settings;
using Advobot.Core.Classes.TypeReaders;
using Advobot.Core.Enums;
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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Advobot.Core.Actions
{
	/// <summary>
	/// Actions creating the services for the main <see cref="IServiceProvider"/>.
	/// </summary>
	public static class CreationActions
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
				.AddSingleton<IGuildSettingsService>(x => new GuildSettingsHolder(x))
				.AddSingleton<ITimersService>(x => new Timers(x))
				.AddSingleton<ILogService>(x => new Log(x))
				.AddSingleton<IInviteListService>(x => new InviteList(x)));
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
			cmds.AddTypeReader<CommandSwitch>(new CommandSwitchTypeReader());
			cmds.AddTypeReader<RuleCategory>(new RuleCategoryTypeReader());
			//Add in generic custom argument type readers
			var customArgumentsClasses = Assembly.GetAssembly(typeof(CustomArguments<>)).GetTypes()
				.Where(t => t.GetConstructors(BindingFlags.Public | BindingFlags.Instance)
				.Any(c => c.GetCustomAttribute<CustomArgumentConstructorAttribute>() != null));
			foreach (var c in customArgumentsClasses)
			{
				var t = typeof(CustomArguments<>).MakeGenericType(c);
				var tr = (TypeReader)Activator.CreateInstance(typeof(CustomArgumentsTypeReader<>).MakeGenericType(c));
				cmds.AddTypeReader(t, tr);
			}

			//Add in commands
			await cmds.AddModulesAsync(Constants.COMMAND_ASSEMBLY).CAF();

			return cmds;
		}
		/// <summary>
		/// Creates settings that the bot uses.
		/// </summary>
		/// <param name="botSettingsType"></param>
		/// <returns></returns>
		internal static IBotSettings CreateBotSettings()
		{
			//Make sure every enum value in botsettings is accurate
			var fields = BotSettings.GetSettings().ToList();
			foreach (BotSetting e in Enum.GetValues(typeof(BotSetting)))
			{
				var matchingField = fields.SingleOrDefault(x => e.EnumName() == x.Name)
					?? throw new Exception($"{nameof(BotSetting)} has an invalid enum {e.EnumName()}.");
				fields.Remove(matchingField);
			}
			if (fields.Any())
			{
				throw new Exception($"The fields {String.Join(", ", fields.Select(x => x.Name))} are not set in {nameof(BotSetting)}.");
			}

			IBotSettings botSettings = null;
			var fileInfo = IOActions.GetBaseBotDirectoryFile(Constants.BOT_SETTINGS_LOCATION);
			if (fileInfo.Exists)
			{
				try
				{
					using (var reader = new StreamReader(fileInfo.FullName))
					{
						botSettings = IOActions.Deserialize<IBotSettings>(reader.ReadToEnd(), Constants.BOT_SETTINGS_TYPE);
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

			if (botSettings == null)
			{
				botSettings = (IBotSettings)Activator.CreateInstance(Constants.BOT_SETTINGS_TYPE);
				botSettings.SaveSettings();
			}
			return botSettings;
		}
		/// <summary>
		/// Creates settings that guilds on the bot use.
		/// </summary>
		/// <param name="guildSettingsType"></param>
		/// <param name="guild"></param>
		/// <returns></returns>
		internal static async Task<IGuildSettings> CreateGuildSettingsAsync(IGuild guild)
		{
			IGuildSettings guildSettings = null;
			var fileInfo = IOActions.GetServerDirectoryFile(guild.Id, Constants.GUILD_SETTINGS_LOCATION);
			if (fileInfo.Exists)
			{
				try
				{
					using (var reader = new StreamReader(fileInfo.FullName))
					{
						guildSettings = IOActions.Deserialize<IGuildSettings>(reader.ReadToEnd(), Constants.GUILD_SETTINGS_TYPE);
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

			if (guildSettings == null)
			{
				guildSettings = (IGuildSettings)Activator.CreateInstance(Constants.GUILD_SETTINGS_TYPE);
				guildSettings.SaveSettings();
			}
			return await guildSettings.PostDeserialize(guild).CAF();
		}
	}
}