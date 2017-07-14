using Advobot.Actions;
using Advobot.Logging;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

/* First, to get the really shitty part of the bot out of the way:
 * I am too lazy to type out .ConfigureAwait(false) on every await I do and I don't really know what it does so I don't use it.
 * A lot of the things that go into DontWaitForResultOfUnimportantBigFunction make the bot hang, so that's why they use async void. I don't know the correct way to not make them hang.
 * I wasn't aware of the arg parsing of Discord.Net when I first used it, so that's why I have my custom arg parsing.
 * My arg parsing is definitely more inefficient, but since I'm the one writing it I can provide more specific error messages.
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

			//Things that when not loaded fuck the bot completely
			var botInfo = SavingAndLoading.LoadCriticalInformation();
			var logging = new LogHolder(); //Circular logic, whoops
			var client = SavingAndLoading.CreateBotClient(botInfo, logging);
			logging.StartLogging(client, botInfo);

			var provider = ConfigureServices(client, botInfo, logging);
			await CommandHandler.Install(provider);

			//If not a console application then start the UI
			if (!botInfo.Console)
			{
				new System.Windows.Application().Run(new BotWindow(provider));
			}
			else
			{
				var startup = true;
				while (!botInfo.GotPath)
				{
					SavingAndLoading.ValidatePath(botInfo, (startup ? Properties.Settings.Default.Path : Console.ReadLine()), startup);
					startup = false;
				}
				startup = true;
				while (!botInfo.GotKey)
				{
					await SavingAndLoading.ValidateBotKey(client, botInfo, (startup ? Properties.Settings.Default.BotKey : Console.ReadLine()), startup);
					startup = false;
				}

				await SavingAndLoading.MaybeStartBot(client, botInfo);
			}
		}

		public async Task Start(IDiscordClient client)
		{
			Messages.WriteLine("Connecting the client...");

			try
			{
				await client.StartAsync();
			}
			catch (Exception e)
			{
				Messages.ExceptionToConsole(e);
				return;
			}

			await Task.Delay(-1);
		}

		private IServiceProvider ConfigureServices(IDiscordClient client, BotGlobalInfo globalInfo, LogHolder logHolder)
		{
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddSingleton(client);
			serviceCollection.AddSingleton(new CommandService(new CommandServiceConfig { CaseSensitiveCommands = false, ThrowOnError = false, }));
			serviceCollection.AddSingleton(globalInfo);
			serviceCollection.AddSingleton(logHolder);

			return new DefaultServiceProviderFactory().CreateServiceProvider(serviceCollection);
		}
	}
}