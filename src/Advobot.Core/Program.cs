using System;
using System.Threading.Tasks;
using Advobot.Classes;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Core
{
	/// <summary>
	/// Starting point for Advobot.
	/// </summary>
	public class ConsoleLauncher
	{
		private static async Task Main()
		{
			AppDomain.CurrentDomain.UnhandledException += (sender, e) => IOUtils.LogUncaughtException(e.ExceptionObject);

			//Get the save path
			var savePath = true;
			while (!Config.ValidatePath((savePath ? null : Console.ReadLine()), savePath))
			{
				savePath = false;
			}

			var provider = CreationUtils.CreateDefaultServiceProvider<BotSettings, GuildSettings>(DiscordUtils.GetCommandAssemblies());
			var commandHandler = new CommandHandler(provider);
			var client = provider.GetService<IDiscordClient>();

			//Get the bot key
			var botKey = true;
			while (!await Config.ValidateBotKey(client, (botKey ? null : Console.ReadLine()), botKey).CAF())
			{
				botKey = false;
			}

			await ClientUtils.StartAsync(client).CAF();
		}
	}
}