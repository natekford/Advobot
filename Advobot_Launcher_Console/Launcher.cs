using Advobot.Actions;
using Advobot.NonSavedClasses;
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
				new ConsoleLauncher().MainAsync().GetAwaiter().GetResult();
			}

			private async Task MainAsync()
			{
				AppDomain.CurrentDomain.UnhandledException += (sender, e) => SavingAndLoadingActions.LogUncaughtException(sender, e);

				//Make sure only one instance is running at the same time
#if RELEASE
				if (System.Diagnostics.Process.GetProcessesByName(System.IO.Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetEntryAssembly().Location)).Length > 1)
					return;
#endif
				var botSettings = SavingAndLoadingActions.CreateBotSettings(Constants.GLOBAL_SETTINGS_TYPE);
				var guildSettings = new MyGuildSettingsModule(Constants.GUILDS_SETTINGS_TYPE);
				var client = ClientActions.CreateBotClient(botSettings);
				var provider = CommandHandler.ConfigureServices(client, botSettings, guildSettings);

				await CommandHandler.Install(provider);
				await ClientActions.MaybeStartBotWithConsole(client, botSettings);
			}
		}
	}
}
