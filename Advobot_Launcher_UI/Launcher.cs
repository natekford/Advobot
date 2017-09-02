using Advobot.Actions;
using Advobot.Graphics.UserInterface;
using Advobot.NonSavedClasses;
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
				MainAsync().GetAwaiter().GetResult();
			}

			private static async Task MainAsync()
			{
				AppDomain.CurrentDomain.UnhandledException += (sender, e) => SavingAndLoadingActions.LogUncaughtException(sender, e);

				//Make sure only one instance is running at the same time
#if RELEASE
				if (System.Diagnostics.Process.GetProcessesByName(System.IO.Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetEntryAssembly().Location)).Length > 1)
					return;
#endif
				//Only has a reason to call this method in the UI launcher because the console version has no way of displaying them.
				ConsoleActions.CreateWrittenLines();

				var botSettings = SavingAndLoadingActions.CreateBotSettings(Constants.GLOBAL_SETTINGS_TYPE);
				var guildSettings = new MyGuildSettingsModule(Constants.GUILDS_SETTINGS_TYPE);
				var client = ClientActions.CreateBotClient(botSettings);
				var provider = CommandHandler.ConfigureServices(client, botSettings, guildSettings);

				await CommandHandler.Install(provider);
				new System.Windows.Application().Run(new MyWindow(provider));
			}
		}
	}
}
