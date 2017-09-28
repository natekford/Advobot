using Advobot.Interfaces;
using Discord;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Advobot.Modules.Log
{
	public abstract class Logger
	{
		protected ILogModule _Logging;
		protected IDiscordClient _Client;
		protected IBotSettings _BotSettings;
		protected IGuildSettingsModule _GuildSettings;
		protected ITimersModule _Timers;

		public Logger(ILogModule logging, IServiceProvider provider)
		{
			_Logging		= logging;
			_Client			= provider.GetService<IDiscordClient>();
			_BotSettings	= provider.GetService<IBotSettings>();
			_GuildSettings	= provider.GetService<IGuildSettingsModule>();
			_Timers			= provider.GetService<ITimersModule>();

			HookUpEvents();
		}

		/// <summary>
		/// Called in the constructor of the abstract method.
		/// </summary>
		protected abstract void HookUpEvents();
	}
}
