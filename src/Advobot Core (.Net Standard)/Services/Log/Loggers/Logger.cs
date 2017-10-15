using Advobot.Actions;
using Advobot.Enums;
using Advobot.Interfaces;
using Discord;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Advobot.Services.Log.Loggers
{
	public abstract class Logger
	{
		protected ILogService _Logging;
		protected IDiscordClient _Client;
		protected IBotSettings _BotSettings;
		protected IGuildSettingsService _GuildSettings;
		protected ITimersService _Timers;

		public Logger(ILogService logging, IServiceProvider provider)
		{
			_Logging = logging;
			_Client = provider.GetService<IDiscordClient>();
			_BotSettings = provider.GetService<IBotSettings>();
			_GuildSettings = provider.GetService<IGuildSettingsService>();
			_Timers = provider.GetService<ITimersService>();
		}
	}
}
