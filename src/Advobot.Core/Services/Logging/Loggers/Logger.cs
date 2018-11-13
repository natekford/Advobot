using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Advobot.Classes;
using Advobot.Enums;
using Advobot.Interfaces;
using Advobot.Services.Logging.Interfaces;
using Advobot.Services.Logging.LogCounters;
using Advobot.Services.Logging.LoggingContexts;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Services.Logging.Loggers
{
	/// <summary>
	/// Logs specific things.
	/// </summary>
	internal abstract class Logger : ILogger
	{
		/// <summary>
		/// The bot client.
		/// </summary>
		protected DiscordShardedClient Client { get; }
		/// <summary>
		/// The settings used in the bot.
		/// </summary>
		protected IBotSettings BotSettings { get; }
		/// <summary>
		/// The settings used in guilds.
		/// </summary>
		protected IGuildSettingsFactory GuildSettings { get; }
		/// <summary>
		/// Timers for punishments.
		/// </summary>
		protected ITimerService Timers { get; }

		/// <inheritdoc />
		public event LogCounterIncrementEventHandler LogCounterIncrement;

		/// <summary>
		/// Creates an instance of logger.
		/// </summary>
		/// <param name="provider"></param>
		protected Logger(IServiceProvider provider)
		{
			Client = provider.GetRequiredService<DiscordShardedClient>();
			BotSettings = provider.GetRequiredService<IBotSettings>();
			GuildSettings = provider.GetRequiredService<IGuildSettingsFactory>();
			Timers = provider.GetRequiredService<ITimerService>();
		}

		/// <summary>
		/// Fires the log counter increment event.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="count"></param>
		protected void NotifyLogCounterIncrement(string name, int count)
			=> LogCounterIncrement?.Invoke(this, new LogCounterIncrementEventArgs(name, count));
		/// <summary>
		/// Awaits task in <paramref name="tasks"/> and if the context can log will await every task in <paramref name="whenCanLog"/>.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="name"></param>
		/// <param name="tasks"></param>
		/// <param name="whenCanLog"></param>
		/// <returns></returns>
		protected async Task HandleAsync(LoggingContext context, string name, Task[] tasks, Func<Task>[] whenCanLog)
		{
			if (BotSettings.Pause)
			{
				return;
			}
			if (context.CanLog)
			{
				NotifyLogCounterIncrement(name, 1);
				await Task.WhenAll(whenCanLog.Select(x => x.Invoke())).CAF();
			}
			if (tasks.Length > 0)
			{
				await Task.WhenAll(tasks).CAF();
			}
		}
		protected Task ReplyAsync(SocketTextChannel channel, string content = null, EmbedWrapper embedWrapper = null, TextFileInfo textFile = null)
			=> MessageUtils.SendMessageAsync(channel, content, embedWrapper, textFile);
	}
}
