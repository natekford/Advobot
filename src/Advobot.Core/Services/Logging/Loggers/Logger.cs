using System;
using System.Threading.Tasks;
using Advobot.Classes;
using Advobot.Services.BotSettings;
using Advobot.Services.GuildSettings;
using Advobot.Services.Logging.Interfaces;
using Advobot.Services.Logging.LogCounters;
using Advobot.Services.Timers;
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
		protected BaseSocketClient Client { get; }
		/// <summary>
		/// The settings used in the bot.
		/// </summary>
		protected IBotSettings BotSettings { get; }
		/// <summary>
		/// The settings used in guilds.
		/// </summary>
		protected IGuildSettingsFactory GuildSettingsFactory { get; }
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
			Client = provider.GetRequiredService<BaseSocketClient>();
			BotSettings = provider.GetRequiredService<IBotSettings>();
			GuildSettingsFactory = provider.GetRequiredService<IGuildSettingsFactory>();
			Timers = provider.GetRequiredService<ITimerService>();
		}

		protected void NotifyLogCounterIncrement(string name, int count)
			=> LogCounterIncrement?.Invoke(this, new LogCounterIncrementEventArgs(name, count));
		protected Task ReplyAsync(
			ITextChannel? channel,
			string content = "",
			EmbedWrapper? embedWrapper = null,
			TextFileInfo? textFile = null)
		{
			if (channel == null)
			{
				return Task.CompletedTask;
			}
			return MessageUtils.SendMessageAsync(channel, content, embedWrapper, textFile);
		}
		protected async Task HandleAsync(
			IGuildUser user,
			LoggingContextArgs<IUserLoggingContext> args)
		{
			var context = await LoggingContext.CreateAsync(user, GuildSettingsFactory).CAF();
			await HandleAsync(context, args).CAF();
		}
		protected async Task HandleAsync(
			IMessage message,
			LoggingContextArgs<IMessageLoggingContext> args)
		{
			var context = await LoggingContext.CreateAsync(message, GuildSettingsFactory).CAF();
			await HandleAsync(context, args).CAF();
		}
		private async Task HandleAsync<T>(T? context, LoggingContextArgs<T> args)
			where T : class, ILoggingContext
		{
			if (context == null || BotSettings.Pause)
			{
				return;
			}
			if (context.CanLog(args.Action))
			{
				NotifyLogCounterIncrement(args.LogCounterName, 1);
				foreach (var task in args.WhenCanLog)
				{
					await task.Invoke(context).CAF();
				}
			}
			foreach (var task in args.AnyTime)
			{
				await task.Invoke(context).CAF();
			}
		}
	}
}
