using Advobot.Actions;
using Advobot.Logging;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

/* First, to get the really shitty part of the bot out of the way:
 * 0.	I am too lazy to type out .ConfigureAwait(false) on every await I do and I don't really know what it does so I don't use it.
 * 
 * 1.	A lot of guild settings return a readonly collection because that forces whoever is messing with them to reassign instead of just using .add
 *		This forces the setter to be used, which saves the list, thus keeping any changes made.
 *		
 * 2.	I didn't know about Discord.Net's arg parsing until about 7 months into this project. That's why some parts may look like my own custom arg parsing.
 * 
 * 3.	ILogModule goes into MyCommandContext to be used in exactly one command. The getinfo bot command.
 */
namespace Advobot
{
	public class Program
	{
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

			//Things that when not loaded fuck the bot completely. These things have to go in this order because I'm a dumdum who made stuff have dependencies.
			CriticalInformation criticalInfo = SavingAndLoading.LoadCriticalInformation();
			IBotSettings botSettings = SavingAndLoading.CreateBotSettings(Constants.GLOBAL_SETTINGS_TYPE, criticalInfo.Windows, criticalInfo.Console, criticalInfo.FirstInstance);
			IGuildSettingsModule guildSettingsModule = new GuildSettingsModule(Constants.GUILDS_SETTINGS_TYPE);
			IDiscordClient client = ClientActions.CreateBotClient(botSettings);
			ILogModule logModule = new LogModule(client, botSettings, guildSettingsModule);

			var provider = ConfigureServices(client, botSettings, guildSettingsModule, logModule);
			await CommandHandler.Install(provider);

			//If not a console application then start the UI
			if (!botSettings.Console)
			{
				new System.Windows.Application().Run(new BotWindow(client, botSettings, logModule));
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

		private IServiceProvider ConfigureServices(IDiscordClient client, IBotSettings botSettings, IGuildSettingsModule guildSettingsModule, ILogModule logModule)
		{
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddSingleton(client);
			serviceCollection.AddSingleton(botSettings);
			serviceCollection.AddSingleton(guildSettingsModule);
			serviceCollection.AddSingleton(logModule);
			serviceCollection.AddSingleton(new CommandService(new CommandServiceConfig { CaseSensitiveCommands = false, ThrowOnError = false, }));

			return new DefaultServiceProviderFactory().CreateServiceProvider(serviceCollection);
		}
	}
}