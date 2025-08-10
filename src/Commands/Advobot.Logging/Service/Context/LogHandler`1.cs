using Advobot.Logging.Database;
using Advobot.Logging.Models;

using Discord;

using Microsoft.Extensions.Logging;

namespace Advobot.Logging.Service.Context;

public sealed class LogHandler<T>(
	LogAction action,
	ILogger logger,
	ILoggingDatabase db
) where T : ILogState
{
	public required IReadOnlyList<Func<ILogContext<T>, Task>> Handlers { get; init; }

	public async Task HandleAsync(T state)
	{
		var context = await CreateContextAsync(state).ConfigureAwait(false);
		if (context is null)
		{
			return;
		}

		var canLog = await state.CanLog(db, context).ConfigureAwait(false);
		if (!canLog)
		{
			return;
		}

		// Run logging in background since the results do not matter
		_ = Task.Run(async () =>
		{
			foreach (var handler in Handlers)
			{
				try
				{
					await handler.Invoke(context).ConfigureAwait(false);
				}
				catch (Exception e)
				{
					logger.LogWarning(
						exception: e,
						message: "Exception occurred while logging ({Action}) to Discord. {@Info}",
						args: [action, context.State]
					);
				}
			}
		});
	}

	private async Task<LogContext?> CreateContextAsync(T state)
	{
		// Invalid state or state somehow doesn't have a guild
		if (!state.IsValid || state.Guild is not IGuild guild)
		{
			return null;
		}

		// Action is disabled so don't bother logging
		var actions = await db.GetLogActionsAsync(guild.Id).ConfigureAwait(false);
		if (!actions.Contains(action))
		{
			return null;
		}

		var channels = await db.GetLogChannelsAsync(guild.Id).ConfigureAwait(false);
		var imageLog = await guild.GetTextChannelAsync(channels.ImageLogId).ConfigureAwait(false);
		var modLog = await guild.GetTextChannelAsync(channels.ModLogId).ConfigureAwait(false);
		var serverLog = await guild.GetTextChannelAsync(channels.ServerLogId).ConfigureAwait(false);
		// No log channels so there's no point in going further
		if (imageLog is null && modLog is null && serverLog is null)
		{
			return null;
		}

		var bot = await guild.GetCurrentUserAsync().ConfigureAwait(false);
		return new(state, guild, bot, imageLog, modLog, serverLog);
	}

	private sealed class LogContext(
		T state,
		IGuild guild,
		IGuildUser bot,
		ITextChannel? imageLog,
		ITextChannel? modLog,
		ITextChannel? serverLog) : ILogContext<T>
	{
		public IGuildUser Bot { get; } = bot;
		public IGuild Guild { get; } = guild;
		public ITextChannel? ImageLog { get; } = imageLog;
		public ITextChannel? ModLog { get; } = modLog;
		public ITextChannel? ServerLog { get; } = serverLog;
		public T State { get; } = state;
	}
}