using Advobot.Actions;
using Advobot.Graphics.UserInterface;
using Advobot.Logging;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Advobot
{
	namespace Launcher
	{
		public class UILauncher
		{
			[STAThread]
			private static void Main()
			{
				new UILauncher().SubMain().GetAwaiter().GetResult();
			}

			private async Task SubMain()
			{
				//Make sure only one instance is running at the same time
#if RELEASE
			if (System.Diagnostics.Process.GetProcessesByName(System.IO.Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetEntryAssembly().Location)).Length > 1)
				return;
#endif
				//Things that when not loaded fuck the bot completely.
				var criticalInfo = SavingAndLoading.LoadCriticalInformation();

				IBotSettings botSettings = SavingAndLoading.CreateBotSettings(Constants.GLOBAL_SETTINGS_TYPE, criticalInfo.Windows, criticalInfo.Console, criticalInfo.FirstInstance);
				IGuildSettingsModule guildSettings = new MyGuildSettingsModule(Constants.GUILDS_SETTINGS_TYPE);
				ITimersModule timers = new MyTimersModule(guildSettings);
				IDiscordClient client = ClientActions.CreateBotClient(botSettings);
				ILogModule logging = new MyLogModule(client, botSettings, guildSettings, timers);
				IServiceProvider provider = ConfigureServices(client, botSettings, guildSettings, timers, logging);

				await CommandHandler.Install(provider);
				new System.Windows.Application().Run(new MyWindow(provider));
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
