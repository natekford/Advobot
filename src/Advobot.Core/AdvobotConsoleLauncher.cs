using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Advobot.Classes;
using Advobot.Interfaces;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace Advobot
{
	/// <summary>
	/// Puts the similarities from launching the console application and the .Net Core UI application into one.
	/// </summary>
	public sealed class AdvobotConsoleLauncher
	{
		private readonly ILowLevelConfig _Config;
		private IServiceCollection _Services;

		/// <summary>
		/// Creates an instance of <see cref="AdvobotConsoleLauncher"/>.
		/// </summary>
		/// <param name="args"></param>
		public AdvobotConsoleLauncher(string[] args)
		{
			AppDomain.CurrentDomain.UnhandledException += (sender, e) => IOUtils.LogUncaughtException(e.ExceptionObject);
			Console.Title = "Advobot";
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
		/// Gets the path and bot key from user input if they're not already stored in file.
		/// </summary>
		/// <returns></returns>
		public async Task GetPathAndKey()
		{
			SetPath();
			await SetBotKey().CAF();
		}
		/// <summary>
		/// Sets the path to use for the bot.
		/// </summary>
		private void SetPath()
		{
			//Get the save path
			var startup = true;
			while (!_Config.ValidatedPath)
			{
				startup = _Config.ValidatePath(startup ? null : Console.ReadLine(), startup);
			}
		}
		/// <summary>
		/// Sets the bot key to use for the bot.
		/// </summary>
		/// <returns></returns>
		private async Task SetBotKey()
		{
			//Get the bot key
			var startup = true;
			while (!_Config.ValidatedKey)
			{
				startup = await _Config.ValidateBotKey(startup ? null : Console.ReadLine(), startup, ClientUtils.RestartBotAsync).CAF();
			}
		}
		/// <summary>
		/// Returns the default services for the bot if both the path and key have been set.
		/// </summary>
		/// <returns></returns>
		public IServiceCollection GetDefaultServices(IEnumerable<Assembly> commands)
		{
			if (!(_Config.ValidatedPath && _Config.ValidatedKey))
			{
				throw new InvalidOperationException("Attempted to start the bot before the path and key have been set.");
			}
			return _Services ?? (_Services = _Config.CreateDefaultServices(commands));
		}
		/// <summary>
		/// Creates the service provider and starts the Discord bot.
		/// </summary>
		/// <returns></returns>
		public async Task Start(IServiceProvider provider)
		{
			if (!(_Config.ValidatedPath && _Config.ValidatedKey))
			{
				throw new InvalidOperationException("Attempted to start the bot before the path and key have been set.");
			}
			await _Config.StartAsync(provider.GetRequiredService<DiscordShardedClient>());
		}
	}
}