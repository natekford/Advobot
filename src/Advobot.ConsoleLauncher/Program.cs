using Advobot.Core;
using Advobot.Core.Classes;
using Advobot.Core.Classes.UserInformation;
using Advobot.Core.Enums;
using Advobot.Core.Utilities;
using Discord;
using LiteDB;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Advobot.ConsoleLauncher
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

			var provider = CreationUtils.CreateServiceProvider(typeof(BotSettings), typeof(GuildSettings));
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