using Advobot.Commands;
using Advobot.Core;
using Advobot.Core.Classes.TypeReaders;
using Advobot.Core.Utilities;
using Discord;
using System;
using System.Diagnostics;
using System.Linq;
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
			//TODO: remove later, probably should make a test project at some point
#if DEBUG
			var testAmt = 250000;
			var value = (GuildPermission)101234971231223;

			var genericSw = new Stopwatch();
			genericSw.Start();
			for (int i = 0; i < testAmt; ++i)
			{
				var names = Utils.GetNamesFromEnum(value).ToList();
			}
			genericSw.Stop();
			Console.WriteLine($"{genericSw.ElapsedTicks}ticks, {genericSw.ElapsedMilliseconds}ms");
			genericSw.Reset();

			var specificSw = new Stopwatch();
			specificSw.Start();
			for (int i = 0; i < testAmt; ++i)
			{
				var names = Utils.GetNamesFromEnum2(value).ToList();
			}
			specificSw.Stop();
			Console.WriteLine($"{specificSw.ElapsedTicks}ticks, {specificSw.ElapsedMilliseconds}ms");
#endif

			AppDomain.CurrentDomain.UnhandledException += (sender, e) => IOUtils.LogUncaughtException(e.ExceptionObject);

			//Get the save path
			var savePath = true;
			while (!Config.ValidatePath((savePath ? null : Console.ReadLine()), savePath))
			{
				savePath = false;
			}

			var provider = await CreationUtils.CreateServiceProvider().CAF();
			var client = CommandHandler.Install(provider);

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