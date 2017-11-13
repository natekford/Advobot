using Advobot.Core.Interfaces;
using Discord;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Advobot.Core.Services.Log.Loggers
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
			_Client = provider.GetRequiredService<IDiscordClient>();
			_BotSettings = provider.GetRequiredService<IBotSettings>();
			_GuildSettings = provider.GetRequiredService<IGuildSettingsService>();
			_Timers = provider.GetRequiredService<ITimersService>();
		}
	}
}
