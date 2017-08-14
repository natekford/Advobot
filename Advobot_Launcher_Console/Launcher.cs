using Advobot.Actions;
using Advobot.Interfaces;
using Advobot.Logging;
using Advobot.NonSavedClasses;
using Advobot.Timers;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Advobot
{
	namespace Launcher
	{
		public class ConsoleLauncher
		{
			private static void Main()
			{
				new ConsoleLauncher().SubMain().GetAwaiter().GetResult();
			}

			private async Task SubMain()
			{
				//Make sure only one instance is running at the same time
#if RELEASE
				if (System.Diagnostics.Process.GetProcessesByName(System.IO.Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetEntryAssembly().Location)).Length > 1)
					return;
#endif
				//Things that when not loaded fuck the bot completely.
				var criticalInfo = SavingAndLoadingActions.LoadCriticalInformation();

				IBotSettings botSettings = SavingAndLoadingActions.CreateBotSettings(Constants.GLOBAL_SETTINGS_TYPE, criticalInfo.Windows, criticalInfo.Console, criticalInfo.FirstInstance);
				IGuildSettingsModule guildSettings = new MyGuildSettingsModule(Constants.GUILDS_SETTINGS_TYPE);
				ITimersModule timers = new MyTimersModule(guildSettings);
				IDiscordClient client = ClientActions.CreateBotClient(botSettings);
				ILogModule logging = new MyLogModule(client, botSettings, guildSettings, timers);
				IServiceProvider provider = ConfigureServices(client, botSettings, guildSettings, timers, logging);

				AppDomain.CurrentDomain.UnhandledException += (sender, e) => SavingAndLoadingActions.LogUncaughtException(sender, e, logging);

				await CommandHandler.Install(provider);
				await StartBot(provider, client, botSettings);
			}

			private async Task StartBot(IServiceProvider provider, IDiscordClient client, IBotSettings botSettings)
			{
				var startup = true;
				while (!botSettings.GotPath)
				{
					var input = startup ? GetActions.GetSavePath() : Console.ReadLine();
					if (SavingAndLoadingActions.ValidatePath(input, botSettings.Windows, startup))
					{
						botSettings.SetGotPath();
					}
					startup = false;
				}
				startup = true;
				while (!botSettings.GotKey)
				{
					var input = startup ? GetActions.GetBotKey() : Console.ReadLine();
					if (await SavingAndLoadingActions.ValidateBotKey(client, input, startup))
					{
						botSettings.SetGotKey();
					}
					startup = false;
				}

				await ClientActions.MaybeStartBot(client, botSettings);
			}

			private IServiceProvider ConfigureServices(IDiscordClient client, IBotSettings botSettings, IGuildSettingsModule guildSettings, ITimersModule timers, ILogModule logging)
			{
				var serviceCollection = new ServiceCollection();
				serviceCollection.AddSingleton(client);
				serviceCollection.AddSingleton(botSettings);
				serviceCollection.AddSingleton(guildSettings);
				serviceCollection.AddSingleton(timers);
				serviceCollection.AddSingleton(logging);
				serviceCollection.AddSingleton(new CommandService(new CommandServiceConfig { CaseSensitiveCommands = false, ThrowOnError = false, }));

				return new DefaultServiceProviderFactory().CreateServiceProvider(serviceCollection);
			}
		}
	}
}
