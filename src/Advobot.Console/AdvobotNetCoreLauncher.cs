using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Advobot.Classes;
using Advobot.Interfaces;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Console
{
	using Console = System.Console;

	/// <summary>
	/// Puts the similarities from launching the console application and the .Net Core UI application into one.
	/// </summary>
	public sealed class AdvobotNetCoreLauncher
	{
		private readonly ILowLevelConfig _Config;
		private IIterableServiceProvider _Provider;

		/// <summary>
		/// Creates an instance of <see cref="AdvobotNetCoreLauncher"/>.
		/// </summary>
		/// <param name="args"></param>
		public AdvobotNetCoreLauncher(string[] args)
		{
			AppDomain.CurrentDomain.UnhandledException += (sender, e) => IOUtils.LogUncaughtException(e.ExceptionObject);
			ConsoleUtils.PrintingFlags = 0
				| ConsolePrintingFlags.Print
				| ConsolePrintingFlags.LogTime
				| ConsolePrintingFlags.LogCaller
				| ConsolePrintingFlags.RemoveDuplicateNewLines;

			_Config = LowLevelConfig.Load(args);
			ConsoleUtils.DebugWrite($"Args: {_Config.CurrentInstance}|{_Config.PreviousProcessId}", "Launcher Arguments");
		}

		/// <summary>
		/// Waits until the old process is killed. This is blocking.
		/// </summary>
		public void WaitUntilOldProcessKilled()
		{
			//Wait until the old process is killed
			if (_Config.PreviousProcessId != -1)
			{
				try
				{
					while (Process.GetProcessById(_Config.PreviousProcessId) != null)
					{
						Thread.Sleep(25);
					}
				}
				catch (ArgumentException) { }
			}
		}
		/// <summary>
		/// Sets the path to use for the bot.
		/// </summary>
		public void SetPath()
		{
			//Get the save path
			var startup = true;
			while (!_Config.ValidatedPath)
			{
				startup = _Config.ValidatePath((startup ? null : Console.ReadLine()), startup);
			}
		}
		/// <summary>
		/// Sets the bot key to use for the bot.
		/// </summary>
		/// <returns></returns>
		public async Task SetBotKey()
		{
			//Get the bot key
			var startup = true;
			while (!_Config.ValidatedKey)
			{
				startup = await _Config.ValidateBotKey((startup ? null : Console.ReadLine()), startup, ClientUtils.RestartBotAsync).CAF();
			}
		}
		/// <summary>
		/// Gets the services this bot will use.
		/// </summary>
		/// <returns></returns>
		public IIterableServiceProvider GetServiceProvider()
		{
			if (!(_Config.ValidatedPath && _Config.ValidatedKey))
			{
				throw new InvalidOperationException("Attempted to get the service provider before the path and key have been set.");
			}
			if (_Provider != null)
			{
				return _Provider;
			}

			_Provider = new IterableServiceProvider(_Config.CreateDefaultServices(), true);
			foreach (var db in _Provider.OfType<IUsesDatabase>())
			{
				db.Start();
			}
			return _Provider;
		}
		/// <summary>
		/// Creates the service provider and starts the Discord bot.
		/// </summary>
		/// <returns></returns>
		public async Task Start()
		{
			await _Config.StartAsync(GetServiceProvider().GetRequiredService<DiscordShardedClient>());
		}
	}
}