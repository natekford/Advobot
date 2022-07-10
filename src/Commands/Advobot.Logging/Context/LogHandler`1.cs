using Advobot.Logging.Database;

using AdvorangesUtils;

using Discord;

using Microsoft.Extensions.Logging;

using System.Collections;

namespace Advobot.Logging.Context;

public sealed class LogHandler<T> : ICollection<Func<ILogContext<T>, Task>>
	where T : ILogState
{
	private readonly ICollection<Func<ILogContext<T>, Task>> _Actions =
		new List<Func<ILogContext<T>, Task>>();

	private readonly ILoggingDatabase _Db;
	private readonly ILogger _Logger;

	public LogAction Action { get; }
	public int Count => _Actions.Count;
	public bool IsReadOnly => _Actions.IsReadOnly;

	public LogHandler(LogAction action, ILogger logger, ILoggingDatabase db)
	{
		_Logger = logger;
		_Db = db;
		Action = action;
	}

	public void Add(Func<ILogContext<T>, Task> item)
		=> _Actions.Add(item);

	public void Clear()
		=> _Actions.Clear();

	public bool Contains(Func<ILogContext<T>, Task> item)
		=> _Actions.Contains(item);

	public void CopyTo(Func<ILogContext<T>, Task>[] array, int arrayIndex)
		=> _Actions.CopyTo(array, arrayIndex);

	public IEnumerator<Func<ILogContext<T>, Task>> GetEnumerator()
		=> _Actions.GetEnumerator();

	public async Task HandleAsync(T state)
	{
		var context = await CreateContextAsync(state).CAF();
		if (context is null)
		{
			return;
		}

		var canLog = await state.CanLog(_Db, context).CAF();
		if (!canLog)
		{
			return;
		}

		// Run logging in background since the results do not matter
		_ = Task.Run(async () =>
		{
			foreach (var action in this)
			{
				try
				{
					await action.Invoke(context).CAF();
				}
				catch (Exception e)
				{
					_Logger.LogWarning(
						eventId: new EventId(1, Action.ToString()),
						exception: e,
						message: "Exception occurred during logging to Discord. Info: {@Info}",
						context.State
					);
				}
			}
		});
	}

	public bool Remove(Func<ILogContext<T>, Task> item)
		=> _Actions.Remove(item);

	IEnumerator IEnumerable.GetEnumerator()
		=> ((IEnumerable)_Actions).GetEnumerator();

	private async Task<LogContext?> CreateContextAsync(T state)
	{
		// Invalid state or state somehow doesn't have a guild
		if (!state.IsValid || state.Guild is not IGuild guild)
		{
			return null;
		}

		// Action is disabled so don't bother logging
		var actions = await _Db.GetLogActionsAsync(guild.Id).CAF();
		if (!actions.Contains(Action))
		{
			return null;
		}

		var channels = await _Db.GetLogChannelsAsync(guild.Id).CAF();
		var imageLog = await guild.GetTextChannelAsync(channels.ImageLogId).CAF();
		var modLog = await guild.GetTextChannelAsync(channels.ModLogId).CAF();
		var serverLog = await guild.GetTextChannelAsync(channels.ServerLogId).CAF();
		// No log channels so there's no point in going further
		if (imageLog is null && modLog is null && serverLog is null)
		{
			return null;
		}

		var bot = await guild.GetCurrentUserAsync().CAF();
		return new(state, guild, bot, imageLog, modLog, serverLog);
	}

	private sealed class LogContext : ILogContext<T>
	{
		public IGuildUser Bot { get; }
		public IGuild Guild { get; }
		public ITextChannel? ImageLog { get; }
		public ITextChannel? ModLog { get; }
		public ITextChannel? ServerLog { get; }
		public T State { get; }

		public LogContext(
			T state,
			IGuild guild,
			IGuildUser bot,
			ITextChannel? imageLog,
			ITextChannel? modLog,
			ITextChannel? serverLog)
		{
			State = state;
			Guild = guild;
			Bot = bot;
			ImageLog = imageLog;
			ModLog = modLog;
			ServerLog = serverLog;
		}
	}
}