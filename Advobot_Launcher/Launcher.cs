using Advobot.Actions;
using Advobot.Logging;
using Advobot.Graphics.UserInterface;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

/* First, to get the really shitty part of the bot out of the way:
 * 0.	I am too lazy to type out .ConfigureAwait(false) on every await I do and I don't really know what it does so I don't use it.
 * 
 * 1.	A lot of guild settings return a readonly collection because that forces whoever is messing with them to reassign instead of just using .add
 *		This forces the setter to be used, which saves the list, thus keeping any changes made.
 *		
 * 2.	I didn't know about Discord.Net's arg parsing until about 7 months into this project. That's why some parts may look like my own custom arg parsing.
 * 
 * 3.	ILogModule goes into MyCommandContext to be used in exactly one command. The getinfo bot command.
 * 
 * To get some of the better parts out there:
 * 0.	The bot can easily be switched from sharded to single shard and console to a WPF. Can't easily switch OSes yet though.
 * 
 * 1.	The UI is completely optional. If removing it, you only need to remove about five lines from SubMain() then remove the .cs file from the project.
 * 
 * 2.	I'm slowly making each module completely optional (aside from IBotSettings and IGuildSettingsModule)
 *		ILogModule can be removed completely and only gives six easy to comment out errors.
 *		ITimersModule shouldn't be too hard to code out (just null check a lot of places).
 *		IInviteListModule will be easy to replace when I get around to creating it.
 */
namespace Advobot
{
	namespace Launcher
	{
		public class Program
		{
			//To hide the console
			[DllImport("kernel32.dll", SetLastError = true)]
			private static extern int FreeConsole();

			[STAThread]
			private static void Main()
			{
				new Program().SubMain().GetAwaiter().GetResult();
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

				//Only ask if the user wants to launch the UI if on windows and the program is targetting console
				var console = true;
				if (criticalInfo.Windows)
				{
					if (!criticalInfo.Console)
					{
						console = false;
					}
					else
					{
						ConsoleActions.WriteLine("Would you like to start the program with a GUI? [Y/N]");
						while (true)
						{
							var response = Console.ReadLine();
							if (response.CaseInsStartsWith("y"))
							{
								console = false;
								break;
							}
							else if (response.CaseInsStartsWith("n"))
							{
								console = true;
								break;
							}
							else
							{
								ConsoleActions.WriteLine("Please enter Y or N.");
							}
						}
					}
				}

				IBotSettings botSettings = SavingAndLoading.CreateBotSettings(Constants.GLOBAL_SETTINGS_TYPE, criticalInfo.Windows, console, criticalInfo.FirstInstance);
				IGuildSettingsModule guildSettings = new MyGuildSettingsModule(Constants.GUILDS_SETTINGS_TYPE);
				ITimersModule timers = new MyTimersModule(guildSettings);
				IDiscordClient client = ClientActions.CreateBotClient(botSettings);
				ILogModule logging = new MyLogModule(client, botSettings, guildSettings, timers);

				var provider = ConfigureServices(client, botSettings, guildSettings, timers, logging);
				await CommandHandler.Install(provider);

				//If not a console application then start the UI
				if (!console)
				{
					FreeConsole();
					new System.Windows.Application().Run(new BotWindow(provider));
				}
				else
				{
					var startup = true;
					while (!botSettings.GotPath)
					{
						if (SavingAndLoading.ValidatePath((startup ? Properties.Settings.Default.Path : Console.ReadLine()), botSettings.Windows, startup))
						{
							botSettings.SetGotPath();
						}
						startup = false;
					}
					startup = true;
					while (!botSettings.GotKey)
					{
						if (await SavingAndLoading.ValidateBotKey(client, (startup ? Properties.Settings.Default.BotKey : Console.ReadLine()), startup))
						{
							botSettings.SetGotKey();
						}
						startup = false;
					}

					await ClientActions.MaybeStartBot(client, botSettings);
				}
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
